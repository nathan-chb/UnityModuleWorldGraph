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

using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using ETSI.ARF.WorldStorage;
using ETSI.ARF.WorldStorage.REST;
using ETSI.ARF.WorldStorage.UI;
using Org.OpenAPITools.Model;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows
{
    public class UtilGraphSingleton : ScriptableSingleton<UtilGraphSingleton>
    {
        [SerializeField]
        public Dictionary<String, Rect> nodePositions;
        public List<String> linkIds;

        public Dictionary<String,Type> elemsToRemove;
        public List<String> elemsToUpdate;

        //keep the info of the graph reframe
        public Boolean toReFrame = false;

        public WorldStorageServer worldStorageServer;
        public WorldStorageUser worldStorageUser;

        public void InitNodePos(WorldStorageServer server, WorldStorageUser user)
        {
            worldStorageServer = server;
            worldStorageUser = user;

            instance.nodePositions = new Dictionary<string, Rect>();
            foreach (Trackable track in TrackableRequest.GetAllTrackables(worldStorageServer))
            {
                if (track.KeyvalueTags.ContainsKey("unityAuthoringPosX") && track.KeyvalueTags.ContainsKey("unityAuthoringPosY"))
                {

                    float posY = 0;
                    float posX = 0;
                    //Debug.Log(track.Name + " : Hard written X : " + track.KeyvalueTags["unityAuthoringPosX"][0]);
                    if (float.TryParse(track.KeyvalueTags["unityAuthoringPosX"][0], out var parseX))
                    {
                        //Debug.Log(track.Name + " : Parsed X : " + parseX);
                        posX = RoundToNearestHalf(parseX);
                        //Debug.Log(track.Name + " : Rounded X : " + posX);
                    }
                    //Debug.Log(track.Name + " : Hard written Y : " + track.KeyvalueTags["unityAuthoringPosY"][0]);
                    if (float.TryParse(track.KeyvalueTags["unityAuthoringPosY"][0], out var parseY))
                    {
                        //Debug.Log(track.Name + " : Parsed Y : " + parseY);
                        posY = RoundToNearestHalf(parseY);
                        //Debug.Log(track.Name + " : Rounded Y : " + posY);
                    }
                    Rect trackPos = new(posX, posY, 135, 77);
                    instance.nodePositions[track.UUID.ToString()] = trackPos;
                }
                else
                {
                    Rect trackPos = new(0, 0, 135, 77);
                    instance.nodePositions[track.UUID.ToString()] = trackPos;
                }
            }
            foreach (WorldAnchor wa in WorldAnchorRequest.GetAllWorldAnchors(worldStorageServer))
            {
                if (wa.KeyvalueTags.ContainsKey("unityAuthoringPosX") && wa.KeyvalueTags.ContainsKey("unityAuthoringPosY"))
                {

                    float posY = 0;
                    float posX = 0;
                    //Debug.Log(wa.Name + " : Hard written X : " + wa.KeyvalueTags["unityAuthoringPosX"][0]);
                    if (float.TryParse(wa.KeyvalueTags["unityAuthoringPosX"][0], out var parseX))
                    {
                        //Debug.Log(wa.Name + " : Parsed Y : " + parseX);
                        posX = RoundToNearestHalf(parseX);
                        //Debug.Log(wa.Name + " : Rounded Y : " + posX);
                    }
                    //Debug.Log(wa.Name + " : Hard written Y : " + wa.KeyvalueTags["unityAuthoringPosX"][0]);
                    if (float.TryParse(wa.KeyvalueTags["unityAuthoringPosY"][0], out var parseY))
                    {
                        //Debug.Log(wa.Name + " : Parsed Y : " + parseX);
                        posY = RoundToNearestHalf(parseY);
                        //Debug.Log(wa.Name + " : Rounded Y : " + posX);
                    }
                    Rect waPos = new(posX, posY, 135, 77);
                    instance.nodePositions[wa.UUID.ToString()] = waPos;
                }
                else
                {
                    Rect trackPos = new(0, 0, 135, 77);
                    instance.nodePositions[wa.UUID.ToString()] = trackPos;
                }
            }

            instance.linkIds = new List<string>();
            foreach (WorldLink link in WorldLinkRequest.GetAllWorldLinks(worldStorageServer))
            {
                instance.linkIds.Add(link.UUID.ToString());
            }

            instance.elemsToRemove = new Dictionary<string, Type>();
            instance.elemsToUpdate = new List<string>();
        }

        //method to predict the position of a node (the float that will be saved in the PositionInfo singleton)
        public static float RoundToNearestHalf(float a)
        {
            return _ = Mathf.Round(a * 2f) * 0.5f;
        }

        public static void SynchronizeWithGameObjects(ARFGraphView graph)
        {

            //loop all corresponding go
            foreach (var gameObject in SceneBuilder.FindElementsPrefabInstances())
            {
                //check on the script of the game obejct (either trackable or worldanchor)
                var trackableScript = (TrackableScript)gameObject.GetComponent<TrackableScript>();
                var worldAnchorScript = (WorldAnchorScript)gameObject.GetComponent<WorldAnchorScript>();
                if (trackableScript != null)
                {
                    //if it's modified, mark it as unsafe
                    if ((trackableScript.modified == true) && (trackableScript.link != null)){
                        UtilGraphSingleton.instance.elemsToUpdate.Add(trackableScript.link.UUID.ToString());

                        //get the corresponding edge
                        var edge = graph.GetEdgeByGuid(trackableScript.link.UUID.ToString());
                        ((ARFEdgeLink)edge).MarkUnsaved();
                    }
                }
                else if(worldAnchorScript != null)
                {
                    if ((worldAnchorScript.modified == true) && (worldAnchorScript.link != null))
                    {
                        UtilGraphSingleton.instance.elemsToUpdate.Add(worldAnchorScript.link.UUID.ToString());

                        //get the corresponding edge
                        var edge = graph.GetEdgeByGuid(worldAnchorScript.link.UUID.ToString());
                        ((ARFEdgeLink)edge).MarkUnsaved();
                    }
                }
                else
                {
                    throw (new Exception("no script in this gameObject"));
                }
            }
        }
    }
}