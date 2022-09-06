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
    public class AdminRequest
    {
        static public string GetAdminInfo(WorldStorageServer ws)
        {
            DefaultApi api = new DefaultApi(ws.URI);
            string state = api.GetAdmin();
            Debug.Log("Server State: " + state);
            return state;
        }

        static public string GetVersion(WorldStorageServer ws)
        {
            DefaultApi api = new DefaultApi(ws.URI);
            string vers = api.GetVersion();
            Debug.Log("Using API Version: " + vers);
            return vers;
        }

        static public string Ping (WorldStorageServer ws)
        {
            DefaultApi api = new DefaultApi(ws.URI);
            api.GetPing();
            return "IsAlive";
        }
    }
}
#endif