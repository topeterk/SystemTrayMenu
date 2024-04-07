// <copyright file="SolidBorder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2024-2024 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using Avalonia;
    using Avalonia.Rendering;

    public class SolidBorder : Avalonia.Controls.Border, ICustomHitTest
    {
        // Border will only return true on hit where child elements exists,
        // this makes sure to return true even when background is not actually "filled"
        // HitTest gets Pointer location relative to current instance's top left corner,
        // so we have to check agains our current bounds but without any offsets to parent control.
        public bool HitTest(Point point) => new Rect(new Size(Bounds.Width, Bounds.Height)).Contains(point);
    }
}
#endif
