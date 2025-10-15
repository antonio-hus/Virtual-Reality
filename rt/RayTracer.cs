using System;

namespace rt
{
    /// <summary>
    /// Core ray tracing engine that renders scenes by casting rays and computing lighting.
    /// Implements the Phong reflection model for realistic surface shading.
    /// Supports shadows, multiple lights, and various geometry types.
    /// </summary>
    class RayTracer(Geometry[] geometries, Light[] lights)
    {
        /// <summary>
        /// Converts image pixel coordinates to view plane coordinates.
        /// Maps from discrete pixel indices to continuous view plane positions.
        /// Centers the view plane so (0,0) pixel maps to the center of the plane.
        /// </summary>
        /// <param name="n">Pixel coordinate (column or row index).</param>
        /// <param name="imgSize">Total image dimension (width or height in pixels).</param>
        /// <param name="viewPlaneSize">Physical size of view plane in that dimension.</param>
        /// <returns>Position on the view plane corresponding to the pixel center.</returns>
        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        /// <summary>
        /// Finds the closest valid intersection of a ray with any geometry in the scene.
        /// Iterates through all geometry objects and returns the nearest hit within [minDist, maxDist].
        /// Used for both primary rays (camera to scene) and shadow rays (surface to light).
        /// </summary>
        /// <param name="ray">Ray to test for intersections.</param>
        /// <param name="minDist">Minimum intersection distance (prevents self-intersection artifacts).</param>
        /// <param name="maxDist">Maximum intersection distance (for optimization or shadow testing).</param>
        /// <returns>Closest intersection, or Intersection.NONE if no valid hit found.</returns>
        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = Intersection.NONE;

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        /// <summary>
        /// Determines if a point on a surface is illuminated by a specific light source.
        /// Casts a shadow ray from the surface point toward the light.
        /// Returns false if any geometry blocks the path (point is in shadow).
        /// </summary>
        /// <param name="point">Surface point to test for illumination.</param>
        /// <param name="light">Light source to check visibility from.</param>
        /// <returns>True if light reaches the point (no occlusion), false if in shadow.</returns>
        private bool IsLit(Vector point, Light light)
        {
            var lightDirection = (light.Position - point).Normalize();
            var lightDistance = (light.Position - point).Length();

            // Create ray from point in direction of light
            var shadowRay = new Line(point, lightDirection);
            var shadowIntersection = FindFirstIntersection(shadowRay, 0.0001, lightDistance - 0.0001);

            return !shadowIntersection.Valid;
        }

        /// <summary>
        /// Main rendering loop that generates an image by ray tracing.
        /// For each pixel: generates a camera ray, finds intersections, computes Phong lighting,
        /// and writes the final color to the output image.
        /// </summary>
        /// <param name="camera">Camera defining viewpoint and projection parameters.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="filename">Output filename for rendered image.</param>
        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color(0.2, 0.2, 0.2, 1.0);
            var image = new Image(width, height);

            camera.Normalize();
            var right = camera.Up ^ camera.Direction;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var x = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    var y = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    var viewPlanePoint = camera.Position 
                        + camera.Direction * camera.ViewPlaneDistance
                        + right * x
                        + camera.Up * y;

                    var ray = new Line(camera.Position, viewPlanePoint);
                    var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    if (!intersection.Valid || !intersection.Visible)
                    {
                        image.SetPixel(i, j, background);
                        continue;
                    }

                    var color = new Color(0, 0, 0, 1);

                    foreach (var light in lights)
                    {
                        var ambient = intersection.Material.Ambient * light.Ambient;
                        color += ambient;

                        var lightDir = (light.Position - intersection.Position).Normalize();
                        var normalDotLight = intersection.Normal * lightDir;
                        
                        // Check shadows before computing diffuse and specular
                        var isLit = IsLit(intersection.Position, light);
                        
                        if (normalDotLight > 0)
                        {
                            // Only add diffuse if not in shadow
                            if (isLit)
                            {
                                var diffuse = intersection.Material.Diffuse * light.Diffuse * normalDotLight;
                                color += diffuse;
                            }

                            // Compute specular reflection
                            var reflectionDir = (intersection.Normal * (2 * normalDotLight) - lightDir).Normalize();
                            var viewDir = (camera.Position - intersection.Position).Normalize();
                            var reflectionDotView = reflectionDir * viewDir;
                            
                            // Only add specular if not in shadow AND reflection is visible
                            if (reflectionDotView > 0 && isLit)
                            {
                                var specularIntensity = Math.Pow(reflectionDotView, intersection.Material.Shininess);
                                var specular = intersection.Material.Specular * light.Specular * specularIntensity;
                                color += specular;
                            }
                        }
                    }

                    image.SetPixel(i, j, color);
                }
            }

            image.Store(filename);
        }
    }
}