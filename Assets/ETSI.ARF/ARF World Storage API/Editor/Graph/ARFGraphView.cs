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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;

namespace ETSI.ARF.WorldStorage.UI
{
    public class ARFGraphView : GraphView
    {
        public ARFGraphView()
        {
            //GridBackground back = new GridBackground();
            //back.StretchToParentSize();
            //Insert(0, back);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            AddElement(GenerateEntryPointNode());
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var cPorts = new List<Port>();
            ports.ForEach (funcCall: port =>
            {
                if (startPort != port && startPort.node != port.node) cPorts.Add(port);
            });
            return cPorts;
        }

        private Port GeneratePort (ARFNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Multi)
        {
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(int)); // dummy
        }

        private ARFNode GenerateEntryPointNode()
        {
            var node = new ARFNode
            {
                title = "World Storage",
                text = "EntryPoint",
                GUID = Guid.NewGuid().ToString(),
                entryPoint = true
            };

            var portOut = GeneratePort(node, Direction.Output);
            portOut.portName = "Link";
            node.outputContainer.Add(portOut);

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(50, 100, 200, 150));
            return node;
        }

        public void CreateNode(string name)
        {
            AddElement(CreateARFNode(name));
        }

        public ARFNode CreateARFNode(string name)
        {
            var node = new ARFNode
            {
                title = name,
                text = name,
                GUID = Guid.NewGuid().ToString()
            };

            var portIn = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            portIn.portName = "Link"; // "Input";
            node.inputContainer.Add(portIn);

            var portOut = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
            portOut.portName = "Link"; // "Output";
            node.outputContainer.Add(portOut);

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(200, 100, 200, 150));

            return node;
        }
    }
}