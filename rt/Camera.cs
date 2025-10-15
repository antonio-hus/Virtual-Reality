namespace rt
{
    /// <summary>
    /// Represents the virtual camera (eye) for ray tracing.
    /// Defines the viewing frustum, position, orientation, and projection parameters.
    /// Rays are cast from the camera position through pixels on the view plane into the scene.
    /// </summary>
    public class Camera(
        Vector position,
        Vector direction,
        Vector up,
        double viewPlaneDistance,
        double viewPlaneWidth,
        double viewPlaneHeight,
        double frontPlaneDistance,
        double backPlaneDistance)
    {
        /// <summary>
        /// Position of the camera in world coordinates.
        /// Origin point for all primary rays cast into the scene.
        /// </summary>
        public Vector Position {get; } = position;

        /// <summary>
        /// Direction the camera is pointing (view direction).
        /// Normalized during setup to ensure consistent ray calculations.
        /// Typically points toward the center of the scene or focal point.
        /// </summary>
        public Vector Direction {get; } = direction;

        /// <summary>
        /// Up vector defining camera orientation (which way is "up" for the camera).
        /// Used to construct the camera's coordinate system along with Direction.
        /// Orthogonalized during Normalize() to ensure perpendicularity.
        /// </summary>
        public Vector Up {get; private set;} = up;

        /// <summary>
        /// Distance from camera position to the view plane.
        /// The view plane is where image pixels are conceptually located.
        /// Affects field of view: smaller values = wider FOV, larger values = narrower FOV.
        /// </summary>
        public double ViewPlaneDistance {get; } = viewPlaneDistance;

        /// <summary>
        /// Width of the view plane in world units.
        /// Combined with ViewPlaneDistance determines horizontal field of view.
        /// Should match image aspect ratio with ViewPlaneHeight for undistorted rendering.
        /// </summary>
        public double ViewPlaneWidth {get; } = viewPlaneWidth;

        /// <summary>
        /// Height of the view plane in world units.
        /// Combined with ViewPlaneDistance determines vertical field of view.
        /// Should match image aspect ratio with ViewPlaneWidth for undistorted rendering.
        /// </summary>
        public double ViewPlaneHeight {get; } = viewPlaneHeight;

        /// <summary>
        /// Near clipping plane distance - minimum render distance from camera.
        /// Objects closer than this are not rendered (prevents ray self-intersection).
        /// Typically a small positive value like 0.1 or 1.0.
        /// </summary>
        public double FrontPlaneDistance {get; } = frontPlaneDistance;

        /// <summary>
        /// Far clipping plane distance - maximum render distance from camera.
        /// Objects beyond this distance are culled for performance.
        /// Sets the effective "draw distance" of the scene.
        /// </summary>
        public double BackPlaneDistance {get; } = backPlaneDistance;

        /// <summary>
        /// Normalizes and orthogonalizes the camera's coordinate system.
        /// Ensures Direction is unit length and Up is perpendicular to Direction.
        /// Uses Gram-Schmidt orthogonalization: Up = (Direction × Up) × Direction.
        /// Must be called after camera construction before rendering.
        /// </summary>
        public void Normalize()
        {
            Direction.Normalize();
            Up.Normalize();
            // Gram-Schmidt: compute right vector, then recompute orthogonal up vector
            Up = (Direction ^ Up) ^ Direction;
        }
    }
}
