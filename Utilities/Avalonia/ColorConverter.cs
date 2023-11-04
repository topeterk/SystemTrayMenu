// <copyright file="ColorConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using Avalonia.Media;

    internal class ColorConverter
    {
        internal static object ConvertFromString(string value) => Color.Parse(value);

        internal object ConvertFromInvariantString(string value) => Color.Parse(value);
    }
}
#endif
