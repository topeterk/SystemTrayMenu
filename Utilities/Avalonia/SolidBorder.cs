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
    using Avalonia.Threading;

    [PseudoClasses(":OpenAnimationStoryboard")]
    internal class SolidBorder : Avalonia.Controls.Border
    {
        internal static readonly StyledProperty<bool> HasHiddenFlagProperty =
            AvaloniaProperty.Register<SolidBorder, bool>(nameof(HasHiddenFlag), defaultValue: false);

        private IDisposable? animationTimer;
        private bool isAnimationRunning;

        ~SolidBorder()
        {
            animationTimer?.Dispose();
        }

        internal bool HasHiddenFlag
        {
            get => GetValue(HasHiddenFlagProperty);
            set => SetValue(HasHiddenFlagProperty, value);
        }

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
