using System;
using SkiaSharp;

namespace rt
{
    /// <summary>
    /// Represents an RGBA color with floating-point precision for ray tracing calculations.
    /// Components range from 0.0 to 1.0, allowing for HDR (high dynamic range) intermediate values
    /// that can exceed 1.0 before final clamping during image output.
    /// </summary>
    public class Color(double red, double green, double blue, double alpha)
    {
        // Predefined common colors for convenience in scene setup
        public static readonly Color NONE =    new Color(0.0, 0.0, 0.0, 0.0); 
        public static readonly Color RED =     new Color(1.0, 0.0, 0.0, 1.0); 
        public static readonly Color GREEN =   new Color(0.0, 1.0, 0.0, 1.0); 
        public static readonly Color BLUE =    new Color(0.0, 0.0, 1.0, 1.0); 
        public static readonly Color YELLOW =  new Color(1.0, 1.0, 0.0, 1.0); 
        public static readonly Color MAGENTA = new Color(1.0, 0.0, 1.0, 1.0); 
        public static readonly Color CYAN =    new Color(0.0, 1.0, 1.0, 1.0); 
        public static readonly Color WHITE =   new Color(1.0, 1.0, 1.0, 1.0); 
        public static readonly Color ORANGE =  new Color(1.0, 0.5, 0.0, 1.0);

        /// <summary>
        /// Red component - intensity from 0.0 (no red) to 1.0 (full red).
        /// Can temporarily exceed 1.0 during lighting calculations before clamping.
        /// </summary>
        private double Red { get; } = red;

        /// <summary>
        /// Green component - intensity from 0.0 (no green) to 1.0 (full green).
        /// Can temporarily exceed 1.0 during lighting calculations before clamping.
        /// </summary>
        private double Green { get; } = green;

        /// <summary>
        /// Blue component - intensity from 0.0 (no blue) to 1.0 (full blue).
        /// Can temporarily exceed 1.0 during lighting calculations before clamping.
        /// </summary>
        private double Blue { get; } = blue;

        /// <summary>
        /// Alpha (transparency) component - 0.0 is fully transparent, 1.0 is fully opaque.
        /// Currently not used in rendering (hardcoded to 255 in output).
        /// </summary>
        public double Alpha { get; } = alpha;

        /// <summary>
        /// Creates a default transparent black color (0, 0, 0, 0).
        /// </summary>
        public Color() : this(0, 0, 0, 0)
        {
        }

        /// <summary>
        /// Copy constructor - creates a new color with the same component values.
        /// </summary>
        /// <param name="c">Color to copy.</param>
        public Color(Color c) : this(c.Red, c.Green, c.Blue, c.Alpha)
        {
        }

        /// <summary>
        /// Converts the floating-point color to SkiaSharp's byte-based color format for image output.
        /// Clamps each component to the range [0, 255] to prevent overflow.
        /// Alpha is forced to 255 (fully opaque) regardless of the Alpha property.
        /// </summary>
        /// <returns>SKColor suitable for writing to image pixels.</returns>
        public SKColor ToSystemColor()
        {
            var r = (byte)Math.Min((int)Math.Ceiling(Red * 255), 255);
            var g = (byte)Math.Min((int)Math.Ceiling(Green * 255), 255);
            var b = (byte)Math.Min((int)Math.Ceiling(Blue * 255), 255);
            // var a = (byte)Math.Min((int)Math.Ceiling(Alpha * 255), 255);

            return new SKColor(r, g, b, 255);
        }

        /// <summary>
        /// Component-wise addition - combines light contributions from multiple sources.
        /// Used to accumulate lighting from ambient, diffuse, and specular components.
        /// </summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns>Sum of the two colors (can exceed 1.0 per component).</returns>
        public static Color operator +(Color a, Color b)
        {
            return new Color(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue, a.Alpha + b.Alpha);
        }

        /// <summary>
        /// Component-wise subtraction - rarely used in standard ray tracing.
        /// </summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <returns>Difference between the two colors.</returns>
        public static Color operator -(Color a, Color b)
        {
            return new Color(a.Red - b.Red, a.Green - b.Green, a.Blue - b.Blue, a.Alpha - b.Alpha);
        }

        /// <summary>
        /// Component-wise multiplication - modulates one color by another.
        /// Critical for combining material colors with light colors in shading calculations.
        /// Example: material diffuse color * light diffuse color.
        /// </summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color (modulator).</param>
        /// <returns>Product of the two colors component-wise.</returns>
        public static Color operator *(Color a, Color b)
        {
            return new Color(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue, a.Alpha * b.Alpha);
        }

        /// <summary>
        /// Component-wise division - rarely used in standard ray tracing.
        /// </summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color (divisor).</param>
        /// <returns>Quotient of the two colors component-wise.</returns>
        public static Color operator /(Color a, Color b)
        {
            return new Color(a.Red / b.Red, a.Green / b.Green, a.Blue / b.Blue, a.Alpha / b.Alpha);
        }

        /// <summary>
        /// Scalar multiplication - scales color intensity uniformly.
        /// Used for attenuation, intensity adjustments, and creating material variants.
        /// Example: base_color * 0.3 for diffuse component.
        /// </summary>
        /// <param name="c">Color to scale.</param>
        /// <param name="k">Scalar multiplier.</param>
        /// <returns>Scaled color.</returns>
        public static Color operator *(Color c, double k)
        {
            return new Color(c.Red * k, c.Green * k, c.Blue * k, c.Alpha * k);
        }

        /// <summary>
        /// Scalar division - reduces color intensity uniformly.
        /// Used for averaging colors or normalizing accumulated light.
        /// </summary>
        /// <param name="c">Color to divide.</param>
        /// <param name="k">Scalar divisor.</param>
        /// <returns>Divided color.</returns>
        public static Color operator /(Color c, double k)
        {
            return new Color(c.Red / k, c.Green / k, c.Blue / k, c.Alpha / k);
        }
    }
}
