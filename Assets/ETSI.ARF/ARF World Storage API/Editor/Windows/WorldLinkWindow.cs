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
using TMPro;
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
        public class Element
        {
            public string UUID = System.Guid.Empty.ToString();
            public string name = "(none)";
            public ObjectType type = ObjectType.NotIdentified;
            public Vector3 pos = Vector3.zero;
        }

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
        
        // From & To elements:
        private bool showListFrom = true;
        private bool showListTo = true;
        private Element FROM = new Element();
        private Element TO = new Element();

        UnitSystem unit = UnitSystem.CM;
        Vector3 transf_pos;
        Vector3 transf_rot;

        [SerializeField] Dictionary<string, List<string>> keyValueTags = new Dictionary<string, List<string>>();
        string key1 = "";
        string value1 = "";

        // UI stuffs
        private Vector2 scrollPos;
        private Color ori;
        private GUIStyle gsTest;

        public static void ShowWindow(WorldStorageServer ws, WorldStorageUser user, string UUID = "")
        {
            winSingleton = EditorWindow.GetWindow(typeof(WorldLinkWindow), false, "ETSI ARF - World Link") as WorldLinkWindow;
            winSingleton.worldStorageServer = ws;
            winSingleton.worldStorageUser = user;
            if (!string.IsNullOrEmpty(UUID))
            {
                winSingleton.UUID = UUID;
                winSingleton.GetWorldLinkParams();
            }
            else
            {
                // Create new one
                winSingleton.AddLink();
            }
        }

        public static GameObject GenerateAndUpdateVisual(string UUID, Element from, Element to)
        {
            ETSI.ARF.WorldStorage.UI.Prefabs.WorldStoragePrefabs prefabs;
            prefabs = (Prefabs.WorldStoragePrefabs)Resources.Load("ARFPrefabs");
            GameObject arf = GameObject.Find("ARF Visuals");
            GameObject visual = GameObject.Find(UUID);

            //Value between from and to
            Vector3 centerPos = (from.pos + to.pos) * 0.5f;
            Vector3 rot = Vector3.zero;  // Direction

            if (arf == null) arf = new GameObject("ARF Visuals");
            if (visual == null)
            {
                visual = SceneAsset.Instantiate<GameObject>(prefabs.worldLinkPrefab, centerPos, Quaternion.Euler(rot), arf.transform); // TODO rot
                visual.name = UUID;
            }
            else
            {
                visual.transform.SetPositionAndRotation(centerPos, Quaternion.Euler(rot));
            }

            // Update the gizno, if GaneObject are founds!!!
            GameObject go1 = GameObject.Find(from.UUID);
            GameObject go2 = GameObject.Find(to.UUID);
            if (go1 && go2)
            {
                LinkVisual gizmo = visual.GetComponent<LinkVisual>();
                if (gizmo)
                {
                    gizmo.fromElement = go1;
                    gizmo.toElement = go2;
                }
            }

            // Update the annotation
            visual.transform.Find("Canvas/Text").GetComponent<TextMeshProUGUI>().text = $"UUID: { UUID }\nFrom: { from.name }\nTo: { to.name }";
            return visual;
        }

        public WorldLinkWindow()
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
            gsTest.fixedHeight = 100;

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandWidth(true));
            WorldStorageWindow.DrawCopyright();

            DrawAnchorStuffs();

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Close Window"))
            {
                Close();
            }
        }

        private void GetElementFROM()
        {
            // Trackable?
            try
            {
                Trackable result = TrackableRequest.GetTrackable(worldStorageServer, FROM.UUID);
                FROM.name = result.Name;
                FROM.type = ObjectType.Trackable;

                Matrix4x4 localCRS = WorldStorageWindow.MatrixFromLocalCRS(result.LocalCRS);
                FROM.pos = localCRS.GetPosition();
            }
            catch
            {
                // Anchor?
                try
                {
                    WorldAnchor result = WorldAnchorRequest.GetWorldAnchor(worldStorageServer, FROM.UUID);
                    FROM.name = result.Name;
                    FROM.type = ObjectType.WorldAnchor;

                    Matrix4x4 localCRS = WorldStorageWindow.MatrixFromLocalCRS(result.LocalCRS);
                    FROM.pos = localCRS.GetPosition();
                }
                catch
                {
                    // Nothing!
                    FROM.name = "";
                    FROM.type = ObjectType.NotIdentified;
                }
            }
        }

        private void GetElementTO()
        {
            // Trackable?
            try
            {
                Trackable result = TrackableRequest.GetTrackable(worldStorageServer, TO.UUID);
                TO.name = result.Name;
                TO.type = ObjectType.Trackable;

                Matrix4x4 localCRS = WorldStorageWindow.MatrixFromLocalCRS(result.LocalCRS);
                TO.pos = localCRS.GetPosition();
            }
            catch
            {
                // Anchor?
                try
                {
                    WorldAnchor result = WorldAnchorRequest.GetWorldAnchor(worldStorageServer, TO.UUID);
                    TO.name = result.Name;
                    TO.type = ObjectType.WorldAnchor;

                    Matrix4x4 localCRS = WorldStorageWindow.MatrixFromLocalCRS(result.LocalCRS);
                    TO.pos = localCRS.GetPosition();
                }
                catch
                {
                    // Nothing!
                    TO.UUID = System.Guid.Empty.ToString();
                    TO.name = "";
                    TO.type = ObjectType.NotIdentified;
                }
            }
        }

        void DrawAnchorStuffs()
        {
            GUILayout.BeginVertical(); // "World Link Editor", gsTest);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUI.backgroundColor = WorldStorageWindow.arfColors[9];
            Texture linkImage = (Texture)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/link.png", typeof(Texture));
            GUILayout.Box(linkImage, GUILayout.Width(24), GUILayout.Height(24));
            GUI.backgroundColor = ori;
            GUILayout.Label("World Link Parameters:", EditorStyles.whiteBoldLabel);
            GUILayout.EndHorizontal();

            Rect rect = EditorGUILayout.GetControlRect(false, WorldStorageWindow.lineH);
            EditorGUI.DrawRect(rect, WorldStorageWindow.arfColors[9]);

            //
            GUILayout.Label("Server: " + worldStorageServer.serverName, EditorStyles.whiteLargeLabel);
            GUILayout.Label("User: " + worldStorageUser.userName, EditorStyles.whiteLargeLabel);
            EditorGUILayout.Space();

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
                Debug.Log("PUT World Link");

                if (!string.IsNullOrEmpty(UUID) && UUID != "0" && UUID != System.Guid.Empty.ToString())
                {
                    WorldLink obj = GenerateWorldLink();
                    UUID = WorldLinkRequest.UpdateWorldLink(worldStorageServer, obj);
                    UUID = UUID.Trim('"'); //Bugfix: remove " from server return value
                    WorldStorageWindow.WorldStorageWindowSingleton.GetWorldLinks();
                    WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
                }
            }

            GUI.backgroundColor = WorldStorageWindow.arfColors[3];
            if (GUILayout.Button("Delete"))
            {
                Debug.Log("Delete World Link");
                WorldLinkRequest.DeleteWorldLink(worldStorageServer, UUID);
                UUID = System.Guid.Empty.ToString();
                customName = "Warning: Object deleted !";
                creatorUUID = System.Guid.Empty.ToString();
                unit = UnitSystem.CM;
                WorldStorageWindow.WorldStorageWindowSingleton.GetWorldLinks();
                WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
            }
            GUI.backgroundColor = ori;

            GUI.backgroundColor = WorldStorageWindow.arfColors[5];
            if (GUILayout.Button("Generate/Update GameObject"))
            {
                GenerateAndUpdateVisual(UUID, FROM, TO);
            }
            GUI.backgroundColor = ori;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // ---------------------
            // Params
            // ---------------------
            string lastFromUUID = FROM.UUID;
            string lastToUUID = TO.UUID;

            if (GUILayout.Button("Use 'From-To' Objects from Scene Selection"))
            {
                GameObject from, to;
                GameObject[] SelectedObjects = Selection.gameObjects;

                if (SelectedObjects.Length == 2)
                {
                    Debug.Log("Creation du lien (Many thanks Eric ;-)");
                    from = SelectedObjects[0];
                    to = SelectedObjects[1];
                    FROM.UUID = from.name;
                    TO.UUID = to.name;
                }
                else
                {
                    EditorUtility.DisplayDialog("Selection", "Please select exactly 2 elements in the scene!", "OK");
                }
            }

            showListFrom = EditorGUILayout.Foldout(showListFrom, "Parent Object (From)");
            if (showListFrom)
            {
                EditorGUILayout.BeginHorizontal();
                FROM.UUID = EditorGUILayout.TextField("UUID:", FROM.UUID);
                if (FROM.UUID.Contains("["))
                {
                    // extract the UUID
                    FROM.UUID = FROM.UUID.Split('[', ']')[1];
                }

                GUI.backgroundColor = WorldStorageWindow.arfColors[0];
                if (GUILayout.Button("Request", EditorStyles.miniButtonLeft, miniButtonWidth) || lastFromUUID != FROM.UUID)
                {
                    GetElementFROM();
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = ori;
                EditorGUILayout.LabelField("Name:", FROM.name);
                EditorGUILayout.LabelField("Type:", FROM.type.ToString());
            }

            EditorGUILayout.Space();
            showListTo = EditorGUILayout.Foldout(showListTo, "Child Object (To)");
            if (showListTo)
            {
                EditorGUILayout.BeginHorizontal();
                TO.UUID = EditorGUILayout.TextField("UUID:", TO.UUID);
                if (TO.UUID.Contains("["))
                {
                    // extract the UUID
                    TO.UUID = TO.UUID.Split('[', ']')[1];
                }
                GUI.backgroundColor = WorldStorageWindow.arfColors[0];
                if (GUILayout.Button("Request", EditorStyles.miniButtonLeft, miniButtonWidth) || lastToUUID != TO.UUID)
                {
                    GetElementTO();
                }
                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = ori;
                EditorGUILayout.LabelField("Name:", TO.name);
                EditorGUILayout.LabelField("Type:", TO.type.ToString());
            }

            EditorGUILayout.Space();
            unit = (UnitSystem)EditorGUILayout.EnumPopup("Unit System:", unit);

            EditorGUILayout.Space();
            //TODO Is this required???
            GUILayout.Label("Transform:");
            transf_pos = EditorGUILayout.Vector3Field("Position:", transf_pos);
            transf_rot = EditorGUILayout.Vector3Field("Rotation:", transf_rot);

            EditorGUILayout.Space();
            groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Parameters:", groupEnabled);
            key1 = EditorGUILayout.TextField("Key 1", key1);
            value1 = EditorGUILayout.TextField("Value 1", value1);
            EditorGUILayout.EndToggleGroup();
            //
            GUILayout.EndVertical();
        }

        private void GetWorldLinkParams()
        {
            WorldLink obj = WorldLinkRequest.GetWorldLink(worldStorageServer, UUID);
            //customName = obj.Name;
            creatorUUID = obj.CreatorUUID.ToString();

            FROM.UUID = obj.UUIDFrom.ToString();
            FROM.type = obj.TypeFrom;

            TO.UUID = obj.UUIDTo.ToString();
            TO.type = obj.TypeTo;
            
            unit = obj.Unit;
            if (obj.Transform.Count == 16)
            {
                Matrix4x4 transf = WorldStorageWindow.MatrixFromLocalCRS(obj.Transform); 
                transf_pos = transf.GetPosition();
                transf_rot = transf.rotation.eulerAngles;
            }
            else
            {
                transf_pos = Vector3.zero;
                transf_rot = Vector3.zero;
            }
            keyValueTags = obj.KeyvalueTags;

            // Get here the params of the from/to elements (GET)
            GetElementFROM();
            GetElementTO();
            
            this.Repaint();
        }

        public void AddLink()
        {
            Debug.Log("POST World Link");
            UUID = System.Guid.Empty.ToString();
            customName = "Default Link";

            WorldLink obj = GenerateWorldLink();
            UUID = WorldLinkRequest.AddWorldLink(worldStorageServer, obj);
            UUID = UUID.Trim('"'); //Bugfix: remove " from server return value
            WorldStorageWindow.WorldStorageWindowSingleton.GetWorldLinks();
            WorldStorageWindow.WorldStorageWindowSingleton.Repaint();
        }

        public WorldLink GenerateWorldLink()
        {
            Matrix4x4 localCRS = new Matrix4x4();
            localCRS = Matrix4x4.TRS(transf_pos, Quaternion.Euler(transf_rot), Vector3.one);
            List<float> _transform3d = new List<float>
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
            System.Guid _from = System.Guid.Parse(FROM.UUID);
            System.Guid _to = System.Guid.Parse(TO.UUID);
            WorldLink t = new WorldLink(_uuid, _creator, _from, _to, FROM.type, TO.type, _transform3d, unit, keyValueTags);
            return t;
        }
    }
}