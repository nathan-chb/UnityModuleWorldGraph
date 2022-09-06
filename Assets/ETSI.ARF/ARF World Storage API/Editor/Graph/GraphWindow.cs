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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
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

    public class GraphWindow : EditorWindow
    {
        [HideInInspector] public WorldStorageServer worldStorageSettings;

        [SerializeField] public List<string> trackables = new List<string>();

        bool groupEnabled;

        string uid = System.Guid.Empty.ToString();
        string customName = "NotDefined";
        string creatorUid = System.Guid.Empty.ToString();
        string type = "Unknow";
        string unit = "Unknow";
        Vector2Int dim;

        private Trackable currentTrackable;
        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        private ARFGraphView myGraph;

        [MenuItem("ARFWorldStorage/Graph Editor")]
        public static void ShowWindow()//WorldStorageServer ws)
        {
            GraphWindow win = EditorWindow.GetWindow(typeof(GraphWindow), false, WorldStorageWindow.winName) as GraphWindow;
            //win.worldStorageSettings = ws;
        }

        public GraphWindow()
        {
            // init somne stuffs
            //currentTrackable = new Trackable();
        }

        public void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }
         
        public void OnDisable()
        {
            rootVisualElement.Remove(myGraph);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var createNodeT = new Button(clickEvent: () => { myGraph.CreateNode("Trackable"); });
            createNodeT.text = "Create Trackable";
            toolbar.Add(createNodeT);

            var createNodeWA = new Button(clickEvent: () => { myGraph.CreateNode("World Anchor"); });
            createNodeWA.text = "Create World Anchor";
            toolbar.Add(createNodeWA);

            var createNodeL = new Button(clickEvent: () => { });
            createNodeL.text = "Create Link";
            toolbar.Add(createNodeL);

            var close = new Button(clickEvent: () => { Close(); });
            close.text = "Close Window";
            toolbar.Add(close);

            rootVisualElement.Add(toolbar);
        }

        private void ConstructGraphView()
        {
            myGraph = new ARFGraphView
            {
                name = "ARF Graph"
            };
            myGraph.StretchToParentSize();
            //myGraph.StretchToParentWidth();
            rootVisualElement.Add(myGraph);
        }

        void OnGUI()
        {
            ori = GUI.backgroundColor; // remember ori color

            gsTest = new GUIStyle("window");
            gsTest.normal.textColor = WorldStorageWindow.arfColors[0];
            gsTest.fontStyle = FontStyle.Bold;
            
            EditorGUILayout.Space(24);
            GUI.contentColor = WorldStorageWindow.arfColors[1];
            WorldStorageWindow.DrawCopyright();

            //TLP.Editor.EditorGraph graph = new TLP.Editor.EditorGraph(0, -1, 10, 1, "Just a sin wave", 100);
            //graph.AddFunction(x => Mathf.Sin(x));
            //graph.Draw();
        }

        /*
        void DrawTrackableStuffs()// Trackable trackable)
        {
            GUILayout.BeginVertical("AR Trackable", gsTest);
            //
            GUILayout.Label("Server: " + worldStorageSettings.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("Creator UID: " + creatorUid, EditorStyles.miniLabel); // readonly
            EditorGUILayout.Space();

            //GUILayout.BeginHorizontal();
            uid = EditorGUILayout.TextField("UID (0 = new one)", uid);
            EditorGUILayout.Space();

            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Get Parameters"))
            {
                Trackable t = RESTfulTrackableRequest.GetTrackable(worldStorageSettings, uid);
                creatorUid = t.CreatorUUID.ToString();
                type = t.GetType().ToString();
                unit = t.Unit.ToString();
            }
            GUI.backgroundColor = ori;

            type = EditorGUILayout.TextField("Trackable Type", type);
            unit = EditorGUILayout.TextField("Unit System", unit);

            EditorGUILayout.Space(10);
            dim = EditorGUILayout.Vector2IntField("Dimension", dim);

            EditorGUILayout.Space();
            GUILayout.Button("Payload from Asset...");

            EditorGUILayout.Space();
            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Parameters:", groupEnabled);
            EditorGUILayout.IntField("Number of KeyValues", 0);
            EditorGUILayout.Space();
            EditorGUILayout.TextField("Key", "");
            EditorGUILayout.TextField("Value", "");
            EditorGUILayout.EndToggleGroup();
            //
            GUILayout.EndVertical();

            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete Trackable"))
            {
                Debug.Log("Deleting Trackable");
                RESTfulTrackableRequest.DeleteTrackable(worldStorageSettings, uid);
                uid = System.Guid.Empty.ToString();
                creatorUid = System.Guid.Empty.ToString();
                type = "";
                unit = "";
                WorldStorageWindow.WorldStorageWindowSingleton.UpdateList();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }
            GUI.backgroundColor = ori;

            GUI.backgroundColor = WorldStorageWindow.arfColors[2];
            if (GUILayout.Button("Create/Update Trackable"))
            {
                Debug.Log("PostAddTrackable");
                if (string.IsNullOrEmpty(uid) || uid == "0") uid = System.Guid.Empty.ToString();
                Trackable t = RESTfulTrackableRequest.TrackableFromStrings(uid, cus, worldStorageSettings.creatorUID);
                RESTfulTrackableRequest.PostAddTrackable(worldStorageSettings, t);
                WorldStorageWindow.WorldStorageWindowSingleton.UpdateList();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();

                uid = t.UUID.ToString();
                type = t.GetType().ToString();
                unit = t.Unit.ToString();
            }
            GUI.backgroundColor = ori;
        }

  */
    }
}