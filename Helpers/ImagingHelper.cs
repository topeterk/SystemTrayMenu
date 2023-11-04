// <copyright file="ImagingHelper.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Helpers
{
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
#if WINDOWS
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
#else
    using Avalonia;
    using Avalonia.Media;
    using Avalonia.Media.Imaging;
    using SystemTrayMenu.Utilities;
    using Bitmap = System.Drawing.Bitmap;
    using Size = System.Drawing.Size;
#endif

    /// <summary>
    /// Provides helper methods for imaging.
    /// </summary>
    internal static class ImagingHelper
    {
        /// <summary>
        /// Converts a PNG image to a icon (ico).
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="size">The size (16x16 px by default).</param>
        /// <param name="preserveAspectRatio">Preserve the aspect ratio.</param>
        /// <returns>Wether or not the icon was succesfully generated.</returns>
        internal static bool ConvertToIcon(Stream input, Stream output, int size = 16, bool preserveAspectRatio = false)
        {
            Bitmap inputBitmap = (Bitmap)Image.FromStream(input);
            if (inputBitmap != null)
            {
                int width, height;
                if (preserveAspectRatio)
                {
                    width = size;
                    height = inputBitmap.Height / inputBitmap.Width * size;
                }
                else
                {
                    width = height = size;
                }

                Bitmap newBitmap = new(inputBitmap, new Size(width, height));
                if (newBitmap != null)
                {
                    // save the resized png into a memory stream for future use
                    using MemoryStream memoryStream = new();
                    newBitmap.Save(memoryStream, ImageFormat.Png);

                    BinaryWriter iconWriter = new(output);
                    if (output != null && iconWriter != null)
                    {
                        // 0-1 reserved, 0
                        iconWriter.Write((byte)0);
                        iconWriter.Write((byte)0);

                        // 2-3 image type, 1 = icon, 2 = cursor
                        iconWriter.Write((short)1);

                        // 4-5 number of images
                        iconWriter.Write((short)1);

                        // image entry 1
                        // 0 image width
                        iconWriter.Write((byte)width);

                        // 1 image height
                        iconWriter.Write((byte)height);

                        // 2 number of colors
                        iconWriter.Write((byte)0);

                        // 3 reserved
                        iconWriter.Write((byte)0);

                        // 4-5 color planes
                        iconWriter.Write((short)0);

                        // 6-7 bits per pixel
                        iconWriter.Write((short)32);

                        // 8-11 size of image data
                        iconWriter.Write((int)memoryStream.Length);

                        // 12-15 offset of image data
                        iconWriter.Write(6 + 16);

                        // write image data
                        // png data must contain the whole png data file
                        iconWriter.Write(memoryStream.ToArray());

                        iconWriter.Flush();

                        return true;
                    }
                }

                return false;
            }

            return false;
        }

        /// <summary>
        /// Converts a PNG image to a icon (ico).
        /// </summary>
        /// <param name="inputPath">The input path.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="size">The size (16x16 px by default).</param>
        /// <param name="preserveAspectRatio">Preserve the aspect ratio.</param>
        /// <returns>Wether or not the icon was succesfully generated.</returns>
        internal static bool ConvertToIcon(string inputPath, string outputPath, int size = 16, bool preserveAspectRatio = false)
        {
            using FileStream inputStream = new(inputPath, FileMode.Open);
            using FileStream outputStream = new(outputPath, FileMode.OpenOrCreate);
            return ConvertToIcon(inputStream, outputStream, size, preserveAspectRatio);
        }

        /// <summary>
        /// Renders an image on top of an image.
        /// </summary>
        /// <param name="originalBitmap">Base image (remains unchanged).</param>
        /// <param name="overlayBitmap">Image on top (remains unchanged).</param>
        /// <returns>Rendered image.</returns>
        internal static RenderTargetBitmap CreateIconWithOverlay(BitmapSource originalBitmap, BitmapSource overlayBitmap)
        {
#if WINDOWS
            DrawingVisual dVisual = new ();
            using (DrawingContext dc = dVisual.RenderOpen())
#else
            // TODO: ???? https://stackoverflow.com/questions/66124655/drawing-a-geometry-onto-a-bitmap-avalonia
            Visual dVisual = new();
            RenderTargetBitmap targetBitmap = new(originalBitmap.PixelSize, originalBitmap.Dpi);
            using (DrawingContext dc = targetBitmap.CreateDrawingContext())
#endif
            {
                dc.DrawImage(originalBitmap, new(0, 0, originalBitmap.PixelWidth, originalBitmap.PixelHeight));
                dc.DrawImage(overlayBitmap, new(0, 0, originalBitmap.PixelWidth, originalBitmap.PixelHeight));
            }

#if WINDOWS
            RenderTargetBitmap targetBitmap = new (originalBitmap.PixelWidth, originalBitmap.PixelHeight, originalBitmap.DpiX, originalBitmap.DpiY, PixelFormats.Default);
#endif
            targetBitmap.Render(dVisual);
            return targetBitmap;
        }

        /// <summary>
        /// Sets a flat the alpha channel value for an image.
        /// </summary>
        /// <param name="originalBitmap">Base image (remains unchanged).</param>
        /// <param name="opacity">Opacity value.</param>
        /// <returns>Rendered image.</returns>
        internal static RenderTargetBitmap ApplyOpactiy(BitmapSource originalBitmap, double opacity)
        {
#if WINDOWS
            DrawingVisual dVisual = new ();
            using (DrawingContext dc = dVisual.RenderOpen())
#else
            // TODO: ???? https://stackoverflow.com/questions/66124655/drawing-a-geometry-onto-a-bitmap-avalonia
            Visual dVisual = new();
            RenderTargetBitmap targetBitmap = new(originalBitmap.PixelSize, originalBitmap.Dpi);
            using (DrawingContext dc = targetBitmap.CreateDrawingContext())
#endif
            {
                dc.PushOpacity(opacity);
                dc.DrawImage(originalBitmap, new(0, 0, originalBitmap.PixelWidth, originalBitmap.PixelHeight));
            }

#if WINDOWS
            RenderTargetBitmap targetBitmap = new(originalBitmap.PixelWidth, originalBitmap.PixelHeight, originalBitmap.DpiX, originalBitmap.DpiY, PixelFormats.Default);
#endif
            targetBitmap.Render(dVisual);
            return targetBitmap;
        }
    }
}
