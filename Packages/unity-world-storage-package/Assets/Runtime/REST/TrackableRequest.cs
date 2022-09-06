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
    public class TrackableRequest
    {
        static public string AddTrackable(WorldStorageServer ws, Trackable trackable)
        {
            Debug.Log("Posting Add Trackable to Server");
            TrackablesApi api = new TrackablesApi(ws.URI);
            string result = api.AddTrackable(trackable);
            Debug.Log(result);
            return result;
        }

        static public string UpdateTrackable(WorldStorageServer ws, Trackable trackable)
        {
            Debug.Log("Posting Add Trackable to Server");
            TrackablesApi api = new TrackablesApi(ws.URI);
            string result = api.ModifyTrackable(trackable);
            Debug.Log(result);
            return result;
        }

        static public List<Trackable> GetAllTrackables(WorldStorageServer ws)
        {
            TrackablesApi api = new TrackablesApi(ws.URI);
            List<Trackable> result = api.GetTrackables();
            return result;
        }

        static public Trackable GetTrackable(WorldStorageServer ws, string uuid)
        {
            System.Guid _uuid = System.Guid.Parse(uuid);
            TrackablesApi api = new TrackablesApi(ws.URI);
            Trackable result = api.GetTrackableById(_uuid);
            return result;
        }

        static public void DeleteTrackable(WorldStorageServer ws, string uuid)
        {
            System.Guid _uuid = System.Guid.Parse(uuid);
            TrackablesApi api = new TrackablesApi(ws.URI);
            api.DeleteTrackable(_uuid);
        }
    }
}
#endif