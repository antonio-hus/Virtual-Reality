using System;

namespace rt
{
    /// <summary>
    /// Represents a 3D vector with X, Y, and Z components.
    /// </summary>
    public class Vector(double x, double y, double z)
    {
        // Standard basis vectors - useful for defining coordinate axes and camera orientation
        public static Vector I = new Vector(1, 0, 0);
        public static Vector J = new Vector(0, 1, 0);
        public static Vector K = new Vector(0, 0, 1);
        
        public double X { get; set; } = x;
        public double Y { get; set; } = y;
        public double Z { get; set; } = z;

        /// <summary>
        /// Initializes a zero vector (0, 0, 0).
        /// </summary>
        public Vector() : this(0, 0, 0)
        {
        }

        /// <summary>
        /// Copy constructor - creates a new vector from an existing one.
        /// </summary>
        /// <param name="v">The vector to copy.</param>
        public Vector(Vector v) : this(v.X, v.Y, v.Z)
        {
        }

        /// <summary>
        /// Vector addition - combines two vectors component-wise.
        /// Useful for translating points in space and combining direction vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Sum of the two vectors.</returns>
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Vector subtraction - computes the difference between two vectors.
        /// Essential for calculating ray directions and vectors between points.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Difference between the two vectors.</returns>
        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Dot product of two vectors - returns the scalar projection.
        /// Critical for lighting calculations (Lambert's law), reflection angles,
        /// and determining if vectors point in similar directions.
        /// Returns positive if vectors point in same general direction, negative if opposite.
        /// </summary>
        /// <param name="v">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Scalar dot product value.</returns>
        public static double operator *(Vector v, Vector b)
        {
            return v.X * b.X + v.Y * b.Y + v.Z * b.Z;
        }

        /// <summary>
        /// Cross product of two vectors - returns a vector perpendicular to both inputs.
        /// Used for calculating surface normals, constructing coordinate systems,
        /// and determining the orientation of triangles in mesh rendering.
        /// The magnitude equals the area of the parallelogram formed by the vectors.
        /// </summary>
        /// <param name="a">First vector.</param>
        /// <param name="b">Second vector.</param>
        /// <returns>Vector perpendicular to both input vectors.</returns>
        public static Vector operator ^(Vector a, Vector b)
        {
            return new Vector(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);
        }

        /// <summary>
        /// Scalar multiplication - scales a vector by a constant factor.
        /// Used for scaling ray directions, adjusting light intensity, and scaling distances.
        /// </summary>
        /// <param name="v">Vector to scale.</param>
        /// <param name="k">Scalar multiplier.</param>
        /// <returns>Scaled vector.</returns>
        public static Vector operator *(Vector v, double k)
        {
            return new Vector(v.X * k, v.Y * k, v.Z * k);
        }

        /// <summary>
        /// Scalar division - divides a vector by a constant factor.
        /// Useful for normalizing vectors and averaging positions.
        /// </summary>
        /// <param name="v">Vector to divide.</param>
        /// <param name="k">Scalar divisor.</param>
        /// <returns>Divided vector.</returns>
        public static Vector operator /(Vector v, double k)
        {
            return new Vector(v.X / k, v.Y / k, v.Z / k);
        }

        /// <summary>
        /// Component-wise multiplication - mutates this vector.
        /// Useful for color blending and texture mapping operations.
        /// </summary>
        /// <param name="k">Vector containing multipliers for each component.</param>
        public void Multiply(Vector k)
        {
            X *= k.X;
            Y *= k.Y;
            Z *= k.Z;
        }

        /// <summary>
        /// Component-wise division - mutates this vector.
        /// Used for inverse operations and texture coordinate transformations.
        /// </summary>
        /// <param name="k">Vector containing divisors for each component.</param>
        public void Divide(Vector k)
        {
            X /= k.X;
            Y /= k.Y;
            Z /= k.Z;
        }

        /// <summary>
        /// Returns the squared length (magnitude) of the vector.
        /// Useful for comparing distances without the expensive square root operation.
        /// Used in ray-sphere intersection tests and nearest-neighbor calculations.
        /// </summary>
        /// <returns>Squared magnitude of the vector.</returns>
        public double Length2()
        {
            return X * X + Y * Y + Z * Z;
        }

        /// <summary>
        /// Returns the length (magnitude) of the vector.
        /// Essential for normalizing vectors and calculating actual distances.
        /// </summary>
        /// <returns>Magnitude of the vector.</returns>
        public double Length()
        {
            return Math.Sqrt(Length2());
        }

        /// <summary>
        /// Normalizes the vector to unit length - mutates this vector.
        /// Critical for ensuring direction vectors are consistent in lighting calculations,
        /// surface normals, and ray directions. Returns this vector for method chaining.
        /// </summary>
        /// <returns>This vector after normalization.</returns>
        public Vector Normalize()
        {
            var norm = Length();
            if (norm > 0.0)
            {
                X /= norm;
                Y /= norm;
                Z /= norm;
            }
            return this;
        }
    }
}
