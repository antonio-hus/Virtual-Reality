namespace rt
{
    /// <summary>
    /// Represents the result of a ray-geometry intersection test in ray tracing.
    /// Contains all information needed for shading calculations including position, normal, and material.
    /// Used to determine the closest visible surface hit by a ray and compute lighting at that point.
    /// </summary>
    public class Intersection
    {
        /// <summary>
        /// Sentinel value representing no intersection (ray missed all geometry).
        /// Used to avoid null checks and simplify ray tracing logic.
        /// </summary>
        public static readonly Intersection NONE = new();
            
        /// <summary>
        /// Indicates whether this represents a valid intersection.
        /// False for NONE or when ray misses geometry, true for actual hits.
        /// </summary>
        public bool Valid{ get; }

        /// <summary>
        /// Indicates whether the intersection point is visible (not occluded).
        /// Used in shadow ray tests - false means the point is in shadow.
        /// </summary>
        public bool Visible{ get; }

        /// <summary>
        /// Parameter t along the ray where intersection occurs (distance from ray origin).
        /// Used to determine which intersection is closest to the camera.
        /// Must be positive for intersections in front of the ray origin.
        /// </summary>
        public double T{ get; }

        /// <summary>
        /// 3D world position of the intersection point.
        /// Calculated as ray origin + t * ray direction.
        /// Used as the starting point for shadow rays and reflection rays.
        /// </summary>
        public Vector Position{ get; }

        /// <summary>
        /// Reference to the geometry object that was intersected.
        /// Allows access to object-specific properties and methods.
        /// </summary>
        public Geometry Geometry{ get; }

        /// <summary>
        /// The ray (line) that produced this intersection.
        /// Stores both the ray origin and direction for shading calculations.
        /// </summary>
        public Line Line{ get; }

        /// <summary>
        /// Surface normal vector at the intersection point.
        /// Always unit length, points outward from the surface.
        /// Essential for diffuse and specular lighting calculations (Phong shading).
        /// </summary>
        public Vector Normal { get; }

        /// <summary>
        /// Material properties of the intersected surface.
        /// Defines ambient, diffuse, specular components and shininess for shading.
        /// </summary>
        public Material Material { get; }

        /// <summary>
        /// Final computed color at this intersection point after lighting calculations.
        /// Initially set to object's base color, then modified by light contributions.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Private constructor for the NONE sentinel value.
        /// Creates an invalid intersection with null/default values.
        /// </summary>
        private Intersection() {
            Geometry = null;
            Line = null;
            Valid = false;
            Visible = false;
            T = 0;
            Position = null;
            Normal = null;
            Material = new();
            Color = new();
        }

        /// <summary>
        /// Creates a valid intersection with complete surface and shading information.
        /// Called when a ray successfully hits geometry and all intersection data is computed.
        /// </summary>
        /// <param name="valid">Whether this is a valid intersection (should be true for actual hits).</param>
        /// <param name="visible">Whether the intersection point is visible (not in shadow).</param>
        /// <param name="geometry">The geometry object that was hit.</param>
        /// <param name="line">The ray that produced this intersection.</param>
        /// <param name="t">Distance parameter along the ray to the intersection point.</param>
        /// <param name="normal">Surface normal at the intersection (must be normalized).</param>
        /// <param name="material">Material properties for lighting calculations.</param>
        /// <param name="color">Base color of the surface at the intersection point.</param>
        public Intersection(bool valid, bool visible, Geometry geometry, Line line, double t, Vector normal, Material material, Color color) {
            Geometry = geometry;
            Line = line;
            Valid = valid;
            Visible = visible;
            T = t;
            Normal = normal;
            Position = Line.CoordinateToPosition(t);
            Material = material;
            Color = color;
        }
    }
}
