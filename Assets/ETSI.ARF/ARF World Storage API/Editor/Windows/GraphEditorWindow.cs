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
using ETSI.ARF.WorldStorage.REST;
using ETSI.ARF.WorldStorage.UI;
using Org.OpenAPITools.Model;
using System;
using System.Collections.Generic;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows
{
    public class GraphEditorWindow : EditorWindow
    {
        public enum GraphEditorType
        {
            TRACKABLE,
            WORLDANCHOR,
            WORLDLINK,
            NULL
        }

        public GraphEditorType type;

        public ARFNodeTrackable trackableNode;
        public ARFNodeWorldAnchor worldAnchorNode;
        public ARFEdgeLink worldLinkEdge;

        public Trackable trackable;
        public WorldAnchor worldAnchor;
        public WorldLink worldLink;

        public Vector3 local_size;
        public Vector3 local_rot;
        public Vector3 local_pos;

        //test
        string m_newKey = "";
        List<string> m_newValues = new List<string>();

        // UI stuffs
        private Vector2 scrollPos;
        static public GraphEditorWindow winSingleton;

        public void OnEnable()
        {
            ResetWindow();
        }

        public static void ResetWindow()
        {
            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            winSingleton = GetWindow<GraphEditorWindow>("Element Editor", true, inspectorType);
            winSingleton.trackable = null;
            winSingleton.worldAnchor = null;
            winSingleton.worldLink = null;

            winSingleton.trackableNode = null;
            winSingleton.worldAnchorNode = null;
            winSingleton.worldLinkEdge = null;

            winSingleton.local_size = Vector3.zero;
            winSingleton.local_rot = Vector3.zero;
            winSingleton.local_pos = Vector3.zero;

            winSingleton.type = GraphEditorType.NULL;
        }

        public static void ShowWindow(ARFNodeTrackable trackableNode)
        {
            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            winSingleton = GetWindow<GraphEditorWindow>("Element Editor", true, inspectorType);
            winSingleton.type = GraphEditorType.TRACKABLE;

            winSingleton.trackable = null;
            winSingleton.worldAnchor = null;
            winSingleton.worldLink = null;

            winSingleton.trackableNode = null;
            winSingleton.worldAnchorNode = null;
            winSingleton.worldLinkEdge = null;

            winSingleton.trackableNode = trackableNode;
            winSingleton.trackable = trackableNode.trackable;

            if (winSingleton.trackable.LocalCRS.Count == 16)
            {
                Matrix4x4 localCRS = new Matrix4x4();
                localCRS.m00 = winSingleton.trackable.LocalCRS[0]; localCRS.m01 = winSingleton.trackable.LocalCRS[1]; localCRS.m02 = winSingleton.trackable.LocalCRS[2]; localCRS.m03 = winSingleton.trackable.LocalCRS[3];
                localCRS.m10 = winSingleton.trackable.LocalCRS[4]; localCRS.m11 = winSingleton.trackable.LocalCRS[5]; localCRS.m12 = winSingleton.trackable.LocalCRS[6]; localCRS.m13 = winSingleton.trackable.LocalCRS[7];
                localCRS.m20 = winSingleton.trackable.LocalCRS[8]; localCRS.m21 = winSingleton.trackable.LocalCRS[9]; localCRS.m22 = winSingleton.trackable.LocalCRS[10]; localCRS.m23 = winSingleton.trackable.LocalCRS[11];
                localCRS.m30 = winSingleton.trackable.LocalCRS[12]; localCRS.m31 = winSingleton.trackable.LocalCRS[13]; localCRS.m32 = winSingleton.trackable.LocalCRS[14]; localCRS.m33 = winSingleton.trackable.LocalCRS[15];
                winSingleton.local_pos = localCRS.GetPosition();
                winSingleton.local_rot = localCRS.rotation.eulerAngles;
            }
        }

        public static void ShowWindow(ARFNodeWorldAnchor worldAnchorNode)
        {
            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            winSingleton = GetWindow<GraphEditorWindow>("Element Editor", true, inspectorType);
            winSingleton.type = GraphEditorType.WORLDANCHOR;

            winSingleton.trackable = null;
            winSingleton.worldAnchor = null;
            winSingleton.worldLink = null;

            winSingleton.trackableNode = null;
            winSingleton.worldAnchorNode = null;
            winSingleton.worldLinkEdge = null;

            winSingleton.worldAnchorNode = worldAnchorNode;
            winSingleton.worldAnchor = worldAnchorNode.worldAnchor;

            if (winSingleton.worldAnchor.LocalCRS.Count == 16)
            {
                Matrix4x4 localCRS = new Matrix4x4();
                localCRS.m00 = winSingleton.worldAnchor.LocalCRS[0]; localCRS.m01 = winSingleton.worldAnchor.LocalCRS[1]; localCRS.m02 = winSingleton.worldAnchor.LocalCRS[2]; localCRS.m03 = winSingleton.worldAnchor.LocalCRS[3];
                localCRS.m10 = winSingleton.worldAnchor.LocalCRS[4]; localCRS.m11 = winSingleton.worldAnchor.LocalCRS[5]; localCRS.m12 = winSingleton.worldAnchor.LocalCRS[6]; localCRS.m13 = winSingleton.worldAnchor.LocalCRS[7];
                localCRS.m20 = winSingleton.worldAnchor.LocalCRS[8]; localCRS.m21 = winSingleton.worldAnchor.LocalCRS[9]; localCRS.m22 = winSingleton.worldAnchor.LocalCRS[10]; localCRS.m23 = winSingleton.worldAnchor.LocalCRS[11];
                localCRS.m30 = winSingleton.worldAnchor.LocalCRS[12]; localCRS.m31 = winSingleton.worldAnchor.LocalCRS[13]; localCRS.m32 = winSingleton.worldAnchor.LocalCRS[14]; localCRS.m33 = winSingleton.worldAnchor.LocalCRS[15];
                winSingleton.local_pos = localCRS.GetPosition();
                winSingleton.local_rot = localCRS.rotation.eulerAngles;
            }
        }

        public static void ShowWindow(ARFEdgeLink graphEdge)
        {
            Type inspectorType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor.dll");
            winSingleton = GetWindow<GraphEditorWindow>("Element Editor", true, inspectorType);
            winSingleton.type = GraphEditorType.WORLDLINK;

            winSingleton.trackable = null;
            winSingleton.worldAnchor = null;
            winSingleton.worldLink = null;

            winSingleton.trackableNode = null;
            winSingleton.worldAnchorNode = null;
            winSingleton.worldLinkEdge = null;

            winSingleton.worldLinkEdge = graphEdge;
            winSingleton.worldLink = graphEdge.worldLink;

            if (winSingleton.worldLink.Transform.Count == 16)
            {
                Matrix4x4 localCRS = new Matrix4x4();
                localCRS.m00 = winSingleton.worldLink.Transform[0]; localCRS.m01 = winSingleton.worldLink.Transform[1]; localCRS.m02 = winSingleton.worldLink.Transform[2]; localCRS.m03 = winSingleton.worldLink.Transform[3];
                localCRS.m10 = winSingleton.worldLink.Transform[4]; localCRS.m11 = winSingleton.worldLink.Transform[5]; localCRS.m12 = winSingleton.worldLink.Transform[6]; localCRS.m13 = winSingleton.worldLink.Transform[7];
                localCRS.m20 = winSingleton.worldLink.Transform[8]; localCRS.m21 = winSingleton.worldLink.Transform[9]; localCRS.m22 = winSingleton.worldLink.Transform[10]; localCRS.m23 = winSingleton.worldLink.Transform[11];
                localCRS.m30 = winSingleton.worldLink.Transform[12]; localCRS.m31 = winSingleton.worldLink.Transform[13]; localCRS.m32 = winSingleton.worldLink.Transform[14]; localCRS.m33 = winSingleton.worldLink.Transform[15];
                winSingleton.local_pos = localCRS.GetPosition();
                winSingleton.local_rot = localCRS.rotation.eulerAngles;
            }
        }

        public void Update()
        {
            if (winSingleton.trackableNode != null)
            {
                if (trackableNode.title != trackable.Name)
                {
                    trackableNode.title = trackable.Name;
                }
            }
            else if (winSingleton.worldAnchorNode != null)
            {
                if (worldAnchorNode.title != worldAnchor.Name)
                {
                    worldAnchorNode.title = worldAnchor.Name;
                }
            }
            else if (winSingleton.worldLinkEdge != null)
            {

            }
        }

        public void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));

            //style for copyrights label (left aligned)
            var leftStyle = GUI.skin.GetStyle("Label");
            leftStyle.alignment = TextAnchor.UpperLeft;

            GUILayout.Label("Augmented Reality Framework", leftStyle);
            GUILayout.Label("Copyright (C) 2022, ETSI (BSD 3-Clause License)", leftStyle);


            switch (type)
            {
                case GraphEditorType.WORLDLINK:
                    BuildWorldLinkUI();
                    break;
                case GraphEditorType.TRACKABLE:
                    BuildTrackableUI();
                    break;
                case GraphEditorType.WORLDANCHOR:
                    BuildWorldAnchorUI();
                    break;
                default:
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        //BUILD UI FOR MODIYING THE WORLDANCHOR
        private void BuildWorldAnchorUI()
        {
            if (worldAnchor != null)
            {

                winSingleton.local_size = new Vector3((float)winSingleton.worldAnchor.WorldAnchorSize[0], (float)winSingleton.worldAnchor.WorldAnchorSize[1], (float)winSingleton.worldAnchor.WorldAnchorSize[2]);
                if (winSingleton.worldAnchor.LocalCRS.Count == 16)
                {
                    Matrix4x4 localCRS = new Matrix4x4();
                    localCRS.m00 = winSingleton.worldAnchor.LocalCRS[0]; localCRS.m01 = winSingleton.worldAnchor.LocalCRS[1]; localCRS.m02 = winSingleton.worldAnchor.LocalCRS[2]; localCRS.m03 = winSingleton.worldAnchor.LocalCRS[3];
                    localCRS.m10 = winSingleton.worldAnchor.LocalCRS[4]; localCRS.m11 = winSingleton.worldAnchor.LocalCRS[5]; localCRS.m12 = winSingleton.worldAnchor.LocalCRS[6]; localCRS.m13 = winSingleton.worldAnchor.LocalCRS[7];
                    localCRS.m20 = winSingleton.worldAnchor.LocalCRS[8]; localCRS.m21 = winSingleton.worldAnchor.LocalCRS[9]; localCRS.m22 = winSingleton.worldAnchor.LocalCRS[10]; localCRS.m23 = winSingleton.worldAnchor.LocalCRS[11];
                    localCRS.m30 = winSingleton.worldAnchor.LocalCRS[12]; localCRS.m31 = winSingleton.worldAnchor.LocalCRS[13]; localCRS.m32 = winSingleton.worldAnchor.LocalCRS[14]; localCRS.m33 = winSingleton.worldAnchor.LocalCRS[15];
                    if ((winSingleton.local_pos != localCRS.GetPosition()) || (winSingleton.local_rot != localCRS.rotation.eulerAngles))
                    {
                        winSingleton.local_pos = localCRS.GetPosition();
                        winSingleton.local_rot = localCRS.rotation.eulerAngles;


                        if (UtilGraphSingleton.instance.nodePositions.ContainsKey(worldAnchor.UUID.ToString()) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(worldAnchor.UUID.ToString())))
                        {
                            UtilGraphSingleton.instance.elemsToUpdate.Add(worldAnchor.UUID.ToString());
                        }
                        worldAnchorNode.MarkUnsaved();
                    }
                }

                //
                //HEADER
                //

                //anchor icon
                EditorGUILayout.BeginHorizontal();
                Texture anchorImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/anchor.png", typeof(Texture));
                GUI.backgroundColor = Color.clear;
                GUILayout.Box(anchorImage, GUILayout.Width(40), GUILayout.Height(40));

                //anchor label
                EditorGUILayout.BeginVertical(GUILayout.Height(50)); 
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("WORLD ANCHOR", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //separator line
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(10));
                DrawUILine(new Color(1, 0.7f, 0, 0.9f), 5, 5);
                EditorGUILayout.EndHorizontal();

                if (worldAnchorNode.titleContainer.Contains(worldAnchorNode.savedIcon))
                {
                    //the icon to add if the node does not correspond to an element in the server
                    Texture2D warningImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/warning.png", typeof(Texture2D));

                    GUI.backgroundColor = Color.clear;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(warningImage, GUILayout.Width(27), GUILayout.Height(27));
                    EditorGUILayout.LabelField("This element is not synchronized with the World Storage");
                    GUILayout.EndHorizontal();
                }

                //
                //ELEMENT PARAMETERS
                //

                EditorGUI.BeginChangeCheck();

                //uuid
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UUID ", EditorStyles.boldLabel, GUILayout.Width(50));
                if (!UtilGraphSingleton.instance.nodePositions.ContainsKey(worldAnchor.UUID.ToString()))
                {
                    EditorGUILayout.LabelField("none yet (element not yet saved in the server)");
                }
                else
                {
                    EditorGUILayout.SelectableLabel(worldAnchor.UUID.ToString(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
                EditorGUILayout.EndHorizontal();

                //name
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Name ", EditorStyles.boldLabel, GUILayout.Width(50));
                var oldName = worldAnchor.Name.ToString();
                worldAnchor.Name = EditorGUILayout.DelayedTextField(worldAnchor.Name);
                if (EditorGUI.EndChangeCheck())
                {
                    //change the name of the GO
                    GameObject waGO = GameObject.Find(oldName);
                    if (waGO != null)
                    {
                        waGO.name = worldAnchor.Name;
                    }
                    worldAnchorNode.title = worldAnchor.Name;
                }
                EditorGUILayout.EndHorizontal();

                //unit system
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Unit ", EditorStyles.boldLabel, GUILayout.Width(50));
                worldAnchor.Unit = (UnitSystem)EditorGUILayout.EnumPopup(worldAnchor.Unit);
                EditorGUILayout.EndHorizontal();

                //style for sublabels (right aligned)
                var rightStyle = GUI.skin.GetStyle("Label");
                rightStyle.alignment = TextAnchor.UpperRight;

                //size
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Size ", EditorStyles.boldLabel, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width", rightStyle, GUILayout.Width(50));
                local_size[0] = EditorGUILayout.DelayedFloatField(local_size[0]);
                EditorGUILayout.LabelField("Length", rightStyle, GUILayout.Width(50));
                local_size[1] = EditorGUILayout.DelayedFloatField(local_size[1]);
                EditorGUILayout.LabelField("Depth", rightStyle, GUILayout.Width(50));
                local_size[2] = EditorGUILayout.DelayedFloatField(local_size[2]);
                EditorGUILayout.EndHorizontal();

                //localCRS
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Local CRS ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //position
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Position ", GUILayout.Width(60));
                EditorGUILayout.LabelField("X", rightStyle, GUILayout.Width(15));
                local_pos[0] = EditorGUILayout.DelayedFloatField(local_pos[0]);
                EditorGUILayout.LabelField("Y", rightStyle, GUILayout.Width(15));
                local_pos[1] = EditorGUILayout.DelayedFloatField(local_pos[1]);
                EditorGUILayout.LabelField("Z", rightStyle, GUILayout.Width(15));
                local_pos[2] = EditorGUILayout.DelayedFloatField(local_pos[2]);
                EditorGUILayout.EndHorizontal();
                //rotation
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rotation ", GUILayout.Width(60));
                EditorGUILayout.LabelField("X", rightStyle, GUILayout.Width(15));
                local_rot[0] = EditorGUILayout.DelayedFloatField(local_rot[0]);
                EditorGUILayout.LabelField("Y", rightStyle, GUILayout.Width(15));
                local_rot[1] = EditorGUILayout.DelayedFloatField(local_rot[1]);
                EditorGUILayout.LabelField("Z", rightStyle, GUILayout.Width(15));
                local_rot[2] = EditorGUILayout.DelayedFloatField(local_rot[2]);
                EditorGUILayout.EndHorizontal();

                //keyvaluetags=================================================================================================TOBEMODIFIED
                /*DrawUILine(Color.gray, 1, 1);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Tags ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                Dictionary<string, List<string>> tempPairs = worldAnchor.KeyvalueTags;
                EditorGUILayout.BeginHorizontal();
                m_newKey = GUILayout.TextField(m_newKey, GUILayout.Width(300));
                if (GUILayout.Button("Add Key"))
                {
                    if (m_newKey != "")
                    {
                        List<string> emptyList = new List<string>();
                        worldAnchor.KeyvalueTags.Add(m_newKey, emptyList);
                        m_newKey = "";
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                //iterator on m_newValues
                int j = 0;
                foreach (KeyValuePair<string, List<string>> entry in tempPairs)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(entry.Key);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        worldAnchor.KeyvalueTags.Remove(entry.Key);
                        m_newValues[j] = "";
                    }
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    List<string> tempValues = entry.Value;
                    foreach (string value in tempValues)
                    {
                        GUILayout.Label(value);

                        if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            tempValues.Remove(value);
                            worldAnchor.KeyvalueTags[entry.Key] = tempValues;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    if (m_newValues.Count < j + 1)
                    {
                        string value = "";
                        m_newValues.Add(value);
                    }
                    m_newValues[j] = GUILayout.TextField(m_newValues[j], GUILayout.Width(200));
                    if (GUILayout.Button("Add Value"))
                    {
                        if (m_newValues[j] != "")
                        {
                            List<string> valueList = entry.Value;
                            valueList.Add(m_newValues[j]);
                            worldAnchor.KeyvalueTags[entry.Key] = valueList;
                            m_newValues[j] = "";
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    j++;
                }
                EditorGUILayout.EndVertical();*/
                //keyvaluetags=================================================================================================TOBEMODIFIED


                //Actions when the ui fields have been changed
                if (EditorGUI.EndChangeCheck())
                {
                    //
                    Matrix4x4 localCRS = Matrix4x4.TRS(local_pos, Quaternion.Euler(local_rot), Vector3.one);
                    List<float> localCRSasFloat = new List<float>
                    {
                        localCRS.m00,    localCRS.m01,    localCRS.m02,    localCRS.m03,
                        localCRS.m10,    localCRS.m11,    localCRS.m12,    localCRS.m13,
                        localCRS.m20,    localCRS.m21,    localCRS.m22,    localCRS.m23,
                        localCRS.m30,    localCRS.m31,    localCRS.m32,    localCRS.m33,
                    };
                    worldAnchor.LocalCRS = localCRSasFloat;

                    List<double> localSizeAsFloat = new List<double>
                    {
                        local_size.x,    local_size.y,    local_size.z
                    };
                    worldAnchor.WorldAnchorSize = localSizeAsFloat;

                    if (UtilGraphSingleton.instance.nodePositions.ContainsKey(worldAnchor.UUID.ToString()) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(worldAnchor.UUID.ToString())))
                    {
                        UtilGraphSingleton.instance.elemsToUpdate.Add(worldAnchor.UUID.ToString());
                    }
                    worldAnchorNode.MarkUnsaved();
                }

                //
                //FOOTER
                //
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var originalColor = GUI.backgroundColor;


                //reload button
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Reload"))
                {
                    //lose focus of fields otherwise the selected field won't updaate
                    EditorGUI.FocusTextInControl(null);
                    if (UtilGraphSingleton.instance.nodePositions.ContainsKey(worldAnchor.UUID.ToString()))
                    {
                        if (UtilGraphSingleton.instance.elemsToUpdate.Contains(worldAnchor.UUID.ToString()) && EditorUtility.DisplayDialog("Reset elements", "Are you sure you want to lose all your changes ?", "Yes", "No"))
                        {
                            var gameObject = GameObject.Find(worldAnchor.Name);

                            worldAnchor = WorldAnchorRequest.GetWorldAnchor(UtilGraphSingleton.instance.worldStorageServer, worldAnchor.UUID.ToString());
                            worldAnchorNode.worldAnchor = worldAnchor;
                            ShowWindow(worldAnchorNode);

                            //Update the gameobject component and the name
                            if (gameObject != null)
                            {
                                gameObject.name = worldAnchor.Name;
                                var worldAnchorScript = (WorldAnchorScript)gameObject.GetComponent<WorldAnchorScript>();
                                worldAnchorScript.worldAnchor = worldAnchor;
                            }

                            UtilGraphSingleton.instance.elemsToUpdate.Remove(worldAnchor.UUID.ToString());
                            worldAnchorNode.MarkSaved();
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Reset elements", "Are you sure you want to lose all your changes ?", "Yes", "No"))
                        {
                            var gameObject = GameObject.Find(worldAnchorNode.worldAnchor.Name);

                            //generate the worldAnchor attributes
                            List<float> localCRS = new();
                            localCRS.Add(1);
                            for (int i = 1; i < 5; i++)
                            {
                                localCRS.Add(0);
                            }
                            localCRS.Add(1);
                            for (int i = 6; i < 10; i++)
                            {
                                localCRS.Add(0);
                            }
                            localCRS.Add(1);
                            for (int i = 11; i < 15; i++)
                            {
                                localCRS.Add(0);
                            }
                            localCRS.Add(1);

                            List<double> worldAnchorSize = new List<double>();
                            for (int i = 0; i < 3; i++)
                            {
                                worldAnchorSize.Add(0);
                            }
                            worldAnchor = new WorldAnchor(Guid.NewGuid(), "DefaultWorldAnchor", Guid.Parse(UtilGraphSingleton.instance.worldStorageUser.UUID), localCRS, UnitSystem.CM, worldAnchorSize, new Dictionary<string, List<string>>());
                            worldAnchorNode.worldAnchor = worldAnchor;

                            //Update the gameobject component and the name
                            if (gameObject != null)
                            {
                                gameObject.name = worldAnchor.Name;
                                var worldAnchorScript = (WorldAnchorScript)gameObject.GetComponent<WorldAnchorScript>();
                                worldAnchorScript.worldAnchor = worldAnchor;
                            }

                            ShowWindow(worldAnchorNode);
                        }
                    }
                }

                //save button
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Save"))
                {
                    if (UtilGraphSingleton.instance.nodePositions.ContainsKey(worldAnchor.UUID.ToString()))
                    {
                        if (UtilGraphSingleton.instance.elemsToUpdate.Contains(worldAnchor.UUID.ToString()))
                        {
                            WorldAnchorRequest.UpdateWorldAnchor(UtilGraphSingleton.instance.worldStorageServer, worldAnchor);
                            UtilGraphSingleton.instance.elemsToUpdate.Remove(worldAnchor.UUID.ToString());
                        }
                    }
                    else
                    {
                        var posX = new List<String>
                        {
                            worldAnchorNode.GetPosition().x.ToString()
                        };
                        var posY = new List<String>
                        {
                            worldAnchorNode.GetPosition().y.ToString()
                        };
                        WorldAnchor worldAnchor = worldAnchorNode.worldAnchor;
                        worldAnchor.KeyvalueTags["unityAuthoringPosX"] = posX;
                        worldAnchor.KeyvalueTags["unityAuthoringPosY"] = posY;

                        String uuid = WorldAnchorRequest.AddWorldAnchor(UtilGraphSingleton.instance.worldStorageServer, worldAnchor);

                        //change the uuid in its edges, if there is a new edge to be added in the world storage it needs to have the correct uuid
                        uuid = uuid.Replace("\"", "");
                        foreach (ARFEdgeLink edge in worldAnchorNode.portIn.connections)
                        {
                        edge.worldLink.UUIDTo = Guid.Parse(uuid);
                        }
                        foreach (ARFEdgeLink edge in worldAnchorNode.portOut.connections)
                        {
                            edge.worldLink.UUIDFrom = Guid.Parse(uuid);
                        }
                        worldAnchorNode.worldAnchor.UUID = Guid.Parse(uuid);
                        worldAnchorNode.GUID = uuid;
                        worldAnchorNode.viewDataKey = worldAnchorNode.GUID;
                        worldAnchorNode.title = worldAnchor.Name;

                        //Add the newly saved World Anchor to the SaveInfo singleton
                        Rect trackPos = new(worldAnchorNode.GetPosition().x, worldAnchorNode.GetPosition().y, 135, 77);
                        UtilGraphSingleton.instance.nodePositions[uuid] = trackPos;
                    }
                    worldAnchorNode.MarkSaved();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                GUI.backgroundColor = originalColor;


            }
        }

        private void BuildTrackableUI()
        {
            if (trackable != null)
            {

                winSingleton.local_size = new Vector3((float)winSingleton.trackable.TrackableSize[0], (float)winSingleton.trackable.TrackableSize[1], (float)winSingleton.trackable.TrackableSize[2]);
                if (winSingleton.trackable.LocalCRS.Count == 16)
                {
                    Matrix4x4 localCRS = new Matrix4x4();
                    localCRS.m00 = winSingleton.trackable.LocalCRS[0]; localCRS.m01 = winSingleton.trackable.LocalCRS[1]; localCRS.m02 = winSingleton.trackable.LocalCRS[2]; localCRS.m03 = winSingleton.trackable.LocalCRS[3];
                    localCRS.m10 = winSingleton.trackable.LocalCRS[4]; localCRS.m11 = winSingleton.trackable.LocalCRS[5]; localCRS.m12 = winSingleton.trackable.LocalCRS[6]; localCRS.m13 = winSingleton.trackable.LocalCRS[7];
                    localCRS.m20 = winSingleton.trackable.LocalCRS[8]; localCRS.m21 = winSingleton.trackable.LocalCRS[9]; localCRS.m22 = winSingleton.trackable.LocalCRS[10]; localCRS.m23 = winSingleton.trackable.LocalCRS[11];
                    localCRS.m30 = winSingleton.trackable.LocalCRS[12]; localCRS.m31 = winSingleton.trackable.LocalCRS[13]; localCRS.m32 = winSingleton.trackable.LocalCRS[14]; localCRS.m33 = winSingleton.trackable.LocalCRS[15];
                    if((winSingleton.local_pos != localCRS.GetPosition()) || (winSingleton.local_rot != localCRS.rotation.eulerAngles))
                    {
                        winSingleton.local_pos = localCRS.GetPosition();
                        winSingleton.local_rot = localCRS.rotation.eulerAngles;
                        if (UtilGraphSingleton.instance.nodePositions.ContainsKey(trackable.UUID.ToString()) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(trackable.UUID.ToString())))
                        {
                            UtilGraphSingleton.instance.elemsToUpdate.Add(trackable.UUID.ToString());
                        }
                        trackableNode.MarkUnsaved();
                    }
                }

                //
                //HEADER
                //

                //trackable icon
                EditorGUILayout.BeginHorizontal();
                Texture trackImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/trackable.png", typeof(Texture));
                GUI.backgroundColor = Color.clear;
                GUILayout.Box(trackImage, GUILayout.Width(40), GUILayout.Height(40));

                //trackable label
                EditorGUILayout.BeginVertical(GUILayout.Height(50));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("TRACKABLE", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //separator line
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(10));
                DrawUILine(new Color(1, 0.31f, 0.31f, 0.9f), 5, 0);
                EditorGUILayout.EndHorizontal();

                if (trackableNode.titleContainer.Contains(trackableNode.savedIcon))
                {
                    //the icon to add if the node does not correspond to an element in the server
                    Texture2D warningImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/warning.png", typeof(Texture2D));

                    GUI.backgroundColor = Color.clear;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(warningImage, GUILayout.Width(27), GUILayout.Height(27));
                    EditorGUILayout.LabelField("This element is not synchronized with the World Storage");
                    GUILayout.EndHorizontal();
                }

                //
                //ELEMENT PARAMETERS
                //

                EditorGUI.BeginChangeCheck();

                //uuid
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UUID ", EditorStyles.boldLabel, GUILayout.Width(50));
                if (!UtilGraphSingleton.instance.nodePositions.ContainsKey(trackable.UUID.ToString()))
                {
                    EditorGUILayout.LabelField("none yet (element not yet saved in the server)");
                }
                else
                {
                    EditorGUILayout.SelectableLabel(trackable.UUID.ToString(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
                EditorGUILayout.EndHorizontal();

                //name
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Name ", EditorStyles.boldLabel, GUILayout.Width(50));
                var oldName = trackable.Name.ToString();
                trackable.Name = EditorGUILayout.DelayedTextField(trackable.Name);
                if (EditorGUI.EndChangeCheck())
                {
                    //change the name of the GO
                    GameObject trackGO = GameObject.Find(oldName);
                    if (trackGO != null)
                    {
                        trackGO.name = trackable.Name;
                    }

                    trackableNode.title = trackable.Name;
                }
                EditorGUILayout.EndHorizontal();

                //trackable's type
                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Type ", EditorStyles.boldLabel, GUILayout.Width(50));
                trackable.TrackableType = (Trackable.TrackableTypeEnum)EditorGUILayout.EnumPopup(trackable.TrackableType);
                //If the type of the trackable changed, change the scene
                if (EditorGUI.EndChangeCheck())
                {
                    SceneBuilder.ChangeTrackableType(trackable);
                }
                EditorGUILayout.EndHorizontal();

                //unit system
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Unit ", EditorStyles.boldLabel, GUILayout.Width(50));
                trackable.Unit = (UnitSystem)EditorGUILayout.EnumPopup(trackable.Unit);
                EditorGUILayout.EndHorizontal();

                //style for sublabels (right aligned)
                var rightStyle = GUI.skin.GetStyle("Label");
                rightStyle.alignment = TextAnchor.UpperRight;

                //size
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Size ", EditorStyles.boldLabel, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Width", rightStyle, GUILayout.Width(50));
                local_size[0] = EditorGUILayout.DelayedFloatField(local_size[0]);
                EditorGUILayout.LabelField("Length", rightStyle, GUILayout.Width(50));
                local_size[1] = EditorGUILayout.DelayedFloatField(local_size[1]);
                EditorGUILayout.LabelField("Depth", rightStyle, GUILayout.Width(50));
                local_size[2] = EditorGUILayout.DelayedFloatField(local_size[2]);
                EditorGUILayout.EndHorizontal();

                //localCRS
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Local CRS ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //position
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Position ", GUILayout.Width(60));
                EditorGUILayout.LabelField("X", rightStyle, GUILayout.Width(15));
                local_pos[0] = EditorGUILayout.DelayedFloatField(local_pos[0]);
                EditorGUILayout.LabelField("Y", rightStyle, GUILayout.Width(15));
                local_pos[1] = EditorGUILayout.DelayedFloatField(local_pos[1]);
                EditorGUILayout.LabelField("Z", rightStyle, GUILayout.Width(15));
                local_pos[2] = EditorGUILayout.DelayedFloatField(local_pos[2]);
                EditorGUILayout.EndHorizontal();
                //rotation
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rotation ", GUILayout.Width(60));
                EditorGUILayout.LabelField("X", rightStyle, GUILayout.Width(15));
                local_rot[0] = EditorGUILayout.DelayedFloatField(local_rot[0]);
                EditorGUILayout.LabelField("Y", rightStyle, GUILayout.Width(15));
                local_rot[1] = EditorGUILayout.DelayedFloatField(local_rot[1]);
                EditorGUILayout.LabelField("Z", rightStyle, GUILayout.Width(15));
                local_rot[2] = EditorGUILayout.DelayedFloatField(local_rot[2]);
                EditorGUILayout.EndHorizontal();

                //encodingInofrmation
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Trackable Information ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Format ", GUILayout.Width(50));
                trackable.TrackableEncodingInformation.DataFormat = (EncodingInformationStructure.DataFormatEnum)EditorGUILayout.EnumPopup(trackable.TrackableEncodingInformation.DataFormat);
                EditorGUILayout.LabelField("Version ", GUILayout.Width(50));
                float floatVersion;
                if (trackable.TrackableEncodingInformation._Version != null)
                {
                    floatVersion = EditorGUILayout.DelayedFloatField(float.Parse(trackable.TrackableEncodingInformation._Version.Replace(".",",")));
                }
                else
                {
                    floatVersion = EditorGUILayout.DelayedFloatField(0);
                }
                trackable.TrackableEncodingInformation._Version = floatVersion.ToString();
                EditorGUILayout.EndHorizontal();

                /*//trackable payload
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Trackable Payload ", EditorStyles.boldLabel, GUILayout.Width(140));
                EditorGUILayout.LabelField("===============================================================================");
                EditorGUILayout.EndHorizontal();*/

                //keyvaluetags=================================================================================================TOBEMODIFIED
                /*EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Tags ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                Dictionary<string, List<string>> tempPairs = trackable.KeyvalueTags;
                EditorGUILayout.BeginHorizontal();
                m_newKey = GUILayout.TextField(m_newKey, GUILayout.Width(300));
                if (GUILayout.Button("Add Key"))
                {
                    if (m_newKey != "")
                    {
                        List<string> emptyList = new List<string>();
                        trackable.KeyvalueTags.Add(m_newKey, emptyList);
                        m_newKey = "";
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                //iterator on m_newValues
                int j = 0;
                foreach (KeyValuePair<string, List<string>> entry in tempPairs)
                {
                    DrawUILine(Color.gray, 1, 1);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(entry.Key);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        trackable.KeyvalueTags.Remove(entry.Key);
                        m_newValues[j] = "";
                    }
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    List<string> tempValues = entry.Value;
                    foreach (string value in tempValues)
                    {
                        GUILayout.Label(value);

                        if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            tempValues.Remove(value);
                            trackable.KeyvalueTags[entry.Key] = tempValues;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    if (m_newValues.Count < j + 1)
                    {
                        string value = "";
                        m_newValues.Add(value);
                    }
                    m_newValues[j] = GUILayout.TextField(m_newValues[j], GUILayout.Width(200));
                    if (GUILayout.Button("Add Value"))
                    {
                        if (m_newValues[j] != "")
                        {
                            List<string> valueList = entry.Value;
                            valueList.Add(m_newValues[j]);
                            trackable.KeyvalueTags[entry.Key] = valueList;
                            m_newValues[j] = "";
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    j++;
                }
                EditorGUILayout.EndVertical();*//*EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Tags ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginVertical();
                Dictionary<string, List<string>> tempPairs = trackable.KeyvalueTags;
                EditorGUILayout.BeginHorizontal();
                m_newKey = GUILayout.TextField(m_newKey, GUILayout.Width(300));
                if (GUILayout.Button("Add Key"))
                {
                    if (m_newKey != "")
                    {
                        List<string> emptyList = new List<string>();
                        trackable.KeyvalueTags.Add(m_newKey, emptyList);
                        m_newKey = "";
                    }
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                //iterator on m_newValues
                int j = 0;
                foreach (KeyValuePair<string, List<string>> entry in tempPairs)
                {
                    DrawUILine(Color.gray, 1, 1);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(entry.Key);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                    {
                        trackable.KeyvalueTags.Remove(entry.Key);
                        m_newValues[j] = "";
                    }
                    EditorGUILayout.EndHorizontal();


                    EditorGUILayout.BeginHorizontal();
                    List<string> tempValues = entry.Value;
                    foreach (string value in tempValues)
                    {
                        GUILayout.Label(value);

                        if (GUILayout.Button("x", GUILayout.Width(18), GUILayout.Height(18)))
                        {
                            tempValues.Remove(value);
                            trackable.KeyvalueTags[entry.Key] = tempValues;
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    if (m_newValues.Count < j + 1)
                    {
                        string value = "";
                        m_newValues.Add(value);
                    }
                    m_newValues[j] = GUILayout.TextField(m_newValues[j], GUILayout.Width(200));
                    if (GUILayout.Button("Add Value"))
                    {
                        if (m_newValues[j] != "")
                        {
                            List<string> valueList = entry.Value;
                            valueList.Add(m_newValues[j]);
                            trackable.KeyvalueTags[entry.Key] = valueList;
                            m_newValues[j] = "";
                        }
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    j++;
                }
                EditorGUILayout.EndVertical();*/
                //keyvaluetags=================================================================================================TOBEMODIFIED


                //Actions when the ui fields have been changed
                if (EditorGUI.EndChangeCheck())
                {
                    //
                    Matrix4x4 localCRS = Matrix4x4.TRS(local_pos, Quaternion.Euler(local_rot), Vector3.one);
                    List<float> localCRSasFloat = new List<float>
                    {
                        localCRS.m00,    localCRS.m01,    localCRS.m02,    localCRS.m03,
                        localCRS.m10,    localCRS.m11,    localCRS.m12,    localCRS.m13,
                        localCRS.m20,    localCRS.m21,    localCRS.m22,    localCRS.m23,
                        localCRS.m30,    localCRS.m31,    localCRS.m32,    localCRS.m33,
                    };
                    trackable.LocalCRS = localCRSasFloat;

                    List<double> localSizeAsFloat = new List<double>
                    {
                        local_size.x,    local_size.y,    local_size.z
                    };
                    trackable.TrackableSize = localSizeAsFloat;

                    if (UtilGraphSingleton.instance.nodePositions.ContainsKey(trackable.UUID.ToString()) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(trackable.UUID.ToString())))
                    {
                        UtilGraphSingleton.instance.elemsToUpdate.Add(trackable.UUID.ToString());
                    }
                    trackableNode.MarkUnsaved();
                }

                //
                //FOOTER
                //
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var originalColor = GUI.backgroundColor;

                //reload button
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Reload"))
                {
                    //lose focus of fields otherwise the selected field won't updaate
                    EditorGUI.FocusTextInControl(null);
                    if (UtilGraphSingleton.instance.nodePositions.ContainsKey(trackable.UUID.ToString()))
                    {
                        if (UtilGraphSingleton.instance.elemsToUpdate.Contains(trackable.UUID.ToString()) && EditorUtility.DisplayDialog("Reset elements", "Are you sure you want to lose all your changes ?", "Yes", "No"))
                        {
                            var gameObject = GameObject.Find(trackable.Name);

                            trackable = TrackableRequest.GetTrackable(UtilGraphSingleton.instance.worldStorageServer, trackable.UUID.ToString());
                            trackableNode.trackable = trackable;

                            UtilGraphSingleton.instance.elemsToUpdate.Remove(trackable.UUID.ToString());
                            trackableNode.MarkSaved();

                            ShowWindow(trackableNode);

                            //update the gameobject component and name
                            if (gameObject != null)
                            {
                                gameObject.name = trackable.Name;
                                var trackableScript = (TrackableScript)gameObject.GetComponent<TrackableScript>();
                                trackableScript.trackable = trackable;
                            }
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Reset elements", "Are you sure you want to lose all your changes ?", "Yes", "No"))
                        {
                            var gameObject = GameObject.Find(trackableNode.trackable.Name);

                            //generate the Trackables's attributes
                            EncodingInformationStructure trackableEncodingInformation = new EncodingInformationStructure(EncodingInformationStructure.DataFormatEnum.OTHER, "0");

                            List<float> localCRS = new();
                            localCRS.Add(1);
                            for (int i = 1; i < 5; i++)
                            {
                                localCRS.Add(0);
                            }
                            localCRS.Add(1);
                            for (int i = 6; i < 10; i++)
                            {
                                localCRS.Add(0);
                            }
                            localCRS.Add(1);
                            for (int i = 11; i < 15; i++)
                            {
                                localCRS.Add(0);
                            }
                            localCRS.Add(1);

                            List<double> trackableSize = new();
                            for (int i = 0; i < 3; i++)
                            {
                                trackableSize.Add(0);
                            }

                            Trackable trackable = new Trackable(Guid.NewGuid(), "DefaultTrackable", Guid.Parse(UtilGraphSingleton.instance.worldStorageUser.UUID), Trackable.TrackableTypeEnum.OTHER, trackableEncodingInformation, new byte[64], localCRS, UnitSystem.CM, trackableSize, new Dictionary<string, List<string>>());
                            trackableNode.trackable = trackable;

                            //update the gameobject component and name
                            if (gameObject != null)
                            {
                                gameObject.name = trackable.Name;
                                var trackableScript = (TrackableScript)gameObject.GetComponent<TrackableScript>();
                                trackableScript.trackable = trackable;
                            }

                            ShowWindow(trackableNode);
                        }
                    }

                    //reset the Element's position in the game scene

                }

                //save button
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Save"))
                {
                    if (UtilGraphSingleton.instance.nodePositions.ContainsKey(trackable.UUID.ToString()))
                    {
                        if (UtilGraphSingleton.instance.elemsToUpdate.Contains(trackable.UUID.ToString()))
                        {
                            TrackableRequest.UpdateTrackable(UtilGraphSingleton.instance.worldStorageServer, trackable);
                            UtilGraphSingleton.instance.elemsToUpdate.Remove(trackable.UUID.ToString());
                        }
                    }
                    else
                    {
                        var posX = new List<String>
                        {
                            trackableNode.GetPosition().x.ToString()
                        };
                        var posY = new List<String>
                        {
                            trackableNode.GetPosition().y.ToString()
                        };
                        Trackable trackable = trackableNode.trackable;
                        trackable.KeyvalueTags["unityAuthoringPosX"] = posX;
                        trackable.KeyvalueTags["unityAuthoringPosY"] = posY;
                        String uuid = TrackableRequest.AddTrackable(UtilGraphSingleton.instance.worldStorageServer, trackable);

                        //change the uuid in its edges, if there is a new edge to be added in the world storage it needs to have the correct uuid
                        uuid = uuid.Replace("\"", "");
                        foreach (ARFEdgeLink edge in trackableNode.portIn.connections)
                        {
                            edge.worldLink.UUIDTo = Guid.Parse(uuid);
                        }
                        foreach (ARFEdgeLink edge in trackableNode.portOut.connections)
                        {
                            edge.worldLink.UUIDFrom = Guid.Parse(uuid);
                        }
                        trackableNode.trackable.UUID = Guid.Parse(uuid);
                        trackableNode.GUID = uuid;
                        trackableNode.viewDataKey = trackableNode.GUID;
                        trackableNode.title = trackable.Name;

                        //Add the newly saved Trackable to the SaveInfo singleton
                        Rect trackPos = new(trackableNode.GetPosition().x, trackableNode.GetPosition().y, 135, 77);
                        UtilGraphSingleton.instance.nodePositions[uuid] = trackPos;
                    }
                    trackableNode.MarkSaved();
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

                GUI.backgroundColor = originalColor;


            }
        }

        private void BuildWorldLinkUI()
        {
            if (worldLink != null)
            {
                //get the changes from the scene
                if (winSingleton.worldLink.Transform.Count == 16)
                {
                    Matrix4x4 localCRS = new Matrix4x4();
                    localCRS.m00 = winSingleton.worldLink.Transform[0]; localCRS.m01 = winSingleton.worldLink.Transform[1]; localCRS.m02 = winSingleton.worldLink.Transform[2]; localCRS.m03 = winSingleton.worldLink.Transform[3];
                    localCRS.m10 = winSingleton.worldLink.Transform[4]; localCRS.m11 = winSingleton.worldLink.Transform[5]; localCRS.m12 = winSingleton.worldLink.Transform[6]; localCRS.m13 = winSingleton.worldLink.Transform[7];
                    localCRS.m20 = winSingleton.worldLink.Transform[8]; localCRS.m21 = winSingleton.worldLink.Transform[9]; localCRS.m22 = winSingleton.worldLink.Transform[10]; localCRS.m23 = winSingleton.worldLink.Transform[11];
                    localCRS.m30 = winSingleton.worldLink.Transform[12]; localCRS.m31 = winSingleton.worldLink.Transform[13]; localCRS.m32 = winSingleton.worldLink.Transform[14]; localCRS.m33 = winSingleton.worldLink.Transform[15];
                    if ((winSingleton.local_pos != localCRS.GetPosition()) || (winSingleton.local_rot != localCRS.rotation.eulerAngles))
                    {
                        winSingleton.local_pos = localCRS.GetPosition();
                        winSingleton.local_rot = localCRS.rotation.eulerAngles;
                        if (UtilGraphSingleton.instance.linkIds.Contains(worldLink.UUID.ToString()) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(worldLink.UUID.ToString())))
                        {
                            UtilGraphSingleton.instance.elemsToUpdate.Add(worldLink.UUID.ToString());
                        }
                        worldLinkEdge.MarkUnsaved();
                    }
                }


                //
                //HEADER
                //

                //world link icon
                EditorGUILayout.BeginHorizontal();
                Texture linkImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/link.png", typeof(Texture));
                GUI.backgroundColor = Color.clear;
                GUILayout.Box(linkImage, GUILayout.Width(40), GUILayout.Height(40));

                //world link label
                EditorGUILayout.BeginVertical(GUILayout.Height(50));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("WORLD LINK", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                //separator line
                var rect = EditorGUILayout.BeginHorizontal(GUILayout.Height(10));
                DrawUILine(new Color(0.66f, 0.39f, 1, 0.77f), 5, 5);
                EditorGUILayout.EndHorizontal();

                if (worldLinkEdge.contentContainer.Contains(worldLinkEdge.savedIcon))
                {
                    //the icon to add if the node does not correspond to an element in the server
                    Texture2D warningImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/warning.png", typeof(Texture2D));

                    GUI.backgroundColor = Color.clear;
                    GUILayout.BeginHorizontal();
                    GUILayout.Box(warningImage, GUILayout.Width(27), GUILayout.Height(27));
                    EditorGUILayout.LabelField("This element is not synchronized with the World Storage");
                    GUILayout.EndHorizontal();
                }

                //ELEMENT'S ATTRIBUTES
                EditorGUI.BeginChangeCheck();

                //uuid
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("UUID ", EditorStyles.boldLabel, GUILayout.Width(50));
                if (!UtilGraphSingleton.instance.linkIds.Contains(worldLink.UUID.ToString()))
                {
                    EditorGUILayout.LabelField("none yet (element not yet saved in the server)");
                }
                else
                {
                    EditorGUILayout.SelectableLabel(worldLink.UUID.ToString(), EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
                EditorGUILayout.EndHorizontal();

                //source element
                EditorGUILayout.LabelField("Source Element (From element)", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.LabelField("Name ", GUILayout.Width(75));
                EditorGUILayout.LabelField(worldLinkEdge.output.node.title);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.LabelField("Type ", GUILayout.Width(75));
                EditorGUILayout.LabelField(worldLink.TypeFrom.ToString(), GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.LabelField("UUID ", GUILayout.Width(75));
                if (UtilGraphSingleton.instance.nodePositions.ContainsKey(worldLink.UUIDFrom.ToString()))
                {
                    EditorGUILayout.LabelField(worldLink.UUIDFrom.ToString());
                }
                else
                {
                    EditorGUILayout.LabelField("no UUID yet (element not yet saved in the server)");
                }
                EditorGUILayout.EndHorizontal();

                //target element
                EditorGUILayout.LabelField("Target Element (To element)", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.LabelField("Name ", GUILayout.Width(70));
                EditorGUILayout.LabelField(worldLinkEdge.input.node.title);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.LabelField("Type ", GUILayout.Width(70));
                EditorGUILayout.LabelField(worldLink.TypeTo.ToString(), GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(50);
                EditorGUILayout.LabelField("UUID ", GUILayout.Width(70));
                if (UtilGraphSingleton.instance.nodePositions.ContainsKey(worldLink.UUIDTo.ToString()))
                {
                    EditorGUILayout.LabelField(worldLink.UUIDTo.ToString());
                }
                else
                {
                    EditorGUILayout.LabelField("no UUID yet (element not yet saved in the server)");
                }
                EditorGUILayout.EndHorizontal();

                //unit system
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Unit ", EditorStyles.boldLabel, GUILayout.Width(50));
                worldLink.Unit = (UnitSystem)EditorGUILayout.EnumPopup(worldLink.Unit);
                EditorGUILayout.EndHorizontal();

                //style for sublabels (right aligned)
                var rightStyle = GUI.skin.GetStyle("Label");
                rightStyle.alignment = TextAnchor.UpperRight;

                //localCRS
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("3D Transform ", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //position
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Position ", GUILayout.Width(60));
                EditorGUILayout.LabelField("X", rightStyle, GUILayout.Width(15));
                local_pos[0] = EditorGUILayout.DelayedFloatField(local_pos[0]);
                EditorGUILayout.LabelField("Y", rightStyle, GUILayout.Width(15));
                local_pos[1] = EditorGUILayout.DelayedFloatField(local_pos[1]);
                EditorGUILayout.LabelField("Z", rightStyle, GUILayout.Width(15));
                local_pos[2] = EditorGUILayout.DelayedFloatField(local_pos[2]);
                EditorGUILayout.EndHorizontal();
                //rotation
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rotation ", GUILayout.Width(60));
                EditorGUILayout.LabelField("X", rightStyle, GUILayout.Width(15));
                local_rot[0] = EditorGUILayout.DelayedFloatField(local_rot[0]);
                EditorGUILayout.LabelField("Y", rightStyle, GUILayout.Width(15));
                local_rot[1] = EditorGUILayout.DelayedFloatField(local_rot[1]);
                EditorGUILayout.LabelField("Z", rightStyle, GUILayout.Width(15));
                local_rot[2] = EditorGUILayout.DelayedFloatField(local_rot[2]);
                EditorGUILayout.EndHorizontal();

                //Actions when the ui fields have been changed
                if (EditorGUI.EndChangeCheck())
                {
                    //
                    Matrix4x4 localCRS = Matrix4x4.TRS(local_pos, Quaternion.Euler(local_rot), Vector3.one);
                    List<float> localCRSasFloat = new List<float>
                    {
                        localCRS.m00,    localCRS.m01,    localCRS.m02,    localCRS.m03,
                        localCRS.m10,    localCRS.m11,    localCRS.m12,    localCRS.m13,
                        localCRS.m20,    localCRS.m21,    localCRS.m22,    localCRS.m23,
                        localCRS.m30,    localCRS.m31,    localCRS.m32,    localCRS.m33,
                    };
                    worldLink.Transform = localCRSasFloat;

                    if (UtilGraphSingleton.instance.linkIds.Contains(worldLink.UUID.ToString()) && (!UtilGraphSingleton.instance.elemsToUpdate.Contains(worldLink.UUID.ToString())))
                    {
                        UtilGraphSingleton.instance.elemsToUpdate.Add(worldLink.UUID.ToString());
                    }
                    worldLinkEdge.MarkUnsaved();

                    //update scene
                    String parentName = worldLinkEdge.output.node.title;
                    String elemName = worldLinkEdge.input.node.title;
                    SceneBuilder.MoveGO(parentName, elemName, localCRS);
                }

                //
                //FOOTER
                //
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                var originalColor = GUI.backgroundColor;

                //reload button
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Reload"))
                {
                    //lose focus of fields otherwise the selected field won't updaate
                    EditorGUI.FocusTextInControl(null);
                    if (UtilGraphSingleton.instance.linkIds.Contains(worldLink.UUID.ToString()))
                    {
                        if (UtilGraphSingleton.instance.elemsToUpdate.Contains(worldLink.UUID.ToString()) && EditorUtility.DisplayDialog("Reset elements", "Are you sure you want to lose all your changes ?", "Yes", "No"))
                        {
                            worldLink = WorldLinkRequest.GetWorldLink(UtilGraphSingleton.instance.worldStorageServer, worldLink.UUID.ToString());
                            worldLinkEdge.worldLink = worldLink;

                            UtilGraphSingleton.instance.elemsToUpdate.Remove(worldLink.UUID.ToString());
                            worldLinkEdge.MarkSaved();

                            ShowWindow(worldLinkEdge);

                            //update the scene
                            String parentName = worldLinkEdge.output.node.title;
                            String elemName = worldLinkEdge.input.node.title;
                            var localCrsAsList = worldLinkEdge.worldLink.Transform;
                            Matrix4x4 localCRS = new Matrix4x4();
                            localCRS.m00 = localCrsAsList[0]; localCRS.m01 = localCrsAsList[1]; localCRS.m02 = localCrsAsList[2]; localCRS.m03 = localCrsAsList[3];
                            localCRS.m10 = localCrsAsList[4]; localCRS.m11 = localCrsAsList[5]; localCRS.m12 = localCrsAsList[6]; localCRS.m13 = localCrsAsList[7];
                            localCRS.m20 = localCrsAsList[8]; localCRS.m21 = localCrsAsList[9]; localCRS.m22 = localCrsAsList[10]; localCRS.m23 = localCrsAsList[11];
                            localCRS.m30 = localCrsAsList[12]; localCRS.m31 = localCrsAsList[13]; localCRS.m32 = localCrsAsList[14]; localCRS.m33 = localCrsAsList[15];
                            SceneBuilder.MoveGO(parentName, elemName, localCRS);
                        }
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("Reset elements", "Are you sure you want to lose all your changes ?", "Yes", "No"))
                        {

                            List<float> transform = new();
                            transform.Add(1);
                            for (int i = 1; i < 5; i++)
                            {
                                transform.Add(0);
                            }
                            transform.Add(1);
                            for (int i = 6; i < 10; i++)
                            {
                                transform.Add(0);
                            }
                            transform.Add(1);
                            for (int i = 11; i < 15; i++)
                            {
                                transform.Add(0);
                            }
                            transform.Add(1);

                            worldLink.Transform = transform;
                            worldLink.Unit = UnitSystem.CM;

                            ShowWindow(worldLinkEdge);

                            //update the scene
                            String parentName = worldLinkEdge.output.node.title;
                            String elemName = worldLinkEdge.input.node.title;
                            var localCrsAsList = worldLinkEdge.worldLink.Transform;
                            Matrix4x4 localCRS = new Matrix4x4();
                            localCRS.m00 = localCrsAsList[0]; localCRS.m01 = localCrsAsList[1]; localCRS.m02 = localCrsAsList[2]; localCRS.m03 = localCrsAsList[3];
                            localCRS.m10 = localCrsAsList[4]; localCRS.m11 = localCrsAsList[5]; localCRS.m12 = localCrsAsList[6]; localCRS.m13 = localCrsAsList[7];
                            localCRS.m20 = localCrsAsList[8]; localCRS.m21 = localCrsAsList[9]; localCRS.m22 = localCrsAsList[10]; localCRS.m23 = localCrsAsList[11];
                            localCRS.m30 = localCrsAsList[12]; localCRS.m31 = localCrsAsList[13]; localCRS.m32 = localCrsAsList[14]; localCRS.m33 = localCrsAsList[15];
                            SceneBuilder.MoveGO(parentName, elemName, localCRS);
                        }
                    }
                }

                //save button
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Save"))
                {
                        //if one of the connected elements is not in the server, you can't save the link
                    if ((UtilGraphSingleton.instance.nodePositions.ContainsKey(worldLink.UUIDTo.ToString()) && UtilGraphSingleton.instance.nodePositions.ContainsKey(worldLink.UUIDFrom.ToString())))
                    { 
                        if (UtilGraphSingleton.instance.linkIds.Contains(worldLink.UUID.ToString()))
                        {
                            if (UtilGraphSingleton.instance.elemsToUpdate.Contains(worldLink.UUID.ToString()))
                            {
                                WorldLinkRequest.UpdateWorldLink(UtilGraphSingleton.instance.worldStorageServer, worldLink);
                                UtilGraphSingleton.instance.elemsToUpdate.Remove(worldLink.UUID.ToString());
                            }
                        }
                        else
                        {
                            String uuid = WorldLinkRequest.AddWorldLink(UtilGraphSingleton.instance.worldStorageServer, worldLink);

                            //Add the newly saved WorldLink to the SaveInfo singleton
                            uuid = uuid.Replace("\"", "");
                            worldLink.UUID = Guid.Parse(uuid);
                            worldLinkEdge.GUID = uuid;
                            worldLinkEdge.viewDataKey = worldLinkEdge.GUID;
                            UtilGraphSingleton.instance.linkIds.Add(uuid);
                        }
                        worldLinkEdge.MarkSaved();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Error", "You are not able to save this link because at least one of its connected elements is not saved in the World Storage", "Ok");
                    }
                }
                GUILayout.Space(10);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);

            }
        }

        //utilty method to draw lines
        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
                r.height = thickness;
            r.y += padding/2;
            r.x-=2;
            r.width +=6;
            EditorGUI.DrawRect(r, color);
        }
    }
}