using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts;
using Dummiesman;
using Org.OpenAPITools.Model;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Scripts
{
    [ExecuteInEditMode]
    public class WorldAnchorScript : MonoBehaviour
    {
        [SerializeField]
        private WorldAnchor _worldAnchor;
        public WorldAnchor worldAnchor
        {
            get { return _worldAnchor; }
            set
            {
                _worldAnchor = value;
                string modelUrl = WorldAnchorScript.GetModelUrl(_worldAnchor);
                if ((modelUrl != "") && (model == null))
                {

                    //That's where we load the Model File, if it already extists, it's loaded from the file system, otherwise it's saved through an Importer

                    //It already exsits
                    if (File.Exists(GetFilePath(modelUrl)))
                    {
                        if (modelUrl.EndsWith(".obj"))
                        {
                            // Save the model into the anchor script attribute
                            model = new OBJLoader().Load(GetFilePath(modelUrl));
                        }
                        else if (modelUrl.EndsWith(".gltf"))
                        {
                            // Save the model into the anchor script attribute
                            model = Importer.LoadFromFile(GetFilePath(modelUrl));
                        }
                        if (model != null)
                        {
                            model.transform.parent = gameObject.transform;
                            model.tag = "EditorOnly";
                            model.name = GetFileName(modelUrl);
                            foreach (Transform child in model.transform)
                            {
                                child.hideFlags |= HideFlags.HideInHierarchy;
                            }
                        }
                    }
                    else
                    //It does not exist
                    {
                        StartCoroutine(LoadModel(modelUrl, (UnityWebRequest req) =>
                        {
                            if ((req.result == UnityWebRequest.Result.ConnectionError) || (req.result == UnityWebRequest.Result.ProtocolError))
                            {
                                // Log any errors that may happen
                                Debug.Log($"{req.error} : {req.downloadHandler.text}");
                            }
                            else
                            {
                                if (modelUrl.EndsWith(".obj"))
                                {
                                    // Save the model into the anchor script attribute
                                    model = new OBJLoader().Load(GetFilePath(modelUrl));
                                }
                                else if (modelUrl.EndsWith(".gltf"))
                                {
                                    // Save the model into the anchor script attribute
                                    model = Importer.LoadFromFile(GetFilePath(modelUrl));
                                }
                                if (model != null)
                                {
                                    model.transform.parent = gameObject.transform;
                                    model.tag = "EditorOnly";
                                    model.name = GetFileName(modelUrl);
                                    foreach (Transform child in model.transform)
                                    {
                                        child.hideFlags |= HideFlags.HideInHierarchy;
                                    }
                                }
                            }
                        }));
                    }
                }
            }
        }
        [HideInInspector]
        public WorldLink link;
        [HideInInspector]
        public bool modified;
        [HideInInspector]
        public GameObject model;

        Vector3 localPosition;
        Quaternion localRotation;
        Vector3 localScale;

        // Use this for initialization
        void Start()
        {
            transform.hasChanged = false;

            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                if (LocalTransfromHasChanged())
                {
                    modified = true;
                    GameObjectToLinkTransform();
                }
                localPosition = transform.localPosition;
                localRotation = transform.localRotation;
                localScale = transform.localScale;
            }

            //hide the model child elements in the hierarchy
            if (model != null)
            {
                foreach (Transform child in model.transform)
                {
                    child.hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }

        private bool LocalTransfromHasChanged()
        {
            if ((localPosition == transform.localPosition) && (localRotation == transform.localRotation) && (localScale == transform.localScale))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void GameObjectToLinkTransform()
        {
            if (transform.parent != null)
            {
                //get the positions relative to the parent
                Vector3 position = transform.localPosition;
                Quaternion rotation = transform.localRotation;
                Vector3 scale = transform.localScale;

                //create the matrix after 
                var worldLinktransform = Matrix4x4.TRS(position, rotation, scale);

                List<float> list = worldLinktransform.ExtractList();
                link.Transform = list;
            }
        }

        //gets the url from the keyvalue attribute
        public static string GetModelUrl(WorldAnchor worldAnchor)
        {
            worldAnchor.KeyvalueTags.TryGetValue("ModelURL", out List<string> list);
            if (list != null)
            {
                return list[0];
            }
            else
            {
                return "";
            }
        }

        public static IEnumerator LoadModel(string url, Action<UnityWebRequest> callback)
        {
            using UnityWebRequest req = UnityWebRequest.Get(url);
            req.downloadHandler = new DownloadHandlerFile(GetFilePath(url));
            yield return req.SendWebRequest();
            callback(req);
        }

        public static string GetFilePath(string url)
        {
            string filename = GetFileName(url);
            return $"{Application.dataPath}/3DModels/{filename}";
        }

        public static string GetFileName(string url)
        {
            string[] strings = null;
            if (url.Contains("\\"))
            {
                strings = url.Split("\\");
            }
            else if (url.Contains("/"))
            {
                strings = url.Split("/");
            }
            if (strings != null)
            {
                return strings[^1];
            }
            else
            {
                return "";
            }
        }
    }
}