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
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph
{
    public class ARFPort : Port
    {
        protected ARFPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }

        public override void Connect(Edge edge)
        {
            base.Connect(edge);
            ARFNode fromNode = edge.output.node as ARFNode;
            ARFNode toNode = edge.input.node as ARFNode;

            if (edge is ARFEdgeLink aRFedge)
            {
                if (aRFedge.worldLink == null)
                {
                    List<float> transform = new List<float>();
                    transform.Add(1);
                    for (int i = 1; i < 5; i++)
                    {
                        transform.Add(0);
                    }
                    transform.Add(1);
                    for (int i = 6; i < 10; i++)
                    {
                        transform.Add(0);
                    }
                    transform.Add(1);
                    for (int i = 11; i < 15; i++)
                    {
                        transform.Add(0);
                    }
                    transform.Add(1);

                    WorldLink worldLink = new(Guid.NewGuid(), Guid.Parse(UtilGraphSingleton.instance.worldStorageUser.UUID), Guid.Parse(fromNode.GUID), Guid.Parse(toNode.GUID), fromNode.GetElemType(), toNode.GetElemType(), transform, UnitSystem.CM, new Dictionary<string, List<string>>());
                    aRFedge.worldLink = worldLink;
                    aRFedge.GUID = worldLink.UUID.ToString();
                    aRFedge.viewDataKey = worldLink.UUID.ToString();
                }
                else
                {
                    aRFedge.worldLink.UUIDFrom = Guid.Parse(fromNode.GUID);
                    aRFedge.worldLink.TypeFrom = fromNode.GetElemType();

                    aRFedge.worldLink.UUIDTo = Guid.Parse(toNode.GUID);
                    aRFedge.worldLink.TypeTo = toNode.GetElemType();
                }

                //update scene
                String parentName = aRFedge.output.node.title;
                String elemName = aRFedge.input.node.title;
                Matrix4x4 transformMatrix = SceneBuilder.ListToMatrix4x4(aRFedge.worldLink.Transform);
                SceneBuilder.MoveGO(parentName, elemName, transformMatrix);


                //update game object
                GameObject prefab = GameObject.Find(elemName);
                if (prefab != null)
                {
                    var trackablePrefab = (TrackableScript)prefab.GetComponent<TrackableScript>();
                    var worldAnchorPrefab = (WorldAnchorScript)prefab.GetComponent<WorldAnchorScript>();
                    if (trackablePrefab != null)
                    {
                        trackablePrefab.link = aRFedge.worldLink;
                    }
                    else if (worldAnchorPrefab != null)
                    {
                        worldAnchorPrefab.link = aRFedge.worldLink;
                    }
                    else
                    {
                        Debug.Log("Not supposed to be here");
                    }
                }
            }
        }



        public static ARFPort CreateARF<TEdge>(Orientation orientation, Direction direction, Capacity capacity, Type type) where TEdge : Edge, new()
        {
            WorldLinkListener listener = new WorldLinkListener();
            ARFPort port = new(orientation, direction, capacity, type)
            {
                m_EdgeConnector = new EdgeConnector<TEdge>(listener)
            };
            port.AddManipulator(port.m_EdgeConnector);
            return port;
        }
    }
}