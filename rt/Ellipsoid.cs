using System;

namespace rt
{
    /// <summary>
    /// Represents an ellipsoid - a 3D shape defined by three semi-axes of different lengths.
    /// Can be stretched along X, Y, and Z axes independently, making it versatile for modeling.
    /// A sphere is a special case where all semi-axes have equal length.
    /// </summary>
    public class Ellipsoid : Geometry
    {
        /// <summary>
        /// Center position of the ellipsoid in world coordinates.
        /// </summary>
        private Vector Center { get; }

        /// <summary>
        /// Semi-axes length scaling factors (rx, ry, rz) for each axis.
        /// Controls the ellipsoid's "stretch" in each dimension independently.
        /// For a unit sphere, this would be (1, 1, 1).
        /// </summary>
        private Vector SemiAxesLength { get; }

        /// <summary>
        /// Base radius multiplier applied after semi-axes scaling.
        /// Final radii along each axis = SemiAxesLength * Radius.
        /// </summary>
        private double Radius { get; }
        
        /// <summary>
        /// Rotation quaternion applied to the ellipsoid.
        /// Defaults to NONE (no rotation). Used for animating or orienting the ellipsoid.
        /// </summary>
        public Quaternion Rotation { get; set;  } = Quaternion.NONE;
        
        
        /// <summary>
        /// Creates an ellipsoid with custom material properties.
        /// </summary>
        /// <param name="center">Center position in world space.</param>
        /// <param name="semiAxesLength">Relative scaling factors for X, Y, Z axes.</param>
        /// <param name="radius">Base radius multiplier.</param>
        /// <param name="material">Material properties for lighting calculations.</param>
        /// <param name="color">Base color of the ellipsoid.</param>
        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        /// <summary>
        /// Creates an ellipsoid with default material derived from color.
        /// </summary>
        /// <param name="center">Center position in world space.</param>
        /// <param name="semiAxesLength">Relative scaling factors for X, Y, Z axes.</param>
        /// <param name="radius">Base radius multiplier.</param>
        /// <param name="color">Base color used to derive material properties.</param>
        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        /// <summary>
        /// Copy constructor - creates a new ellipsoid from an existing one.
        /// </summary>
        /// <param name="e">The ellipsoid to copy.</param>
        public Ellipsoid (Ellipsoid e) : this(new Vector(e.Center), new Vector(e.SemiAxesLength), e.Radius, new Material(e.Material), new Color(e.Color))
        {
        }


        /// <summary>
        /// Computes ray-ellipsoid intersection using the analytic method.
        /// Transforms the ray into local space to handle rotation, then solves the quadratic equation.
        /// Returns the closest valid intersection within [minDist, maxDist] range.
        /// 
        /// Implementation detail:
        /// - To reverse the *visual* rotation direction without modifying Quaternion,
        ///   we apply the forward rotation to bring the world ray into local space.
        /// - To keep intersections correct, we transform normals back using the conjugate (inverse).
        /// </summary>
        /// <param name="line">Ray to test for intersection.</param>
        /// <param name="minDist">Minimum valid distance (prevents self-intersection).</param>
        /// <param name="maxDist">Maximum valid distance (optimization/shadow testing).</param>
        /// <returns>Intersection with closest hit details, or Intersection.NONE if no valid hit.</returns>
        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            // World -> local: translate by center
            var localOrigin = new Vector(line.X0 - Center);
            var localDir = new Vector(line.Dx);

            // Apply the forward rotation here to reverse the perceived spin direction
            // This is paired with using the conjugate when rotating normals back.
            if (Rotation.W != 0 || Rotation.X != 1 || Rotation.Y != 0 || Rotation.Z != 0)
            {
                localOrigin.Rotate(Rotation);
                localDir.Rotate(Rotation);
            }

            // Define the semi-axes lengths of the ellipsoid
            var rx = SemiAxesLength.X * Radius;
            var ry = SemiAxesLength.Y * Radius;
            var rz = SemiAxesLength.Z * Radius;

            // Pre-calculate squared radii for efficiency
            var rx2 = rx * rx;
            var ry2 = ry * ry;
            var rz2 = rz * rz;

            // Calculate coefficients for the quadratic equation at² + bt + c = 0
            // using local space coordinates (after rotation)
            var a = (localDir.X * localDir.X / rx2) + (localDir.Y * localDir.Y / ry2) + (localDir.Z * localDir.Z / rz2);
            var b = 2.0 * ((localOrigin.X * localDir.X / rx2) + (localOrigin.Y * localDir.Y / ry2) + (localOrigin.Z * localDir.Z / rz2));
            var c = (localOrigin.X * localOrigin.X / rx2) + (localOrigin.Y * localOrigin.Y / ry2) + (localOrigin.Z * localOrigin.Z / rz2) - 1.0;

            // Calculate discriminant of the quadratic equation
            var discriminant = b * b - 4.0 * a * c;

            // If discriminant is negative, the ray does not intersect the ellipsoid
            if (discriminant < 0.0)
            {
                return Intersection.NONE;
            }

            // Calculate the two possible solutions (intersection distances) for t
            var sqrtDiscriminant = Math.Sqrt(discriminant);
            var t0 = (-b - sqrtDiscriminant) / (2.0 * a);  // Near intersection
            var t1 = (-b + sqrtDiscriminant) / (2.0 * a);  // Far intersection

            // Choose the closest valid intersection distance
            var t = t0;

            // If the near intersection is behind the ray's origin or too close,
            // try the far intersection
            if (t < minDist)
            {
                t = t1;
            }

            // If the chosen intersection is outside the valid range [minDist, maxDist],
            // then there is no valid intersection
            if (t < minDist || t > maxDist)
            {
                return Intersection.NONE;
            }

            // Calculate intersection point in world space (use original ray, not transformed)
            var position = line.CoordinateToPosition(t);

            // Calculate local position for normal calculation.
            // Because we used FORWARD rotation for world->local above, we must apply the SAME forward rotation
            // to the position - center to get the local-space position consistent with the quad solve.
            var localPos = new Vector(position - Center);
            if (Rotation.W != 0 || Rotation.X != 1 || Rotation.Y != 0 || Rotation.Z != 0)
            {
                localPos.Rotate(Rotation);
            }

            // The normal of an ellipsoid at a point is the gradient of its implicit equation:
            // N_local = (px / rx^2, py / ry^2, pz / rz^2)  (constant factor ignored)
            var normal = new Vector(
                localPos.X / rx2,
                localPos.Y / ry2,
                localPos.Z / rz2
            );
            normal.Normalize();

            // Transform normal back to world space: use the conjugate (inverse) of the rotation that we used
            // to map world->local. This pairs the transforms correctly and preserves correct shading/intersections.
            if (Rotation.W != 0 || Rotation.X != 1 || Rotation.Y != 0 || Rotation.Z != 0)
            {
                var invRotation = new Quaternion(Rotation.W, -Rotation.X, -Rotation.Y, -Rotation.Z);
                normal.Rotate(invRotation);
            }

            // Return the complete intersection data
            return new Intersection(true, true, this, line, t, normal, Material, Color);
        }
    }
}
