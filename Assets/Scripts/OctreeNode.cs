using UnityEngine;

public class OctreeNode
{
    public OctreeNode[] children;
    public bool isLeaf;
    public int depth;
    public Vector3 position = Vector3.zero; // TODO Do I leave this here as default values or should I set it in create root ?
    public Vector3 scale = Vector3.one;

    public bool isFull;
    public bool isEmpty;

    public static OctreeNode CreateRoot(int desiredDepth) { return CreateNode(0, desiredDepth); }

    private static OctreeNode CreateNode(int depth, int maxDepth)
    {
        // Create the node and set it's depth
        OctreeNode node = new OctreeNode();
        node.depth = depth;

        // If we're at maxDepth then the node is a leaf, in that case returns it as is
        if (depth == maxDepth) {
            node.isLeaf = true;
            return node;
        }

        // Else the node it's just a classic node then recursively create it's children before returning it
        node.children = new OctreeNode[8];
        for (int i = 0; i < 8; i++)
            node.children[i] = CreateNode(depth + 1, maxDepth);

        return node;
    }
}