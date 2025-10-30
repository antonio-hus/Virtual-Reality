using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace rt
{
    /// <summary>
    /// Main entry point for the ray tracer application.
    /// Creates an animated scene with rotating ellipsoids and an orbital camera.
    /// Renders 90 frames in parallel for a complete 360-degree rotation.
    /// </summary>
    public static class Program
    {
        public static void Main(string[] args)
        {
            // ===== Cleanup =====
            // Clear any existing animation frames from previous renders
            const string frames = "frames";
            if (Directory.Exists(frames))
            {
                var d = new DirectoryInfo(frames);
                foreach (var file in d.EnumerateFiles("*.png")) {
                    file.Delete();
                }
            }
            Directory.CreateDirectory(frames);

            // ===== Scene Setup =====
            // Define all geometric objects with various shapes, positions, and colors
            var geometries = new Geometry[]
            {
                // Center reference sphere
                new Ellipsoid(new Vector(  0.0, -25.0, 100.0), new Vector(1.0, 1.0, 1.0), 5.0, Color.WHITE),
                
                // Horizontal red ellipsoids (stretched along X-axis)
                new Ellipsoid(new Vector( 15.0, -25.0, 100.0), new Vector(2.0, 0.5, 0.5), 5.0, Color.RED),
                new Ellipsoid(new Vector( 35.0, -25.0, 100.0), new Vector(2.0, 0.5, 0.5), 5.0, Color.RED),
                new Ellipsoid(new Vector( 55.0, -25.0, 100.0), new Vector(2.0, 0.5, 0.5), 5.0, Color.RED),
                
                // Vertical green ellipsoids (stretched along Y-axis)
                new Ellipsoid(new Vector(  0.0, -10.0, 100.0), new Vector(0.5, 2.0, 0.5), 5.0, Color.GREEN),
                new Ellipsoid(new Vector(  0.0,  10.0, 100.0), new Vector(0.5, 2.0, 0.5), 5.0, Color.GREEN),
                new Ellipsoid(new Vector(  0.0,  30.0, 100.0), new Vector(0.5, 2.0, 0.5), 5.0, Color.GREEN),
                
                // Depth blue ellipsoids (stretched along Z-axis)
                new Ellipsoid(new Vector(  0.0, -25.0, 115.0), new Vector(0.5, 0.5, 2.0), 5.0, Color.BLUE),
                new Ellipsoid(new Vector(  0.0, -25.0, 135.0), new Vector(0.5, 0.5, 2.0), 5.0, Color.BLUE),
                new Ellipsoid(new Vector(  0.0, -25.0, 155.0), new Vector(0.5, 0.5, 2.0), 5.0, Color.BLUE),
                
                // Flat ellipsoids demonstrating different orientations
                new Ellipsoid(new Vector( 35.0,  10.0, 100.0), new Vector(5.0, 5.0, 0.5), 5.0, Color.YELLOW),
                new Ellipsoid(new Vector(  0.0,  10.0, 135.0), new Vector(0.5, 5.0, 5.0), 5.0, Color.CYAN),
                new Ellipsoid(new Vector( 35.0, -25.0, 135.0), new Vector(5.0, 0.5, 5.0), 5.0, Color.MAGENTA),
                
                // Large orange sphere for scale reference
                new Sphere(   new Vector(-25.0, -50.0,  75.0),                           25.0, Color.ORANGE),
                
                // CT scan visualization - volumetric walnut rendering
                new CtScan("ctscan/walnut.dat", "ctscan/walnut.raw", new Vector(-5.0, -20.0, 105.0), 0.2,
                    new ColorMap()
                        .Add(1, 1, new Color(0.36, 0.26, 0.16, 0.1))  // Inner walnut material
                        .Add(2, 2, new Color(0.87, 0.72, 0.52, 0.8))  // Outer walnut shell
                ),  
            };

            // ===== Lighting Setup =====
            // Four point lights positioned around the scene for balanced illumination
            var lights = new []
            {
                new Light(new Vector( 65.0,  40.0,  90.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), 1.0),
                new Light(new Vector(-10.0,  40.0, 165.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), 1.0),
                new Light(new Vector( 65.0, -35.0, 165.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), 1.0),
                new Light(new Vector( 65.0,  40.0, 165.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), new Color(0.8, 0.8, 0.8, 1.0), 1.0)
            };

            // ===== Render Settings =====
            const int width = 800;
            const int height = 600;

            // ===== Camera Animation Setup =====
            // Orbit camera around the scene center using Rodrigues' rotation formula
            var middle = new Vector(0.0, -5.0, 100.0);    // Focal point - approximate scene center
            var up = new Vector(0, -1, 0).Normalize();    // Camera up vector
            var first = new Vector(0, 0, 1).Normalize();  // Initial camera direction
            const double dist = 95.0;                     // Distance from focal point to camera
            const int n = 90;                             // Number of frames (4 degrees per frame)
            const double step = 360.0 / n;                // Degrees per frame for full rotation
            
            // ===== Parallel Frame Rendering =====
            // Render all frames concurrently for efficiency
            var tasks = new Task[n];
            for (var i = 0; i < n; i++)
            {
                // Capture loop variable to avoid closure issues in parallel tasks
                var ind = new[]{i};
                tasks[i] = Task.Run(() =>
                {
                    var k = ind[0];
                    var a = (step * k) * Math.PI / 180.0;  // Convert frame angle to radians
                    
                    // Create ellipsoid rotation quaternion around (1,1,1) axis
                    var ellipsoidRotation = Quaternion.FromAxisAngle(a, new Vector(1, 1, 1).Normalize());
                    
                    // Calculate camera rotation using Rodrigues' formula
                    // Rotates 'first' vector around 'up' axis by angle 'a'
                    var ca =  Math.Cos(a);
                    var sa =  Math.Sin(a);
                    var dir = first * ca + (up ^ first) * sa + up * (up * first) * (1.0 - ca);
            
                    // Create camera at rotated position, looking toward center
                    var camera = new Camera(
                        middle - dir * dist,  // Camera position
                        dir,                  // Camera direction
                        up,                   // Camera up vector
                        65.0,                 // View plane distance
                        160.0,                // View plane width
                        120.0,                // View plane height
                        0.0,                  // Near clipping plane
                        1000.0                // Far clipping plane
                    );
                    
                    // Apply rotation to all ellipsoids in the scene
                    var rotatedGeometries = geometries.Select(g => g is not Ellipsoid e ? g : new Ellipsoid(e) {Rotation = ellipsoidRotation}).ToArray();
                    
                    // Generate filename with zero-padded frame number (001.png, 002.png, etc.)
                    var filename = frames+"/" + $"{k + 1:000}" + ".png";
            
                    // Render and save this frame
                    var rt = new RayTracer(rotatedGeometries, lights);
                    rt.Render(camera, width, height, filename);
                    Console.WriteLine($"Frame {k+1}/{n} completed");
                });
            }
            
            // Wait for all frames to complete before exiting
            Task.WaitAll(tasks);
        }
    }
}
