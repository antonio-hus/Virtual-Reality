# Virtual Reality Lab Assignment - Ray Tracer Implementation
**Student:** Antonio Hus, Group 934

---

## Overview

This project implements a complete ray tracing engine in C# capable of rendering 3D scenes with realistic lighting, shadows, and support for various geometric primitives including volumetric data (CT scans).

---

## Completed Implementation Tasks

### 1. Ellipsoid Ray Intersection (`Ellipsoid.cs`)

**Implementation Details:**

Implemented the ray-ellipsoid intersection algorithm using the geometric transformation approach. The key insight is to transform the ray into the ellipsoid's local coordinate space where it becomes a unit sphere, solve the simpler ray-sphere intersection, then transform the results back to world space.

**Algorithm Steps:**

1. **Space Transformation:** Scale the ray origin and direction by the inverse of the ellipsoid's semi-axes lengths and radius to normalize the ellipsoid into a unit sphere
2. **Quadratic Equation:** Solve the standard ray-sphere intersection quadratic equation: at^2 + bt + c = 0 where:
   - a = dir · dir (dot product)
   - b = 2(oc · dir)
   - c = (oc · oc) - 1
3. **Discriminant Test:** Check if b^2 - 4ac >= 0 for valid intersections
4. **Distance Calculation:** Compute both intersection distances t0 and t1 using the quadratic formula
5. **Closest Valid Hit:** Select the nearest intersection within the valid distance range [minDist, maxDist]
6. **Normal Calculation:** Transform the normal back to world space using the gradient: (x/rx^2, y/ry^2, z/rz^2) normalized

**Challenges Solved:**

- Handling edge cases where the ray grazes the ellipsoid surface
- Proper normal computation for non-uniform scaling
- Preventing self-intersection by respecting the `minDist` parameter

---

### 2. CT Scan Volume Rendering (`CtScan.cs`)

**Implementation Details:**

Implemented a volumetric ray marching algorithm for rendering 3D medical CT scan data. This was the most complex component, requiring both ray-AABB intersection and adaptive sampling through the voxel grid.

**Algorithm Steps:**

1. **AABB Intersection:** First, perform axis-aligned bounding box intersection to determine where the ray enters and exits the volume bounds
   - Test against all three pairs of parallel planes (X, Y, Z)
   - Calculate near (t_min) and far (t_max) intersection parameters
   - Early exit if ray misses the volume entirely

2. **Ray Marching:** Step through the volume along the ray path
   - Step size: half of the minimum voxel dimension for adequate sampling
   - At each step, sample the voxel density value
   - Use the color map to determine if the voxel is transparent

3. **Surface Detection:** Stop at the first non-transparent voxel (alpha > 0)
   - Calculate the surface normal using central differences (gradient approximation)
   - Return intersection data for Phong shading

**Technical Decisions:**

- **Step Size:** Used 0.5 * min(thickness) for balance between quality and performance
- **Normal Estimation:** Central differences method provides smooth gradients: gradient_f = (f(x+1) - f(x-1), f(y+1) - f(y-1), f(z+1) - f(z-1))
- **Empty Space Skipping:** The AABB test efficiently skips regions where the ray doesn't intersect the volume

---

### 3. Shadow Ray Testing (`RayTracer.cs` - `IsLit` method)

**Implementation Details:**

Implemented shadow testing to determine if a surface point receives direct illumination from a light source. This creates realistic shadows by casting a ray from the surface toward each light.

**Algorithm:**

1. Calculate the direction vector from the surface point to the light position
2. Measure the distance to the light
3. Create a shadow ray starting at the surface point, pointing toward the light
4. Test for intersections with all geometry in the scene
5. If any intersection occurs between the surface and the light, the point is in shadow

**Key Implementation Details:**

- Used a small epsilon value (1e-6) for `minDist` to prevent self-intersection artifacts
- Limited intersection testing to the light distance to avoid false shadows from objects beyond the light
- Returns `true` if the path is clear (lit), `false` if occluded (shadow)

---

### 4. Primary Ray Generation and Phong Shading (`RayTracer.cs` - `Render` method)

**Implementation Details:**

Completed the main rendering loop that generates camera rays for each pixel and computes final colors using the Phong reflection model.

**Camera Ray Generation:**

1. **Coordinate Transformation:** Convert pixel coordinates (i, j) to view plane coordinates (x, y) using the `ImageToViewPlane` helper
2. **Camera Space:** Construct the camera's orthonormal basis:
   - Forward: camera.Direction
   - Up: camera.Up
   - Right: camera.Direction × camera.Up (cross product)
3. **Ray Construction:** Calculate the point on the view plane and create a ray from camera position through that point

**Phong Lighting Implementation:**

For each light source, I computed three components:

1. **Ambient:** I_ambient = k_ambient * L_ambient
   - Always present, provides base illumination even in shadow

2. **Diffuse:** I_diffuse = k_diffuse * L_diffuse * max(0, N · L)
   - Lambert's cosine law for matte surfaces
   - Depends on angle between surface normal (N) and light direction (L)

3. **Specular:** I_specular = k_specular * L_specular * max(0, R · V)^shininess
   - Creates glossy highlights
   - Reflection vector: R = 2(N · L)N - L
   - View vector (V): from surface to camera

**Final Color:** I_total = I_ambient * ObjectColor + I_diffuse * ObjectColor + I_specular

Where ObjectColor modulates ambient and diffuse, but not specular (highlights typically maintain light color)

**Shadow Integration:**

- If `IsLit()` returns `false`, only ambient component is added
- This creates realistic shadow regions with subtle ambient illumination

---

