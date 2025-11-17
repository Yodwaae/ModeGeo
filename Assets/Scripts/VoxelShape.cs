using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VoxelShape : MonoBehaviour
{
    [Header("Tree values")]
    private OctreeNode treeRoot;
    [SerializeField] private int desiredDepth;

    [Header("Voxel values")]
    [SerializeField] private Mesh cube;
    [SerializeField] private Material material;
    private Vector3[] posArray = { 
        new Vector3(.5f, .5f , .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f),
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(.5f, -.5f , .5f),
        new Vector3(-.5f, -.5f , .5f),
        new Vector3(.5f, -.5f , -.5f),
        new Vector3(-.5f, -.5f , -.5f),
    };

    private void Awake() { treeRoot = OctreeNode.CreateRoot(desiredDepth); }

    private void Start() { RenderVoxelShape(treeRoot); }

    private void RenderVoxelShape(OctreeNode parentNode)
    {
        for (int i = 0; i < parentNode.children.Count(); i++) {
            OctreeNode childNode = parentNode.children[i];

            float factor = 1 / Mathf.Pow(2, childNode.depth);
            childNode.mPosition = parentNode.mPosition + (posArray[i] * factor);
            childNode.mScale = parentNode.mScale * .5f;

            if (childNode.isLeaf)
                SpawnMesh(childNode);
            else
                RenderVoxelShape(childNode);

        }
    }


    private void SpawnMesh(OctreeNode node)
    {
        // Create a new game obj for the voxel, then adds a mesh filter and renderer to it
        GameObject obj = new GameObject("Voxel");
        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();


        // Set the voxel mesh and material
        filter.sharedMesh = cube;
        renderer.sharedMaterial = material;

        // Scale and place
        obj.transform.localScale = node.mScale;
        obj.transform.position = node.mPosition;
        
    }
}
