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
    public class WorldLinkRequest
    {
        static public string AddWorldLink(WorldStorageServer ws, WorldLink link)
        {
            Debug.Log("Posting Add Trackable to Server");
            WorldLinksApi api = new WorldLinksApi(ws.URI);
            string result = api.AddWorldLink(link);
            Debug.Log(result);
            return result;
        }

        static public string UpdateWorldLink(WorldStorageServer ws, WorldLink link)
        {
            Debug.Log("Posting Add Trackable to Server");
            WorldLinksApi api = new WorldLinksApi(ws.URI);
            string result = api.ModifyWorldLink(link);
            Debug.Log(result);
            return result;
        }

        static public List<WorldLink> GetAllWorldLinks(WorldStorageServer ws)
        {
            WorldLinksApi api = new WorldLinksApi(ws.URI);
            List<WorldLink> result = api.GetWorldLinks();
            return result;
        }

        static public WorldLink GetWorldLink(WorldStorageServer ws, string uuid)
        {
            System.Guid _uuid = System.Guid.Parse(uuid);
            WorldLinksApi api = new WorldLinksApi(ws.URI);
            WorldLink result = api.GetWorldLinkById(_uuid);
            return result;
        }

        static public void DeleteWorldLink(WorldStorageServer ws, string uuid)
        {
            System.Guid _uuid = System.Guid.Parse(uuid);
            WorldLinksApi api = new WorldLinksApi(ws.URI);
            api.DeleteWorldLink(_uuid);
        }
    }
}
#endif