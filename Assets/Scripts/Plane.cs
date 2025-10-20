using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public enum Shape
{
    Plane,
    Cylinder
}

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Shape")]
    public Shape shape = Shape.Plane;

    [Header("Plane")]
    public int height;
    public int width;
    public int nbCol;
    public int nbLine;


    [Header("Cylinder")]
    public int faceCount;
    public int cylinderHeight;
    public float radius;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();


    void OnDrawGizmos()
    {
        if (shape == Shape.Cylinder)
        {
            DrawCylinder();
        }
        else if (shape == Shape.Plane)
        {
            DrawPlane(); 
        }
        RenderMesh();
    }

    private void RenderMesh()
    {
        // Get the mesh and clear it
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // Create the new mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        //mesh.RecalculateNormals(); // TODO Deactivated normals recomputation so I I can easily have both way facing triangle (cause a light bug if activated because unity thinks the 2 opposite triangle have thr same normal)
        mesh.RecalculateBounds();

        // Empty the List after creating the mesh
        vertices.Clear();
        triangles.Clear();
    }

    // TODO Improve the draw function by placing the vertices first (avoiding duplication) then loop and create the triangles

    private void DrawCylinder()
    {
        // Initialisation
        float yBottomCoord = -cylinderHeight / 2;
        float yTopCoord = cylinderHeight / 2;
        int bottomVertexIndex = 0;
        int topVertexIndex = 1;
        float angleStep = (2 * Mathf.PI) / faceCount;

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, -cylinderHeight/2, 0));
        vertices.Add(new Vector3(0, cylinderHeight/2, 0));

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

            // === Face first triangle ===
            // Clock- wise
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex);
            // Counter clock-wise
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);

            // === Face second triangle ===
            // Clock-wise
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 1);
            // Counter clock-wise
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 2);

            // === Bottom face triangle ===
            // Clock-wise
            triangles.Add(bottomVertexIndex);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            // Counter clock-wise
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex);
            triangles.Add(bottomVertexIndex);

            // === Top face triangle ===
            // Clock-wise
            triangles.Add(vertexIndex + 3);
            triangles.Add(vertexIndex + 2);
            triangles.Add(topVertexIndex);
            // Counter clock-wise
            triangles.Add(topVertexIndex);
            triangles.Add(vertexIndex + 2);
            triangles.Add(vertexIndex + 3);
        }
    }

    private void DrawPlane()
    {

        for (int x = 0; x < nbCol; x++)
        {
            for (int y = 0; y < nbLine; y++)
            {
                // Loop Initialisation
                int vertexIndex = vertices.Count;

                // Coord computation
                // X
                float xCoord = x * width;
                float xPrimeCoord = (x + 1) * width;
                // Y
                float yCoord = y * height;
                float yPrimeCoord = (y + 1) * height;

                // Quad vertices
                vertices.Add(new Vector3(xCoord, yCoord, 0));
                vertices.Add(new Vector3(xPrimeCoord, yCoord, 0));
                vertices.Add(new Vector3(xCoord, yPrimeCoord, 0));
                vertices.Add(new Vector3(xPrimeCoord, yPrimeCoord, 0));

                // === First triangle ===
                // Clock-wise
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                // Counter clock-wise
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);

                // === Second triangle ===
                // Clock wise
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
                // Counter clock-wise
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
            }
        }
    }
}
