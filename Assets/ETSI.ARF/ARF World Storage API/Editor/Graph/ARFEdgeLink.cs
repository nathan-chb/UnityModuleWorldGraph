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

using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using ETSI.ARF.WorldStorage.UI;
using Org.OpenAPITools.Model;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph
{
    public class ARFEdgeLink : Edge
    {
        public WorldLink worldLink;
        public string GUID;

        public Image savedIcon;

        //used when the edge input destination is changed
        public ARFNode originalDestinationNode;

        public ARFEdgeLink()
        {
            var doubleClickManipulator = new Clickable(Clicked);
            doubleClickManipulator.activators.Clear();
            doubleClickManipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
            this.AddManipulator(doubleClickManipulator);
        }
        public void Clicked()
        {
            Debug.Log(worldLink.ToJson());
            GraphEditorWindow.ShowWindow(this);
        }
        public void MarkUnsaved()
        {
            if (savedIcon == null)
            {
                //the icon to add if the node does not correspond to an element in the server
                Texture2D warningImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/cloud.png", typeof(Texture2D));
                savedIcon = new Image
                {
                    image = warningImage
                };
                savedIcon.style.width = 18;
                savedIcon.style.height = 18;
                savedIcon.style.minWidth = 18;
                savedIcon.style.minHeight = 18;
                savedIcon.style.flexGrow = 1;
                savedIcon.style.alignSelf = Align.Center;

            }
            if (!edgeControl.Contains(savedIcon))
            {
                edgeControl.Add(savedIcon);
            }
            tooltip = "This element is not synchronized with the World Storage";
        }

        public void MarkSaved()
        {
            if (edgeControl.Contains(savedIcon))
            {
                edgeControl.Remove(savedIcon);
                tooltip = "";
            }
            if (UtilGraphSingleton.instance.elemsToUpdate.Contains(GUID))
            {
                UtilGraphSingleton.instance.elemsToUpdate.Remove(GUID);
            }

            //mark the son's GameObject as not modified
            string nodeName = this.input.node.title;
            var go = GameObject.Find(nodeName);
            if(go != null)
            {
                var worldAnchorScript = (WorldAnchorScript)go.GetComponent<WorldAnchorScript>();
                var trackableScript = (TrackableScript)go.GetComponent<TrackableScript>();
                if (worldAnchorScript != null)
                {
                    worldAnchorScript.modified = false;
                }
                else if (trackableScript != null)
                {
                    trackableScript.modified = false;
                }
            }
        }
    }
}