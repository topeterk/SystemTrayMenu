// <copyright file="Mouse.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using Avalonia;
    using Avalonia.Input;

    internal static class Mouse
    {
        internal static Point GetPosition(IInputElement relativeTo)
        {
            return default(Point); // TODO
        }
    }
}
#endif
