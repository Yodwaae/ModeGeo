using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VoxelShape : MonoBehaviour
{
    [Header("Shape values")]
    public float sphereRadius;
    public Vector3 boundingBoxPos; // NOTE Will effectively serve as sphere center
    public float boundingBoxScale; // NOTE Using a float so the scale is uniform and the voxels are cubes

    [Header("Tree values")]
    private OctreeNode treeRoot;
    [SerializeField, Range(0, 5)] private int desiredDepth;

    [Header("Voxel values")]
    [SerializeField] private Mesh cube;
    [SerializeField] private Material material;
    private Vector3[] posArray = new Vector3[8]; 
    private Vector3[] cornerPosArray = { 
        new Vector3(.5f, .5f , .5f),
        new Vector3(-.5f, .5f, .5f),
        new Vector3(.5f, .5f, -.5f),
        new Vector3(-.5f, .5f, -.5f),
        new Vector3(.5f, -.5f , .5f),
        new Vector3(-.5f, -.5f , .5f),
        new Vector3(.5f, -.5f , -.5f),
        new Vector3(-.5f, -.5f , -.5f),
    };

    private void Awake() 
    { 
        // Create the root
        treeRoot = OctreeNode.CreateRoot(desiredDepth, boundingBoxPos, boundingBoxScale);

        // Create the posArray for the voxel based on corners pos and voxel scale
        for (int i = 0; i < 8; i++)
            posArray[i] = cornerPosArray[i] * boundingBoxScale;
    }

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

    // TODO Implement the empty, and make it so a voxel is tag full only if fully inside the sphere
    private void isInsideSphere(OctreeNode node)
    {
        // NOTE We used squared distance to have abs values and avoid using sqrt which can be slower

        // Compute distance between node center and sphere center
        float sqrDist = (node.position - boundingBoxPos).sqrMagnitude;
        // Compute the distance from the center of the sphere to its outer boundary
        float sqrRadius = sphereRadius * sphereRadius;

        // If the distance to the center of the node is greater than the distance to the outer boundary
        // then the voxel is, at least partially, inside the sphere
        node.isFull = sqrDist <= sqrRadius;
        
        
        // node.isEmpty = sqrDist > sqrRadius;
    }

}
