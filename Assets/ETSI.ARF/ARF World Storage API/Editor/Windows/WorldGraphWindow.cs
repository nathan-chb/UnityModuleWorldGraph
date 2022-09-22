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

using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph;
using ETSI.ARF.WorldStorage;
using ETSI.ARF.WorldStorage.REST;
using ETSI.ARF.WorldStorage.UI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows
{
    public class WorldGraphWindow : EditorWindow
    {

        public WorldStorageServer worldStorageServer;
        [HideInInspector] public WorldStorageUser worldStorageUser;

        private ARFGraphView myGraph;

        //to delay the reframe (otherwise it reframes when the graph isn't built yet)
        int twoFrames = 0;
        public static WorldGraphWindow Instance
        {
            get { return GetWindow<WorldGraphWindow>(); }
        }

        [MenuItem("ARFWorldStorage/Edit Graph...")]
        public static void ShowWindow()
        {
            var window = GetWindow<WorldGraphWindow>("Graph Editor", true, typeof(SceneView));
            Debug.Log(AdminRequest.Ping(window.worldStorageServer));
            window.Show();
        }

        public void OnEnable()
        {
            //rootVisualElement.Add(GenerateToolbar());
            if (worldStorageServer != null)
            {
                try { 
                    if (UtilGraphSingleton.instance.nodePositions == null)
                    {
                        UtilGraphSingleton.instance.InitNodePos(worldStorageServer, worldStorageUser);
                    }
                    ConstructGraphView();
                    myGraph.style.top = Length.Percent(11);
                    myGraph.style.bottom = Length.Percent(5);
                    rootVisualElement.Add(myGraph);
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("Error", "The server you selected is unreachable", "Ok");
                    myGraph = null;
                    Debug.Log(e.ToString());
                }
            }
        }

        //initiate the graphView Attribute 
        public void ConstructGraphView()
        {
            myGraph = new ARFGraphView
            {
                name = "ARF Graph",
                worldStorageServer = worldStorageServer,
                worldStorageUser = worldStorageUser
            };
            //top offset so that the graph does'nt overlap with the rest of the ui
            myGraph.style.top = Length.Percent(11);
            myGraph.PaintWorldStorage();
            myGraph.StretchToParentSize();
            UtilGraphSingleton.instance.toReFrame = true;
        }


        void OnGUI()
        {
            if (UtilGraphSingleton.instance.nodePositions == null)
            {
                UtilGraphSingleton.instance.InitNodePos(worldStorageServer, worldStorageUser);
            }


            EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();
            worldStorageServer = (WorldStorageServer)EditorGUILayout.ObjectField("World Storage Server", worldStorageServer, typeof(WorldStorageServer), false, GUILayout.Width(500));
            if (EditorGUI.EndChangeCheck())
            {
                GraphEditorWindow.ResetWindow();

                if((myGraph != null))
                {
                    if (myGraph.ServerAndLocalDifferent() && EditorUtility.DisplayDialog("Saving node positions", "The World Graph has been modified. \nWould you like to push the modifications to the server ?", "Yes", "No"))
                    {
                        myGraph.SaveInServer();
                    }
                    rootVisualElement.Remove(myGraph);
                }
                if(worldStorageServer != null)
                {
                    try
                    {
                        UtilGraphSingleton.instance.InitNodePos(worldStorageServer, worldStorageUser);
                        ConstructGraphView();
                        myGraph.style.top = Length.Percent(11);
                        myGraph.style.bottom = Length.Percent(5);
                        rootVisualElement.Add(myGraph);
                    }
                    catch (Exception e)
                    {
                        EditorUtility.DisplayDialog("Error", "The server you selected is unreachable", "Ok");
                        myGraph = null;
                        Debug.Log(e.ToString());
                    }
                }
                else 
                {
                    myGraph = null;
                }
            }


            //style for copyrights label (left aligned)
            var leftStyle = GUI.skin.GetStyle("Label");
            leftStyle.alignment = TextAnchor.MiddleLeft;

            GUILayout.Label("Augmented Reality Framework", leftStyle);
            GUILayout.Label("Copyright (C) 2022, ETSI (BSD 3-Clause License)", leftStyle);

            //reframe all elements to see them all
            if (UtilGraphSingleton.instance.toReFrame && (twoFrames == 2))
            {
                myGraph.FrameAllElements();
                UtilGraphSingleton.instance.toReFrame = false;
                twoFrames = 0;
            }
            else if (UtilGraphSingleton.instance.toReFrame)
            {
                twoFrames++;
            }
            EditorGUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            //Notify the user that the graph is different from the one in the server
            if (myGraph != null)
            {
                if (myGraph.ServerAndLocalDifferent())
                {
                    //the icon to add if the node does not correspond to an element in the server
                    Texture2D warningImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/warning.png", typeof(Texture2D));

                    GUI.backgroundColor = Color.clear;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(warningImage, GUILayout.Width(27), GUILayout.Height(27));
                    GUILayout.Box("There are elements in your graph that have been added, modified or deleted ! The current graph is not synchronized with the World Storage", leftStyle, GUILayout.ExpandWidth(true), GUILayout.Height(27));
                    GUILayout.EndHorizontal();
                }
            }
        }

        public void Update()
        {
            if (myGraph != null)
            {
                UtilGraphSingleton.SynchronizeWithGameObjects(myGraph);
            }
        }

        public static void RenameNode(String name, String guid)
        {
            var window = WorldGraphWindow.GetWindow<WorldGraphWindow>("Graph Editor", false, typeof(SceneView));
            var graph = window.myGraph;

            graph.GetNodeByGuid(guid).title = name;

            if (UtilGraphSingleton.instance.nodePositions.ContainsKey(guid) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(guid)))
            {
                UtilGraphSingleton.instance.elemsToUpdate.Add(guid);
            }
            ((ARFNode)graph.GetNodeByGuid(guid)).MarkUnsaved();
        }

        public ARFGraphView GetGraph()
        {
            return myGraph;
        }
    }
}