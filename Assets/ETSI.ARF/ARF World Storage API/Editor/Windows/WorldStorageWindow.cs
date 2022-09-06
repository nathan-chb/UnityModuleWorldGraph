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

        static public string winName = "ARF Authoring Editor";
        static public Color[] arfColors = new Color[]
        {
            Color.yellow,                   // paneltext
            new Color(0.3f, 1f, 1f),        // button REST
            new Color(1f, 1f, 0.3f),        // button create + window
            new Color(1f, 0f, 0f),          // button delete (red)
            new Color(1f, 0f, 1f),           // button graph
            new Color(.7f, .5f, 1f)           // button prefab
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
            GUILayout.Label("Copyright(c) 2022, ETSI (BSD 3-Clause License)");
        }

        void OnGUI()
        {
            ori = GUI.backgroundColor;
            gsTest = new GUIStyle("window");
            gsTest.fontStyle = FontStyle.Bold;
            gsTest.normal.textColor = WorldStorageWindow.arfColors[0];

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            WorldStorageWindow.DrawCopyright();

            // Server info
            GUILayout.BeginVertical("World Storage Server", gsTest);
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
                GraphWindow.ShowWindow();
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

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);

            // ###########################################################
            // Get creators
            // ###########################################################
            #region Get all creator UUID
            EditorGUILayout.Space(10);
            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Request all Creator ID")) GetCreators();
            GUI.backgroundColor = ori;

            SerializedProperty stringsProperty = so.FindProperty("creators");
            EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties
            #endregion

            // ###########################################################
            // Handle trackables
            // ###########################################################
            #region Get all trackable objects
            EditorGUILayout.Space(10);

            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Request Trackables"))
            {
                GetTrackables();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create/Edit Trackable..."))
            {
                Debug.Log("Open Trackable Window");
                TrackableWindow.ShowWindow(worldStorageServer, worldStorageUser);
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete all Trackables (3 stay in!!!)"))
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
            GUI.backgroundColor = ori;
           
            // Show list
            stringsProperty = so.FindProperty("trackables");
            //EditorGUILayout.PropertyField(stringsProperty, /*new GUIContent("Trackbales"),*/ true); // True means show children
            //so.ApplyModifiedProperties(); // Remember to apply modified properties

            // New version with "Edit" button:
            showListT = EditorGUILayout.BeginFoldoutHeaderGroup(showListT, "List of Trackables");
            if (showListT)
                for (int i = 0; i < stringsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(stringsProperty.GetArrayElementAtIndex(i));
                    //EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);

                    string UUID = WorldStorageWindow.GetUUIDFromString(stringsProperty.GetArrayElementAtIndex(i).stringValue);
                    if (UUID == null) UUID = trackables[i]; // try this
                    if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
                    {
                        TrackableRequest.DeleteTrackable(worldStorageServer, UUID);
                        WorldStorageWindowSingleton.GetTrackables();
                        WorldStorageWindowSingleton.Repaint();
                    }
                    if (GUILayout.Button("Edit...", EditorStyles.miniButtonLeft, buttonWidth))
                    {
                        Debug.Log("Open Trackable Window");
                        TrackableWindow.ShowWindow(worldStorageServer, worldStorageUser, UUID);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            // ###########################################################
            // Handle anchors
            // ###########################################################
            #region Get all anchor objects
            EditorGUILayout.Space(10);

            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Request World Anchors"))
            {
                GetWorldAnchors();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create/Edit World Anchor..."))
            {
                Debug.Log("Open World Anchor Window");
                WorldAnchorWindow.ShowWindow(worldStorageServer, worldStorageUser);
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete all World Anchors (3 stay in!!!)"))
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
            GUI.backgroundColor = ori;

            // Show list
            stringsProperty = so.FindProperty("anchors");
            //EditorGUILayout.PropertyField(stringsProperty, /*new GUIContent("Trackbales"),*/ true); // True means show children
            //so.ApplyModifiedProperties(); // Remember to apply modified properties

            // New version with "Edit" button:
            showListA = EditorGUILayout.BeginFoldoutHeaderGroup(showListA, "List of World Anchors");
            if (showListA)
                for (int i = 0; i < stringsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(stringsProperty.GetArrayElementAtIndex(i));
                    //EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);

                    string UUID = WorldStorageWindow.GetUUIDFromString(stringsProperty.GetArrayElementAtIndex(i).stringValue);
                    if (UUID == null) UUID = anchors[i]; // try this
                    if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
                    {
                        WorldAnchorRequest.DeleteWorldAnchor(worldStorageServer, UUID);
                        WorldStorageWindowSingleton.GetWorldAnchors();
                        WorldStorageWindowSingleton.Repaint();
                    }
                    if (GUILayout.Button("Edit...", EditorStyles.miniButtonLeft, buttonWidth))
                    {
                        Debug.Log("Open Anchor Window");
                        WorldAnchorWindow.ShowWindow(worldStorageServer, worldStorageUser, UUID);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            EditorGUILayout.EndFoldoutHeaderGroup();
            #endregion

            // ###########################################################
            // Handle Links
            // ###########################################################
            #region Get all link objects
            EditorGUILayout.Space(10);

            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Request World Links"))
            {
                GetWorldLinks();
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create/Edit World Link..."))
            {
                Debug.Log("Open World Link Window");
                WorldLinkWindow.ShowWindow(worldStorageServer, worldStorageUser);
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete all World Links (3 stay in!!!)"))
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
            GUI.backgroundColor = ori;

            // Show list
            stringsProperty = so.FindProperty("links");
            //EditorGUILayout.PropertyField(stringsProperty, /*new GUIContent("Trackbales"),*/ true); // True means show children
            //so.ApplyModifiedProperties(); // Remember to apply modified properties

            // New version with "Edit" button:
            showListL = EditorGUILayout.BeginFoldoutHeaderGroup(showListL, "List of World Links");
            if (showListL)
                for (int i = 0; i < stringsProperty.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(stringsProperty.GetArrayElementAtIndex(i));
                    //EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none);

                    string UUID = WorldStorageWindow.GetUUIDFromString(stringsProperty.GetArrayElementAtIndex(i).stringValue);
                    if (UUID == null) UUID = links[i]; // try this
                    if (GUILayout.Button("-", EditorStyles.miniButtonLeft, miniButtonWidth))
                    {
                        WorldLinkRequest.DeleteWorldLink(worldStorageServer, UUID);
                        WorldStorageWindowSingleton.GetWorldLinks();
                        WorldStorageWindowSingleton.Repaint();
                    }
                    if (GUILayout.Button("Edit...", EditorStyles.miniButtonLeft, buttonWidth))
                    {
                        Debug.Log("Open Link Window");
                        
                        WorldLinkWindow.ShowWindow(worldStorageServer, worldStorageUser, UUID);
                    }
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

        public void GetTrackables()
        {
            // Get all objects
            Debug.Log("Get all server objects");
            List<Trackable> res = TrackableRequest.GetAllTrackables(worldStorageServer);
            trackables.Clear();
            foreach (var item in res)
            {
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