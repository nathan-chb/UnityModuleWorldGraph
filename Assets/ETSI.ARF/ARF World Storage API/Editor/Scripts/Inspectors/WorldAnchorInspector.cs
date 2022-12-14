using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts.Inspectors
{
    [CustomEditor(typeof(WorldAnchorScript))]
    public class WorldAnchorInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("World Anchor : ");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name : ");
            EditorGUILayout.LabelField(((WorldAnchorScript)target).worldAnchor.Name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UUID : ");
            if (UtilGraphSingleton.instance.nodePositions.ContainsKey(((WorldAnchorScript)target).worldAnchor.UUID.ToString()))
            {
                EditorGUILayout.LabelField(((WorldAnchorScript)target).worldAnchor.UUID.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("No UUID yet (not yet saved in the server");
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
