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
        // File Reading
        offText = meshOff.text;
        eachLine = new List<string>();
        eachLine.AddRange(offText.Split("\n"));

        // Early exit if the file is not valid
        if (!FileIsValid()){
            Debug.Log("File is not valid");
            return;
        }
        

        CreateVertices();
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

        // What if, after recalculating Normals, I looped through the normal and set every even one to be the opposite of the odd just before ?

        // Empty the List after creating the mesh
        vertices.Clear();
        triangles.Clear();
    }

    private void CreateVertices()
    {
        // Get the lines ...
        vertexLines = eachLine.Skip(2).Take(verticesNb).ToList();

        // TODO Loop directly on the list ?
        for (int i = 0; i < verticesNb; i++){
            // TODO Improve this, potentially parse before that ?
            List<string> coords = new List<string>();
            coords.AddRange(vertexLines[i].Split(" "));
            vertices.Add(new Vector3(float.Parse(coords[0], CultureInfo.InvariantCulture), float.Parse(coords[1], CultureInfo.InvariantCulture), float.Parse(coords[2], CultureInfo.InvariantCulture)));
        }
    }


    private void CreateTriangles()
    {
        // Get the lines ....
        triangleLines = eachLine.Skip(2 + verticesNb).Take(trianglesNb).ToList();

        // TODO Loop directly on the list
        for (int i = 0; i < trianglesNb; i++)
        {
            List<string> indexes = new List<string>();
            indexes.AddRange(triangleLines[i].Split(" "));
            AddTriangles(int.Parse(indexes[1]), int.Parse(indexes[2]), int.Parse(indexes[3]));
        }

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
}
