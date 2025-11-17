using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VoxelShape : MonoBehaviour
{
    [Header("Shape values")]
    public float sphereRadius;
    // TODO Could add a sphereCenter that set to the root node pos

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
        // Loop through all children
        for (int i = 0; i < parentNode.children.Length; i++) {
            OctreeNode childNode = parentNode.children[i];

            // Compute the scale and position of the node
            float factor = 1 / Mathf.Pow(2, childNode.depth);
            childNode.position = parentNode.position + (posArray[i] * factor);
            childNode.scale = parentNode.scale * .5f;

            // TODO Improve the function : should take directly the node as argument and test wheter it's completely inside, partially inside or not inside at all
            isInsideSphere(childNode);

            // If the node is a leaf or is full create the mesh
            // Else if the node is not fully empty, create it's children recursively
            if (childNode.isLeaf && childNode.isFull) // TODO Right now it's an && because isFull is partially implemanted, but in the future isLeaf will be removed from the cond
                SpawnMesh(childNode);
            else if (!childNode.isEmpty && !childNode.isLeaf) // TODO should I just skip leaf at the start of the function it would be less aribtrary than putting that here
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
        obj.transform.localScale = node.scale;
        obj.transform.position = node.position;
        
    }

    private void isInsideSphere(OctreeNode node) 
    { 
        node.isFull = node.position.sqrMagnitude <= sphereRadius * sphereRadius; 
    }
}
