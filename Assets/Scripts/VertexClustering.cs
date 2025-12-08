using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class VertexClustering : MonoBehaviour
{

    private float cellSize;
    public int cellNumber;

    private Mesh mesh;
    private List<GameObject> cells = new List<GameObject>();
    private bool pendingRebuild;
    private Dictionary<int, List<Vector3>> cellVertices = new Dictionary<int, List<Vector3>>();
    private Dictionary<int, Vector3> cellAverage = new Dictionary<int, Vector3>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void RegenerateGrid()
    {
        // Mesh
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

        // Mesh infos
        mesh = meshFilter.sharedMesh;
        Vector3 meshSize = mesh.bounds.size;
        cellSize = Mathf.Max(meshSize.x, meshSize.y, meshSize.z)/ cellNumber;
        float offset = (cellNumber - 1) * cellSize / 2;

        // Reset
        pendingRebuild = false;
        foreach (GameObject cell in cells)
            DestroyImmediate(cell);

        cells.Clear();

        // N3 complexity (kinda shit)
        for (int i = 0; i < cellNumber; i++) {
            for (int j = 0; j < cellNumber; j++) {
                for (int k = 0; k < cellNumber; k++) {

                    // New Cell Creation
                    GameObject obj = new GameObject("Cell");
                    obj.transform.SetParent(transform, false);
                    cells.Add(obj);

                    // Collider (use for visualisation)
                    BoxCollider collider = obj.AddComponent<BoxCollider>();
                    collider.isTrigger = true;

                    // Transform
                    obj.transform.localScale = new Vector3(cellSize, cellSize, cellSize);
                    obj.transform.position = transform.position - new Vector3(k * cellSize - offset, j * cellSize - offset, i * cellSize - offset);

                }

            }
        }

        // Launch the Vertex average logic
        GetVertexPerCell();
        VertexAverage();
        AverageDebugSphere();
    }

    private void GetVertexPerCell()
    {
        // Initialisation
        Vector3[] vertices = mesh.vertices;
        cellVertices.Clear();
        float offset = (cellNumber - 1) * cellSize / 2; // TODO Make it member ?


        // To world space // TODO Do I really need two loops ? couldn't I do that in the same loop
        for (int i = 0; i < vertices.Length; i++)
            vertices[i] = transform.TransformPoint(vertices[i]); // TODO Should I really modify the vertices directly ?

        for (int i = 0; i < vertices.Length; i++) { 
            Vector3 pos = vertices[i];

            // Use vertex position to compute flatten index
            Vector3 local = pos - transform.position + new Vector3(offset, offset, offset);
            int xIndex = Mathf.Clamp((int)(local.x / cellSize), 0, cellNumber - 1);
            int yIndex = Mathf.Clamp((int)(local.y / cellSize), 0, cellNumber - 1);
            int zIndex = Mathf.Clamp((int)(local.z / cellSize), 0, cellNumber - 1);
            int index = xIndex + (yIndex * cellNumber) + zIndex * (cellNumber * cellNumber);

            // Store vertex
            if (!cellVertices.ContainsKey(index))
                cellVertices[index] = new List<Vector3>();

            cellVertices[index].Add(pos);
        }
    }

    private void VertexAverage()
    {

        cellAverage.Clear();

        foreach (var (key, vertices) in cellVertices) {
         
            // Early exit if no vertices in the cell
            if (vertices.Count == 0)
                continue;

            // Compute average vertex
            Vector3 sum = Vector3.zero;
            foreach (Vector3 vec in vertices)
                sum += vec;

            cellAverage[key] = sum / vertices.Count;
        }

    }

    private void AverageDebugSphere()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if (cellAverage.TryGetValue(i, out Vector3 avg))
            {

                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.transform.localScale = Vector3.one * 0.05f;
                marker.transform.position = avg;
                marker.transform.SetParent(cells[i].transform);
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
