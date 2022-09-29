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

using UnityEngine;

#if USING_OPENAPI_GENERATOR
using Org.OpenAPITools.Model;
#else
using IO.Swagger.Api;
using IO.Swagger.Model;
#endif
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;

namespace ETSI.ARF.WorldStorage.UI
{
    public class ARFNodeWorldAnchor : ARFNode
    {
        public WorldAnchor worldAnchor;

        public ARFNodeWorldAnchor(WorldAnchor worldAnchor)
        {
            this.worldAnchor = worldAnchor;
            this.GUID = worldAnchor.UUID.ToString();
            this.viewDataKey = GUID;
            this.title = worldAnchor.Name;

            /*COLOR*/
            var colorRectangle = new VisualElement();
            colorRectangle.style.height = 160;
            colorRectangle.style.height = 5;
            colorRectangle.style.backgroundColor = new Color(1, 0.7f, 0, 0.9f);
            mainContainer.Insert(1, colorRectangle);

            /*PORTS*/
            var portIn = GeneratePort(this, Direction.Input, Port.Capacity.Single);
            portIn.portColor = new Color(0.66f, 0.39f, 1, 0.77f);
            portIn.portName = "Target"; // "Input";
            //portIn.AddManipulator(new EdgeConnector<ARFEdgeLink>(new WorldLinkListener()));
            inputContainer.Add(portIn);

            var portOut = GeneratePort(this, Direction.Output, Port.Capacity.Multi);
            portOut.portColor = new Color(0.66f, 0.39f, 1, 0.77f);
            portOut.portName = "Source"; // "Output";
            //portOut.AddManipulator(new EdgeConnector<ARFEdgeLink>(new WorldLinkListener()));
            outputContainer.Add(portOut);

            RefreshExpandedState();
            RefreshPorts();

            /*MANIPULATOR*/
            var doubleClickManipulator = new Clickable(Clicked);
            doubleClickManipulator.activators.Clear();
            doubleClickManipulator.activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse, clickCount = 2 });
            this.AddManipulator(doubleClickManipulator);
        }

        public void Clicked()
        {
            GraphEditorWindow.ShowWindow(this);
        }
        public override ObjectType GetElemType()
        {
            return ObjectType.WorldAnchor;
        }
    }
}