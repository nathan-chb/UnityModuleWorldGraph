using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts;
using Org.OpenAPITools.Model;
using Siccity.GLTFUtility;
using System;
using System.Collections;
using System.Collections.Generic;
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
                    StartCoroutine(LoadModel(modelUrl, (UnityWebRequest req) =>
                    {
                        if ((req.result == UnityWebRequest.Result.ConnectionError) || (req.result == UnityWebRequest.Result.ProtocolError))
                        {
                            // Log any errors that may happen
                            Debug.Log($"{req.error} : {req.downloadHandler.text}");
                        }
                        else
                        {
                            // Save the model into the anchor script attribute
                            model = Importer.LoadFromFile(GetFilePath(modelUrl));
                            model.transform.parent = gameObject.transform;
                            model.tag = "EditorOnly";
                            model.name = GetFileName(modelUrl);
                            foreach (Transform child in model.transform)
                            {
                                child.hideFlags |= HideFlags.HideInHierarchy;
                            }
                        }
                    }));
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
            if(transform.parent != null)
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