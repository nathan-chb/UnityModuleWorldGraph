using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts
{
    //This class handles the behavior for when the dievelopper plays its application
    [InitializeOnLoad]
    internal class EditorStatus
    {
        static EditorStatus()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            switch (obj)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    DestroyEditorOnlyGO();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    CreateElementGo();
                    break;
                default:
                    Debug.Log(obj.ToString());
                    break;
            }
        }

        private static void DestroyEditorOnlyGO()
        {
            var objects = GameObject.FindGameObjectsWithTag("EditorOnly");
            foreach (var obj in objects)
            {
                Object.DestroyImmediate(obj);
            }
        }

        private static void CreateElementGo()
        {
            if (WorldGraphWindow.IsOpen)
            {
                var graphView = WorldGraphWindow.Instance.GetGraph();
                if (graphView != null)
                {
                    SceneBuilder.InstantiateGraph(graphView);
                }
            }
        }
    }
}
