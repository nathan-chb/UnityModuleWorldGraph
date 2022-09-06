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
using ETSI.ARF.WorldStorage;
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
    public class WorldStorageWindow : EditorWindow
    {
        static public WorldStorageWindow WorldStorageWindowSingleton;

        [HideInInspector] public WorldStorageServer worldStorageServer;
        [HideInInspector] public WorldStorageUser worldStorageUser;

        [SerializeField] public List<string> creators = new List<string>();
        [SerializeField] public List<string> trackables = new List<string>();
        [SerializeField] public List<string> anchors = new List<string>();
        [SerializeField] public List<string> links = new List<string>();

        string ping = "-";
        string state = "Unknow";
        string vers = "Unknow";

        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        private static GUILayoutOption miniButtonWidth = GUILayout.Width(32);
        private static GUILayoutOption buttonWidth = GUILayout.Width(64f);
        private bool showListT = true;
        private bool showListA = true;
        private bool showListL = true;
        
        private string filterByKeyValueTag = "";

        static public string winName = "ARF Authoring Editor";
        static public int lineH = 5;
        static public Color[] arfColors = new Color[]
        {
            Color.yellow,                   // paneltext
            new Color(0.3f, 1f, 1f),        // button REST
            new Color(0.3f, 1f, 0.3f),      // button create
            new Color(1f, 0f, 0f),          // button delete (red)
            new Color(.7f, .5f, 1f),          // button graph window
            new Color(.3f, .7f, 1f),        // button generate prefab
            new Color(1f, 1f, 0.3f),        // button request
            new Color(1f, 0.3f, 0.3f),        // color for trackables
            new Color(1f, 0.7f, 0f),        // color for anchors
            new Color(.66f, .4f, 1f)         // color for links
        };


        //[MenuItem("[ ISG-ARF ]/World Storage Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(WorldStorageWindow), false, WorldStorageWindow.winName);
        }

        public WorldStorageWindow()
        {
            WorldStorageWindowSingleton = this;
        }

        static public void DrawCopyright()
        {
            // Title 
            GUILayout.Label("Augmented Reality Framework", EditorStyles.boldLabel);
            GUILayout.Label("Copyright (C) 2022, ETSI (BSD 3-Clause License)");
        }

        void OnGUI()
        {
            ori = GUI.backgroundColor;
            gsTest = new GUIStyle("window");
            //gsTest.normal.textColor = WorldStorageWindow.arfColors[0];
            gsTest.fontStyle = FontStyle.Bold;
            gsTest.alignment = TextAnchor.UpperLeft;
            gsTest.fontSize = 16;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            WorldStorageWindow.DrawCopyright();

            // Server info
            GUILayout.BeginVertical("World Storage Server", gsTest);
            EditorGUILayout.Space();
            Rect rect = EditorGUILayout.GetControlRect(false, 1); // WorldStorageWindow.lineH);
            EditorGUI.DrawRect(rect, Color.black);
            //
            GUILayout gl = new GUILayout();

            GUILayout.Label("Server Name: " + worldStorageServer.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("User Name: " + worldStorageUser.userName, EditorStyles.whiteLargeLabel);
#if isDEBUG
            GUILayout.Label("Creator UID: " + worldStorageUser.UUID);
            GUILayout.Label("Base Path: " + worldStorageServer.basePath);
            GUILayout.Label("Port: " + worldStorageServer.port);
#endif

            GUI.backgroundColor = WorldStorageWindow.arfColors[4];
            if (GUILayout.Button("Open World Representation Graph Window..."))
            {
            }
            GUI.backgroundColor = ori;

            DrawElementStuffs();

            EditorGUILayout.EndScrollView();
        }

        public void OnInspectorUpdate()
        {
            this.Repaint();
        }

        void DrawElementStuffs()
        {

            EditorGUILayout.Space();

            // ###########################################################
            // Handle admin
            // ###########################################################
            #region Ping
            GUILayout.BeginHorizontal();
            ping = EditorGUILayout.TextField("Last Ping", ping);
            if (GUILayout.Button("Ping"))
            {
                ping = AdminRequest.Ping(worldStorageServer);
            }
            GUI.backgroundColor = ori;
            GUILayout.EndHorizontal();
            #endregion

            #region State
            GUILayout.BeginHorizontal();
            state = EditorGUILayout.TextField("State", state);

            if (GUILayout.Button("Get World Storage Sate"))
            {
                state = AdminRequest.GetAdminInfo(worldStorageServer);
            }
            GUI.backgroundColor = ori;
            GUILayout.EndHorizontal();
            #endregion

            #region Version
            GUILayout.BeginHorizontal();
            vers = EditorGUILayout.TextField("Version", vers);

            if (GUILayout.Button("Get World Storage API Version"))
            {
                vers = AdminRequest.GetVersion(worldStorageServer);
            }
            GUI.backgroundColor = ori;
            GUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.Space();

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);

            GUI.backgroundColor = WorldStorageWindow.arfColors[4];
            if (GUILayout.Button("Open World Graph Window..."))
            {
            }
            GUI.backgroundColor = ori;

            // ###########################################################
            // Get creators
            // ###########################################################
            #region Get all creator UUID
            EditorGUILayout.Space();
            GUI.backgroundColor = WorldStorageWindow.arfColors[0];
            if (GUILayout.Button("Request UUID of Creators")) GetCreators();
            GUI.backgroundColor = ori;

            SerializedProperty stringsProperty = so.FindProperty("creators");
            EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties
            #endregion

            //EditorGUILayout.Space();
            //GUILayout.Label("World Storage Elements:", EditorStyles.whiteLargeLabel);


            // ###########################################################
            // Filter (Key = Group)
            // ###########################################################
            #region Filter
            EditorGUILayout.Space();
            filterByKeyValueTag = EditorGUILayout.TextField("Filter for KeyValue Group:", filterByKeyValueTag);
            #endregion

            // ###########################################################
            // Handle trackables
            // ###########################################################
            #region Get all trackable objects
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[7];
            Texture trackableImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/trackable.png", typeof(Texture));
            GUILayout.Box(trackableImage, GUILayout.Width(24), GUILayout.Height(24));
            GUI.backgroundColor = ori;
            GUILayout.Label("Trackables:", EditorStyles.whiteBoldLabel);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[0];
            if (GUILayout.Button("Request Trackables"))
            {
                GetTrackables();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create New"))
            {
                Debug.Log("Create trackable and open window");
                TrackableWindow.ShowWindow(worldStorageServer, worldStorageUser);
            }

            GUI.backgroundColor = ori;
            //GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete all Trackables (3 stay in!!!)"))
            {
                if (EditorUtility.DisplayDialog("Deleting elements", "Do you really want to delete all trackables?", "Yes", "No"))
                {
                    Debug.Log("Deleting all Trackable ");
                    int n = 0;
                    string UUID;
                    foreach (var customName in trackables)
                    {
                        if (!customName.Contains("[")) UUID = customName;
                        else
                        {
                            // extract the UUID
                            UUID = customName.Split('[', ']')[1];
                        }
                        if (++n > 3) TrackableRequest.DeleteTrackable(worldStorageServer, UUID);
                    }

                    GetTrackables();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
            }
            GUI.backgroundColor = ori;
            EditorGUILayout.EndHorizontal();

            // Show list
            stringsProperty = so.FindProperty("trackables");
            showListT = EditorGUILayout.BeginFoldoutHeaderGroup(showListT, "List of Trackables");
            if (showListT)
                for (int i = 0; i < stringsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(stringsProperty.GetArrayElementAtIndex(i));

                    string UUID = WorldStorageWindow.GetUUIDFromString(stringsProperty.GetArrayElementAtIndex(i).stringValue);
                    if (UUID == null) UUID = trackables[i]; // try this
                    if (GUILayout.Button("Edit...", EditorStyles.miniButtonLeft, buttonWidth))
                    {
                        Debug.Log("Open Trackable Window");
                        TrackableWindow.ShowWindow(worldStorageServer, worldStorageUser, UUID);
                    }

                    GUI.backgroundColor = WorldStorageWindow.arfColors[3];
                    if (GUILayout.Button("X", EditorStyles.miniButtonLeft, miniButtonWidth))
                    {
                        if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete this element?", "Delete", "Cancel"))
                        {
                            TrackableRequest.DeleteTrackable(worldStorageServer, UUID);
                            WorldStorageWindowSingleton.GetTrackables();
                            WorldStorageWindowSingleton.Repaint();
                        }
                    }
                    GUI.backgroundColor = ori;

                    EditorGUILayout.EndHorizontal();
                }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            // ###########################################################
            // Handle anchors
            // ###########################################################
            #region Get all anchor objects
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[8];
            Texture anchorImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/anchor.png", typeof(Texture));
            GUILayout.Box(anchorImage, GUILayout.Width(24), GUILayout.Height(24));
            GUI.backgroundColor = ori;
            GUILayout.Label("World Anchors:", EditorStyles.whiteBoldLabel);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[0];
            if (GUILayout.Button("Request Anchors"))
            {
                GetWorldAnchors();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create New"))
            {
                Debug.Log("Create anchor and open window");
                WorldAnchorWindow.ShowWindow(worldStorageServer, worldStorageUser);
            }

            GUI.backgroundColor = ori;
            //GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete all Anchors (3 stay in!!!)"))
            {
                if (EditorUtility.DisplayDialog("Deleting elements", "Do you really want to delete all anchors?", "Yes", "No"))
                {
                    Debug.Log("Deleting all World Anchors ");
                    int n = 0;
                    string UUID;
                    foreach (var customName in anchors)
                    {
                        if (!customName.Contains("[")) UUID = customName;
                        else
                        {
                            // extract the UUID
                            UUID = customName.Split('[', ']')[1];
                        }
                        if (++n > 3) WorldAnchorRequest.DeleteWorldAnchor(worldStorageServer, UUID);
                    }

                    GetWorldAnchors();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
            }
            GUI.backgroundColor = ori;
            EditorGUILayout.EndHorizontal();

            // Show list
            stringsProperty = so.FindProperty("anchors");
            showListA = EditorGUILayout.BeginFoldoutHeaderGroup(showListA, "List of World Anchors");
            if (showListA)
                for (int i = 0; i < stringsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(stringsProperty.GetArrayElementAtIndex(i));

                    string UUID = WorldStorageWindow.GetUUIDFromString(stringsProperty.GetArrayElementAtIndex(i).stringValue);
                    if (UUID == null) UUID = anchors[i]; // try this
                    if (GUILayout.Button("Edit...", EditorStyles.miniButtonLeft, buttonWidth))
                    {
                        Debug.Log("Open Anchor Window");
                        WorldAnchorWindow.ShowWindow(worldStorageServer, worldStorageUser, UUID);
                    }

                    GUI.backgroundColor = WorldStorageWindow.arfColors[3];
                    if (GUILayout.Button("X", EditorStyles.miniButtonLeft, miniButtonWidth))
                    {
                        if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete this element?", "Delete", "Cancel"))
                        {
                            WorldAnchorRequest.DeleteWorldAnchor(worldStorageServer, UUID);
                            WorldStorageWindowSingleton.GetWorldAnchors();
                            WorldStorageWindowSingleton.Repaint();
                        }
                    }
                    GUI.backgroundColor = ori;

                    EditorGUILayout.EndHorizontal();
                }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            // ###########################################################
            // Handle Links
            // ###########################################################
            #region Get all link objects
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[9];
            Texture linkImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/link.png", typeof(Texture));
            GUILayout.Box(linkImage, GUILayout.Width(24), GUILayout.Height(24));
            GUI.backgroundColor = ori;
            GUILayout.Label("World Links:", EditorStyles.whiteBoldLabel);
            GUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[0];
            if (GUILayout.Button("Request Links"))
            {
                GetWorldLinks();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create New"))
            {
                Debug.Log("Create link and open window");
                WorldLinkWindow.ShowWindow(worldStorageServer, worldStorageUser);
            }

            GUI.backgroundColor = ori;
            //GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete all Links (3 stay in!!!)"))
            {
                if (EditorUtility.DisplayDialog("Deleting elements", "Do you really want to delete all links?", "Yes", "No"))
                {
                    Debug.Log("Deleting all World Links");
                    int n = 0;
                    string UUID;
                    foreach (var customName in links)
                    {
                        if (!customName.Contains("[")) UUID = customName;
                        else
                        {
                            // extract the UUID
                            UUID = customName.Split('[', ']')[1];
                        }
                        if (++n > 3) WorldLinkRequest.DeleteWorldLink(worldStorageServer, UUID);
                    }

                    GetWorldLinks();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
            }
            GUI.backgroundColor = ori;
            EditorGUILayout.EndHorizontal();

            // Show list
            stringsProperty = so.FindProperty("links");
            showListL = EditorGUILayout.BeginFoldoutHeaderGroup(showListL, "List of World Links");
            if (showListL)
                for (int i = 0; i < stringsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(stringsProperty.GetArrayElementAtIndex(i));

                    string UUID = WorldStorageWindow.GetUUIDFromString(stringsProperty.GetArrayElementAtIndex(i).stringValue);
                    if (UUID == null) UUID = links[i]; // try this
                    if (GUILayout.Button("Edit...", EditorStyles.miniButtonLeft, buttonWidth))
                    {
                        Debug.Log("Open Link Window");
                        
                        WorldLinkWindow.ShowWindow(worldStorageServer, worldStorageUser, UUID);
                    }

                    GUI.backgroundColor = WorldStorageWindow.arfColors[3];
                    if (GUILayout.Button("X", EditorStyles.miniButtonLeft, miniButtonWidth))
                    {
                        if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete this element?", "Delete", "Cancel"))
                        {
                            WorldLinkRequest.DeleteWorldLink(worldStorageServer, UUID);
                            WorldStorageWindowSingleton.GetWorldLinks();
                            WorldStorageWindowSingleton.Repaint();
                        }
                    }
                    GUI.backgroundColor = ori;

                    EditorGUILayout.EndHorizontal();
                }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            //
            GUILayout.EndVertical();
        }

        // ###########################################################
        // Get elements from current server
        // ###########################################################
        #region Helpers
        static public string GetUUIDFromString(string text)
        {
            if (!text.Contains("[")) return null;
            else
            {
                // extract the UUID
                return text.Split('[', ']')[1];
            }
        }
        public void GetCreators()
        {
            // Get all objects
            Debug.Log("Get all creators id");
            List<Trackable> res = TrackableRequest.GetAllTrackables(worldStorageServer);
            creators.Clear();
            foreach (var item in res)
            {
                if (!creators.Contains(item.CreatorUUID.ToString())) creators.Add(item.CreatorUUID.ToString());
            }
        }

        static public (string, string) GetFirstKeyValueTags(Dictionary<string, List<string>> dict)
        {
            if (dict.Count >= 1)
            {
                // Get the first value in account (demo)
                foreach (var item in dict)
                {
                    string key1 = item.Key;
                    if (item.Value.Count >= 1)
                    {
                        string value1 = item.Value[0];
                        return (key1, value1);
                    }
                }
            }
            return ("", "");
        }

        static public Matrix4x4 MatrixFromLocalCRS(List<float> localCRS)
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = localCRS[0]; matrix.m01 = localCRS[1]; matrix.m02 = localCRS[2]; matrix.m03 = localCRS[3];
            matrix.m10 = localCRS[4]; matrix.m11 = localCRS[5]; matrix.m12 = localCRS[6]; matrix.m13 = localCRS[7];
            matrix.m20 = localCRS[8]; matrix.m21 = localCRS[9]; matrix.m22 = localCRS[10]; matrix.m23 = localCRS[11];
            matrix.m30 = localCRS[12]; matrix.m31 = localCRS[13]; matrix.m32 = localCRS[14]; matrix.m33 = localCRS[15];
            return matrix;
        }

        public void GetTrackables()
        {
            // Get all objects
            Debug.Log("Get all server objects");
            List<Trackable> res = TrackableRequest.GetAllTrackables(worldStorageServer);
            trackables.Clear();
            foreach (var item in res)
            {
                if (filterByKeyValueTag != "")
                {
                    var first = GetFirstKeyValueTags(item.KeyvalueTags);
                    if (first.Item1.ToLower() != "group" || first.Item2 != filterByKeyValueTag) continue;
                }
                if (!string.IsNullOrEmpty(item.Name)) trackables.Add(item.Name + " [" + item.UUID.ToString() + "]");
                else trackables.Add(item.UUID.ToString());
            }
        }

        public void GetWorldAnchors()
        {
            // Get all objects
            Debug.Log("Get all server objects");
            List<WorldAnchor> res = WorldAnchorRequest.GetAllWorldAnchors(worldStorageServer);
            anchors.Clear();
            foreach (var item in res)
            {
                if (filterByKeyValueTag != "")
                {
                    var first = GetFirstKeyValueTags(item.KeyvalueTags);
                    if (first.Item1.ToLower() != "group" || first.Item2 != filterByKeyValueTag) continue;
                }
                if (!string.IsNullOrEmpty(item.Name)) anchors.Add(item.Name + " [" + item.UUID.ToString() + "]");
                else anchors.Add(item.UUID.ToString());
            }
        }

        public void GetWorldLinks()
        {
            // Get all objects
            Debug.Log("Get all server objects");
            List<WorldLink> res = WorldLinkRequest.GetAllWorldLinks(worldStorageServer);
            links.Clear();
            foreach (var item in res)
            {
                links.Add(item.UUID.ToString());
            }
        }
        #endregion
    }
}