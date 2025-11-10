using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshImporter : MonoBehaviour
{
    // TODO Regroup and create helpers function (could even create a separate script since some helpers are common to RenderShape and MeshImporter)
    // TODO Is there a way to parseInt earlier ? Whil reading the file ? (Not sure since some of them needs to be float and other int (float for vertices, int for triangles))
    [SerializeField] private TextAsset meshOff;
    [SerializeField][Min(0)] private int facesToRemove = 0;
    [SerializeField] private float normalsAngle = 180;
    [SerializeField] private bool meshExported = true; // NOTE : Not the cleanest but works for now


    // Mesh var
    private int verticesNb;
    private int trianglesNb;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();

    // Text parsing var
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

        // Export Mesh
        if (!meshExported)
            ExportMesh();

        // Render Mesh
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
        CustomRecalculateNormals(mesh, normalsAngle);
        //mesh.RecalculateNormals();
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
        // NOTE : We only remove faces not vertices
        // Clamp the number of faces to remove so it stays within valid bounds then re-set the trianglesNb
        if (facesToRemove > trianglesNb)
            facesToRemove = trianglesNb;
        else if (facesToRemove < 0)
            facesToRemove = 0;
        trianglesNb -= facesToRemove;

        // Get the lines with the triangles "corrds"
        triangleLines = eachLine.Skip(2 + verticesNb).Take(trianglesNb).ToList();

        // Create the triangles
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

    public void ExportMesh()
    {
        // Set the flag to prevent further mesh export
        meshExported = true;
        int faceNumber = trianglesNb / 3;

        // Create the File Header // TODO FIX THE PROBLEM HERE
        string content = "OFF\n" + verticesNb + " " + faceNumber + " " + trianglesNb + "\n"; 

        // Add the vertices line to the text
        for (int i = 0; i < verticesNb; i++)
            content += vertices[i][0].ToString(CultureInfo.InvariantCulture) + " " + vertices[i][1].ToString(CultureInfo.InvariantCulture) + " " + vertices[i][2].ToString(CultureInfo.InvariantCulture) + "\n";
        
        // Add the triangles line to the text
        for (int i = 0; i < faceNumber; i+=3)
            content += "3 " + triangles[i] + " " + triangles[i+1] + " " + triangles[i+2] + "\n";
        
        // File Path
        string path = Application.persistentDataPath + "/" + meshOff.name + "Output.off";
        // Write the file
        File.WriteAllText(path, content);
        Debug.Log("File written at: " + path);
    }

    private static void CustomRecalculateNormals(Mesh mesh, float angle)
    {
        // Initialisation
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;
        var normals = new Vector3[vertices.Length];
        //var angleCos = Mathf.Cos(angle * Mathf.Deg2Rad);

        // Compute the normal for each triangles
        for (int i = 0; i < triangles.Length; i += 3)
        {
            // Compute current triangle vertices' index
            int firstPointIndex = triangles[i];
            int secondPointIndex = triangles[i + 1];
            int thirdPointIndex = triangles[i + 2];

            // Compute the current triangle normal
            Vector3 v1 = vertices[secondPointIndex] - vertices[firstPointIndex];
            Vector3 v2 = vertices[thirdPointIndex] - vertices[firstPointIndex];
            Vector3 normal = Vector3.Cross(v1, v2).normalized;

            // Add 
            normals[firstPointIndex] += normal;
            normals[secondPointIndex] += normal;
            normals[thirdPointIndex] += normal;
        }

        // Normalise the normals and set them as the mesh normals
        for (int i = 0; i < normals.Length; i++)
            normals[i] = normals[i].normalized;
        mesh.normals = normals;
    }
}
