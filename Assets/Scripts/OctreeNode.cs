using UnityEngine;

public class OctreeNode
{
    public OctreeNode[] children;
    public bool isLeaf;
    public int depth;

    public static OctreeNode CreateRoot(int desiredDepth) {
        Debug.Log("Started Tree Creation");
        return CreateNode(0, desiredDepth); 
    }

    private static OctreeNode CreateNode(int depth, int maxDepth)
    {
        Debug.Log(depth);

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