using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshImporter : MonoBehaviour
{
    [SerializeField] private TextAsset meshOff;
    private int verticesNb;
    private int trianglesNb;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // TODO Regroup and create helpers function (could even create a separate script since some helpers are common to RenderShape and MeshImporter)
    // TODO Is there a way to parseInt earlier ? Whil reading the file ? (Not sure since some of them needs to be float and other int (float for vertices, int for triangles))

    // TEMP ?
    private string offText;
    private List<string> eachLine;
    private List<string> vertexLines;
    private List<string> triangleLines;

    private void OnDrawGizmos()
    {
        // Early exit if no file
        if (!meshOff)
            return;

        // File Reading
        offText = meshOff.text;
        eachLine = new List<string>();
        eachLine.AddRange(offText.Split("\n"));

        // Early exit if the file is not valid
        if (!FileIsValid()){
            Debug.Log("File is not valid");
            return;
        }
        
        // Mesh Creation
        CreateVertices();
        CenterMeshOnCentroid();
        NormaliseMesh();
        CreateTriangles();
        RenderMesh();

    }

    private bool FileIsValid()
    {
        // TODO Also there a problem right now as the file has 2 responsibilities since it sets vertices and triangles nb
        // TODO The security isn't robuste enough, but for now it will suffice + clean up

        // If the first line isn't OFF (format definer) then the file is not valid
        /*if (eachLine[0] != "OFF")
            return false;*/

        if (eachLine.Count > 1){

            // Split the second line to separate the number of vertices, triangles and faces
            List<string> counterLine = new List<string>();
            counterLine.AddRange(eachLine[1].Split(" "));

            Debug.Log(string.Join(" | ", counterLine));

            // If there is not 3 entries in the second line then the file isn't valid
            if (counterLine.Count != 3)
                    return false;

            // Set the number of vertices and triangles to create
            verticesNb = int.Parse(counterLine[0]);
            trianglesNb = int.Parse(counterLine[1]);

            // If there is not enough lines for the nb of vertices and triangles + the 2 header lines then the file is not valid
            int totalNbOfLines = verticesNb + trianglesNb;
            if (eachLine.Count < totalNbOfLines + 2)
                return false;
        }


        return true;
    }

    private void RenderMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            return;

#if UNITY_EDITOR
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

        // Empty the List after creating the mesh
        vertices.Clear();
        triangles.Clear();
    }

    private void CreateVertices()
    {
        // Get the lines with the vertices coords
        vertexLines = eachLine.Skip(2).Take(verticesNb).ToList();

        for (int i = 0; i < verticesNb; i++){
            // Slit the 3 coords (x, y, z) of the line
            List<string> coords = new List<string>();
            coords.AddRange(vertexLines[i].Split(" "));

            // Parse the coords from text then add them to the vertices
            float x = float.Parse(coords[0], CultureInfo.InvariantCulture);
            float y = float.Parse(coords[1], CultureInfo.InvariantCulture);
            float z = float.Parse(coords[2], CultureInfo.InvariantCulture);
            vertices.Add(new Vector3(x, y, z));
        }

    }

    private void CreateTriangles()
    {
        // Get the lines with the triangles "corrds"
        triangleLines = eachLine.Skip(2 + verticesNb).Take(trianglesNb).ToList();

        for (int i = 0; i < trianglesNb; i++)
        {
            // NOTE This MeshImporter script only works with triangles
            // Split the triangle pt indexes (nbPt, p1, p2, p3)
            List<string> indexes = new List<string>();
            indexes.AddRange(triangleLines[i].Split(" "));

            // Par the pt indexes from text then add the triangle
            int p1 = int.Parse(indexes[1]);
            int p2 = int.Parse(indexes[2]);
            int p3 = int.Parse(indexes[3]);
            AddTriangles(p1, p2, p3);
        }

    }

    private void CenterMeshOnCentroid()
    {
        // Compute the centroid
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < verticesNb; i++)
            sum += vertices[i];

        // Fix the centroid
        Vector3 centroid = sum / verticesNb;
        for (int i = 0; i < verticesNb; i++)
            vertices[i] -= centroid;
    }

    private void NormaliseMesh()
    {
        // Initialisation
        float max = 0;

        // Get the max coord value (abs) for normalisation
        for (int i = 0; i < verticesNb; i++) {
            float vertexMax = Mathf.Max(Mathf.Abs(vertices[i][1]), Mathf.Abs(vertices[i][1]), Mathf.Abs(vertices[i][2]));
            if (vertexMax > max) 
                max = vertexMax;
        }

        // Shouldn't happen but we're never too careful
        if (max == 0)
            return;

        // Apply the normalisation
        for (int i = 0; i < verticesNb; i++)
            vertices[i] /= max;
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

    private void ExportMesh()
    {

    }
}
