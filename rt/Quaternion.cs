using System;

namespace rt;

/// <summary>
/// Represents a quaternion for 3D rotations - a 4D number system used to represent orientations.
/// Quaternions avoid gimbal lock and provide smooth interpolation for animations.
/// Stored as (w, x, y, z) where w is the real component and (x, y, z) are imaginary components.
/// </summary>
public class Quaternion(double w, double x, double y, double z)
{
    public static readonly Quaternion NONE = new(0, 1, 0, 0);
    
    /// <summary>
    /// Real (scalar) component of the quaternion.
    /// For rotations, W = cos(angle/2).
    /// </summary>
    public double W { get; set; } = w;
    
    /// <summary>
    /// X component of the imaginary (vector) part.
    /// For rotations, X = axis.X * sin(angle/2).
    /// </summary>
    public double X { get; set; } = x;
    
    /// <summary>
    /// Y component of the imaginary (vector) part.
    /// For rotations, Y = axis.Y * sin(angle/2).
    /// </summary>
    public double Y { get; set; } = y;
    
    /// <summary>
    /// Z component of the imaginary (vector) part.
    /// For rotations, Z = axis.Z * sin(angle/2).
    /// </summary>
    public double Z { get; set; } = z;

    /// <summary>
    /// Normalizes this quaternion to unit length - mutates this quaternion.
    /// Essential for valid rotations since only unit quaternions represent pure rotations.
    /// Returns this quaternion for method chaining.
    /// </summary>
    /// <returns>This quaternion after normalization.</returns>
    public Quaternion Normalize()
    {
        var a = Math.Sqrt(W*W+X*X+Y*Y+Z*Z);
        W /= a;
        X /= a;
        Y /= a;
        Z /= a;
        return this;
    }
    
    /// <summary>
    /// Creates a rotation quaternion from axis-angle representation.
    /// Uses half-angle formulas: q = (cos(θ/2), sin(θ/2) * axis).
    /// The axis should be normalized for correct results.
    /// </summary>
    /// <param name="angle">Rotation angle in radians (right-hand rule).</param>
    /// <param name="axis">Unit vector representing the rotation axis.</param>
    /// <returns>Unit quaternion representing the rotation.</returns>
    public static Quaternion FromAxisAngle(double angle, Vector axis)
    {
        // Convert axis-angle to quaternion using half-angle formulas
        var halfAngle = angle / 2.0;
        var sinHalf = Math.Sin(halfAngle);
        var cosHalf = Math.Cos(halfAngle);
        
        return new Quaternion(
            cosHalf,              // Real component
            axis.X * sinHalf,     // Imaginary X
            axis.Y * sinHalf,     // Imaginary Y
            axis.Z * sinHalf      // Imaginary Z
        );
    }
}
