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

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.ApplicationLifetimes;
    using Avalonia.Input;
    using Avalonia.Interactivity;

    public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

    internal static class AvaloniaExtensions
    {
        internal static void Shutdown(this Application application)
        {
            ((IClassicDesktopStyleApplicationLifetime?)application.ApplicationLifetime)?.Shutdown();
        }

        internal static object GetData(this IDataObject obj, string dataFormat) => obj.Get(dataFormat) ?? new (); // TODO

        // TODO: Optimize out searching for childrn by datacontext
        internal static Control? FindControlByDataContext(this Panel parent, object? context)
        {
            foreach (Control item in parent.Children)
            {
                if (item.DataContext == context)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
#endif
