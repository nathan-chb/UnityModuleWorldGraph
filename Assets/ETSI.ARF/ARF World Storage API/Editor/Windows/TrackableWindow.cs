//
// ARF - Augmented Reality Framework (ETSI ISG ARF)
//
// Copyright 2022 ETSI
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Last change: July 2022
//

#define USING_OPENAPI_GENERATOR // alt. is Swagger
#define isDEBUG

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;
using ETSI.ARF.WorldStorage.REST;
using System;

#if USING_OPENAPI_GENERATOR
using Org.OpenAPITools.Model;
#else
using IO.Swagger.Api;
using IO.Swagger.Model;
#endif


namespace ETSI.ARF.WorldStorage.UI
{
    public class TrackableWindow : EditorWindow
    {
        static public TrackableWindow winSingleton;

        [HideInInspector] public WorldStorageServer worldStorageServer;
        [HideInInspector] public WorldStorageUser worldStorageUser;

        [SerializeField] public List<string> trackables = new List<string>();

        bool groupEnabled;

        // Trackable params
        string UUID = System.Guid.Empty.ToString();
        string customName = "NotDefined";
        string creatorUUID = System.Guid.Empty.ToString();
        Trackable.TrackableTypeEnum type = Trackable.TrackableTypeEnum.OTHER;
        UnitSystem unit = UnitSystem.CM;
        Vector3 trackableSize;
        Vector3 localCRS_pos;
        Vector3 localCRS_rot;
        byte[] trackablePayload = new byte[1] { 0 };

        [SerializeField] Dictionary<string, List<string>> keyValueTags = new Dictionary<string, List<string>>();
        string key1 = "";
        string value1 = "";

        // UI stuffs
        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        //graph params to generate the node
        public bool useCoord;
        public float nodePosX = 0;
        public float nodePosY = 0;

        public static void ShowWindow(WorldStorageServer ws, WorldStorageUser user, string UUID = "")
        {
            winSingleton = EditorWindow.GetWindow(typeof(TrackableWindow), false, "ETSI ARF - Trackable") as TrackableWindow;
            winSingleton.worldStorageServer = ws;
            winSingleton.worldStorageUser = user;
            if (!string.IsNullOrEmpty(UUID))
            {
                winSingleton.UUID = UUID;
                winSingleton.GetTrackableParams();
            }
            else
            {
                // Create new one
                winSingleton.AddTrackable();
            }
        }

        public static GameObject GenerateAndUpdateVisual(string UUID, string name, Vector3 pos, Vector3 rot)
        {
            Prefabs.WorldStoragePrefabs prefabs;
            prefabs = (Prefabs.WorldStoragePrefabs)Resources.Load("ARFPrefabs");
            GameObject arf = GameObject.Find("ARF Visuals");
            GameObject visual = GameObject.Find(UUID);

            if (arf == null) arf = new GameObject("ARF Visuals");
            if (visual == null)
            {
                visual = SceneAsset.Instantiate<GameObject>(prefabs.trackablePrefab, pos, Quaternion.Euler(rot), arf.transform); // TODO rot
                visual.name = UUID;
            }
            else
            {
                visual.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
            }
            visual.transform.Find("Canvas/Text").GetComponent<TextMeshProUGUI>().text = $"Name: { name }\nUUID: { UUID }";
            return visual;
        }

        public TrackableWindow()
        {
            // init somne stuffs
        }

        void OnGUI()
        {
            ori = GUI.backgroundColor; // remember ori color

            gsTest = new GUIStyle("window");
            //gsTest.normal.textColor = WorldStorageWindow.arfColors[0];
            gsTest.fontStyle = FontStyle.Bold;
            gsTest.alignment = TextAnchor.UpperLeft;
            gsTest.fontSize = 16;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            WorldStorageWindow.DrawCopyright();

            DrawTrackableStuffs();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Close Window"))
            {
                Close();
            }
        }

        void DrawTrackableStuffs()// Trackable trackable)
        {
            GUILayout.BeginVertical(); // "Trackable Editor", gsTest);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[7];
            Texture trackableImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/trackable.png", typeof(Texture));
            GUILayout.Box(trackableImage, GUILayout.Width(24), GUILayout.Height(24));
            GUI.backgroundColor = ori;
            GUILayout.Label("Trackable Parameters:", EditorStyles.whiteBoldLabel);
            GUILayout.EndHorizontal();

            Rect rect = EditorGUILayout.GetControlRect(false, WorldStorageWindow.lineH);
            EditorGUI.DrawRect(rect, WorldStorageWindow.arfColors[7]);

            //
            GUILayout.Label("Server: " + worldStorageServer.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("User: " + worldStorageUser.userName, EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space();

            customName = EditorGUILayout.TextField("Name of Trackable:", customName);
#if isDEBUG
            GUILayout.Label("UUID: " + UUID, EditorStyles.miniLabel); // readonly
            GUILayout.Label("Creator UID: " + creatorUUID, EditorStyles.miniLabel); // readonly
#endif
            EditorGUILayout.Space();

            // ---------------------
            // Toolbar
            // ---------------------
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Save"))
            {
                Debug.Log("PUT Trackable");

                if (!string.IsNullOrEmpty(UUID) && UUID != "0" && UUID != System.Guid.Empty.ToString())
                {
                    Trackable obj = GenerateTrackable();
                    UUID = TrackableRequest.UpdateTrackable(worldStorageServer, obj);
                    UUID = UUID.Trim('"'); //Bugfix: remove " from server return value
                }
                if (WorldStorageWindow.WorldStorageWindowSingleton != null)
                {
                    WorldStorageWindow.WorldStorageWindowSingleton.GetTrackables();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
                Close();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete"))
            {
                Debug.Log("Delete Trackable");
                TrackableRequest.DeleteTrackable(worldStorageServer, UUID);
                UUID = System.Guid.Empty.ToString();
                customName = "Warning: Object deleted !";
                creatorUUID = System.Guid.Empty.ToString();
                type = Trackable.TrackableTypeEnum.OTHER;
                unit = UnitSystem.CM;
                if (WorldStorageWindow.WorldStorageWindowSingleton != null)
                {
                    WorldStorageWindow.WorldStorageWindowSingleton.GetTrackables();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
                Close();
            }
            GUI.backgroundColor = ori;

            GUI.backgroundColor = WorldStorageWindow.arfColors[5];
            if (GUILayout.Button("Generate/Update GameObject"))
            {
                GenerateAndUpdateVisual(UUID, customName, localCRS_pos, localCRS_rot);
            }
            GUI.backgroundColor = ori;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // ---------------------
            // Params
            // ---------------------
            type = (Trackable.TrackableTypeEnum)EditorGUILayout.EnumPopup("Trackable Type:", type);
            unit = (UnitSystem)EditorGUILayout.EnumPopup("Unit System:", unit);

            EditorGUILayout.Space();
            trackableSize = EditorGUILayout.Vector3Field("Trackable Size:", trackableSize);

            EditorGUILayout.Space();
            GUILayout.Label("Local CRS:");
            localCRS_pos = EditorGUILayout.Vector3Field("   Position:", localCRS_pos);
            localCRS_rot = EditorGUILayout.Vector3Field("   Rotation:", localCRS_rot);

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate Dummy Payload"))
            {
                // dummy
                trackablePayload = new byte[100];
                for (int i = 0; i < trackablePayload.Length; i++)
                {
                    trackablePayload[i] = (byte)i;
                }
            }

            EditorGUILayout.Space();
            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Parameters:", groupEnabled);
            key1 = EditorGUILayout.TextField("Key 1", key1);
            value1 = EditorGUILayout.TextField("Value 1", value1);
            EditorGUILayout.EndToggleGroup();
            //
            GUILayout.EndVertical();
        }

        private void GetTrackableParams()
        {
            Trackable obj = TrackableRequest.GetTrackable(worldStorageServer, UUID);
            customName = obj.Name;
            creatorUUID = obj.CreatorUUID.ToString();
            type = obj.TrackableType;
            unit = obj.Unit;
            if (obj.TrackableSize.Count == 3)
            {
                trackableSize = new Vector3((float)obj.TrackableSize[0], (float)obj.TrackableSize[1], (float)obj.TrackableSize[2]);
            }
            else trackableSize = Vector3.zero;

            if (obj.LocalCRS.Count == 16)
            {
                Matrix4x4 localCRS = WorldStorageWindow.MatrixFromLocalCRS(obj.LocalCRS);
                localCRS_pos = localCRS.GetPosition();
                localCRS_rot = localCRS.rotation.eulerAngles;
            }
            else
            {
                localCRS_pos = Vector3.zero;
                localCRS_rot = Vector3.zero;
            }

            // Read a key value (demo)
            var first = WorldStorageWindow.GetFirstKeyValueTags(obj.KeyvalueTags);
            key1 = first.Item1;
            value1 = first.Item2;

            this.Repaint();
        }

        public void AddTrackable()
        {
            Debug.Log("POST Trackable");
            UUID = System.Guid.Empty.ToString();
            customName = "Default Trackable";

            Trackable obj = GenerateTrackable();
            UUID = TrackableRequest.AddTrackable(worldStorageServer, obj);
            UUID = UUID.Trim('"'); //Bugfix: remove " from server return value
            WorldStorageWindow.WorldStorageWindowSingleton.GetTrackables();
            WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
        }

        public Trackable GenerateTrackable()
        {
            EncodingInformationStructure trackableEncodingInformation = new EncodingInformationStructure(EncodingInformationStructure.DataFormatEnum.ARCORE, "1.0");
            Debug.Log("Created encoding information");

#if USING_OPENAPI_GENERATOR
            List<double> _trackableSize = new List<double>();
#else
    List<double?> _trackableSize = new List<double?>();
#endif
            _trackableSize.Add(trackableSize.x);
            _trackableSize.Add(trackableSize.y);
            _trackableSize.Add(trackableSize.z);
            Debug.Log("Created dimension");

            Matrix4x4 localCRS = new Matrix4x4();
            localCRS = Matrix4x4.TRS(localCRS_pos, Quaternion.Euler(localCRS_rot), Vector3.one);
            List<float> _localCRS = new List<float>
            {
                localCRS.m00,    localCRS.m01,    localCRS.m02,    localCRS.m03,
                localCRS.m10,    localCRS.m11,    localCRS.m12,    localCRS.m13,
                localCRS.m20,    localCRS.m21,    localCRS.m22,    localCRS.m23,
                localCRS.m30,    localCRS.m31,    localCRS.m32,    localCRS.m33,
            };

            // Create a key value (one from demo)
            keyValueTags.Clear();
            keyValueTags.Add(key1, new List<string> { value1 });

            System.Guid _uuid = System.Guid.Parse(UUID);
            System.Guid _creator = System.Guid.Parse(worldStorageUser.UUID);
            Trackable t = new Trackable(_uuid, customName, _creator, type, trackableEncodingInformation, trackablePayload, _localCRS, unit, _trackableSize, keyValueTags);

            var posX = new List<String>();
            posX.Add(nodePosX.ToString());
            t.KeyvalueTags["unityAuthoringPosX"] = posX;
            var posY = new List<String>();
            posY.Add(nodePosY.ToString());
            t.KeyvalueTags["unityAuthoringPosY"] = posY;

            return t;
        }
    }
}