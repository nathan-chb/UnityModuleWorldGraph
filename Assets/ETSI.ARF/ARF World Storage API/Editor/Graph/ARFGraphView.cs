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

#define USING_OPENAPI_GENERATOR

using System.Collections.Generic;
#if USING_OPENAPI_GENERATOR
using Org.OpenAPITools.Model;
#else
using IO.Swagger.Api;
using IO.Swagger.Model;
#endif
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using System;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph;
using ETSI.ARF.WorldStorage.REST;
using UnityEditor;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using System.Linq;

namespace ETSI.ARF.WorldStorage.UI
{
    public class ARFGraphView : GraphView
    {
        public WorldStorageServer worldStorageServer;
        public WorldStorageUser worldStorageUser;

        public ARFGraphView()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            //deleSection
            deleteSelection += DeleteFunc;


            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

        }

        //method called when an element is deleted from the graphview
        public void DeleteFunc(string operationName, AskUser askUser)
        {
            //build the message to list all the deleted elements
            String message = "Are you sure you want to delete ";
            if (selection.Count > 1)
            {
                message += selection.Count + " elements ?";
            }
            else
            {
                message += "this element ?";
            }

            //remove from the graph all the deleted elements
            if (EditorUtility.DisplayDialog("Deleting elements", message, "Yes", "No"))
            {
                if (SaveInfo.instance.elemsToRemove == null)
                {
                    SaveInfo.instance.elemsToRemove = new Dictionary<string, Type>();
                }
                foreach (GraphElement elt in selection.ToArray())
                {
                    ARFNodeWorldAnchor nodeAnchor = elt as ARFNodeWorldAnchor;
                    if (nodeAnchor != null)
                    {
                        nodeAnchor.DisconnectAllPorts(this);
                        if (SaveInfo.instance.nodePositions.ContainsKey(nodeAnchor.GUID))
                        {
                            SaveInfo.instance.elemsToRemove.Add(nodeAnchor.GUID, typeof(WorldAnchor));
                        }
                        RemoveElement(elt);
                        continue;
                    }
                    ARFNodeTrackable nodeTrackable = elt as ARFNodeTrackable;
                    if (nodeTrackable != null)
                    {
                        nodeTrackable.DisconnectAllPorts(this);
                        if (SaveInfo.instance.nodePositions.ContainsKey(nodeTrackable.GUID))
                        {
                            SaveInfo.instance.elemsToRemove.Add(nodeTrackable.GUID, typeof(Trackable));
                        }
                        RemoveElement(elt);
                        continue;
                    }
                    ARFEdgeLink edgeLink = elt as ARFEdgeLink;
                    if (edgeLink != null)
                    {
                        edgeLink.input.Disconnect(edgeLink);
                        edgeLink.output.Disconnect(edgeLink);
                        if (SaveInfo.instance.linkIds.Contains(edgeLink.GUID))
                        {
                            SaveInfo.instance.elemsToRemove.Add(edgeLink.GUID, typeof(WorldLink));
                        }
                        RemoveElement(elt);
                        continue;
                    }
                }
            }

            GraphEditorWindow.ResetWindow();
        }

        public override void BuildContextualMenu(UnityEngine.UIElements.ContextualMenuPopulateEvent evt)
        {
            Vector2 localMousePos = evt.localMousePosition;
            Vector2 actualGraphPosition = viewTransform.matrix.inverse.MultiplyPoint(localMousePos);

            if (!(evt.target is ARFNode || evt.target is Group || evt.target is ARFEdgeLink))
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Save graph", delegate
                {
                    if (ServerAndLocalDifferent())
                    {
                        SaveInServer();
                    }
                }, (DropdownMenuAction a) => DropdownMenuAction.Status.Normal);
                evt.menu.AppendAction("Reload graph", delegate
                {
                    if (ServerAndLocalDifferent() && EditorUtility.DisplayDialog("Saving node positions", "The World Graph has been modified. \nWould you like to push the modifications to the server ?", "Yes", "No"))
                    {
                        SaveInServer();
                    }
                    Reload();
                    SaveInfo.instance.toReFrame = true;
                }, (DropdownMenuAction a) => DropdownMenuAction.Status.Normal);
                evt.menu.AppendAction("Create Trackable", delegate
                {
                    //generate the Trackables's attributes
                    EncodingInformationStructure trackableEncodingInformation = new EncodingInformationStructure(EncodingInformationStructure.DataFormatEnum.OTHER, "0");

                    List<float> localCRS = new();
                    for (int i = 0; i < 15; i++)
                    {
                        localCRS.Add(0);
                    }
                    localCRS.Add(1);

                    List<double> trackableSize = new();
                    for (int i = 0; i < 3; i++)
                    {
                        trackableSize.Add(0);
                    }

                    string name = "DefaultTrackable";

                    //trying to add number after default name
                    var defaultNodes = nodes.ToList().Where(node => node.title.StartsWith("DefaultTrackable"));
                    if (defaultNodes.Any())
                    {
                        for (int i = 0; i < defaultNodes.Count(); i++)
                        {
                            Debug.Log($"{i} : " + defaultNodes.ElementAt(i).title);
                            if (!(defaultNodes.Where(node => node.title.EndsWith((i + 1).ToString() + ")")).Any()))
                            {
                                name = name + " (" + (i + 1).ToString() + ")";
                                break;
                            }
                        }                 
                    }

                    Trackable trackable = new Trackable(Guid.NewGuid(), name, Guid.Parse(worldStorageUser.UUID), Trackable.TrackableTypeEnum.OTHER, trackableEncodingInformation, new byte[64], localCRS, UnitSystem.CM, trackableSize, new Dictionary<string, List<string>>());

                    selection.Clear();
                    var node = CreateTrackableNode(trackable, actualGraphPosition.x, actualGraphPosition.y);
                    node.MarkUnsaved();
                    GraphEditorWindow.ShowWindow((ARFNodeTrackable)node);

                }, (DropdownMenuAction a) => DropdownMenuAction.Status.Normal);
                evt.menu.AppendAction("Create World Anchor", delegate
                {
                //generate the worldAnchor attributes
                List<float> localCRS = new List<float>();
                    for (int i = 0; i < 15; i++)
                    {
                        localCRS.Add(0);
                    }
                    localCRS.Add(1);

                    List<double> worldAnchorSize = new List<double>();
                    for (int i = 0; i < 3; i++)
                    {
                        worldAnchorSize.Add(0);
                    }

                    string name = "DefaultWorldAnchor";

                    //trying to add number after default name
                    var defaultNodes = nodes.ToList().Where(node => node.title.StartsWith("DefaultWorldAnchor"));
                    if (defaultNodes.Any())
                    {
                        for (int i = 0; i < defaultNodes.Count(); i++)
                        {
                            if (!(defaultNodes.Where(node => node.title.EndsWith((i + 1).ToString() + ")")).Any()))
                            {
                                name = name + " (" + (i + 1).ToString() + ")";
                                break;
                            }
                        }
                    }

                    WorldAnchor anchor = new WorldAnchor(Guid.NewGuid(), name, Guid.Parse(worldStorageUser.UUID), localCRS, UnitSystem.CM, worldAnchorSize, new Dictionary<string, List<string>>());
                    
                    selection.Clear();
                    var node = CreateAnchorNode(anchor, actualGraphPosition.x, actualGraphPosition.y);
                    node.MarkUnsaved();
                    GraphEditorWindow.ShowWindow((ARFNodeWorldAnchor)node);

                }, (DropdownMenuAction a) => DropdownMenuAction.Status.Normal);
            }
            evt.menu.AppendSeparator();
            if (evt.target is ARFNode || evt.target is Group || evt.target is ARFEdgeLink)
            {
                evt.menu.AppendSeparator();
                evt.menu.AppendAction("Delete", delegate
                {
                    DeleteSelectionCallback(AskUser.AskUser);
                }, (DropdownMenuAction a) => canDeleteSelection ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled);
                evt.menu.AppendSeparator();
            }
        }

        public bool ServerAndLocalDifferent()
        {
            if ((SaveInfo.instance.elemsToRemove.Count != 0) || (SaveInfo.instance.elemsToUpdate.Count != 0))
            {
                return true;
            }
            foreach (ARFNode node in nodes)
            {
                float nodeX = node.GetPosition().x;
                float nodeY = node.GetPosition().y;
                if (!SaveInfo.instance.nodePositions.ContainsKey(node.GUID))
                {
                    return true;
                }
                else
                {
                    float dataX = SaveInfo.instance.nodePositions[node.GUID].x;
                    float dataY = SaveInfo.instance.nodePositions[node.GUID].y;
                    if ((nodeX != dataX) || (nodeY != dataY))
                    {
                        return true;
                    }
                }
            }
            foreach (ARFEdgeLink edge in edges)
            {
                if (!SaveInfo.instance.linkIds.Contains(edge.GUID))
                {
                    return true;
                }
            }
            return false;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var cPorts = new List<Port>();
            ports.ForEach(funcCall: port =>
           {
               if (startPort != port && startPort.node != port.node) cPorts.Add(port);
           });
            return cPorts;
        }

        public void PaintWorldStorage()
        {
            List<Trackable> trackables = TrackableRequest.GetAllTrackables(worldStorageServer);
            List<WorldAnchor> worldAnchors = WorldAnchorRequest.GetAllWorldAnchors(worldStorageServer);
            List<WorldLink> worldLinks = WorldLinkRequest.GetAllWorldLinks(worldStorageServer);

            foreach (WorldAnchor worldAnchor in worldAnchors)
            {
                var waNode = new ARFNodeWorldAnchor(worldAnchor);

                Rect posTemp = new(26, 93, 160, 77);
                SaveInfo.instance.nodePositions.TryGetValue(worldAnchor.UUID.ToString(), out posTemp);
                waNode.SetPosition(posTemp);

                AddElement(waNode);
            }

            foreach (Trackable trackable in trackables)
            {
                var tracknode = new ARFNodeTrackable(trackable);

                Rect posTemp = new(26, 93, 160, 77);
                SaveInfo.instance.nodePositions.TryGetValue(trackable.UUID.ToString(), out posTemp);
                tracknode.SetPosition(posTemp);

                AddElement(tracknode);
            }
            foreach (WorldLink worldLink in worldLinks)
            {
                var portPair = GetPortsFromWorldLink(worldLink);
                ARFEdgeLink edge = portPair.Key.ConnectTo<ARFEdgeLink>(portPair.Value);
                edge.worldLink = worldLink;
                edge.GUID = worldLink.UUID.ToString();

                AddElement(edge);
            }

        }

        internal ARFNodeTrackable CreateTrackableNode(Trackable track, float posX, float posY)
        {
            var tracknode = new ARFNodeTrackable(track);
            Rect pos = new Rect(RoundToNearestHalf(posX), RoundToNearestHalf(posY), 160, 77);
            tracknode.SetPosition(pos);

            AddElement(tracknode);
            return tracknode;
        }

        internal ARFNodeWorldAnchor CreateAnchorNode(WorldAnchor wa, float posX, float posY)
        {
            var waNode = new ARFNodeWorldAnchor(wa);

            Rect pos = new Rect(RoundToNearestHalf(posX), RoundToNearestHalf(posY), 160, 77);
            waNode.SetPosition(pos);

            AddElement(waNode);
            return waNode;
        }

        internal ARFEdgeLink CreateLink(WorldLink worldLink)
        {
            var portPair = GetPortsFromWorldLink(worldLink);
            ARFEdgeLink edge = portPair.Key.ConnectTo<ARFEdgeLink>(portPair.Value);
            edge.worldLink = worldLink;
            Debug.Log(worldLink.UUID.ToString());
            edge.GUID = worldLink.UUID.ToString();

            AddElement(edge);
            return edge;
        }

        public void Reload()
        {
            GraphEditorWindow.ResetWindow();
            DeleteElements(graphElements);
            SaveInfo.instance.InitNodePos(worldStorageServer, worldStorageUser);
            PaintWorldStorage();
            FrameAllElements();
        }

        public Dictionary<string, Rect> GetNodePositions()
        {
            Dictionary<string, Rect> ret = new Dictionary<string, Rect>();
            foreach (ARFNode elem in nodes)
            {
                ret.Add(elem.GUID, elem.GetPosition());
            }
            return ret;
        }

        private KeyValuePair<Port, Port> GetPortsFromWorldLink(WorldLink worldLink)
        {
            var ret = new KeyValuePair<Port, Port>();

            //To
            Guid idTo = worldLink.UUIDTo;
            Port portIn = null;
            switch (worldLink.TypeTo)
            {
                case ObjectType.Trackable:
                    foreach (GraphElement node in this.graphElements)
                    {
                        ARFNodeTrackable nodeTrackable = node as ARFNodeTrackable;
                        if ((nodeTrackable != null) && (nodeTrackable.trackable.UUID == idTo))
                        {
                            portIn = nodeTrackable.portIn;
                            break;
                        }
                    }
                    break;
                case ObjectType.WorldAnchor:
                    foreach (GraphElement node in this.graphElements)
                    {
                        ARFNodeWorldAnchor nodeAnchor = node as ARFNodeWorldAnchor;
                        if ((nodeAnchor != null) && nodeAnchor.worldAnchor.UUID == idTo)
                        {
                            portIn = nodeAnchor.portIn;
                            break;
                        }
                    }
                    break;
                default:
                    Debug.Log("what are you doing here...");
                    break;
            }

            //From
            Guid idFrom = worldLink.UUIDFrom;
            Port portOut = null;
            switch (worldLink.TypeFrom)
            {
                case ObjectType.Trackable:
                    foreach (GraphElement node in this.graphElements)
                    {
                        ARFNodeTrackable nodeTrackable = node as ARFNodeTrackable;
                        if ((nodeTrackable != null) && (nodeTrackable.trackable.UUID == idFrom))
                        {
                            portOut = nodeTrackable.portOut;
                            break;
                        }
                    }
                    break;
                case ObjectType.WorldAnchor:
                    foreach (GraphElement node in this.graphElements)
                    {
                        ARFNodeWorldAnchor nodeAnchor = node as ARFNodeWorldAnchor;
                        if ((nodeAnchor != null) && nodeAnchor.worldAnchor.UUID == idFrom)
                        {
                            portOut = nodeAnchor.portOut;
                            break;
                        }
                    }
                    break;
                default:
                    Debug.Log("what are you doing here...");
                    break;
            }

            if ((portOut != null) && (portIn != null))
            {
                ret = new KeyValuePair<Port, Port>(portOut, portIn);
            }

            return ret;
        }

        //
        // Résumé :
        //     Calculate the rectangle size and position to fit all elements in graph.
        //
        // Paramètres :
        //   container:
        //     This should be the view container.
        //
        // Retourne :
        //     The calculated rectangle.
        public override Rect CalculateRectToFitAll(VisualElement container)
        {
            Rect rectToFit = container.layout;
            bool reachedFirstChild = false;
            graphElements.ForEach(delegate (GraphElement ge)
            {
                if (!(ge is ARFEdgeLink) && !(ge is Port))
                {
                    if (!reachedFirstChild)
                    {
                        rectToFit = ge.ChangeCoordinatesTo(contentViewContainer, ge.contentRect);
                        reachedFirstChild = true;
                    }
                    else
                    {
                        rectToFit = RectUtils.Encompass(rectToFit, ge.ChangeCoordinatesTo(contentViewContainer, ge.contentRect));
                    }
                }
            });
            return rectToFit;
        }

        //k_FrameBorder is private readOnly graphView attribute, had to redeclare it to access it
        private readonly int k_FrameBorder = 30;
        public void FrameAllElements()
        {
            Vector3 frameTranslation = Vector3.zero;
            Vector3 frameScaling = Vector3.one;
            var rectToFit = CalculateRectToFitAll(contentViewContainer);
            CalculateFrameTransform(rectToFit, layout, k_FrameBorder, out frameTranslation, out frameScaling);
            Matrix4x4.TRS(frameTranslation, Quaternion.identity, frameScaling);
            UpdateViewTransform(frameTranslation, frameScaling);
        }

        //method to predict the position of a node (the float that will be saved in the PositionInfo singleton)
        public static float RoundToNearestHalf(float a)
        {
            return a = Mathf.Round(a * 2f) * 0.5f;
        }

        //Save all modified/deleted/added elements to the server
        public void SaveInServer()
        {
            //DELETE ELEMENTS FROM THE SERVER
            foreach (KeyValuePair<String, Type> elemToRemove in SaveInfo.instance.elemsToRemove)
            {
                string typeName = elemToRemove.Value.Name;
                switch (typeName)
                {
                    case nameof(Trackable):
                        TrackableRequest.DeleteTrackable(worldStorageServer, elemToRemove.Key);
                        break;
                    case nameof(WorldAnchor):
                        Debug.Log("delete worldanchor");
                        WorldAnchorRequest.DeleteWorldAnchor(worldStorageServer, elemToRemove.Key);
                        break;
                    case nameof(WorldLink):
                        WorldLinkRequest.DeleteWorldLink(worldStorageServer, elemToRemove.Key);
                        break;
                    default:
                        Debug.Log("oops");
                        break;
                }
            }

            //UPDATE AND ADD ELEMENTS
            foreach (ARFNode node in nodes)
            {
                if (!SaveInfo.instance.nodePositions.ContainsKey(node.GUID))
                {
                    //POST TRACKABLE
                    if (node is ARFNodeTrackable aRFNodeTrackable)
                    {
                        var posX = new List<String>();
                        posX.Add(aRFNodeTrackable.GetPosition().x.ToString());
                        var posY = new List<String>();
                        posY.Add(aRFNodeTrackable.GetPosition().y.ToString());
                        Trackable trackable = aRFNodeTrackable.trackable;
                        trackable.KeyvalueTags["unityAuthoringPosX"] = posX;
                        trackable.KeyvalueTags["unityAuthoringPosY"] = posY;
                        String uuid = TrackableRequest.AddTrackable(worldStorageServer, trackable);

                        //change the uuid in its edges, if there is a new edge to be added in the world storage it needs to have the correct uuid
                        uuid = uuid.Replace("\"", "");
                        foreach (ARFEdgeLink edge in aRFNodeTrackable.portIn.connections)
                        {
                            edge.worldLink.UUIDTo = Guid.Parse(uuid);
                        }
                        foreach (ARFEdgeLink edge in aRFNodeTrackable.portOut.connections)
                        {
                            edge.worldLink.UUIDFrom = Guid.Parse(uuid);
                        }
                        aRFNodeTrackable.trackable.UUID = Guid.Parse(uuid);
                        aRFNodeTrackable.GUID = uuid;
                        aRFNodeTrackable.title = trackable.Name;
                    }
                    //POST WORLDANCHOR
                    if (node is ARFNodeWorldAnchor aRFNodeWorldAnchor)
                    {
                        var posX = new List<String>();
                        posX.Add(aRFNodeWorldAnchor.GetPosition().x.ToString());
                        var posY = new List<String>();
                        posY.Add(aRFNodeWorldAnchor.GetPosition().y.ToString());
                        WorldAnchor worldAnchor = aRFNodeWorldAnchor.worldAnchor;
                        worldAnchor.KeyvalueTags["unityAuthoringPosX"] = posX;
                        worldAnchor.KeyvalueTags["unityAuthoringPosY"] = posY;

                        String uuid = WorldAnchorRequest.AddWorldAnchor(worldStorageServer, worldAnchor);

                        //change the uuid in its edges, if there is a new edge to be added in the world storage it needs to have the correct uuid
                        uuid = uuid.Replace("\"","");
                        foreach (ARFEdgeLink edge in aRFNodeWorldAnchor.portIn.connections)
                        {
                            edge.worldLink.UUIDTo = Guid.Parse(uuid);
                        }
                        foreach (ARFEdgeLink edge in aRFNodeWorldAnchor.portOut.connections)
                        {
                            edge.worldLink.UUIDFrom = Guid.Parse(uuid);
                        }
                        aRFNodeWorldAnchor.worldAnchor.UUID = Guid.Parse(uuid);
                        aRFNodeWorldAnchor.GUID = uuid;
                        aRFNodeWorldAnchor.title = worldAnchor.Name;
                    }
                }
                else
                {
                    float xLocal = node.GetPosition().x;
                    float yLocal = node.GetPosition().y;
                    float xServer = SaveInfo.instance.nodePositions[node.GUID].x; ;
                    float yServer = SaveInfo.instance.nodePositions[node.GUID].y;
                    if (((xLocal != xServer) || (yLocal != yServer)) || SaveInfo.instance.elemsToUpdate.Contains(node.GUID))
                    {
                        if(node is ARFNodeTrackable aRFNodeTrackable)
                        {
                            var posX = new List<String>();
                            posX.Add(aRFNodeTrackable.GetPosition().x.ToString());
                            var posY = new List<String>();
                            posY.Add(aRFNodeTrackable.GetPosition().y.ToString());
                            Trackable trackable = aRFNodeTrackable.trackable;
                            trackable.KeyvalueTags["unityAuthoringPosX"] = posX;
                            trackable.KeyvalueTags["unityAuthoringPosY"] = posY;
                            TrackableRequest.UpdateTrackable(worldStorageServer, trackable);
                            aRFNodeTrackable.title = trackable.Name;
                        }
                        if (node is ARFNodeWorldAnchor aRFNodeWorldAnchor)
                        {
                            var posX = new List<String>();
                            posX.Add(aRFNodeWorldAnchor.GetPosition().x.ToString());
                            var posY = new List<String>();
                            posY.Add(aRFNodeWorldAnchor.GetPosition().y.ToString());
                            WorldAnchor worldAnchor = aRFNodeWorldAnchor.worldAnchor;
                            worldAnchor.KeyvalueTags["unityAuthoringPosX"] = posX;
                            worldAnchor.KeyvalueTags["unityAuthoringPosY"] = posY;
                            WorldAnchorRequest.UpdateWorldAnchor(worldStorageServer, worldAnchor);
                            aRFNodeWorldAnchor.title = worldAnchor.Name;
                        }
                    }
                }
                node.MarkSaved();
            }
            foreach (ARFEdgeLink edge in edges)
            {
                if (edge is ARFEdgeLink aRFEdgeLink)
                {
                    if (!SaveInfo.instance.linkIds.Contains(aRFEdgeLink.GUID))
                    {
                        WorldLink worldLink = aRFEdgeLink.worldLink;
                        string uuid = WorldLinkRequest.AddWorldLink(worldStorageServer, worldLink);
                        uuid = uuid.Replace("\"", "");

                        aRFEdgeLink.worldLink.UUID = Guid.Parse(uuid);
                        aRFEdgeLink.GUID = uuid;
                    }
                    else if (SaveInfo.instance.elemsToUpdate.Contains(aRFEdgeLink.GUID))
                    {
                        WorldLink worldLink = aRFEdgeLink.worldLink;
                        WorldLinkRequest.UpdateWorldLink(worldStorageServer, worldLink);
                    }
                    aRFEdgeLink.MarkSaved();
                }
            }
            SaveInfo.instance.InitNodePos(worldStorageServer, worldStorageUser);

            GraphEditorWindow.ResetWindow();
        }
    }
}