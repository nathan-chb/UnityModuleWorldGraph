using Org.OpenAPITools.Model;
using UnityEditor;
using UnityEngine;

namespace Assets.ETSI.ARF.ARF_World_Storage_API.Scripts
{
    [ExecuteInEditMode]
    public class WorldAnchorScript : MonoBehaviour
    {
        [SerializeField]
        public WorldAnchor worldAnchor;

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
                //TODO change the link trasform
                print("The transform has changed!");
                transform.hasChanged = false;
            }
        }
    }
}