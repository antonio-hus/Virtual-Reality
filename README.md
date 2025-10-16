# Virtual Reality Lab Assignment - Ray Tracer Implementation
**Student:** Antonio Hus, Group 934

***

## Overview

This project completes an implementation of a ray tracing engine in C# capable of rendering 3D scenes with realistic lighting, shadows, and support for various geometric primitives including volumetric data (CT scans) with alpha-composited transparency.

***

## Completed Implementation Tasks

### 1. Ellipsoid Ray Intersection (`Ellipsoid.cs`)

**Implementation Details:**

Implemented the ray-ellipsoid intersection algorithm using the **direct analytic method in world space**. The algorithm directly solves the intersection equation by substituting the parametric ray equation into the ellipsoid's implicit equation.

**Algorithm Steps:**

1. **Define Ellipsoid Parameters:** Extract actual semi-axes lengths (rx, ry, rz) by multiplying SemiAxesLength by Radius, and pre-compute squared values for efficiency
2. **Ray-to-Center Vector:** Calculate oc = ray_origin - ellipsoid_center
3. **Quadratic Coefficients:** Directly compute coefficients for at² + bt + c = 0 using the ellipsoid equation:
   - a = (dir.X²/rx²) + (dir.Y²/ry²) + (dir.Z²/rz²)
   - b = 2[(oc.X × dir.X/rx²) + (oc.Y × dir.Y/ry²) + (oc.Z × dir.Z/rz²)]
   - c = (oc.X²/rx²) + (oc.Y²/ry²) + (oc.Z²/rz²) - 1
4. **Discriminant Test:** Check if b² - 4ac ≥ 0 for valid intersections
5. **Distance Calculation:** Compute both solutions t₀ = (-b - √discriminant)/(2a) and t₁ = (-b + √discriminant)/(2a)
6. **Closest Valid Hit:** Choose t₀ (near intersection), or t₁ if t₀ < minDist, within [minDist, maxDist] range
7. **Normal Calculation:** Compute gradient of implicit function at intersection point: N = (localPos.X/rx², localPos.Y/ry², localPos.Z/rz²) then normalize

---

### 2. CT Scan Volume Rendering (`CtScan.cs`)

**Implementation Details:**

Implemented **volumetric ray marching with front-to-back alpha compositing** for rendering semi-transparent 3D CT scan data. This advanced technique allows viewing through outer semi-transparent layers to see interior structures, unlike simple surface detection.

**Algorithm Steps:**

1. **AABB Intersection:** Perform axis-aligned bounding box test to find volume entry/exit points
   - Test against all three plane pairs (X, Y, Z)
   - Calculate tMin (entry) and tMax (exit) parameters
   - Early exit if ray misses volume

2. **Ray Marching Setup:**
   - Step size: 0.5 × min(voxel thickness) for adequate sampling
   - Initialize accumulated color and alpha to zero
   - Start slightly offset from entry point to avoid boundary artifacts

3. **Front-to-Back Compositing:** March through volume accumulating semi-transparent layers:
   - At each step, sample voxel density and map to color with alpha
   - **Compositing formula:**
      - weight = color.Alpha × (1 - accumulatedAlpha)
      - accumulatedColor += color × weight
      - accumulatedAlpha += weight
   - Track first visible hit position for normal calculation
   - Continue until accumulatedAlpha ≥ 1 (opaque) or ray exits volume

4. **Intersection Return:** Return composited result with normal from first visible voxel using central difference gradient estimation
***

### 3. Shadow Ray Testing (`RayTracer.cs` - `IsLit` method)

**Implementation Details:**

Implemented shadow testing with **special handling for volumetric objects** to determine direct light visibility. Creates realistic hard shadows while treating CT scans as transparent.

**Algorithm:**

1. Construct shadow ray from surface point toward light position
2. Calculate exact distance to light source
3. Test for intersections using epsilon = 0.001 for minDist to prevent self-intersection
4. Limit maxDist to just before the light (distanceToLight - epsilon)
5. **CT Scan Exception:** If blocking object is a CtScan, ignore it and return true (treat as transparent)
6. Return false if any non-volumetric geometry occludes, true if path is clear

***

### 4. Phong Shading Rendering Pipeline (`RayTracer.cs` - `Render` method)

**Implementation Details:**

Implemented the complete rendering pipeline with proper Phong illumination model decomposition into ambient, diffuse, and specular components with shadow integration.

**Camera Ray Generation:**

1. Convert pixel indices to view plane coordinates centered at origin
2. Construct camera basis: Forward (Direction), Up, Right (Up × Direction)
3. Calculate view plane point: cameraPos + Direction × distance + Right × x + Up × y
4. Create ray from camera position through view plane point

**Phong Lighting Model (per light source):**

**1. Ambient Component** (always applied, outside shadow check):
```
I_ambient = material.Ambient × light.Ambient
```
Provides base illumination independent of geometry

**2. Shadow Test:** Call IsLit() to determine if light reaches surface

**3. Diffuse Component** (only if lit and facing light):
```
dotProduct = normal · lightDirection
if dotProduct > 0:
    I_diffuse = material.Diffuse × light.Diffuse × dotProduct
```
Implements Lambertian reflectance

**4. Specular Component** (only if lit and facing light):
```
reflectDir = 2(normal · lightDir) × normal - lightDir
specDot = max(0, reflectDir · viewDir)
I_specular = material.Specular × light.Specular × (specDot^shininess)
```
Creates glossy highlights using Phong reflection model

**Final Color Accumulation:**

Iterate through all lights, summing contributions. Each light adds its ambient component plus (if lit) diffuse and specular components. This multi-light accumulation creates realistic scenes with complex illumination.

***
