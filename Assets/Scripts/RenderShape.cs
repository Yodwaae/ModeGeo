using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public enum Shape
{
    Plane,
    Cylinder,
    Cone,
    Sphere,
    None,
}

// TODO Rename variables for clarity sake (and maybe also add/change some comments)
public class RenderShape : MonoBehaviour
{
    [Header("Shape")]
    public Shape shape = Shape.Plane;

    [Header("Plane")]
    public int planeHeight = 4;
    public int planeWidth = 4;
    public int planeNbCol = 1;
    public int planeNbLine = 1;

    [Header("Cylinder")]
    public int cylinderFaceCount = 16;
    public int cylinderHeight = 32;
    public float cylinderRadius = 8f;

    [Header("Cone")]
    public int coneFaceCount = 16;
    public int coneHeight = 32;
    public float coneRadius = 8f;
    [Range(0f, 1f)]public float coneHeightFactor = 1f;
    [Range(0f, 1f)]public float coneTopRadiusFactor= 0f;

    [Header("Sphere")]
    public int sphereNbMeridian = 20;
    public int sphereNbParallel = 20;
    public float sphereRadius = 10f;
    [Range(0f, 1f)] public float sphereTruncationRatio = 0f;


    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // Helper function, will advise on if it's helpful or not
    private Vector3 Multiply(Vector3 vec1, Vector3 vec2)
    {
        Vector3 res = new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);

        return res;
    }

    private void OnDrawGizmos()
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
            case Shape.Sphere:
                DrawSphere();
                break;
            case Shape.None:
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
        // Counter clock-wise
        //triangles.Add(index3);
        //triangles.Add(index2);
        //triangles.Add(index1);
    }

    private void RenderMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

        #if UNITY_EDITOR
            // TODO To see as this can create a problem, the changes are applied to all object using the same base mesh filter while in editor
            // In editor mode use shared mesh to avoid memory leakage
            Mesh mesh = meshFilter.sharedMesh;
            if (mesh == null)
            {
                mesh = new Mesh();
                meshFilter.sharedMesh = mesh;
            }
        #else
            // In play mode use the mesh (singleton)
            Mesh mesh = meshFilter.mesh;
        #endif

        // Clear the mesh
        mesh.Clear();


        // TODO Double sided geometry temp implementation (should I move that to the addTriangles func() ?
        int originalVertCount = vertices.Count;
        // Duplicate vertices and flip normals
        List<Vector3> doubleVertices = new List<Vector3>(vertices);
        doubleVertices.AddRange(vertices);
        List<int> doubleTriangles = new List<int>(triangles);

        // Add flipped triangles
        for (int i = 0; i < triangles.Count; i += 3)
        {
            doubleTriangles.Add(triangles[i] + originalVertCount);
            doubleTriangles.Add(triangles[i + 2] + originalVertCount);
            doubleTriangles.Add(triangles[i + 1] + originalVertCount);
        }

        // Create the new mesh
        mesh.vertices = doubleVertices.ToArray();
        mesh.triangles = doubleTriangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // What if, after recalculating Normals, I looped through the normal and set every even one to be the opposite of the odd just before ?

        // Empty the List after creating the mesh
        vertices.Clear();
        triangles.Clear();
    }

    // TODO Improve the draw functions by placing the vertices first (avoiding duplication) then loop and create the triangles
    // TODO Could also add arguments to the draw functions so they could be used by multiple shape with only slight tweaking brought on by the arguments (eg. cylinder and cone), this would avoid duplication for shape with a common base
    private void DrawPlane()
    {

        for (int i = 0; i < planeNbCol; i++)
        {
            for (int j = 0; j < planeNbLine; j++)
            {
                // Loop Initialisation
                int vertexIndex = vertices.Count;

                // Coord computation
                // X
                float xCur = i * planeWidth;
                float xNext = (i + 1) * planeWidth;
                // Y
                float yCur = j * planeHeight;
                float yNext = (j + 1) * planeHeight;

                // Quad vertices
                vertices.Add(new Vector3(xCur, yCur, 0));
                vertices.Add(new Vector3(xNext, yCur, 0));
                vertices.Add(new Vector3(xCur, yNext, 0));
                vertices.Add(new Vector3(xNext, yNext, 0));

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
        float yBot = -cylinderHeight / 2;
        float yTop = cylinderHeight / 2;
        int botVertexIndex = 0;
        int topVertexIndex = 1;
        float angleStep = (2 * Mathf.PI) / cylinderFaceCount;

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, -cylinderHeight / 2, 0)); // Bottom
        vertices.Add(new Vector3(0, cylinderHeight / 2, 0)); // Top

        for (int i = 0; i < cylinderFaceCount; i++)
        {
            // Loop initialisation
            int vertexIndex = vertices.Count;
            float angle = i * angleStep;
            float nextAngle = ((i + 1)) * angleStep;

            // Coord computation
            // X
            float xCoord = cylinderRadius * Mathf.Cos(angle);
            float xNextCoord = cylinderRadius * Mathf.Cos(nextAngle);
            // Z
            float zCoord = cylinderRadius * Mathf.Sin(angle);
            float zNextCoord = cylinderRadius * Mathf.Sin(nextAngle);

            // Bottom vertices
            vertices.Add(new Vector3(xCoord, yBot, zCoord));
            vertices.Add(new Vector3(xNextCoord, yBot, zNextCoord));

            // Top vertices
            vertices.Add(new Vector3(xCoord, yTop, zCoord));
            vertices.Add(new Vector3(xNextCoord, yTop, zNextCoord));

            // Face first triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 1, vertexIndex);
            // Face second triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 3, vertexIndex + 1);
            // Bottom face triangle
            AddTriangles(botVertexIndex, vertexIndex, vertexIndex + 1);
            // Top face triangle
            AddTriangles(vertexIndex + 3, vertexIndex + 2, topVertexIndex);
        }
    }

    // TODO Could optimize the function more if the cone isn't truncated, but is it worth it ?
    private void DrawCone()
    {
        // Initialisation
        float height = coneHeight * coneHeightFactor;
        float yBot = 0;
        float yTop = height;
        int botVertexIndex = 0;
        int topVertexIndex = 1;
        float angleStep = (2 * Mathf.PI) / coneFaceCount;
        Vector3 topFactorVector = new Vector3(coneTopRadiusFactor, 1, coneTopRadiusFactor);

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, yBot, 0)); // BOTTOM
        vertices.Add(new Vector3(0, yTop, 0)); // TOP

        for (int i = 0; i < coneFaceCount; i++)
        {
            // Loop initialisation
            int vertexIndex = vertices.Count;
            float angle = i * angleStep;
            float nextAngle = ((i + 1)) * angleStep;

            // Coord computation
            // X
            float xCur = coneRadius * Mathf.Cos(angle);
            float xNext = coneRadius * Mathf.Cos(nextAngle);
            // Z
            float zCur = coneRadius * Mathf.Sin(angle);
            float zNext = coneRadius * Mathf.Sin(nextAngle);

            // Bottom vertices
            vertices.Add(new Vector3(xCur, yBot, zCur));
            vertices.Add(new Vector3(xNext, yBot, zNext));
            
            // Top vertices 
            vertices.Add(Multiply(new Vector3(xCur, yTop, zCur), topFactorVector));
            vertices.Add(Multiply(new Vector3(xNext, yTop, zNext), topFactorVector));

            // Face first triangle
            AddTriangles(vertexIndex + 2, vertexIndex + 1, vertexIndex);
            // Bottom face triangle
            AddTriangles(botVertexIndex, vertexIndex, vertexIndex + 1);
       
            // Add those triangles only if the cone is truncated, else they are useless
            if (coneTopRadiusFactor > 0){
                // Face second triangle
                AddTriangles(vertexIndex + 2, vertexIndex + 3, vertexIndex + 1);
                // Top face triangle
                AddTriangles(vertexIndex + 3, vertexIndex + 2, topVertexIndex);
            }

        }
    }

    private void DrawSphere()
    { 
        // Initialisation
        float medianStep = (2 * Mathf.PI) / sphereNbMeridian;
        float parallelStep = Mathf.PI / sphereNbParallel;
        int lastMeridian = (int) (sphereNbMeridian * (1- sphereTruncationRatio));
        int botVertexIndex = 0; 
        int topVertexIndex = 1; 

        // Adding centered top and bottom vertex
        vertices.Add(new Vector3(0, -sphereRadius, 0)); // BOTTOM
        vertices.Add(new Vector3(0, sphereRadius, 0)); // TOP
        
        // Meridian Loop
        for (int i = 0; i < lastMeridian; i++) { 

            // Meridian loop initialisation
            float meridian = i * medianStep; 
            float nextMeridian = ((i + 1)) * medianStep;

            // Parallels Loop
            for (int j = 0; j < sphereNbParallel; j++) {

                // Parallels loop initialisation
                int vertexIndex = vertices.Count;
                float parallel = j * parallelStep;
                float nextParallel = (j + 1) * parallelStep;
                float parallelRadius = sphereRadius * Mathf.Sin(parallel);
                float nextParallelRadius = sphereRadius * Mathf.Sin(nextParallel);

                // Coord computation 
                // X
                float xCur = parallelRadius * Mathf.Cos(meridian);
                float xNextPar = nextParallelRadius * Mathf.Cos(meridian);
                float xNextMer = parallelRadius * Mathf.Cos(nextMeridian);
                float xNextParMer = nextParallelRadius * Mathf.Cos(nextMeridian);
                // Y
                float yCur = sphereRadius * Mathf.Cos(parallel);
                float yNextPar = sphereRadius * Mathf.Cos(nextParallel);
                // Z
                float zCur = parallelRadius * Mathf.Sin(meridian);
                float zNextPar = nextParallelRadius * Mathf.Sin(meridian);
                float zNextMer = parallelRadius * Mathf.Sin(nextMeridian);
                float zNextParMer = nextParallelRadius * Mathf.Sin(nextMeridian);

                // Top vertices
                vertices.Add(new Vector3(xCur, yCur, zCur));  // Top left
                vertices.Add(new Vector3(xNextMer, yCur, zNextMer));  // Top right

                // Bottom vertices
                vertices.Add(new Vector3(xNextPar, yNextPar, zNextPar));  // Bottom left
                vertices.Add(new Vector3(xNextParMer, yNextPar, zNextParMer));  // Bottom right

                // Face first triangle
                AddTriangles(vertexIndex + 2, vertexIndex + 1, vertexIndex); 
                // Face second triangle
                AddTriangles(vertexIndex + 2, vertexIndex + 3, vertexIndex + 1);

                // Bottom face triangle if we're at the last parallel
                if (j == sphereNbParallel){
                    AddTriangles(botVertexIndex, vertexIndex, vertexIndex + 1);
                }
                // Top face triangle if we're at the last parallel
                if (j == 0){
                    AddTriangles(vertexIndex, vertexIndex + 1, topVertexIndex);
                    AddTriangles(vertexIndex, vertexIndex + 1, topVertexIndex);
                }

                // Create the truncation vertices and triangles only if the sphere is truncated (if not truncated not drawing them avoid normal problems)
                if (sphereTruncationRatio != 0)
                {
                    // Truncation vertices
                    vertices.Add(new Vector3(0, yCur, 0));
                    vertices.Add(new Vector3(0, yNextPar, 0));

                    // Truncation last meridian faces
                    if (i == lastMeridian - 1)
                    {
                        AddTriangles(vertexIndex + 1, vertexIndex + 4, vertexIndex + 3);
                        AddTriangles(vertexIndex + 4, vertexIndex + 5, vertexIndex + 3);
                    }
                    // Truncation first meridian faces
                    if (i == 0)
                    {
                        AddTriangles(vertexIndex, vertexIndex + 2, vertexIndex + 4);
                        AddTriangles(vertexIndex + 2, vertexIndex + 5, vertexIndex + 4);
                    }
                }
            } 
        }
    }

}
