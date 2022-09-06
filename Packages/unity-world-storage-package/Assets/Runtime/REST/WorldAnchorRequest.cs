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

#define USING_OPENAPI_GENERATOR // alt. is Swagger

using System.IO;
using System.Collections.Generic;
using UnityEngine;

#if USING_OPENAPI_GENERATOR
using Org.OpenAPITools.Api;
using Org.OpenAPITools.Model;
#else
using IO.Swagger.Api;
using IO.Swagger.Model;
#endif

#if UNITY_EDITOR
namespace ETSI.ARF.WorldStorage.REST
{
    public class WorldAnchorRequest
    {
        static public string AddWorldAnchor(WorldStorageServer ws, WorldAnchor anchor)
        {
            Debug.Log("Posting Add World Anchor to Server");
            WorldAnchorsApi api = new WorldAnchorsApi(ws.URI);
            string result = api.AddWorldAnchor(anchor);
            Debug.Log(result);
            return result;
        }

        static public string UpdateWorldAnchor(WorldStorageServer ws, WorldAnchor anchor)
        {
            Debug.Log("Posting Add World Anchor to Server");
            WorldAnchorsApi api = new WorldAnchorsApi(ws.URI);
            string result = api.ModifyWorldAnchor(anchor);
            Debug.Log(result);
            return result;
        }

        static public List<WorldAnchor> GetAllWorldAnchors(WorldStorageServer ws)
        {
            WorldAnchorsApi api = new WorldAnchorsApi(ws.URI);
            List<WorldAnchor> result = api.GetWorldAnchors();
            return result;
        }

        static public WorldAnchor GetWorldAnchor(WorldStorageServer ws, string uuid)
        {
            System.Guid _uuid = System.Guid.Parse(uuid);
            WorldAnchorsApi api = new WorldAnchorsApi(ws.URI);
            WorldAnchor result = api.GetWorldAnchorById(_uuid);
            return result;
        }

        static public void DeleteWorldAnchor(WorldStorageServer ws, string uuid)
        {
            System.Guid _uuid = System.Guid.Parse(uuid);
            WorldAnchorsApi api = new WorldAnchorsApi(ws.URI);
            api.DeleteWorldAnchor(_uuid);
        }
    }
}
#endif