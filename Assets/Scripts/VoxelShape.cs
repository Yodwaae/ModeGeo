using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VoxelShape : MonoBehaviour
{
    [Header("Shape values")]
    public float sphereRadius;
    public Vector3 boundingBoxPos = Vector3.one; // NOTE Will effectively serve as sphere center
    public float boundingBoxScale = 1; // NOTE Using a single float so the scale is uniform and the voxels are cubes
    [Tooltip("The number of entry in this array determines the numbers of sphere, the vector 3 determines it's center")] public Vector3[] sphereCenters;


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
        // If the node is a leaf makes an ealy exit
        if (parentNode.isLeaf) 
            return;

        // Loop through all children
        for (int i = 0; i < parentNode.children.Length; i++) {
            OctreeNode childNode = parentNode.children[i];

            // Compute the scale and position of the node
            float factor = 1 / Mathf.Pow(2, childNode.depth);
            childNode.position = parentNode.position + (posArray[i] * factor);
            childNode.scale = parentNode.scale * .5f;

            // TODO Improve the function : should take directly the node as argument and test wheter it's completely inside, partially inside or not inside at all
            isInsideSphere(childNode);

            // If the node is full create the mesh
            // Else if the node is not fully empty, create it's children recursively
            if (childNode.isFull)
                SpawnMesh(childNode);
            else if (!childNode.isEmpty)
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
        // NOTE We used squared distance to avoid using sqrt which can be slower

        // Initialisation
        bool allCornersInside = true;
        bool allCornersOutside = true;
        float sqrRadius = sphereRadius * sphereRadius;

        // Loop through each corners of the voxel to see if they are inside the sphere
        for (int i = 0; i < 8; i++) {

            // Loop Initialisation
            bool cornerIsInside = false;

            // TODO COM Check for each spheres
            for (int j = 0; j < sphereCenters.Length; j++) { // TODO For each instead ?
                // Compute distance between node corner and sphere center
                float sqrDist = ((node.position + posArray[i] * node.scale.x) - sphereCenters[j]).sqrMagnitude;  // TODO Clean this mess of a computation

                // If the dist. to the corner is smaller than the radius then it's inside the sphere
                if (sqrDist <= sqrRadius)
                    cornerIsInside = true;
            }

            // If one corner is inside then all corners are not outisde
            // Else if the conrer is outside all corners are not inside
            if (cornerIsInside)
                allCornersOutside = false;
            else
                allCornersInside = false;
        }

        // If all corners are inside/outisde the sphere, the voxel is full/empty
        node.isFull = allCornersInside;
        node.isEmpty = allCornersOutside;
    }
}
