using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VertexClustering : MonoBehaviour
{

    private int gridSize;
    public float cellSize;

    private List<GameObject> cells = new List<GameObject>();
    private bool pendingRebuild;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void RegenerateGrid()
    {

        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

        // Mesh infos
        Mesh mesh = meshFilter.sharedMesh;
        Vector3 meshSize = mesh.bounds.size;

        gridSize = Mathf.CeilToInt(Mathf.Max(meshSize.x, meshSize.y, meshSize.z) / cellSize);

        // TODO Compute gridsize by dividing the mesh size by the cell size (start with the greatest between width, length, depth; then I'll do one for each)
        // TODO Base myself on the mesh centroid and go in both direction ?


        // Reset
        pendingRebuild = false;
        foreach (GameObject cell in cells)
            DestroyImmediate(cell);

        // N3 complexity (kinda shit)
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                for (int k = 0; k < gridSize; k++) {

                    // New Cell Creation
                    GameObject obj = new GameObject("Cell");
                    obj.transform.SetParent(transform, false);
                    cells.Add(obj);

                    // Collider
                    BoxCollider collider = obj.AddComponent<BoxCollider>();
                    collider.isTrigger = true;

                    // Transform
                    obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    obj.transform.position = transform.position + new Vector3(k * cellSize, j * cellSize, i * cellSize) - (meshSize/4);

                }

            }
        }
    }

    private void OnValidate()
    {         
        // In edit mode, schedule regeneration OUTSIDE OnValidate (otherwise we can't use destroyImmediate)
        if (!Application.isPlaying && !pendingRebuild) {
            pendingRebuild = true;
            EditorApplication.delayCall += RegenerateGrid;
        }
    }

}
