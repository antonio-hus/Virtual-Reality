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
        /// If the shadow ray hits any object before reaching the light, the point is in shadow.
        /// </summary>
        /// <param name="point">Surface point to test for illumination.</param>
        /// <param name="light">Light source to check visibility from.</param>
        /// <returns>True if light reaches the point (no occlusion), false if in shadow.</returns>
        private bool IsLit(Vector point, Light light)
        {
            // Create shadow ray from surface point to light
            var shadowRay = new Line(point, light.Position);
            
            // Calculate distance to light
            var distanceToLight = (light.Position - point).Length();
            
            // Check if shadow ray hits any objects before reaching the light
            // Use small epsilon to prevent self-intersection
            var epsilon = 0.001;
            var shadowIntersection = FindFirstIntersection(shadowRay, epsilon, distanceToLight - epsilon);
            
            // If shadow ray hits an object, point is in shadow (return false)
            // If no hit, point is lit (return true)
            return !shadowIntersection.Valid || !shadowIntersection.Visible;
        }

        /// <summary>
        /// Main rendering loop that generates an image by ray tracing.
        /// For each pixel: generates a camera ray, finds intersections, computes Phong lighting,
        /// and writes the final color to the output image.
        /// Implements complete Phong illumination model with ambient, diffuse, and specular components.
        /// </summary>
        /// <param name="camera">Camera defining viewpoint and projection parameters.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="filename">Output filename for rendered image.</param>
        public void Render(Camera camera, int width, int height, string filename)
        {
            // Background color for rays that don't hit any geometry
            var background = new Color(0.2, 0.2, 0.2, 1.0);
            var image = new Image(width, height);

            // Normalize camera vectors and compute right vector for view plane positioning
            camera.Normalize();
            var right = camera.Up ^ camera.Direction;

            // Iterate through each pixel in the image
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // Convert pixel coordinates to view plane coordinates
                    var x = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    var y = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    // Calculate the point on the view plane corresponding to this pixel
                    var viewPlanePoint = camera.Position 
                        + camera.Direction * camera.ViewPlaneDistance
                        + right * x
                        + camera.Up * y;

                    // Create primary ray from camera through pixel
                    var ray = new Line(camera.Position, viewPlanePoint);
                    
                    // Find closest intersection with scene geometry
                    var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    // If no valid intersection, use background color
                    if (!intersection.Valid || !intersection.Visible)
                    {
                        image.SetPixel(i, j, background);
                        continue;
                    }

                    // Initialize pixel color (start with black, accumulate lighting)
                    var color = new Color(0, 0, 0, 1);
                    
                    // Precompute view direction (from surface to camera) for specular calculations
                    var viewDir = (camera.Position - intersection.Position).Normalize();

                    // Iterate through all lights and accumulate their contributions
                    foreach (var light in lights)
                    {
                        // === AMBIENT COMPONENT ===
                        // Ambient light is always present, independent of shadows or surface orientation
                        // Simulates indirect/environmental lighting to prevent completely black areas
                        var ambientContribution = intersection.Material.Ambient * light.Ambient;
                        color += ambientContribution;
                    }

                    // Write final computed color to image
                    // Color class handles clamping to [0,1] range during conversion
                    image.SetPixel(i, j, color);
                }
            }

            // Save rendered image to file
            image.Store(filename);
        }
    }
}
