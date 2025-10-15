namespace rt
{
    /// <summary>
    /// Represents a point light source in 3D space for ray tracing illumination.
    /// Emits light uniformly in all directions from a single position (like a light bulb).
    /// Provides ambient, diffuse, and specular components for Phong shading calculations.
    /// </summary>
    public class Light
    {
        /// <summary>
        /// Position of the light source in world coordinates.
        /// Used to calculate light direction vectors and distance for shadow rays.
        /// </summary>
        public Vector Position { get; }

        /// <summary>
        /// Ambient light component - constant illumination added to all surfaces.
        /// Simulates indirect bounced light and prevents completely dark areas.
        /// Applied uniformly regardless of surface orientation or position.
        /// </summary>
        public Color Ambient { get; }

        /// <summary>
        /// Diffuse light component - main directional lighting for matte surfaces.
        /// Intensity varies with angle between surface normal and light direction (Lambert's law).
        /// Determines the primary color and brightness of illuminated objects.
        /// </summary>
        public Color Diffuse { get; }

        /// <summary>
        /// Specular light component - creates bright highlights on glossy surfaces.
        /// Combined with material specular properties to produce shiny reflections.
        /// Typically white or the light's color for realistic rendering.
        /// </summary>
        public Color Specular { get; }

        /// <summary>
        /// Creates a default light at origin with no illumination (black light).
        /// </summary>
        public Light()
        {
            Position = new Vector();
            Ambient = new Color();
            Diffuse = new Color();
            Specular = new Color();
        }

        /// <summary>
        /// Creates a point light with specified position, color components, and intensity.
        /// </summary>
        /// <param name="position">3D position of the light source in world space.</param>
        /// <param name="ambient">Ambient light color - constant global illumination.</param>
        /// <param name="diffuse">Diffuse light color - directional illumination for matte surfaces.</param>
        /// <param name="specular">Specular light color - creates glossy highlights.</param>
        /// <param name="intensity">Light intensity multiplier (note: parameter not currently used in constructor).</param>
        public Light(Vector position, Color ambient, Color diffuse, Color specular, double intensity)
        {
            Position = new Vector(position);
            Ambient = new Color(ambient);
            Diffuse = new Color(diffuse);
            Specular = new Color(specular);
        }
    }
}
