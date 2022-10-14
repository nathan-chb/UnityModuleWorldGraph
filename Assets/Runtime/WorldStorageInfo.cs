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

using ETSI.ARF.WorldStorage;
using UnityEngine;

public class WorldStorageInfo : MonoBehaviour
{
    public WorldStorageServer worldStorageServer;

    public bool isServerAlive()
    {
        if (worldStorageServer == null) return false;
        return !string.IsNullOrEmpty(ETSI.ARF.WorldStorage.REST.AdminRequest.Ping(worldStorageServer));
    }

    public string GetServerState()
    {
        if (worldStorageServer == null) return "No Server Defined!";
        return ETSI.ARF.WorldStorage.REST.AdminRequest.GetAdminInfo(worldStorageServer);
    }

    public string GetAPIVersion()
    {
        if (worldStorageServer == null) return "Unknown Version!";
        return ETSI.ARF.WorldStorage.REST.AdminRequest.GetVersion(worldStorageServer);
    }
}
