using UnityEngine;

public class OctreeNode
{
    public OctreeNode[] children;
    public bool isLeaf;
    public bool isRoot;
    public int depth;
    public Vector3 position;
    public Vector3 scale;

    public bool isFull;
    public bool isEmpty;


    public static OctreeNode CreateRoot(int desiredDepth, Vector3 position, float scale) 
    {

        // Create the root
        OctreeNode root = new OctreeNode();
        root.scale = new Vector3(scale, scale, scale);
        root.position = position;
        root.isRoot = true;

        return root;  
    }

    public static OctreeNode CreateNode(int depth, int maxDepth)
    {
        Debug.Log(depth);
        // Create the node and set it's depth
        OctreeNode node = new OctreeNode();
        node.depth = depth;

        // If we're at maxDepth then the node is a leaf
        if (depth == maxDepth)
            node.isLeaf = true;

        return node;
    }
}