using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts;
using Org.OpenAPITools.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Scripts
{
    [ExecuteInEditMode]
    public class TrackableScript : MonoBehaviour
    {
        [SerializeField]
        public Trackable trackable;
        [HideInInspector]
        public WorldLink link;
        [HideInInspector]
        public bool modified;

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
                GameObjectToLinkTransform();
                if (LocalTransfromHasChanged())
                {
                    GameObjectToLinkTransform();
                    modified = true;
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
    }
}