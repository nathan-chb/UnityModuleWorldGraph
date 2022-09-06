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
// Last change: June 2022
//

#define USING_OPENAPI_GENERATOR // alt. is Swagger
#define isDEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ETSI.ARF.WorldStorage.REST;

#if USING_OPENAPI_GENERATOR
using Org.OpenAPITools.Api;
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

        // UI stuffs
        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        public static void ShowWindow(WorldStorageServer ws, WorldStorageUser user, string UUID = "")
        {
            winSingleton = EditorWindow.GetWindow(typeof(TrackableWindow), false, WorldStorageWindow.winName) as TrackableWindow;
            winSingleton.worldStorageServer = ws;
            winSingleton.worldStorageUser = user;
            if (!string.IsNullOrEmpty(UUID))
            {
                winSingleton.UUID = UUID;
                winSingleton.GetTrackableParams();
            }
        }

        public TrackableWindow()
        {
            // init somne stuffs
        }

        void OnGUI()
        {
            ori = GUI.backgroundColor; // remember ori color

            gsTest = new GUIStyle("window");
            gsTest.normal.textColor = WorldStorageWindow.arfColors[0];
            gsTest.fontStyle = FontStyle.Bold;

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
            GUILayout.BeginVertical("Trackable Editor", gsTest);
            //
            GUILayout.Label("Server: " + worldStorageServer.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("User: " + worldStorageUser.userName, EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space();

            //GUILayout.BeginHorizontal();
            customName = EditorGUILayout.TextField("Name of Trackable", customName);
#if isDEBUG
            GUILayout.Label("UUID: " + UUID, EditorStyles.miniLabel); // readonly
            GUILayout.Label("Creator UID: " + creatorUUID, EditorStyles.miniLabel); // readonly
#endif
            EditorGUILayout.Space();

            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Read Parameters"))
            {
                UUID = WorldStorageWindow.GetUUIDFromString(customName);
                if (UUID == null) UUID = customName; // try this
                GetTrackableParams();
            }
            GUI.backgroundColor = ori;

            type = (Trackable.TrackableTypeEnum)EditorGUILayout.EnumPopup("Trackable Type:", type);
            unit = (UnitSystem)EditorGUILayout.EnumPopup("Unit System:", unit);

            EditorGUILayout.Space(10);
            trackableSize = EditorGUILayout.Vector3Field("Trackable Size:", trackableSize);

            EditorGUILayout.Space(10);
            GUILayout.Label("Local CRS:");
            localCRS_pos = EditorGUILayout.Vector3Field("Position:", localCRS_pos);
            localCRS_rot = EditorGUILayout.Vector3Field("Rotation:", localCRS_rot);

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
            //EditorGUILayout.IntField("Number of KeyValues", 0);
            //EditorGUILayout.Space();
            //EditorGUILayout.TextField("Key", "");
            //EditorGUILayout.TextField("Value", "");
            if (GUILayout.Button("Generate Dummy Key Values"))
            {
                // dummy
                keyValueTags.Clear();
                keyValueTags.Add("Location", new List<string> { "Room1" });
            }
            // Test
            //keyValueTags.Add("1", "a");
            //ScriptableObject target = this;
            //SerializedObject so = new SerializedObject(target);
            //SerializedProperty stringsProperty = so.FindProperty("trackables");
            //EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
            //so.ApplyModifiedProperties(); // Remember to apply modified properties
            EditorGUILayout.EndToggleGroup();
            
            GUILayout.EndVertical();

            // ###########################################################
            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Create New Trackable"))
            {
                Debug.Log("POST Trackable");

                UUID = "0";
                if (string.IsNullOrEmpty(UUID) || UUID == "0") UUID = System.Guid.Empty.ToString();
                Trackable obj = GenerateTrackable();
                UUID = TrackableRequest.AddTrackable(worldStorageServer, obj);
                WorldStorageWindow.WorldStorageWindowSingleton.GetTrackables();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Modify Trackable"))
            {
                Debug.Log("PUT Trackable");

                if (!string.IsNullOrEmpty(UUID) && UUID != "0")
                {
                    Trackable obj = GenerateTrackable();
                    UUID = TrackableRequest.UpdateTrackable(worldStorageServer, obj);
                }
                WorldStorageWindow.WorldStorageWindowSingleton.GetTrackables();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }

            // ###########################################################
            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete Trackable"))
            {
                Debug.Log("Delete Trackable");
                TrackableRequest.DeleteTrackable(worldStorageServer, UUID);
                UUID = System.Guid.Empty.ToString();
                creatorUUID = System.Guid.Empty.ToString();
                type = Trackable.TrackableTypeEnum.OTHER;
                unit = UnitSystem.CM;
                WorldStorageWindow.WorldStorageWindowSingleton.GetTrackables();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }
            GUI.backgroundColor = ori;

            // ###########################################################
            GUI.backgroundColor = WorldStorageWindow.arfColors[5];
            if (GUILayout.Button("Generate GameObject Component"))
            {
            }
            GUI.backgroundColor = ori;
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
                Matrix4x4 localCRS = new Matrix4x4();
                localCRS.m00 = obj.LocalCRS[0]; localCRS.m01 = obj.LocalCRS[1]; localCRS.m02 = obj.LocalCRS[2]; localCRS.m03 = obj.LocalCRS[3];
                localCRS.m10 = obj.LocalCRS[4]; localCRS.m11 = obj.LocalCRS[5]; localCRS.m12 = obj.LocalCRS[6]; localCRS.m13 = obj.LocalCRS[7];
                localCRS.m20 = obj.LocalCRS[8]; localCRS.m21 = obj.LocalCRS[9]; localCRS.m22 = obj.LocalCRS[10]; localCRS.m23 = obj.LocalCRS[11];
                localCRS.m30 = obj.LocalCRS[12]; localCRS.m31 = obj.LocalCRS[13]; localCRS.m32 = obj.LocalCRS[14]; localCRS.m33 = obj.LocalCRS[15];
                localCRS_pos = localCRS.GetPosition();
                localCRS_rot = localCRS.rotation.eulerAngles;
            }
            else
            {
                localCRS_pos = Vector3.zero;
                localCRS_rot = Vector3.zero;
            }
            keyValueTags = obj.KeyvalueTags;
            this.Repaint(); // TODO
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

            System.Guid _uuid = System.Guid.Parse(UUID);
            System.Guid _creator = System.Guid.Parse(worldStorageUser.UUID);
            Trackable t = new Trackable(_uuid, customName, _creator, type, trackableEncodingInformation, trackablePayload, _localCRS, unit, _trackableSize, keyValueTags);
            return t;
        }
    }
}