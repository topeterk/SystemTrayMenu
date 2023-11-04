// <copyright file="BitmapSource.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System.IO;
    using Avalonia;
    using Avalonia.Media.Imaging;
    using Avalonia.Platform;

    public class BitmapSource : Avalonia.Media.Imaging.Bitmap
    {
        public BitmapSource(string fileName)
            : base(fileName)
        {
        }

        public BitmapSource(Stream stream)
            : base(stream)
        {
        }

        public BitmapSource(PixelFormat format, AlphaFormat alphaFormat, nint data, PixelSize size, Vector dpi, int stride)
            : base(format, alphaFormat, data, size, dpi, stride)
        {
        }

        protected BitmapSource(IBitmapImpl impl)
            : base(impl)
        {
        }

        internal int PixelWidth => PixelSize.Width;

        internal int PixelHeight => PixelSize.Height;

        internal double DpiX => Dpi.X;

        internal double DpiY => Dpi.Y;

        public static implicit operator BitmapSource(RenderTargetBitmap source)
        {
            using (MemoryStream memoryStream = new())
            {
                source.Save(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return new BitmapSource(memoryStream);
            }
        }

        internal void Freeze()
        {
        }
    }
}
#endif
