using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public enum Shape
{
    Plane,
    Cylinder,
    Cone,
}

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Shape")]
    public Shape shape = Shape.Plane;

    [Header("Plane")]
    public int planeHeight;
    public int planeWidth;
    public int planeNbCol;
    public int planeNbLine;

    [Header("Cylinder")]
    public int cylinderFaceCount;
    public int cylinderHeight;
    public float cylinderRadius;

    [Header("Cone")]
    public int coneFaceCount;
    public int coneHeight;
    public float coneRadius;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();


    void OnDrawGizmos()
    {
        // Draw the mesh
        switch (shape) {

            case Shape.Plane:
                DrawPlane();
                break;
            case Shape.Cylinder:
                DrawCylinder();
                break;
            case Shape.Cone:
                DrawCone();
                break;
        }

        // Render the mesh
        RenderMesh();
    }

    private void AddTriangles(int index1, int index2, int index3)
    {
        // Clock-wise
        triangles.Add(index1);
        triangles.Add(index2);
        triangles.Add(index3);
        // Counter clock-wise //TODO Find a way to have two way geometry without causing lighting problem
        //triangles.Add(index3);
        //triangles.Add(index2);
        //triangles.Add(index1);
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

        // Empty the List after creating the mesh
        vertices.Clear();
        triangles.Clear();
    }

    // TODO Improve the draw functions by placing the vertices first (avoiding duplication) then loop and create the triangles
    // TODO Could also add arguments to the draw functions so they could be used by multiple shape with only slight tweaking brought on by the arguments (eg. cylinder and cone), this would avoid duplication for shape with a common base
    private void DrawPlane()
    {

        for (int x = 0; x < planeNbCol; x++)
        {
            for (int y = 0; y < planeNbLine; y++)
            {
                // Loop Initialisation
                int vertexIndex = vertices.Count;

                // Coord computation
                // X
                float xCoord = x * planeWidth;
                float xPrimeCoord = (x + 1) * planeWidth;
                // Y
                float yCoord = y * planeHeight;
                float yPrimeCoord = (y + 1) * planeHeight;

                // Quad vertices
                vertices.Add(new Vector3(xCoord, yCoord, 0));
                vertices.Add(new Vector3(xPrimeCoord, yCoord, 0));
                vertices.Add(new Vector3(xCoord, yPrimeCoord, 0));
                vertices.Add(new Vector3(xPrimeCoord, yPrimeCoord, 0));

                // First triangle
                AddTriangles(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                // Second triangle
                AddTriangles(vertexIndex + 1, vertexIndex + 3, vertexIndex + 2);
            }
        }
    }

    private void DrawCylinder()
    {
        // Initialisation
        float yBottomCoord = -cylinderHeight / 2;
        float yTopCoord = cylinderHeight / 2;
        int bottomVertexIndex = 0;
        int topVertexIndex = 1;
        float angleStep = (2 * Mathf.PI) / cylinderFaceCount;

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, -cylinderHeight / 2, 0));
        vertices.Add(new Vector3(0, cylinderHeight / 2, 0));

        for (int i = 0; i < cylinderFaceCount; i++)
        {
            // Loop initialisation
            int vertexIndex = vertices.Count;
            float angle = i * angleStep;
            float nextAngle = ((i + 1)) * angleStep;

            // Coord computation
            // X
            float xCoord = cylinderRadius * Mathf.Cos(angle);
            float xPrimeCoord = cylinderRadius * Mathf.Cos(nextAngle);
            // Z
            float zCoord = cylinderRadius * Mathf.Sin(angle);
            float zPrimeCoord = cylinderRadius * Mathf.Sin(nextAngle);

            // Bottom vertices
            vertices.Add(new Vector3(xCoord, yBottomCoord, zCoord));
            vertices.Add(new Vector3(xPrimeCoord, yBottomCoord, zPrimeCoord));

            // Top vertices
            vertices.Add(new Vector3(xCoord, yTopCoord, zCoord));
            vertices.Add(new Vector3(xPrimeCoord, yTopCoord, zPrimeCoord));

            // Face first triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 1, vertexIndex);
            // Face second triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 3, vertexIndex + 1);
            // Bottom face triangle
            AddTriangles(bottomVertexIndex, vertexIndex, vertexIndex + 1);
            // Top face triangle
            AddTriangles(vertexIndex + 3, vertexIndex + 2, topVertexIndex);
        }
    }

    // TODO Quick cone implementation, honestly could use drawCylinder with an additionnal argument to the function to control the cylinder/cone "opening"
    // Could make the cone up or down facing just put the 0 in the top or bottom vertices
    private void DrawCone()
    {
        // Initialisation
        float yBottomCoord = -coneHeight / 2;
        float yTopCoord = coneHeight / 2;
        int bottomVertexIndex = 0;
        int topVertexIndex = 1;
        float angleStep = (2 * Mathf.PI) / coneFaceCount;

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, -coneHeight / 2, 0)); // BOTTOM
        vertices.Add(new Vector3(0, coneHeight / 2, 0)); // TOP

        for (int i = 0; i < coneFaceCount; i++)
        {
            // Loop initialisation
            int vertexIndex = vertices.Count;
            float angle = i * angleStep;
            float nextAngle = ((i + 1)) * angleStep;

            // Coord computation
            // X
            float xCoord = coneRadius * Mathf.Cos(angle);
            float xPrimeCoord = coneRadius * Mathf.Cos(nextAngle);
            // Z
            float zCoord = coneRadius * Mathf.Sin(angle);
            float zPrimeCoord = coneRadius * Mathf.Sin(nextAngle);

            // Bottom vertices
            vertices.Add(new Vector3(xCoord, yBottomCoord, zCoord));
            vertices.Add(new Vector3(xPrimeCoord, yBottomCoord, zPrimeCoord));
            
            // TODO Found the quick trick to create truncated cone, instead of hardcoding 0 as the x and z coords
            // I compute them as usual then I multiply them by factor to reduce the size of the top section (if I wanto have something consistant I should multiply the ycoord by the same factor)
            // Top vertices
            vertices.Add(new Vector3(xCoord * 0, yTopCoord, zCoord * 0));
            vertices.Add(new Vector3(xPrimeCoord *0, yTopCoord, zPrimeCoord * 0));

            // TODO If the cylinder isn't truncated the second triangle and top face triangle is useless
            // Face first triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 1, vertexIndex);
            // Face second triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 3, vertexIndex + 1);
            // Bottom face triangle
            AddTriangles(bottomVertexIndex, vertexIndex, vertexIndex + 1);
            // Top face triangle
            AddTriangles(vertexIndex + 3, vertexIndex + 2, topVertexIndex);
        }
    }

}
