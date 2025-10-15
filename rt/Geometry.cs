namespace rt
{
    /// <summary>
    /// Abstract base class for all geometric shapes in the ray tracer.
    /// Defines the common interface for ray-geometry intersection testing.
    /// Concrete implementations include spheres, planes, ellipsoids.
    /// </summary>
    public abstract class Geometry(Material material, Color color)
    {
        /// <summary>
        /// Base color of the geometry used in shading calculations.
        /// This color is modulated by lighting and material properties to produce the final pixel color.
        /// </summary>
        protected Color Color { get; } = color;

        /// <summary>
        /// Material properties defining how light interacts with this geometry's surface.
        /// Contains ambient, diffuse, specular components and shininess for Phong shading.
        /// </summary>
        protected Material Material { get; } = material;

        /// <summary>
        /// Convenience constructor that generates material properties from a color.
        /// Uses Material.FromColor to create balanced lighting with default ratios.
        /// Useful for quickly creating geometry without manually specifying materials.
        /// </summary>
        /// <param name="color">Base color used to derive both the surface color and material properties.</param>
        protected Geometry(Color color) : this(Material.FromColor(color), color)
        {
        }

        /// <summary>
        /// Computes the intersection between a ray and this geometry.
        /// Must be implemented by all concrete geometry types (sphere, plane, etc.).
        /// Returns the closest valid intersection within the specified distance range.
        /// </summary>
        /// <param name="line">The ray to test for intersection (origin and direction).</param>
        /// <param name="minDist">Minimum valid distance along the ray (prevents self-intersection artifacts).</param>
        /// <param name="maxDist">Maximum valid distance along the ray (for optimization and shadow testing).</param>
        /// <returns>Intersection object with hit details, or Intersection.NONE if no valid hit exists.</returns>
        public abstract Intersection GetIntersection(Line line, double minDist, double maxDist);
    }
}
