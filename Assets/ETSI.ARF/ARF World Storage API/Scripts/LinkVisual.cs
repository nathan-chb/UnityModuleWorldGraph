using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETSI.ARF.WorldStorage.UI
{
    [ExecuteAlways]
    public class LinkVisual : MonoBehaviour
    {
        public GameObject fromElement, toElement;


        private void Update()
        {
#if UNITY_EDITOR
            if (fromElement != null && toElement != null) transform.position = (fromElement.transform.position + toElement.transform.position) / 2;
#endif
        }

        void OnDrawGizmos()
        {
#if UNITY_EDITOR
            if (fromElement != null && toElement != null)
            {
                // Draws a blue line from this transform to the target
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(fromElement.transform.position, toElement.transform.position);
            }
            else
            {
                Debug.Log("Rien à tracer");
            }
        }
#endif
    }
}