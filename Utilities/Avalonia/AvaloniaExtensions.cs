// <copyright file="AvaloniaExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// ===============================================================================
// MIT License
//
// Copyright (c) 2021-2024 Peter Kirmeier
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#pragma warning disable SA1402 // File may only contain a single type

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Reflection;
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.Platform;

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    internal static class AvaloniaExtensions
    {
        internal static void Shutdown(this Application application)
        {
            ((IClassicDesktopStyleApplicationLifetime?)application.ApplicationLifetime)?.Shutdown();
        }

        internal static object GetData(this IDataObject obj, string dataFormat) => obj.Get(dataFormat) ?? new (); // TODO

        internal static void Offset(this Point point, double x, double y) => point += new Vector(x, y);
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

#pragma warning disable SA1201 // ElementsMustAppearInTheCorrectOrder
    internal enum MouseEvent
    {
        IconLeftMouseUp,
        IconLeftDoubleClick,
    }

    // TODO TODO TODO:
    internal enum MouseButtonState
    {
        Released = 0,
        Pressed = 1,
    }

    internal class MouseButtonEventArgs
    {
        internal bool Handled { get; set; }

        internal MouseButtonState LeftButton { get; }
    }

    internal class TextCompositionEventArgs
    {
        internal bool Handled { get; set; }

        internal string Text => "TODO: TextCompositionEventArgs.Text";
    }

#pragma warning restore SA1201 // ElementsMustAppearInTheCorrectOrder
}
#endif
