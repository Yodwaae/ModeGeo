using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public int height, width, nbCol, nbLine;


    void Start()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Some vertices are added 2 times but the ToArray() later get rid of the duplicates (TODO IMPROVE)
        for (int x = 0; x < nbCol; x++)
        {
            for (int y = 0; y < nbLine; y++)
            {
                int vertexIndex = vertices.Count;

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

        // Get the mesh and clear it
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private void CustomMeshRender()
    {

    }

    void Update()
    {
        
    }
}
