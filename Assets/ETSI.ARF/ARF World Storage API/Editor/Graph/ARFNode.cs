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

#define USING_OPENAPI_GENERATOR // alt. is Swagger

using UnityEditor;

#if USING_OPENAPI_GENERATOR
using Org.OpenAPITools.Model;
#else
using IO.Swagger.Api;
using IO.Swagger.Model;
#endif
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph;
using System;
using UnityEngine;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;

namespace ETSI.ARF.WorldStorage.UI
{
    public abstract class ARFNode : Node
    {
        public string GUID;
        public bool entryPoint = false;
        public ARFPort portOut;
        public ARFPort portIn;
        public GUID id;

        public Image savedIcon;

        public ARFNode() : base()
        {
        }
        public override Port InstantiatePort(Orientation orientation, Direction direction, Port.Capacity capacity, Type type)
        {
            switch (direction)
            {
                case Direction.Input:
                    portIn = ARFPort.CreateARF<ARFEdgeLink>(orientation, direction, capacity, type);
                    return portIn;
                case Direction.Output:
                    portOut = ARFPort.CreateARF<ARFEdgeLink>(orientation, direction, capacity, type);
                    return portOut;
                default:
                    return null;
            }
        }

        public void DisconnectAllPorts(ARFGraphView graphView)
        {
            DisconnectInputPorts(graphView);
            DisconnectOutputPorts(graphView);
        }

        private void DisconnectInputPorts(ARFGraphView graphView)
        {
            DisconnectPorts(inputContainer, graphView);
        }

        private void DisconnectOutputPorts(ARFGraphView graphView)
        {
            DisconnectPorts(outputContainer, graphView);
        }

        private void DisconnectPorts(VisualElement container, ARFGraphView graphView)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }

                graphView.DeleteElements(port.connections);
            }
        }

        public Port GeneratePort(ARFNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Multi)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(int)); // dummy
        }

        //override the BuildContextualMenu method to prevent the "disconnect" option from appearing in the contextual menu
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
        }

        public abstract ObjectType GetElemType();
        public void MarkUnsaved()
        {
            if(savedIcon == null)
            {
                //the icon to add if the node does not correspond to an element in the server
                Texture2D warningImage = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Images/cloud.png", typeof(Texture2D));
                savedIcon = new Image
                {
                    image = warningImage,
                    scaleMode = ScaleMode.ScaleToFit
                };
                savedIcon.style.width = 25;
                savedIcon.style.height = 25;
                savedIcon.style.minWidth = 25;
                savedIcon.style.minHeight = 25;
                savedIcon.style.left = 8;
                savedIcon.style.paddingRight = 8;
                savedIcon.style.alignSelf = Align.Center;

            }
            if (!titleContainer.Contains(savedIcon))
            {
                titleContainer.Insert(0,savedIcon);
            }
            tooltip = "This element is not synchronized with the World Storage";
        }

        public void MarkSaved()
        {
            if (titleContainer.Contains(savedIcon))
            {
                titleContainer.Remove(savedIcon);
                tooltip = "";
            }
        }

    }
}