namespace rt
{
    /// <summary>
    /// Represents a perfect sphere - a special case of an ellipsoid with equal radii in all directions.
    /// </summary>
    public class Sphere : Ellipsoid
    {
        /// <summary>
        /// Creates a sphere with specified center, radius, material, and color.
        /// </summary>
        /// <param name="center">The center point of the sphere in world coordinates.</param>
        /// <param name="radius">The radius of the sphere - distance from center to surface.</param>
        /// <param name="material">The material properties (diffuse, specular, reflective, etc.) affecting lighting calculations.</param>
        /// <param name="color">The base color of the sphere used in shading calculations.</param>
        public Sphere(Vector center, double radius, Material material, Color color) : base(center, new Vector(1, 1, 1), radius, material, color)
        {
        }

        /// <summary>
        /// Creates a sphere with specified center, radius, and color using default material properties.
        /// </summary>
        /// <param name="center">The center point of the sphere in world coordinates.</param>
        /// <param name="radius">The radius of the sphere - distance from center to surface.</param>
        /// <param name="color">The base color of the sphere used in shading calculations.</param>
        public Sphere(Vector center, double radius, Color color) : base(center, new Vector(1, 1, 1), radius, color)
        {
        }
    }
}
