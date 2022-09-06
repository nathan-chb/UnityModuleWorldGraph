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
    public class WorldLinkWindow : EditorWindow
    {
        static public WorldLinkWindow winSingleton;

        [HideInInspector] public WorldStorageServer worldStorageServer;
        [HideInInspector] public WorldStorageUser worldStorageUser;

        [SerializeField] public List<string> anchors = new List<string>();

        bool groupEnabled;
        private static GUILayoutOption miniButtonWidth = GUILayout.Width(50);

        // World Anchors params
        string UUID = System.Guid.Empty.ToString();
        string customName = "(no name for World Links)";
        string creatorUUID = System.Guid.Empty.ToString();
        // From...
        private bool showListFrom = true;
        string fromName = "(none)";
        string fromUUID = System.Guid.Empty.ToString();
        // To...
        private bool showListTo = true;
        string toName = "(none)";
        string toUUID = System.Guid.Empty.ToString();

        UnitSystem unit = UnitSystem.CM;
        ObjectType fromType = ObjectType.NotIdentified, toType = ObjectType.NotIdentified;
        Vector3 localCRS_pos;
        Vector3 localCRS_rot;
        [SerializeField] Dictionary<string, List<string>> keyValueTags = new Dictionary<string, List<string>>();

        // UI stuffs
        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        public static void ShowWindow(WorldStorageServer ws, WorldStorageUser user, string UUID = "")
        {
            winSingleton = EditorWindow.GetWindow(typeof(WorldLinkWindow), false, WorldStorageWindow.winName) as WorldLinkWindow;
            winSingleton.worldStorageServer = ws;
            winSingleton.worldStorageUser = user;
            if (!string.IsNullOrEmpty(UUID))
            {
                winSingleton.UUID = UUID;
                winSingleton.GetWorldLinkParams();
            }
        }

        public WorldLinkWindow()
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
            GUILayout.BeginVertical("World Link Editor", gsTest);
            //
            GUILayout.Label("Server: " + worldStorageServer.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("User: " + worldStorageUser.userName, EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space();

            //GUILayout.BeginHorizontal();
            customName = EditorGUILayout.TextField("Name of Link", customName);
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
                GetWorldLinkParams();
            }
            GUI.backgroundColor = ori;

            unit = (UnitSystem)EditorGUILayout.EnumPopup("Unit System:", unit);

            EditorGUILayout.Space();
            showListFrom = EditorGUILayout.Foldout(showListFrom, "Parent Object (From)");
            if (showListFrom)
            {
                EditorGUILayout.BeginHorizontal();
                fromUUID = EditorGUILayout.TextField("UUID", fromUUID);
                GUI.backgroundColor = WorldStorageWindow.arfColors[1];
                if (GUILayout.Button("Find", EditorStyles.miniButtonLeft, miniButtonWidth))
                {
                    // TODO Request the object from the server
                    fromName = "(not implemented yet)";
                    fromType = ObjectType.NotIdentified;
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = ori;
                fromName = EditorGUILayout.TextField("Name", fromName);
                fromType = (ObjectType)EditorGUILayout.EnumPopup("Type:", fromType);
            }

            EditorGUILayout.Space();
            showListTo = EditorGUILayout.Foldout(showListTo, "Child Object (To)");
            if (showListTo)
            {
                EditorGUILayout.BeginHorizontal();
                toUUID = EditorGUILayout.TextField("UUID", toUUID);
                GUI.backgroundColor = WorldStorageWindow.arfColors[1];
                if (GUILayout.Button("Find", EditorStyles.miniButtonLeft, miniButtonWidth))
                {
                    // TODO Request the object from the server
                    toName = "(not implemented yet)";
                    toType = ObjectType.NotIdentified;
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = ori;
                toName = EditorGUILayout.TextField("Name", toName);
                toType = (ObjectType)EditorGUILayout.EnumPopup("Type:", toType);
            }

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
            if (GUILayout.Button("Create New World Link"))
            {
                Debug.Log("POST World Link");

                UUID = "0";
                if (string.IsNullOrEmpty(UUID) || UUID == "0") UUID = System.Guid.Empty.ToString();
                WorldLink obj = GenerateWorldLink();
                UUID = WorldLinkRequest.AddWorldLink(worldStorageServer, obj);
                WorldStorageWindow.WorldStorageWindowSingleton.GetWorldLinks();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Modify World Link"))
            {
                Debug.Log("PUT World Link");

                if (!string.IsNullOrEmpty(UUID) && UUID != "0")
                {
                    WorldLink obj = GenerateWorldLink();
                    UUID = WorldLinkRequest.UpdateWorldLink(worldStorageServer, obj);
                    WorldStorageWindow.WorldStorageWindowSingleton.GetWorldLinks();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
            }

            // ###########################################################
            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete World Link"))
            {
                Debug.Log("Delete World Link");
                WorldLinkRequest.DeleteWorldLink(worldStorageServer, UUID);
                UUID = System.Guid.Empty.ToString();
                creatorUUID = System.Guid.Empty.ToString();
                unit = UnitSystem.CM;
                WorldStorageWindow.WorldStorageWindowSingleton.GetWorldLinks();
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

        private void GetWorldLinkParams()
        {
            WorldLink obj = WorldLinkRequest.GetWorldLink(worldStorageServer, UUID);
            //customName = obj.Name;
            creatorUUID = obj.CreatorUUID.ToString();
            fromUUID = obj.UUIDFrom.ToString();
            toUUID = obj.UUIDTo.ToString();
            fromType = obj.TypeFrom;
            toType = obj.TypeTo;
            unit = obj.Unit;
            if (obj.Transform.Count == 16)
            {
                Matrix4x4 localCRS = new Matrix4x4();
                localCRS.m00 = obj.Transform[0]; localCRS.m01 = obj.Transform[1]; localCRS.m02 = obj.Transform[2]; localCRS.m03 = obj.Transform[3];
                localCRS.m10 = obj.Transform[4]; localCRS.m11 = obj.Transform[5]; localCRS.m12 = obj.Transform[6]; localCRS.m13 = obj.Transform[7];
                localCRS.m20 = obj.Transform[8]; localCRS.m21 = obj.Transform[9]; localCRS.m22 = obj.Transform[10]; localCRS.m23 = obj.Transform[11];
                localCRS.m30 = obj.Transform[12]; localCRS.m31 = obj.Transform[13]; localCRS.m32 = obj.Transform[14]; localCRS.m33 = obj.Transform[15];
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

        public WorldLink GenerateWorldLink()
        {
            Matrix4x4 localCRS = new Matrix4x4();
            localCRS = Matrix4x4.TRS(localCRS_pos, Quaternion.Euler(localCRS_rot), Vector3.one);
            List<float> _transform3d = new List<float>
            {
                localCRS.m00,    localCRS.m01,    localCRS.m02,    localCRS.m03,
                localCRS.m10,    localCRS.m11,    localCRS.m12,    localCRS.m13,
                localCRS.m20,    localCRS.m21,    localCRS.m22,    localCRS.m23,
                localCRS.m30,    localCRS.m31,    localCRS.m32,    localCRS.m33,
            };

            System.Guid _uuid = System.Guid.Parse(UUID);
            System.Guid _creator = System.Guid.Parse(worldStorageUser.UUID);
            System.Guid _from = System.Guid.Parse(fromUUID);
            System.Guid _to = System.Guid.Parse(toUUID);
            WorldLink t = new WorldLink(_uuid, _creator, _to, _from, fromType, toType, _transform3d, unit, keyValueTags);
            return t;
        }
    }
}