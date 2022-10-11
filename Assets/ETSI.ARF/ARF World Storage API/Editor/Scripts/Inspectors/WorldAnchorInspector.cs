using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using Org.OpenAPITools.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts.Inspectors
{

    [CustomEditor(typeof(WorldAnchorScript))]
    public class WorldAnchorInspector : UnityEditor.Editor
    {
        bool activated;
        String modelUrl;
        WorldAnchorScript script; 

        public void Awake()
        {
            script = ((WorldAnchorScript)target);
            if(script.model != null)
            {
                activated = script.model.activeInHierarchy;
            }
            else
            {
                activated = false;
            }
            modelUrl = GetModelUrl(script.worldAnchor);
        }
        public override void OnInspectorGUI()
        {

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("World Anchor : ");

            //PARENT LINK
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Parent Link : ");
            if (script.link != null)
            {
                EditorGUILayout.LabelField(script.link.ToString());
            }
            EditorGUILayout.EndHorizontal();

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
                    if((script.model != null) && (!script.gameObject.activeInHierarchy))
                    {
                        script.gameObject.SetActive(true); ;
                    }
                }
                else
                {
                    if ((script.model != null) && (script.gameObject.activeInHierarchy))
                    {
                        script.gameObject.SetActive(false); ;
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

                //FAIRE LA COROUTINE
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadModel(modelUrl));

            }
            EditorGUILayout.EndHorizontal();
        }

        public static string GetModelUrl(WorldAnchor worldAnchor)
        {
            List<string> list;
            worldAnchor.KeyvalueTags.TryGetValue("ModelURL", out list);
            if(list != null)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        IEnumerator LoadModel(string url)
        {
            /*using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();
                callback(req);
            }*/
            Debug.Log(url);
            yield return null;
        }
    }
}
