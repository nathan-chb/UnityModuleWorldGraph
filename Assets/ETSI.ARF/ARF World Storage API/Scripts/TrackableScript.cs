using Org.OpenAPITools.Model;
using System.Collections;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Scripts
{
    [ExecuteInEditMode]
    public class TrackableScript : MonoBehaviour
    {
        [SerializeField]
        public Trackable trackable;
        [HideInInspector]
        public 

        // Use this for initialization
        void Start()
        {
            transform.hasChanged = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                print("The transform has changed!");
            }
        }
    }

}