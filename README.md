# ModeGeo  
Unity project (v6000.2.10f1)

## Overview
**ModeGeo** is a lightweight geometry generation and mesh import tool for Unity.  
It allows you to create procedural 3D shapes directly in the editor or at runtime, and also import mesh data from **.off** files.

Geometry updates live in the editor via OnDrawGizmos.


---

## Built-In Procedural Shapes

### **Plane**
**Parameters**
- Width — Horizontal size in world units  
- Height — Vertical size in world units  
- Nb Cols — Subdivisions along width  
- Nb Lines — Subdivisions along height  

### **Cylinder**
**Parameters**
- Face Count — Radial segment count  
- Height — Vertical size  
- Radius — Base radius  

### **Cone**
**Parameters**
- Face Count — Radial segment count  
- Height — Full height  
- Radius — Base radius  
- Cone Top Height Factor — 0–1, scales the top height  
- Cone Top Radius Factor — 0–1, scales the top radius (0 = pointed, 1 = cylinder)

Supports pointed and truncated cones.

### **Sphere**
**Parameters**
- Nb Meridian — Longitudinal subdivisions  
- Nb Parallel — Latitudinal subdivisions  
- Radius — Sphere radius  
- Truncation Ratio — 0–1 fraction of sphere preserved along meridians  

---

## Importing OFF Mesh Files

### **OffImporter**
A Unity ScriptedImporter allowing `.off` files to appear as `TextAsset` objects inside Unity.  
This importer simply loads the text contents and assigns it as the main object.  
Files are editable and accessible directly as text data.

Place `.off` files anywhere under `Assets/`. Unity will import them automatically.

### **MeshImporter Component**

Attach **MeshImporter** to a GameObject containing a **MeshFilter** and **MeshRenderer**.  
Assign a `.off` TextAsset and the mesh will be generated in the Scene view.

**Inspector Fields**

| Property        | Description |
|----------------|-------------|
| meshOff        | The `.off` file as TextAsset |
| facesToRemove  | Number of faces to skip when constructing mesh |
| normalsAngle   | Angle threshold for custom normal calculation |

The importer:
1. Reads vertex and face data from the `.off` file
2. Recenters geometry on its centroid
3. Normalizes scale (largest absolute coordinate becomes 1)
4. Constructs triangles
5. Renders the result to the MeshFilter

The generated mesh is rendered **double-sided** by duplicating vertex and triangle data and flipping the second set.

#### Exporting the Mesh

The mesh can be exported back to a `.off` file at any time using the **context menu**:  
Right-click the component in the Inspector → **Export Mesh**  
This writes the current normalized mesh to: `Application.persistentDataPath/<OriginalFileName>Output.off`
