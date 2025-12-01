using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VertexClustering : MonoBehaviour
{

    private int cellSize;
    public int cellNumber;

    private List<GameObject> cells = new List<GameObject>();
    private bool pendingRebuild;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void RegenerateGrid()
    {
        // Mesh
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

        // Mesh infos
        Mesh mesh = meshFilter.sharedMesh;
        Vector3 meshSize = mesh.bounds.size;
        float cellSize = Mathf.Max(meshSize.x, meshSize.y, meshSize.z)/ cellNumber;
        float offset = (cellNumber - 1) * cellSize / 2;

        // Reset
        pendingRebuild = false;
        foreach (GameObject cell in cells)
            DestroyImmediate(cell);

        // N3 complexity (kinda shit)
        for (int i = 0; i < cellNumber; i++) {
            for (int j = 0; j < cellNumber; j++) {
                for (int k = 0; k < cellNumber; k++) {

                    // New Cell Creation
                    GameObject obj = new GameObject("Cell");
                    obj.transform.SetParent(transform, false);
                    cells.Add(obj);

                    // Collider
                    BoxCollider collider = obj.AddComponent<BoxCollider>();
                    collider.isTrigger = true;

                    // Transform
                    obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    obj.transform.position = transform.position - new Vector3(k * cellSize - offset, j * cellSize - offset, i * cellSize - offset);

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
