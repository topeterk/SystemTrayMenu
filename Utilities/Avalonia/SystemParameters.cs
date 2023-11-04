// <copyright file="SystemParameters.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if !WINDOWS
namespace SystemTrayMenu.Utilities
{
    using SystemTrayMenu.DllImports;

    public static class SystemParameters
    {
        internal static double SmallIconWidth => 32D; // TODO!

        internal static double SmallIconHeight => 32D; // TODO!

        internal static double VirtualScreenHeight
        {
            get
            {
                double maxBottom = 0D;
                foreach (var screen in NativeMethods.Screen.Screens)
                {
                    if (maxBottom < screen.Bottom)
                    {
                        maxBottom = screen.Bottom;
                    }
                }

                return maxBottom;
            }
        }
    }
}
#endif
