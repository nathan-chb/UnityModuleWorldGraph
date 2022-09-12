using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts.Inspectors
{
    [CustomEditor(typeof(TrackableScript))]
    public class TrackableInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Trackable : ");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name : ");
            EditorGUILayout.LabelField(((TrackableScript)target).trackable.Name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UUID : ");
            if (UtilGraphSingleton.instance.nodePositions.ContainsKey(((TrackableScript)target).trackable.UUID.ToString()))
            {
                EditorGUILayout.LabelField(((TrackableScript)target).trackable.UUID.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("No UUID yet (not yet saved in the server");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}