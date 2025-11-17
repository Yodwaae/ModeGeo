using UnityEngine;

public class OctreeNode
{
    public OctreeNode[] children;
    public bool isLeaf;
    public int depth;
    public Vector3 position;
    public Vector3 scale;

    public bool isFull;
    public bool isEmpty;

    public static OctreeNode CreateRoot(int desiredDepth, Vector3 position, float scale) 
    {

        // Create the root
        OctreeNode root = CreateNode(0, desiredDepth);
        root.scale = new Vector3(scale, scale, scale);
        root.position = position;

        return root;  
    }

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