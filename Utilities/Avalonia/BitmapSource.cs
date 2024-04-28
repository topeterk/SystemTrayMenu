// <copyright file="BitmapSource.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2024 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Versioning;
    using Avalonia;
    using Avalonia.Media.Imaging;
    using Avalonia.Platform;

#pragma warning disable SA1402 // File may only contain a single type

    public class BitmapSource : Avalonia.Media.Imaging.Bitmap
    {
        public BitmapSource(Stream stream)
            : base(stream)
        {
        }

#if WINDOWS
        public BitmapSource(PixelFormat format, AlphaFormat alphaFormat, nint data, PixelSize size, Vector dpi, int stride)
            : base(format, alphaFormat, data, size, dpi, stride)
        {
        }
#else
        public BitmapSource(string path)
            : base(path)
        {
        }
#endif

        public static implicit operator BitmapSource(RenderTargetBitmap source)
        {
            using (MemoryStream memoryStream = new())
            {
                source.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new BitmapSource(memoryStream);
            }
        }

#if WINDOWS
        [SupportedOSPlatform("Windows")]
        public static implicit operator BitmapSource(System.Drawing.Bitmap bitmap)
        {
            System.Drawing.Imaging.BitmapData bitmapdata = bitmap.LockBits(
                new (0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            BitmapSource result = new (
                PixelFormat.Bgra8888,
                Avalonia.Platform.AlphaFormat.Premul,
                bitmapdata.Scan0,
                new PixelSize(bitmapdata.Width, bitmapdata.Height),
                new Vector(96, 96),
                bitmapdata.Stride);
            bitmap.UnlockBits(bitmapdata);
            return result;
        }

        [SupportedOSPlatform("Windows")]
        public static implicit operator BitmapSource(System.Drawing.Icon icon)
        {
            System.Drawing.Bitmap bitmap = icon.ToBitmap();
            BitmapSource result = (BitmapSource)bitmap;
            bitmap.Dispose();
            return result;
        }
#endif
    }

    /// <summary>
    /// Loads an image (Bitmap) from local resources (avares://).
    /// </summary>
    internal class LocalResourceBitmap : BitmapSource
    {
        public LocalResourceBitmap(string path)
            : base(AssetLoader.Open(new Uri($"avares://{Assembly.GetEntryAssembly()!.GetName().Name!}{path}")))
        {
        }
    }
}
#endif
