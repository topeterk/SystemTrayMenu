// <copyright file="SolidBorder.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2024-2024 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Controls.Metadata;
    using Avalonia.Rendering;
    using Avalonia.Threading;

    [PseudoClasses(":OpenAnimationStoryboard")]
    internal class SolidBorder : Avalonia.Controls.Border, ICustomHitTest
    {
        internal static readonly StyledProperty<bool> HasHiddenFlagProperty =
            AvaloniaProperty.Register<SolidBorder, bool>(nameof(HasHiddenFlag), defaultValue: false);

        private IDisposable? animationTimer;
        private bool isAnimationRunning;

        internal bool HasHiddenFlag
        {
            get => GetValue(HasHiddenFlagProperty);
            set => SetValue(HasHiddenFlagProperty, value);
        }

        // Border will only return true on hit where child elements exists,
        // this makes sure to return true even when background is not actually "filled"
        // HitTest gets Pointer location relative to current instance's top left corner,
        // so we have to check agains our current bounds but without any offsets to parent control.
        public bool HitTest(Point point) => new Rect(new Size(Bounds.Width, Bounds.Height)).Contains(point);

        internal void StartOpenAnimation()
        {
            if (!isAnimationRunning)
            {
                // By assigning the pseudo class, the animation starts playing
                // After the animation completed, the class gets reset that allows firing it again
                PseudoClasses.Set(":OpenAnimationStoryboard", isAnimationRunning = true);
                animationTimer = DispatcherTimer.RunOnce(
                    () => Dispatcher.UIThread.Post(() => PseudoClasses.Set(":OpenAnimationStoryboard", isAnimationRunning = false)),
                    TimeSpan.FromSeconds(1),
                    DispatcherPriority.Default);
            }
        }
    }
}
#endif
