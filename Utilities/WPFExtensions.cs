// <copyright file="WPFExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2022-2023 Peter Kirmeier

namespace SystemTrayMenu.Utilities
{
    using System;
#if !AVALONIA
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
#else
    using System.Linq;
    using Avalonia;
    using Avalonia.Threading;
    using Avalonia.VisualTree;
    using DependencyObject = Avalonia.Visual;
    using DispatcherObject = Window;
#endif

    internal static class WPFExtensions
    {
#if !AVALONIA
        internal static Dispatcher CurrentDispatcher => Dispatcher.CurrentDispatcher;
#else
        internal static Dispatcher CurrentDispatcher => Dispatcher.UIThread;

        internal static void Invoke(this Dispatcher dispatcher, Action callback)
        {
            if (dispatcher.CheckAccess())
            {
                callback();
            }
            else
            {
                dispatcher.InvokeAsync(callback).Wait();
            }
        }

        internal static void Invoke(this Dispatcher dispatcher, Action callback, DispatcherPriority priority) => dispatcher.Post(callback, priority);

        internal static void Invoke(this Dispatcher dispatcher, DispatcherPriority priority, Action callback) => dispatcher.Post(callback, priority);
#endif

        internal static void HandleInvoke(this DispatcherObject instance, Action action)
        {
            if (instance!.CheckAccess())
            {
                action();
            }
            else
            {
                instance.Dispatcher.Invoke(action);
            }
        }

        internal static T? FindVisualChildOfType<T>(this DependencyObject depObj, int index = 0)
            where T : DependencyObject
        {
#if !AVALONIA
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
#else
            int i = -1;
            foreach (DependencyObject child in depObj.GetVisualDescendants().OfType<T>())
            {
                i++;
#endif
                if (child != null)
                {
                    if (child is T validChild)
                    {
                        if (index-- == 0)
                        {
                            return validChild;
                        }

                        continue;
                    }

                    T? childItem = child.FindVisualChildOfType<T>(index);
                    if (childItem != null)
                    {
                        return childItem;
                    }
                }
            }

            return null;
        }

        internal static Point GetRelativeChildPositionTo(this Visual parent, Visual? child)
        {
#if !AVALONIA
            return child == null ? default : child.TransformToAncestor(parent).Transform(default);
#else
            return default; // TODO: ??? https://github.com/AvaloniaUI/Avalonia/discussions/11969
#endif
        }

#if !AVALONIA
        internal static Visibility GetVisibility(this UIElement uIElement) => uIElement.Visibility;
#else
        internal static Visibility GetVisibility(this Visual visual) => visual.IsVisible ? Visibility.Visible : Visibility.Hidden;
#endif

#if !AVALONIA
        internal static void SetVisibility(this UIElement uIElement, Visibility visibility) => uIElement.Visibility = visibility;
#else
        internal static void SetVisibility(this Visual visual, Visibility visibility) => visual.IsVisible = visibility == Visibility.Visible;
#endif
    }
}
