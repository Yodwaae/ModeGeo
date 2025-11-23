using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum VoxelMode {
    Union,
    Intersection,
    WIPUnionMinusIntersectionWIP,
}

public class VoxelShape : MonoBehaviour
{
    [Header("Shape values")]
    public VoxelMode mode;
    public float sphereRadius;
    public Vector3 boundingBoxPos = Vector3.one; // NOTE Will effectively serve as sphere center
    public float boundingBoxScale = 1; // NOTE Using a single float so the scale is uniform and the voxels are cubes
    // TODO Precise it's local or world pos and modify distance to center distance accodringly
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

    // TODO I'm sure there a better implementation to find but for the moment it's working !
    private void RenderVoxelShape(OctreeNode parentNode)
    {
        // If the node is a leaf makes an ealy exit
        if (parentNode.isLeaf)
            return;

        if (parentNode.isRoot) {
            isInsideSphere(parentNode);
            if (parentNode.isFull){
                SpawnMesh(parentNode);
                return;
            }
        }
            

        // Loop through all children
        for (int i = 0; i < 8; i++) {
            OctreeNode childNode = OctreeNode.CreateNode(parentNode.depth + 1, desiredDepth);

            // Compute the scale and position of the node
            float factor = 1 / Mathf.Pow(2, childNode.depth);
            childNode.position = parentNode.position + (posArray[i] * factor);
            childNode.scale = parentNode.scale * .5f;

            // Check if the node is inside a sphere
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

            // Check the corner against each sphere
            foreach (Vector3 sphereCenter in sphereCenters) {

                // Compute corner distance to sphere center
                Vector3 cornerPos = (node.position + posArray[i] * node.scale.x); // FIXME/TODO Right now sphereCenter isn't taken into account
                float sqrDist = (cornerPos - sphereCenter).sqrMagnitude;

                // If the dist. to the corner is smaller than the radius then it's inside the sphere
                if (sqrDist <= sqrRadius)
                    cornerIsInside = true;

                // If we're in intersection mode as soon as the corner is not in a sphere, set the flag back to false and exit
                if (mode == VoxelMode.Intersection && sqrDist > sqrRadius){
                    cornerIsInside = false;
                    break;
                }
            }
 

            // If one corner is inside then obviously all corners are not outside
            // Idem with the reverse
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
