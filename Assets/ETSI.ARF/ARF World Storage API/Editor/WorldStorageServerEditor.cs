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

using UnityEditor;
using UnityEngine;

namespace ETSI.ARF.WorldStorage.UI
{
    [CustomEditor(typeof(WorldStorageServer))]
    public class WorldStorageServerEditor : Editor
    {
        WorldStorageServer worldStorageServer;
        WorldStorageWindow win;

        public void OnEnable()
        {
            worldStorageServer = (WorldStorageServer)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Color ori = GUI.backgroundColor;

            GUILayout.Label("Copyright(c) 2022, ETSI - ARF");
            EditorGUILayout.Space();
            GUILayout.Label("Parameters:", EditorStyles.boldLabel);
            DrawDefaultInspector();
            EditorGUILayout.Space();

            // open window button
            GUI.backgroundColor = WorldStorageWindow.arfColors[1];
            if (GUILayout.Button("Open World Storage Window..."))
            {
                Debug.Log("Open Main ARF Window");
                win = EditorWindow.GetWindow(typeof(WorldStorageWindow), false, "ETSI ARF - Authoring Editor") as WorldStorageWindow;
                win.worldStorageServer = worldStorageServer;
                win.worldStorageUser = worldStorageServer.currentUser;
            }
            GUI.backgroundColor = ori;
        }
    }
}