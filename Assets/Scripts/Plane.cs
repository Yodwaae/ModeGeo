using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class NewMonoBehaviourScript : MonoBehaviour
{

    // TODO I really need to find how to add triangles both way without creating lighting problems

    [Header("Custom Shape")]
    public int height, width, nbCol, nbLine;


    [Header("Cylinder")]
    public int faceCount, cylinderHeight;
    public float radius;


    public bool isCylinder = false; //TODO Use an enum later
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();


    void OnDrawGizmos()
    {
        if (isCylinder)
        {
            DrawCylinder();
        }
        else
        {
            DrawCustomMesh();
        }

        RenderMesh();
        vertices.Clear();
        triangles.Clear();
    }

    private void RenderMesh()
    {
        // Get the mesh and clear it
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // Create the new mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void DrawCylinder()
    {
        // Initialisation
        float yBottomCoord = -cylinderHeight / 2;
        float yTopCoord = cylinderHeight / 2;
        int bottomVertexIndex = 0;
        int topVertexIndex = 1;

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, -cylinderHeight/2, 0));
        vertices.Add(new Vector3(0, cylinderHeight/2, 0));

        float angleStep = (2 * Mathf.PI) / faceCount;
        // Some vertices are added 2 times but the ToArray() later get rid of the duplicates (TODO IMPROVE)
        for (int i = 0; i < faceCount; i++)
        {
            // Loop initialisation
            int vertexIndex = vertices.Count;
            float angle = i * angleStep;
            float nextAngle = ((i + 1)) * angleStep;

            // Coord computation
            // X
            float xCoord = radius * Mathf.Cos(angle);
            float xPrimeCoord = radius * Mathf.Cos(nextAngle);
            // Z
            float zCoord = radius * Mathf.Sin(angle);
            float zPrimeCoord = radius * Mathf.Sin(nextAngle);

            // Bottom vertices
            vertices.Add(new Vector3(xCoord, yBottomCoord, zCoord));
            vertices.Add(new Vector3(xPrimeCoord, yBottomCoord, zPrimeCoord));

            // Top vertices
            vertices.Add(new Vector3(xCoord, yTopCoord, zCoord));
            vertices.Add(new Vector3(xPrimeCoord, yTopCoord, zPrimeCoord));

            // First triangle clock wise
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex);

            // Second triangle clock wise
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 1);

            // Bottom triangle
            triangles.Add(bottomVertexIndex);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);

            // Top triangle
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 2);
            triangles.Add(topVertexIndex);

        }
    }

    private void DrawCustomMesh()
    {

        // Some vertices are added 2 times but the ToArray() later get rid of the duplicates (TODO IMPROVE)
        for (int x = 0; x < nbCol; x++)
        {
            for (int y = 0; y < nbLine; y++)
            {
                int vertexIndex = vertices.Count;

                // Quad vertices
                vertices.Add(new Vector3(x * width, y * height, 0));
                vertices.Add(new Vector3((x + 1) * width, y * height, 0));
                vertices.Add(new Vector3(x * width, (y + 1) * height, 0));
                vertices.Add(new Vector3((x + 1) * width, (y + 1) * height, 0));

                // First triangle clock wise
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);

                // Second triangle clock wise
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
            }
        }
    }
}
