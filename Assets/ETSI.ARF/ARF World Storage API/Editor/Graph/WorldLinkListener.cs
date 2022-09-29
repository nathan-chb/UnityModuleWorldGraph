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
using ETSI.ARF.WorldStorage.UI;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph
{
    public class WorldLinkListener : IEdgeConnectorListener
    {
        private GraphViewChange m_GraphViewChange;

        private List<Edge> m_EdgesToCreate;

        private List<GraphElement> m_EdgesToDelete;

        public WorldLinkListener()
        {
            m_EdgesToCreate = new List<Edge>();
            m_EdgesToDelete = new List<GraphElement>();
            m_GraphViewChange.edgesToCreate = m_EdgesToCreate;
        }
        public void OnDrop(GraphView graphView, Edge edge)
        {
            m_EdgesToCreate.Clear();
            m_EdgesToCreate.Add(edge);
            m_EdgesToDelete.Clear();
            if (edge.input.capacity == Capacity.Single)
            {
                foreach (Edge connection in edge.input.connections)
                {
                    if (connection != edge)
                    {
                        m_EdgesToDelete.Add(connection);
                    }
                }
            }

            if (edge.output.capacity == Capacity.Single)
            {
                foreach (Edge connection2 in edge.output.connections)
                {
                    if (connection2 != edge)
                    {
                        m_EdgesToDelete.Add(connection2);
                    }
                }
            }

            if (m_EdgesToDelete.Count > 0)
            {
                graphView.DeleteElements(m_EdgesToDelete);
            }

            List<Edge> edgesToCreate = m_EdgesToCreate;
            if (graphView.graphViewChanged != null)
            {
                edgesToCreate = graphView.graphViewChanged(m_GraphViewChange).edgesToCreate;
            }

            foreach (Edge item in edgesToCreate)
            {
                graphView.AddElement(item);
                edge.input.Connect(item);
                edge.output.Connect(item);
            }
            if (!UtilGraphSingleton.instance.linkIds.Contains(((ARFEdgeLink)edge).GUID))
            {
                ((ARFEdgeLink)edge).MarkUnsaved();
            }
            GraphEditorWindow.ShowWindow((ARFEdgeLink)edge);
            //if the edge was previously connected to another node, move that node in the scene hierarchy and put it at 0,0,0
            if (((ARFEdgeLink)edge).originalDestinationNode != null)
            {
                var gameObject = GameObject.Find(((ARFEdgeLink)edge).originalDestinationNode.title);
                gameObject.transform.parent = null;
                SceneBuilder.MoveGO(null, gameObject.name, Matrix4x4.identity);

                //mark it as modified
                ((ARFEdgeLink)edge).MarkUnsaved();
                UtilGraphSingleton.instance.elemsToUpdate.Add(((ARFEdgeLink)edge).GUID);
            }
            ((ARFEdgeLink)edge).originalDestinationNode = (ARFNode)edge.input.node;
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            ;
        }
    }
}