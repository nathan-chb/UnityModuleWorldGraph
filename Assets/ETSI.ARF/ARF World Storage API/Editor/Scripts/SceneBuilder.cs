using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using ETSI.ARF.WorldStorage.UI;
using Org.OpenAPITools.Model;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Windows;
using Assets.ETSI.ARF.ARF_World_Storage_API.Scripts;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using System;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Graph;
using Assets.ETSI.ARF.ARF_World_Storage_API.Editor.Scripts;

public class SceneBuilder : MonoBehaviour
{
    // Reference to the Prefab. Drag a Prefab into this field in the Inspector.
    public GameObject myPrefab;


    // Start is called before the first frame update
    void Start()
    {
    }

    [MenuItem("Examples/Instantiate trackable")]
    static void InstantiatePrefab()
    {
        UnityEngine.Object prefab = AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackable.prefab", typeof(GameObject));
        PrefabUtility.InstantiatePrefab(prefab as GameObject);
    }

    public static void InstantiateGraph(ARFGraphView graphview)
    {
        //TODO delete old GO
        var objects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject o in objects)
        {
            DestroyImmediate(o.gameObject);
        }

        //TODO extract all roots elem
        List<ARFNode> roots = GetRoots(graphview);

        //TODO instantiate GO from root elem
        foreach(ARFNode root in roots)
        {
            GenerateTreeFromRoot(root, Matrix4x4.identity);
        }

        /*
        foreach (ARFNodeTrackable node in graphview.nodes)
        {
            Debug.Log(node.trackable.Name + " " + node.trackable.UUID.ToString());
            UtilGraphSingleton.instance.elemsToUpdate.Add(node.trackable.UUID.ToString());
            node.trackable.Name = "nathan";
            node.title = "nathan";
        }*/
    }

    private static void GenerateTreeFromRoot(ARFNode root, Matrix4x4 transform)
    {
        //check if root is WA or Trackable
        //create the root GO
        if(root is ARFNodeTrackable)
        {
            InstantiateTrackableGO(((ARFNodeTrackable)root).trackable, transform);
        }
        else if(root is ARFNodeWorldAnchor)
        {
            InstantiateWorldAnchorGO(((ARFNodeWorldAnchor)root).worldAnchor, transform);
        }
        else
        {
            Debug.Log("Pas censé être là");
        }

        //go in the childs
        foreach (var child in root.portOut.connections)
        {
            //fetch the transfrom
            var wl = ((ARFEdgeLink)child).worldLink;

            //List<float> to mAtrix4x4
            Matrix4x4 edgeTransfrom = new();
            edgeTransfrom.m00 = wl.Transform[0]; edgeTransfrom.m01 = wl.Transform[1]; edgeTransfrom.m02 = wl.Transform[2]; edgeTransfrom.m03 = wl.Transform[3];
            edgeTransfrom.m10 = wl.Transform[4]; edgeTransfrom.m11 = wl.Transform[5]; edgeTransfrom.m12 = wl.Transform[6]; edgeTransfrom.m13 = wl.Transform[7];
            edgeTransfrom.m20 = wl.Transform[8]; edgeTransfrom.m21 = wl.Transform[9]; edgeTransfrom.m22 = wl.Transform[10]; edgeTransfrom.m23 = wl.Transform[11];
            edgeTransfrom.m30 = wl.Transform[12]; edgeTransfrom.m31 = wl.Transform[13]; edgeTransfrom.m32 = wl.Transform[14]; edgeTransfrom.m33 = wl.Transform[15];


            //add it to the one in arg
            Vector3 position = transform.ExtractPosition() + edgeTransfrom.ExtractPosition();
            Quaternion rotation = transform.ExtractRotation() * edgeTransfrom.ExtractRotation();
            Vector3 scale = Vector3.Scale(transform.ExtractScale(), edgeTransfrom.ExtractScale()) ;

            //create the matrix after 
            var matrix = Matrix4x4.TRS(position, rotation, scale);

            //get the node and recursive call with the updtaed position
            var childNode = (ARFNode)child.input.node;
            GenerateTreeFromRoot(childNode, matrix);

        }
    }

    //Since we're manipulating trees, checks if the node has parents, if it doesn't, it's a root
    private static List<ARFNode> GetRoots(ARFGraphView graphview)
    {
        List<ARFNode> rootList = new();
        List<string> visited = new();
        List<Node> graphElements = new List<Node>();
        graphview.nodes.ToList(graphElements);


        foreach (ARFNode node in graphElements)
        {
            if (!visited.Contains(node.GUID))
            {
                if (!node.portIn.connected)
                {
                    rootList.Add(node);
                    visited = VisitChildren(node, visited);
                }
                visited.Add(node.GUID);
            }
        }
        return rootList;
    }

    private static List<string> VisitChildren(ARFNode node, List<string> visited)
    {
        foreach (var child in node.portOut.connections)
        {
            var childNode = (ARFNode)child.input.node;
            if (!visited.Contains(childNode.GUID))
            {
                visited.Add(childNode.GUID);
                visited = VisitChildren(childNode, visited);
            }
        }
        return visited;
    }

    public static void InstantiateTrackableGO(Trackable trackable, Matrix4x4 transform)
    {
        GameObject prefab = GameObject.Find(trackable.UUID.ToString());
        if(prefab == null)
        {
            prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackable.prefab", typeof(GameObject));
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab as GameObject);
            go.name = trackable.Name;
            go.transform.localScale = transform.lossyScale;
            go.transform.rotation = transform.rotation;
            go.transform.position = transform.GetPosition();
            var myComponent = (TrackableScript)go.GetComponent<TrackableScript>();
            myComponent.trackable = trackable;
        }       
    }

    static void InstantiateWorldAnchorGO(WorldAnchor worldAnchor, Matrix4x4 transform)
    {
        GameObject prefab = GameObject.Find(worldAnchor.UUID.ToString());
        if (prefab == null)
        {
            prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFWorldAnchor.prefab", typeof(GameObject));
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab as GameObject);
            go.name = worldAnchor.Name;
            go.transform.localScale = transform.lossyScale;
            go.transform.rotation = transform.rotation;
            go.transform.position = transform.GetPosition();
            var myComponent = (WorldAnchorScript)go.GetComponent<WorldAnchorScript>();
            myComponent.worldAnchor = worldAnchor;
        }
    }


    // Update is called once per frame
    void Update()
    {
        GameObject visual = GameObject.Find("test");
        if (visual == null)
        {
            Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            visual.name = "test";
        }
        else
        {
            Debug.Log("oops");
        }
    }
     public GameObject GenerateAndUpdateVisual(string UUID, string name, Vector3 pos, Vector3 rot)
        {

            //create the parentgameobject 
            GameObject arf = GameObject.Find("Graph");
            if (arf == null) arf = new GameObject("Graph");

            GameObject visual = GameObject.Find(UUID);

            if (visual == null)
        {
                Instantiate(myPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                visual.name = UUID;
            }
            else
            {
                visual.transform.SetPositionAndRotation(pos, Quaternion.Euler(rot));
            }
            visual.transform.Find("Canvas/Text").GetComponent<TextMeshProUGUI>().text = $"Name: { name }\nUUID: { UUID }";
            return visual;
        }
}
