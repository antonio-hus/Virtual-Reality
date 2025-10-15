namespace rt
{
    /// <summary>
    /// Represents an infinite line in 3D space defined by a point and a direction.
    /// In ray tracing, rays are lines with parametric equation P(t) = X0 + t*Dx.
    /// Used for both camera rays and shadow rays in intersection calculations.
    /// </summary>
    public class Line
    {
        /// <summary>
        /// Origin point of the line (X0) - the starting position in 3D space.
        /// For camera rays, this is typically the camera position or a point on the image plane.
        /// For shadow rays, this is the surface intersection point.
        /// </summary>
        public Vector X0 { get; }

        /// <summary>
        /// Direction vector (Dx) - normalized vector indicating the line's direction.
        /// Always unit length after construction, ensuring consistent parametric behavior.
        /// For rays, this points from the camera through a pixel or toward a light source.
        /// </summary>
        public Vector Dx { get; }

        /// <summary>
        /// Creates a default line starting at origin (0,0,0) pointing along the positive X-axis.
        /// </summary>
        public Line()
        {
            X0 = new Vector(0.0, 0.0, 0.0);
            Dx = new Vector(1.0, 0.0, 0.0);
        }

        /// <summary>
        /// Creates a line passing through two points in 3D space.
        /// The direction is automatically normalized to unit length for consistent parameterization.
        /// </summary>
        /// <param name="x0">Starting point of the line (origin).</param>
        /// <param name="x1">Second point on the line - used to determine direction.</param>
        public Line(Vector x0, Vector x1)
        {
            X0 = new Vector(x0);
            Dx = new Vector(x1 - x0);
            Dx.Normalize();
        }

        /// <summary>
        /// Calculates a 3D position along the line using the parametric equation P(t) = X0 + t*Dx.
        /// The parameter t represents distance along the line (since Dx is normalized).
        /// t > 0 gives points in the direction of Dx, t < 0 gives points in opposite direction.
        /// Critical for ray-object intersection tests where t represents ray travel distance.
        /// </summary>
        /// <param name="t">Parameter value - distance from X0 along direction Dx.</param>
        /// <returns>3D position at parameter t along the line.</returns>
        public Vector CoordinateToPosition(double t)
        {
            return new Vector(Dx * t + X0);
        }
    }
}
