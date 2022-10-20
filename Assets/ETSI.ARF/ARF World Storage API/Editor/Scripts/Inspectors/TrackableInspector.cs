using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts.Inspectors
{
    [CustomEditor(typeof(TrackableScript))]
    public class TrackableInspector : UnityEditor.Editor
    {

        //The target object of this inspector
        TrackableScript script;

        public void Awake()
        {
            script = ((TrackableScript)target);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Trackable : ");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name : ");
            EditorGUI.BeginChangeCheck();
            ((TrackableScript)target).trackable.Name = EditorGUILayout.DelayedTextField(((TrackableScript)target).trackable.Name);
            if (EditorGUI.EndChangeCheck())
            {
                ((TrackableScript)target).name = ((TrackableScript)target).trackable.Name;
                WorldGraphWindow.RenameNode(((TrackableScript)target).name, ((TrackableScript)target).trackable.UUID.ToString());
            }
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

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        static void DrawGizmoForMyScript(TrackableScript myScript, GizmoType gizmoType)
        {
            if (myScript.link != null)
            {
                Gizmos.DrawLine(myScript.transform.position, myScript.transform.parent.position);
            }
        }

    }
}