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
using Org.OpenAPITools.Model;
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

        [HideInInspector] public WorldStorageServer worldStorageServer;
        [HideInInspector] public WorldStorageUser worldStorageUser;

        private ARFGraphView myGraph;

        //to delay the reframe (otherwise it reframes when the graph isn't built yet)
        int twoFrames = 0;

        [MenuItem("ARFWorldStorage/Edit Graph...")]
        public static void ShowWindow()
        {
            GetWindow<WorldGraphWindow>("Graph Editor", true, typeof(SceneView));
        }

        public void OnEnable()
        {
            //rootVisualElement.Add(GenerateToolbar());
            if (worldStorageServer != null)
            {
                try { 
                    if (SaveInfo.instance.nodePositions == null)
                    {
                        SaveInfo.instance.InitNodePos(worldStorageServer, worldStorageUser);
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
            SaveInfo.instance.toReFrame = true;
        }


        void OnGUI()
        {
            if (SaveInfo.instance.nodePositions == null)
            {
                SaveInfo.instance.InitNodePos(worldStorageServer, worldStorageUser);
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
                        SaveInfo.instance.InitNodePos(worldStorageServer, worldStorageUser);
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
            if (SaveInfo.instance.toReFrame && (twoFrames == 2))
            {
                myGraph.FrameAllElements();
                SaveInfo.instance.toReFrame = false;
                twoFrames = 0;
            }
            else if (SaveInfo.instance.toReFrame)
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

        public void DeleteNode(ARFNode node)
        {
            rootVisualElement.Remove(myGraph);
            node.DisconnectAllPorts(myGraph);
            myGraph.DeleteElements(new List<GraphElement>{ node });
            rootVisualElement.Add(myGraph);
        }

        public void DeleteEdge(ARFEdgeLink edge)
        {
            rootVisualElement.Remove(myGraph);
            myGraph.DeleteElements(new List<GraphElement> { edge });
            rootVisualElement.Add(myGraph);
        }
    }

    public class SaveInfo : ScriptableSingleton<SaveInfo>
    {
        [SerializeField]
        public Dictionary<String, Rect> nodePositions;
        public List<String> linkIds;

        public Dictionary<String,Type> elemsToRemove;
        public List<String> elemsToUpdate;

        //keep the info of the graph reframe
        public Boolean toReFrame = false;

        public WorldStorageServer worldStorageServer;
        public WorldStorageUser worldStorageUser;

        public void InitNodePos(WorldStorageServer server, WorldStorageUser user)
        {
            worldStorageServer = server;
            worldStorageUser = user;

            instance.nodePositions = new Dictionary<string, Rect>();
            foreach (Trackable track in TrackableRequest.GetAllTrackables(worldStorageServer))
            {
                if (track.KeyvalueTags.ContainsKey("unityAuthoringPosX") && track.KeyvalueTags.ContainsKey("unityAuthoringPosY"))
                {
                    var posX = RoundToNearestHalf(float.Parse(track.KeyvalueTags["unityAuthoringPosX"][0]));
                    var posY = RoundToNearestHalf(float.Parse(track.KeyvalueTags["unityAuthoringPosY"][0]));
                    Rect trackPos = new(posX, posY, 135, 77);
                    instance.nodePositions[track.UUID.ToString()] = trackPos;
                }
                else
                {
                    Rect trackPos = new(0, 0, 135, 77);
                    instance.nodePositions[track.UUID.ToString()] = trackPos;
                }
            }
            foreach (WorldAnchor wa in WorldAnchorRequest.GetAllWorldAnchors(worldStorageServer))
            {
                if (wa.KeyvalueTags.ContainsKey("unityAuthoringPosX") && wa.KeyvalueTags.ContainsKey("unityAuthoringPosY"))
                {
                    var posX = RoundToNearestHalf(float.Parse(wa.KeyvalueTags["unityAuthoringPosX"][0]));
                    var posY = RoundToNearestHalf(float.Parse(wa.KeyvalueTags["unityAuthoringPosY"][0]));
                    Rect waPos = new(posX, posY, 135, 77);
                    instance.nodePositions[wa.UUID.ToString()] = waPos;
                }
                else
                {
                    Rect trackPos = new(0, 0, 135, 77);
                    instance.nodePositions[wa.UUID.ToString()] = trackPos;
                }
            }

            instance.linkIds = new List<string>();
            foreach (WorldLink link in WorldLinkRequest.GetAllWorldLinks(worldStorageServer))
            {
                instance.linkIds.Add(link.UUID.ToString());
            }

            instance.elemsToRemove = new Dictionary<string, Type>();
            instance.elemsToUpdate = new List<string>();
        }

        //method to predict the position of a node (the float that will be saved in the PositionInfo singleton)
        public static float RoundToNearestHalf(float a)
        {
            return a = Mathf.Round(a * 2f) * 0.5f;
        }

        public static void PrintInfo()
        {
            Debug.Log("elems to delete : " + string.Join(", ", instance.elemsToRemove.Keys));
            Debug.Log("elems to update : " + string.Join(", ", instance.elemsToUpdate));
            Debug.Log("elems tout court : " + string.Join(", ", instance.nodePositions.Keys));
        }
    }
}