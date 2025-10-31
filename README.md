# ModeGeo
Unity project (v6000.0.57f1) 

# Overview  
**ModeGeo** is a lightweight geometry generation tool for Unity.  
It allows you to create and manipulate basic 3D shapes directly within the editor or at runtime.  
Each shape is generated procedurally based on user-defined parameters, making it easy to prototype and customize geometry without relying on pre-made meshes.  

# Shapes  

### <u>**Plane**</u>  
*Parameters:*
- **Width**  
- **Height**  
- **Nb Cols** (number of columns for subdivision)  
- **Nb Lines** (number of lines for subdivision)  
 

### <u>**Cylinder**</u>  
*Parameters:*
- **Face Count** (number of sides)  
- **Height**  
- **Radius**  

### <u>**Cone**</u>  
*Parameters:*
- **Face Count**  
- **Height**  
- **Radius**  
- **Cone Top Height Factor** (controls how far the top extends vertically)  
- **Cone Top Radius Factor** (controls the size of the cone’s top radius)  

Allows generation of both pointed and truncated cones.  

### <u>**Sphere**</u>  
*Not yet implemented*  

## Usage  

1. Add the **RenderShape** component to any GameObject that includes both a **MeshFilter** and a **MeshRenderer**.  
2. In the **Inspector**, choose the desired **Shape** from the dropdown.  
3. Adjust the available parameters to customize the geometry.  


**Tip:** You can modify parameters at runtime to dynamically update the mesh, making it ideal for procedural content or editor tools.  