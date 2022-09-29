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

    public static void InstantiateGraph(ARFGraphView graphview)
    {
        //delete old GO
        //delete Elements prefab
        foreach (var pref in FindElementsPrefabInstances())
        {
            DestroyImmediate(pref);
        }

        //extract all roots elem
        List<ARFNode> roots = GetRoots(graphview);

        //instantiate GO from root elem
        foreach (ARFNode root in roots)
        {
            GenerateTreeFromRoot(root, Matrix4x4.identity, null);
        }
    }

    internal static void PutAtTop(ARFNode node)
    {
        var gameObject = GameObject.Find(node.title);
        if(gameObject != null)
        {
            gameObject.transform.parent = null;
        }
    }

    private static void GenerateTreeFromRoot(ARFNode root, Matrix4x4 transform, GameObject parent)
    {
        //get the parent world Link
        WorldLink parentLink = null;
        if (root.portIn.connected)
        {
            parentLink = ((ARFEdgeLink)root.portIn.connections.ElementAt(0)).worldLink;
        }
        //check if root is WA or Trackable
        //create the root GO
        GameObject newGameObject;
        if (root is ARFNodeTrackable)
        {
            newGameObject = InstantiateTrackableGO(((ARFNodeTrackable)root).trackable, parentLink, transform, parent);
        }
        else if(root is ARFNodeWorldAnchor)
        {
            newGameObject = InstantiateWorldAnchorGO(((ARFNodeWorldAnchor)root).worldAnchor, parentLink, transform, parent);
        }
        else
        {
            newGameObject = null;
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
            GenerateTreeFromRoot(childNode, matrix, newGameObject);

        }
    }

    //Since we're manipulating trees, this method checks if the node has parents, if it doesn't, it's a root
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

    public static GameObject InstantiateTrackableGO(Trackable trackable, WorldLink parentLink, Matrix4x4 transform, GameObject parent)
    {
        GameObject prefab = GameObject.Find(trackable.Name);
        if(prefab == null)
        {
            //Create the visual of the trackable gameObject
            switch (trackable.TrackableType)
            {
                case Trackable.TrackableTypeEnum.FIDUCIALMARKER:
                    prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableFiducial.prefab", typeof(GameObject));
                    break;
                case Trackable.TrackableTypeEnum.IMAGEMARKER:
                    prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableImage.prefab", typeof(GameObject));
                    break;
                case Trackable.TrackableTypeEnum.GEOPOSE:
                    prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableGeoPose.prefab", typeof(GameObject));
                    break;
                case Trackable.TrackableTypeEnum.MAP:
                    prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackablePointCloud.prefab", typeof(GameObject));
                    break;
                case Trackable.TrackableTypeEnum.OTHER:
                    prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableOther.prefab", typeof(GameObject));
                    break;
            }

            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab as GameObject);
            go.name = trackable.Name;
            if (parent != null)
            {
                go.transform.parent = parent.transform;
            }

            //hide the prefab elements in the hierarchy
            foreach (Transform child in go.transform)
            {
                child.hideFlags = HideFlags.HideInHierarchy;
            }

            go.transform.localScale = transform.lossyScale;
            go.transform.rotation = transform.rotation;
            go.transform.position = transform.GetPosition();

            var trackScript = (TrackableScript)go.GetComponent<TrackableScript>();
            trackScript.trackable = trackable;
            trackScript.link = parentLink;
            return go;
        }
        else
        {
            prefab.name = trackable.Name;
            if (parent != null)
            {
                prefab.transform.parent = parent.transform;
            }
            prefab.transform.localScale = transform.lossyScale;
            prefab.transform.rotation = transform.rotation;
            prefab.transform.position = transform.GetPosition();
            var myComponent = (TrackableScript)prefab.GetComponent<TrackableScript>();
            myComponent.trackable = trackable;
            myComponent.link = parentLink;
            return prefab;
        }       
    }

    public static GameObject InstantiateWorldAnchorGO(WorldAnchor worldAnchor, WorldLink parentLink, Matrix4x4 transform, GameObject parent)
    {
        GameObject prefab = GameObject.Find(worldAnchor.Name);
        if (prefab == null)
        {
            prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFWorldAnchor.prefab", typeof(GameObject));
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab as GameObject);
            go.name = worldAnchor.Name;
            if(parent != null)
            {
                go.transform.parent = parent.transform;
            }
            foreach (Transform child in go.transform)
            {
                child.hideFlags = HideFlags.HideInHierarchy;
            }
            go.transform.localScale = transform.lossyScale;
            go.transform.rotation = transform.rotation;
            go.transform.position = transform.GetPosition();
            var myComponent = (WorldAnchorScript)go.GetComponent<WorldAnchorScript>();
            myComponent.worldAnchor = worldAnchor;
            myComponent.link = parentLink;
            return go;
        }
        else
        {
            prefab.name = worldAnchor.Name;
            if (parent != null)
            {
                prefab.transform.parent = parent.transform;
            }
            prefab.transform.localScale = transform.lossyScale;
            prefab.transform.rotation = transform.rotation;
            prefab.transform.position = transform.GetPosition();
            var myComponent = (WorldAnchorScript)prefab.GetComponent<WorldAnchorScript>();
            myComponent.worldAnchor = worldAnchor;
            myComponent.link = parentLink;
            return prefab;
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

    public static List<GameObject> FindElementsPrefabInstances()
    {
        UnityEngine.Object trackablePrefabFiducial = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableFiducial.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabImage = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableImage.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabGeoPose = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableGeoPose.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabPointCloud = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackablePointCloud.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabOther = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableOther.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefab = AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackable.prefab", typeof(GameObject));
        UnityEngine.Object worldAnchorPrefab = AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFWorldAnchor.prefab", typeof(GameObject));
        List<GameObject> result = new List<GameObject>();
        GameObject[] allObjects = (GameObject[])FindObjectsOfType(typeof(GameObject));
        foreach (GameObject GO in allObjects)
        {
            if (PrefabUtility.GetPrefabType(GO) == PrefabType.PrefabInstance)
            {
                UnityEngine.Object GO_prefab = EditorUtility.GetPrefabParent(GO);
                if ((trackablePrefab == GO_prefab) || (trackablePrefabFiducial == GO_prefab) || (trackablePrefabImage == GO_prefab) || (trackablePrefabGeoPose == GO_prefab) || (trackablePrefabPointCloud == GO_prefab) || (trackablePrefabOther == GO_prefab) || (worldAnchorPrefab == GO_prefab))
                {
                    result.Add(GO);
                }
            }
        }
        return result;
    }
    public static List<GameObject> FindElementsPrefabInstancesInChilds(GameObject parent)
    {
        UnityEngine.Object trackablePrefabFiducial = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableFiducial.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabImage = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableImage.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabGeoPose = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableGeoPose.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabPointCloud = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackablePointCloud.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefabOther = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableOther.prefab", typeof(GameObject));
        UnityEngine.Object trackablePrefab = AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackable.prefab", typeof(GameObject));
        UnityEngine.Object worldAnchorPrefab = AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFWorldAnchor.prefab", typeof(GameObject));
        List<GameObject> result = new List<GameObject>(); 
        foreach (Transform transform in parent.transform)
        {
            var GO = transform.gameObject;
            if (EditorUtility.GetPrefabType(GO) == PrefabType.PrefabInstance)
            {
                UnityEngine.Object GO_prefab = EditorUtility.GetPrefabParent(GO);
                if ((trackablePrefab == GO_prefab) || (trackablePrefabFiducial == GO_prefab) || (trackablePrefabImage == GO_prefab) || (trackablePrefabGeoPose == GO_prefab) || (trackablePrefabPointCloud == GO_prefab) || (trackablePrefabOther == GO_prefab) || (worldAnchorPrefab == GO_prefab))
                {
                    result.Add(GO);
                }
            }
        }
        return result;
    }

    public static void DeleteGO(string name)
    {
        var toDelete = GameObject.Find(name);
        DestroyImmediate(toDelete);
    }

    public static void MoveGO(String parentName, String elemName, Matrix4x4 transform)
    {
        //Get both gameObjects from names
        var parent = GameObject.Find(parentName);
        var elem = GameObject.Find(elemName);
        if (elem != null)
        {
            if (parent != null)
            {
                if (elem.transform.parent != parent.transform)
                {
                    elem.transform.parent = parent.transform;
                }

                //Get the parent position
                Matrix4x4 parentPos = parent.transform.localToWorldMatrix;

                //Add the transform to the parent position
                Vector3 position = parentPos.ExtractPosition() + transform.ExtractPosition();
                Quaternion rotation = parentPos.ExtractRotation() * transform.ExtractRotation();
                Vector3 scale = Vector3.Scale(parentPos.ExtractScale(), transform.ExtractScale());

                //create the matrix after 
                var matrix = Matrix4x4.TRS(position, rotation, scale);

                //Move the game object to the new position
                {
                    elem.transform.localScale = matrix.lossyScale;
                    elem.transform.SetPositionAndRotation(matrix.GetPosition(), matrix.rotation);
                }
            }
            else
            {
                //Move the game object to the new position
                elem.transform.localScale = transform.lossyScale;
                elem.transform.SetPositionAndRotation(transform.GetPosition(), transform.rotation);
            }
        }
    }

    public static Matrix4x4 ListToMatrix4x4(List<float> list)
    {
        Matrix4x4 localCRS = new Matrix4x4();
        localCRS.m00 = list[0]; localCRS.m01 = list[1]; localCRS.m02 = list[2]; localCRS.m03 = list[3];
        localCRS.m10 = list[4]; localCRS.m11 = list[5]; localCRS.m12 = list[6]; localCRS.m13 = list[7];
        localCRS.m20 = list[8]; localCRS.m21 = list[9]; localCRS.m22 = list[10]; localCRS.m23 = list[11];
        localCRS.m30 = list[12]; localCRS.m31 = list[13]; localCRS.m32 = list[14]; localCRS.m33 = list[15];

        return localCRS;
    }

    public static void ChangeTrackableType(Trackable trackable)
    {
        GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableOther.prefab", typeof(GameObject)); ;
        switch (trackable.TrackableType)
        {
            case Trackable.TrackableTypeEnum.FIDUCIALMARKER:
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableFiducial.prefab", typeof(GameObject));
                break;
            case Trackable.TrackableTypeEnum.IMAGEMARKER:
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableImage.prefab", typeof(GameObject));
                break;
            case Trackable.TrackableTypeEnum.GEOPOSE:
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackableGeoPose.prefab", typeof(GameObject));
                break;
            case Trackable.TrackableTypeEnum.MAP:
                prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/ETSI.ARF/ARF World Storage API/Prefabs/ARFTrackablePointCloud.prefab", typeof(GameObject));
                break;
            default:
                break;
        }

        //find the gameObject related to the trackable
        var oldGameObject = GameObject.Find(trackable.Name);
        var oldTrackScript = (TrackableScript)oldGameObject.GetComponent<TrackableScript>();

        //create the new gameObject
        GameObject newGameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab as GameObject);
        //hide the prefab elements in the hierarchy
        foreach (Transform child in newGameObject.transform)
        {
            child.hideFlags = HideFlags.HideInHierarchy;
        }
        var newTrackScript = (TrackableScript)newGameObject.GetComponent<TrackableScript>();

        //give it all its attributes
        newTrackScript.trackable = oldTrackScript.trackable;
        newTrackScript.link = oldTrackScript.link;
        newGameObject.transform.position = oldGameObject.transform.position;
        newGameObject.transform.rotation = oldGameObject.transform.rotation;

        //put it at the same place in the hierarchy
        newGameObject.transform.parent = oldGameObject.transform.parent;
        foreach (var child in SceneBuilder.FindElementsPrefabInstancesInChilds(oldGameObject))
        {
            child.transform.parent = newGameObject.transform;
        }

        //finaly, delete the old gameObject & change the name of the new GameObejct
        SceneBuilder.DeleteGO(trackable.Name);
        newGameObject.name = trackable.Name;
    }
}
