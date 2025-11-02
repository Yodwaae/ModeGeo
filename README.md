# ModeGeo
Unity project (v6000.0.57f1)

# Overview
**ModeGeo** is a lightweight geometry generation tool for Unity.
It allows you to create and manipulate basic 3D shapes directly within the editor or at runtime.
Each shape is generated procedurally based on user-defined parameters, making it easy to prototype and customize geometry without relying on pre-made meshes.

# Shapes

### <u>**Plane**</u>
*Parameters:*
- **Width** — total horizontal size of the plane in world units.
- **Height** — total vertical size of the plane in world units.
- **Nb Cols** — number of planes to generate along the width.
- **Nb Lines** — number of planes to generate along the height.

### <u>**Cylinder**</u>
*Parameters:*
- **Face Count** — number of radial segments forming the circular cross-section.
- **Height** — vertical size of the cylinder from base to top.
- **Radius** — distance from the center to the edge of the circular base.

### <u>**Cone**</u>
*Parameters:*
- **Face Count** — number of radial segments forming the base.  
- **Height** — total vertical size of the cone.
- **Radius** — radius of the base circle.
- **Cone Top Height Factor** — value between 0 and 1 defining the relative height of the cone’s top section; 1 means full height, 0 means no height.
- **Cone Top Radius Factor** — multiplier (0–1) defining the radius of the top relative to the base; 0 makes it pointed, values >0 create truncated cones, 1 makes a cylinder.

Allows generation of both pointed and truncated cones.

### <u>**Sphere**</u>
*Parameters:*
- **Nb Meridian** — number of longitudinal segments.
- **Nb Parallel** — number of latitudinal segments.
- **Radius** — distance from the center to the sphere’s surface.
- **Truncation Ratio** — value between 0 and 1 defining how much of the sphere’s meridians are removed; 1 keeps the full sphere, 0 removes it entirely, producing a truncated/sliced effect.

## Usage

1. Add the **RenderShape** component to any GameObject that includes both a **MeshFilter** and a **MeshRenderer**.
2. In the **Inspector**, choose the desired **Shape** from the dropdown.
3. Adjust the available parameters to customize the geometry.

**Tip:** You can modify parameters at runtime to dynamically update the mesh, making it ideal for procedural content or editor tools.