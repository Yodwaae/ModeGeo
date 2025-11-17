using UnityEngine;

public class VoxelShape : MonoBehaviour
{
    [SerializeField] private Mesh cube;
    [SerializeField] private Material material;
    [SerializeField] private int desiredDepth;
    private OctreeNode treeRoot;

    private void Awake() { treeRoot = OctreeNode.CreateRoot(desiredDepth); }

    private void Start()
    {
        RenderVoxelShape(treeRoot);
    }

    private void RenderVoxelShape(OctreeNode node)
    {
        foreach (OctreeNode childNode in treeRoot.children)
        {
            if (childNode.isLeaf)
            {
                int subdivisionSize = (int)Mathf.Pow(2, childNode.depth);
                SpawnMesh(new Vector3(subdivisionSize, subdivisionSize, subdivisionSize));
            }
            else
                RenderVoxelShape(childNode);
        }
    }

    private void SpawnMesh(Vector3 coords)
    {
        // Create a new game obj for the voxel, then adds a mesh filter and renderer to it
        GameObject obj = new GameObject("Voxel");
        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        // Set the voxel mesh and material, then place it at the correct coords
        filter.sharedMesh = cube;
        renderer.sharedMaterial = material;
        obj.transform.position = coords;
        
    }
}
// .5 .5 .5
// -.5 .5 .5
// .5 .5 -.5
// -.5 .5 -.5
// .5 -.5 .5
// -.5 -.5 .5
// .5 -.5 -.5
// -.5 -.5 -.5