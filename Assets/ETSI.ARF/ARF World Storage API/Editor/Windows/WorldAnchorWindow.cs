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
    public class WorldAnchorWindow : EditorWindow
    {
        static public WorldAnchorWindow winSingleton;

        [HideInInspector] public WorldStorageServer worldStorageServer;
        [HideInInspector] public WorldStorageUser worldStorageUser;

        [SerializeField] public List<string> anchors = new List<string>();

        bool groupEnabled;

        // World Anchors params
        string UUID = System.Guid.Empty.ToString();
        string customName = "NotDefined";
        string creatorUUID = System.Guid.Empty.ToString();
        UnitSystem unit = UnitSystem.CM;
        Vector3 worldAnchorSize;
        Vector3 localCRS_pos;
        Vector3 localCRS_rot;
        [SerializeField] Dictionary<string, List<string>> keyValueTags = new Dictionary<string, List<string>>();

        // UI stuffs
        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        public static void ShowWindow(WorldStorageServer ws, WorldStorageUser user, string UUID = "")
        {
            winSingleton = EditorWindow.GetWindow(typeof(WorldAnchorWindow), false, WorldStorageWindow.winName) as WorldAnchorWindow;
            winSingleton.worldStorageServer = ws;
            winSingleton.worldStorageUser = user;
            if (!string.IsNullOrEmpty(UUID))
            {
                winSingleton.UUID = UUID;
                winSingleton.GetWorldAnchorParams();
            }
        }

        public WorldAnchorWindow()
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

            DrawAnchorStuffs();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Close Window"))
            {
                Close();
            }
        }

        void DrawAnchorStuffs()
        {
            GUILayout.BeginVertical("World Anchor Editor", gsTest);
            //
            GUILayout.Label("Server: " + worldStorageServer.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("User: " + worldStorageUser.userName, EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space();

            //GUILayout.BeginHorizontal();
            customName = EditorGUILayout.TextField("Name of Anchor", customName);
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
                GetWorldAnchorParams();
            }
            GUI.backgroundColor = ori;

            unit = (UnitSystem)EditorGUILayout.EnumPopup("Unit System:", unit);

            EditorGUILayout.Space(10);
            worldAnchorSize = EditorGUILayout.Vector3Field("Trackable Size:", worldAnchorSize);

            EditorGUILayout.Space(10);
            GUILayout.Label("Local CRS:");
            localCRS_pos = EditorGUILayout.Vector3Field("Position:", localCRS_pos);
            localCRS_rot = EditorGUILayout.Vector3Field("Rotation:", localCRS_rot);

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
            EditorGUILayout.EndToggleGroup();
            //
            GUILayout.EndVertical();

            // ###########################################################
            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Create New World Anchor"))
            {
                Debug.Log("POST World Anchor");

                UUID = "0";
                if (string.IsNullOrEmpty(UUID) || UUID == "0") UUID = System.Guid.Empty.ToString();
                WorldAnchor obj = GenerateWorldAnchor();
                UUID = WorldAnchorRequest.AddWorldAnchor(worldStorageServer, obj);
                WorldStorageWindow.WorldStorageWindowSingleton.GetWorldAnchors();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Modify World Anchor"))
            {
                Debug.Log("PUT World Anchor");

                if (!string.IsNullOrEmpty(UUID) && UUID != "0")
                {
                    WorldAnchor obj = GenerateWorldAnchor();
                    UUID = WorldAnchorRequest.UpdateWorldAnchor(worldStorageServer, obj);
                    WorldStorageWindow.WorldStorageWindowSingleton.GetWorldAnchors();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
            }

            // ###########################################################
            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete World Anchor"))
            {
                Debug.Log("Delete World Anchor");
                WorldAnchorRequest.DeleteWorldAnchor(worldStorageServer, UUID);
                UUID = System.Guid.Empty.ToString();
                creatorUUID = System.Guid.Empty.ToString();
                unit = UnitSystem.CM;
                WorldStorageWindow.WorldStorageWindowSingleton.GetWorldAnchors();
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

        private void GetWorldAnchorParams()
        {
            WorldAnchor obj = WorldAnchorRequest.GetWorldAnchor(worldStorageServer, UUID);
            customName = obj.Name;
            creatorUUID = obj.CreatorUUID.ToString();
            unit = obj.Unit;
            if (obj.WorldAnchorSize.Count == 3)
            {
                worldAnchorSize = new Vector3((float)obj.WorldAnchorSize[0], (float)obj.WorldAnchorSize[1], (float)obj.WorldAnchorSize[2]);
            }
            else worldAnchorSize = Vector3.zero;
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

        public WorldAnchor GenerateWorldAnchor()
        {
#if USING_OPENAPI_GENERATOR
            List<double> _worldAnchorSize = new List<double>();
#else
    List<double?> trackableDimension = new List<double?>();
#endif
            _worldAnchorSize.Add(worldAnchorSize.x);
            _worldAnchorSize.Add(worldAnchorSize.x);
            _worldAnchorSize.Add(worldAnchorSize.y);
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
            WorldAnchor t = new WorldAnchor(_uuid, customName, _creator, _localCRS, unit, _worldAnchorSize, keyValueTags);
            return t;
        }
    }
}