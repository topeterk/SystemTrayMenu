// <copyright file="AvaloniaExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// ===============================================================================
// MIT License
//
// Copyright (c) 2021-2023 Peter Kirmeier
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

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Reflection;
    using Avalonia;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.Markup.Xaml;
    using Avalonia.Media.Imaging;
    using Avalonia.Platform;

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    internal static class AvaloniaExtensions
    {
        internal static void Shutdown(this Application application)
        {
            ((IClassicDesktopStyleApplicationLifetime?)application.ApplicationLifetime)?.Shutdown(); // TODO
        }

        internal static object GetData(this IDataObject obj, string dataFormat) => obj.Get(dataFormat) ?? new (); // TODO

        internal static void Offset(this Point point, double x, double y) => point += new Vector(x, y);
    }

    /// <summary>
    /// Loads an image (Bitmap) from local resources (avares://).
    /// </summary>
    internal class LocalResourceBitmap : Bitmap
    {
        public LocalResourceBitmap(string path)
            : base(
            AvaloniaLocator.Current.GetService<IAssetLoader>()!.Open(
                new Uri($"avares://{Assembly.GetEntryAssembly()!.GetName().Name!}{path}")))
        {
        }
    }

#if TODO_LINUX // TODO: Delete?
    /// <summary>
    /// Should be replaced with DynamicResource.
    /// Workaround: Avalonia DataTriggerBehavior does not always work when page is loaded.
    ///             This class seems to work as kind of a proxy.
    ///             During first pass the DataTriggerBehavior is created and this object is instantiated,
    ///             however, the trigger will somehow not be fired.
    ///             During second pass the markup extension (ProvideValue) gets executed,
    ///             but this time the trigger was/gets fired as well.
    /// See: https://github.com/wieslawsoltes/AvaloniaBehaviors/issues/56
    /// See: https://stackoverflow.com/questions/68979876/how-to-simulate-datatrigger-with-avalonia .
    /// </summary>
    internal class LazyResource : MarkupExtension
    {
        internal LazyResource(string key) => Key = key;

        private string Key { get; init; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Application.Current?.Resources.ContainsKey(Key) ?? false)
            {
                return Application.Current.Resources[Key]!;
            }

            throw
        }
    }
#endif

    // TODO TODO TODO:
#pragma warning disable SA1201 // ElementsMustAppearInTheCorrectOrder
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

    internal class StartupTaskState
    {
    }
#pragma warning restore SA1201 // ElementsMustAppearInTheCorrectOrder
}
#endif
