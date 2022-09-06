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
using UnityEditor;

namespace ETSI.ARF.WorldStorage
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "ARFWorldStorageServer", menuName = "ARF World Storage/Create Server", order = 1)]
    public class WorldStorageServer : ScriptableObject
    {
        [SerializeField] public string serverName = "myServerName";
        [SerializeField] public string company = "";
        [SerializeField] public string basePath = "https://";
        [SerializeField] public int port = 8080;

        [Space(8)]
        [SerializeField] public WorldStorageUser currentUser = null;

        public string URI => basePath + ":" + port.ToString();
    }
}