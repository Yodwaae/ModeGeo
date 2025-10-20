using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    // TODO I really need to find how to add triangles both way without creating lighting problems

    public int height, width, nbCol, nbLine, faceCount, cylHeight;
    public float radius;
    public bool isCylinder = false; //TODO Use an enum later


    void OnDrawGizmos()
    {
        CustomMeshRender();
    }

    private void CustomMeshRender()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();


        // TODO Not clean for the moment
        if (isCylinder)
        {
            float angleStep = (2 * Mathf.PI) / faceCount;
            // Some vertices are added 2 times but the ToArray() later get rid of the duplicates (TODO IMPROVE)
            for (int i = 0; i < faceCount; i++)
            {
                int vertexIndex = vertices.Count;

                float angle = i * angleStep;
                float nextAngle = ((i + 1))* angleStep;

                float xCoord = radius * Mathf.Cos(angle);
                float xPrimeCoord = radius * Mathf.Cos(nextAngle);
                float yCoord = -cylHeight / 2;
                float yPrimeCoord = cylHeight / 2;
                float zCoord = radius * Mathf.Sin(angle) ;
                float zPrimeCoord = radius * Mathf.Sin(nextAngle);

                // Bottom vertices
                vertices.Add(new Vector3(xCoord, yCoord, zCoord));
                vertices.Add(new Vector3(xPrimeCoord, yCoord, zPrimeCoord));

                // Top vertices
                vertices.Add(new Vector3(xCoord, yPrimeCoord, zCoord));
                vertices.Add(new Vector3(xPrimeCoord, yPrimeCoord, zPrimeCoord));

                // First triangle clock wise
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);

                // Second triangle clock wise
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);

            }

        }
        else
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


        // Get the mesh and clear it
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        // Create the new mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

}
