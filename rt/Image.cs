using System.IO;
using SkiaSharp;

namespace rt
{
    /// <summary>
    /// Represents a 2D image buffer for ray tracing output.
    /// Provides pixel manipulation and file export functionality using SkiaSharp.
    /// The image serves as the render target where ray-traced colors are accumulated.
    /// </summary>
    public class Image(int width, int height)
    {
        /// <summary>
        /// Internal bitmap buffer storing pixel data.
        /// Uses SkiaSharp's SKBitmap for efficient pixel operations and encoding.
        /// </summary>
        private readonly SKBitmap _bitmap = new(width, height);

        /// <summary>
        /// Sets the color of a specific pixel in the image.
        /// Called for each ray traced through the scene to record the computed color.
        /// Coordinates follow standard image conventions: (0,0) at top-left.
        /// </summary>
        /// <param name="x">Horizontal pixel coordinate (0 to width-1).</param>
        /// <param name="y">Vertical pixel coordinate (0 to height-1).</param>
        /// <param name="c">Color to assign to the pixel, converted from ray tracer's Color format.</param>
        public void SetPixel(int x, int y, Color c) {
            _bitmap.SetPixel(x, y, c.ToSystemColor());
        }

        /// <summary>
        /// Saves the rendered image to disk as a PNG file.
        /// Called after all rays have been traced to export the final result.
        /// Uses lossless PNG encoding with quality parameter 0 (SkiaSharp ignores quality for PNG).
        /// </summary>
        /// <param name="filename">Output file path where the PNG image will be saved.</param>
        public void Store(string filename) {
            using var stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            using var image = SKImage.FromBitmap(_bitmap); 
            using var encodedImage = image.Encode(SKEncodedImageFormat.Png, 0); 
            encodedImage.SaveTo(stream);
        }
    }
}