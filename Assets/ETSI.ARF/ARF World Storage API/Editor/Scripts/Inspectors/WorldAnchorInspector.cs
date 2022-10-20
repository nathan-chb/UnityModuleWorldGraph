using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using Dummiesman;
using Org.OpenAPITools.Model;
using Siccity.GLTFUtility;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts.Inspectors
{

    [CustomEditor(typeof(WorldAnchorScript))]
    public class WorldAnchorInspector : UnityEditor.Editor
    {
        //The chackbox to show the model or not
        bool activated;

        //The url field to change the aanchor's model
        String modelUrl;

        //The target object of this inspector
        WorldAnchorScript script;

        public void Awake()
        {
            script = ((WorldAnchorScript)target);
            if (script.model != null)
            {
                activated = script.model.activeInHierarchy;
            }
            else
            {
                activated = true;
            }
            modelUrl = WorldAnchorScript.GetModelUrl(script.worldAnchor);
        }
        public override void OnInspectorGUI()
        {

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("World Anchor : ");

            //NAME
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name : ");
            EditorGUI.BeginChangeCheck();
            script.worldAnchor.Name = EditorGUILayout.DelayedTextField(script.worldAnchor.Name);
            if (EditorGUI.EndChangeCheck())
            {
                script.name = script.worldAnchor.Name;
                WorldGraphWindow.RenameNode(script.name, script.worldAnchor.UUID.ToString());
            }
            EditorGUILayout.EndHorizontal();

            //MODEL CHECKBOX
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Model ");
            EditorGUI.BeginChangeCheck();
            activated = EditorGUILayout.Toggle(activated);
            if (EditorGUI.EndChangeCheck())
            {
                if (activated)
                {
                    if ((script.model != null) && (!script.model.activeInHierarchy))
                    {
                        script.model.SetActive(true); ;
                    }
                }
                else
                {
                    if ((script.model != null) && (script.model.activeInHierarchy))
                    {
                        script.model.SetActive(false); ;
                    }
                }
            }
            EditorGUILayout.EndHorizontal();


            //MODEL URL
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Model URL : ");
            EditorGUI.BeginChangeCheck();
            modelUrl = EditorGUILayout.DelayedTextField(modelUrl);
            if (EditorGUI.EndChangeCheck())
            {
                WorldGraphWindow.ChangeAnchorURL(modelUrl, script.worldAnchor.UUID.ToString());

                //destroy the first GameObject
                if (script.model != null)
                {
                    DestroyImmediate(script.model);
                }
                if (modelUrl != "")
                {

                    //That's where we load the Model File, if it already extists, it's loaded from the file system, otherwise it's saved through an Importer

                    //It already exsits
                    if (File.Exists(WorldAnchorScript.GetFilePath(modelUrl)))
                    {
                        if (modelUrl.EndsWith(".obj"))
                        {
                            // Save the model into the anchor script attribute
                            script.model = new OBJLoader().Load(WorldAnchorScript.GetFilePath(modelUrl));
                        }
                        else if (modelUrl.EndsWith(".gltf"))
                        {
                            // Save the model into the anchor script attribute
                            script.model = Importer.LoadFromFile(WorldAnchorScript.GetFilePath(modelUrl));
                        }
                        if (script.model != null)
                        {
                            script.model.transform.parent = script.gameObject.transform;
                            script.model.tag = "EditorOnly";
                            script.model.name = WorldAnchorScript.GetFileName(modelUrl);
                            foreach (Transform child in script.model.transform)
                            {
                                child.hideFlags |= HideFlags.HideInHierarchy;
                            }
                        }
                    }
                    else
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(WorldAnchorScript.LoadModel(modelUrl, (UnityWebRequest req) =>
                        {
                            if ((req.result == UnityWebRequest.Result.ConnectionError) || (req.result == UnityWebRequest.Result.ProtocolError))
                            {
                                // Log any errors that may happen
                                Debug.Log($"{req.error} : {req.downloadHandler.text}");
                            }
                            else
                            {
                                if (modelUrl.EndsWith(".obj"))
                                {
                                    // Save the model into the anchor script attribute
                                    script.model = new OBJLoader().Load(WorldAnchorScript.GetFilePath(modelUrl));
                                }
                                else if (modelUrl.EndsWith(".gltf"))
                                {
                                    // Save the model into the anchor script attribute
                                    script.model = Importer.LoadFromFile(WorldAnchorScript.GetFilePath(modelUrl));
                                }
                                if (script != null)
                                {
                                    script.model.transform.parent = script.gameObject.transform;
                                    script.model.tag = "EditorOnly";
                                    script.model.name = WorldAnchorScript.GetFileName(modelUrl);
                                    foreach (Transform child in script.model.transform)
                                    {
                                        child.hideFlags |= HideFlags.HideInHierarchy;
                                    }
                                }
                            }
                        }));
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        //gets the url from the keyvalue attribute
        public static string GetModelUrl(WorldAnchor worldAnchor)
        {
            List<string> list;
            worldAnchor.KeyvalueTags.TryGetValue("ModelURL", out list);
            if (list != null)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForMyScript(WorldAnchorScript myScript, GizmoType gizmoType)
        {
            if (myScript.link != null)
            {
                Gizmos.DrawLine(myScript.transform.position, myScript.transform.parent.position);
            }
        }
    }
}
