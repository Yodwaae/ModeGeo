using UnityEngine;

public class VoxelShape : MonoBehaviour
{
    [SerializeField] private int desiredDepth;
    [SerializeField] private OctreeNode treeRoot;

    private void Awake() { treeRoot = OctreeNode.CreateRoot(desiredDepth); }
}