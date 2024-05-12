// <copyright file="AppColors.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
#if !AVALONIA
    using System.Windows.Media;
#else
    using Avalonia.Controls;
    using Avalonia.Controls.Converters;
    using Avalonia.Media;
#endif
    using SystemTrayMenu.Properties;
    using SystemTrayMenu.Utilities;

    internal static class AppColors
    {
#if !AVALONIA
        private static readonly ColorConverter ColorConverter = new();
#else
        /* -- Non-User controlled -- */

        // prefered CSS color value for links, see: https://html.spec.whatwg.org/multipage/rendering.html#phrasing-content-0
        public static SolidColorBrush Hyperlink => new(Color.FromRgb(0, 0, 238));
#endif

        /* -- General -- */

        public static SolidColorBrush SelectedItem { get; internal set; } = new(Color.FromRgb(204, 232, 255));

        public static SolidColorBrush DarkModeSelecetedItem { get; internal set; } = new (Color.FromRgb(51, 51, 51));

        public static SolidColorBrush SelectedItemBorder { get; internal set; } = new (Color.FromRgb(153, 209, 255));

        public static SolidColorBrush DarkModeSelectedItemBorder { get; internal set; } = new (Color.FromRgb(20, 29, 75));

        public static SolidColorBrush OpenFolder { get; internal set; } = new (Color.FromRgb(194, 245, 222));

        public static SolidColorBrush DarkModeOpenFolder { get; internal set; } = new (Color.FromRgb(20, 65, 42));

        public static SolidColorBrush OpenFolderBorder { get; internal set; } = new (Color.FromRgb(153, 255, 165));

        public static SolidColorBrush DarkModeOpenFolderBorder { get; internal set; } = new (Color.FromRgb(20, 75, 85));

        public static SolidColorBrush Background { get; internal set; } = new (Color.FromRgb(255, 255, 255));

        public static SolidColorBrush DarkModeBackground { get; internal set; } = new (Color.FromRgb(32, 32, 32));

        public static SolidColorBrush BackgroundBorder { get; internal set; } = new (Color.FromRgb(0, 0, 0));

        public static SolidColorBrush DarkModeBackgroundBorder { get; internal set; } = new (Color.FromRgb(0, 0, 0));

        public static SolidColorBrush SearchField { get; internal set; } = new (Color.FromRgb(255, 255, 255));

        public static SolidColorBrush DarkModeSearchField { get; internal set; } = new (Color.FromRgb(25, 25, 25));

        public static SolidColorBrush Icons { get; internal set; } = new (Color.FromRgb(149, 160, 166));

        public static SolidColorBrush DarkModeIcons { get; internal set; } = new (Color.FromRgb(149, 160, 166));

        /* -- ScrollBar -- */

        public static SolidColorBrush Arrow { get; internal set; } = new(Color.FromRgb(96, 96, 96));

        public static SolidColorBrush ArrowHoverBackground { get; internal set; } = new(Color.FromRgb(218, 218, 218));

        public static SolidColorBrush ArrowHover { get; internal set; } = new(Color.FromRgb(0, 0, 0));

        public static SolidColorBrush ArrowClick { get; internal set; } = new(Color.FromRgb(255, 255, 255));

        public static SolidColorBrush ArrowClickBackground { get; internal set; } = new(Color.FromRgb(96, 96, 96));

        public static SolidColorBrush Slider { get; internal set; } = new(Color.FromRgb(205, 205, 205));

        public static SolidColorBrush SliderHover { get; internal set; } = new(Color.FromRgb(166, 166, 166));

        public static SolidColorBrush SliderDragging { get; internal set; } = new(Color.FromRgb(96, 96, 96));

        public static SolidColorBrush ScrollbarBackground { get; internal set; } = new(Color.FromRgb(240, 240, 240));

        public static SolidColorBrush ArrowDarkMode { get; internal set; } = new(Color.FromRgb(103, 103, 103));

        public static SolidColorBrush ArrowHoverBackgroundDarkMode { get; internal set; } = new(Color.FromRgb(55, 55, 55));

        public static SolidColorBrush ArrowHoverDarkMode { get; internal set; } = new(Color.FromRgb(103, 103, 103));

        public static SolidColorBrush ArrowClickDarkMode { get; internal set; } = new(Color.FromRgb(23, 23, 23));

        public static SolidColorBrush ArrowClickBackgroundDarkMode { get; internal set; } = new(Color.FromRgb(166, 166, 166));

        public static SolidColorBrush SliderDarkMode { get; internal set; } = new(Color.FromRgb(77, 77, 77));

        public static SolidColorBrush SliderHoverDarkMode { get; internal set; } = new(Color.FromRgb(122, 122, 122));

        public static SolidColorBrush SliderDraggingDarkMode { get; internal set; } = new(Color.FromRgb(166, 166, 166));

        public static SolidColorBrush ScrollbarBackgroundDarkMode { get; internal set; } = new(Color.FromRgb(23, 23, 23));

        internal static void Initialize(bool save)
        {
            bool resetDefaults = false;

            // TODO: Remove obsolete colors?
            AppColors.SelectedItem = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSelectedItem), AppColors.SelectedItem, ref resetDefaults);
            AppColors.DarkModeSelecetedItem = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeSelecetedItem), AppColors.DarkModeSelecetedItem, ref resetDefaults);
            AppColors.SelectedItemBorder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSelectedItemBorder), AppColors.SelectedItemBorder, ref resetDefaults);
            AppColors.DarkModeSelectedItemBorder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeSelectedItemBorder), AppColors.DarkModeSelectedItemBorder, ref resetDefaults);
            AppColors.OpenFolder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorOpenFolder), AppColors.OpenFolder, ref resetDefaults);
            AppColors.DarkModeOpenFolder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeOpenFolder), AppColors.DarkModeOpenFolder, ref resetDefaults);
            AppColors.OpenFolderBorder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorOpenFolderBorder), AppColors.OpenFolderBorder, ref resetDefaults);
            AppColors.DarkModeOpenFolderBorder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeOpenFolderBorder), AppColors.DarkModeOpenFolderBorder, ref resetDefaults);
            AppColors.Icons = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorIcons), AppColors.Icons, ref resetDefaults);
            AppColors.DarkModeIcons = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeIcons), AppColors.DarkModeIcons, ref resetDefaults);
            AppColors.SearchField = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSearchField), AppColors.SearchField, ref resetDefaults);
            AppColors.DarkModeSearchField = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeSearchField), AppColors.DarkModeSearchField, ref resetDefaults);
            AppColors.Background = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorBackground), AppColors.Background, ref resetDefaults);
            AppColors.DarkModeBackground = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeBackground), AppColors.DarkModeBackground, ref resetDefaults);
            AppColors.BackgroundBorder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorBackgroundBorder), AppColors.BackgroundBorder, ref resetDefaults);
            AppColors.DarkModeBackgroundBorder = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorDarkModeBackgroundBorder), AppColors.DarkModeBackgroundBorder, ref resetDefaults);
            AppColors.Arrow = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrow), AppColors.Arrow, ref resetDefaults);
            AppColors.ArrowHoverBackground = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowHoverBackground), AppColors.ArrowHoverBackground, ref resetDefaults);
            AppColors.ArrowHover = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowHover), AppColors.ArrowHover, ref resetDefaults);
            AppColors.ArrowClick = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowClick), AppColors.ArrowClick, ref resetDefaults);
            AppColors.ArrowClickBackground = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowClickBackground), AppColors.ArrowClickBackground, ref resetDefaults);
            AppColors.Slider = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSlider), AppColors.Slider, ref resetDefaults);
            AppColors.SliderHover = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSliderHover), AppColors.SliderHover, ref resetDefaults);
            AppColors.SliderDragging = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSliderDragging), AppColors.SliderDragging, ref resetDefaults);
            AppColors.ScrollbarBackground = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorScrollbarBackground), AppColors.ScrollbarBackground, ref resetDefaults);
            AppColors.ArrowDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowDarkMode), AppColors.ArrowDarkMode, ref resetDefaults);
            AppColors.ArrowHoverBackgroundDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowHoverBackgroundDarkMode), AppColors.ArrowHoverBackgroundDarkMode, ref resetDefaults);
            AppColors.ArrowHoverDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowHoverDarkMode), AppColors.ArrowHoverDarkMode, ref resetDefaults);
            AppColors.ArrowClickDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowClickDarkMode), AppColors.ArrowClickDarkMode, ref resetDefaults);
            AppColors.ArrowClickBackgroundDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorArrowClickBackgroundDarkMode), AppColors.ArrowClickBackgroundDarkMode, ref resetDefaults);
            AppColors.SliderDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSliderDarkMode), AppColors.SliderDarkMode, ref resetDefaults);
            AppColors.SliderHoverDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSliderHoverDarkMode), AppColors.SliderHoverDarkMode, ref resetDefaults);
            AppColors.SliderDraggingDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorSliderDraggingDarkMode), AppColors.SliderDraggingDarkMode, ref resetDefaults);
            AppColors.ScrollbarBackgroundDarkMode = GetSettingsOrSetDefaultColor(nameof(Settings.Default.ColorScrollbarBackgroundDarkMode), AppColors.ScrollbarBackgroundDarkMode, ref resetDefaults);

            if (save && resetDefaults)
            {
                Settings.Default.Save();
            }
        }

        private static SolidColorBrush GetSettingsOrSetDefaultColor(string settingsName, SolidColorBrush defaultBrush, ref bool resetDefaults)
        {
            string? valueString = (string?)Convert.ChangeType(Settings.Default[settingsName], typeof(string));
            if (valueString is null)
            {
                Log.Info($"Could not read color \"{settingsName}\" from settings!");
            }
            else
            {
                try
                {
#if AVALONIA
                    Color? color = Color.Parse(valueString);
#else
                    Color? color = (Color?)ColorConverter.ConvertFromInvariantString(valueString);
#endif
                    if (color is not null)
                    {
                        return new(color.Value);
                    }

                    Log.Info($"Could not read color \"{settingsName}\" from settings!");
                }
                catch (Exception ex)
                {
                    Log.Warn($"Could not parse color \"{settingsName}\" from settings: \"{valueString}\"!", ex);
                }
#if AVALONIA
                Settings.Default[settingsName] = ColorToHexConverter.ToHexString(defaultBrush.Color, AlphaComponentPosition.Leading, false, true);
#else
                Settings.Default[settingsName] = System.Drawing.ColorTranslator.ToHtml(
                    System.Drawing.Color.FromArgb(defaultBrush.Color.R, defaultBrush.Color.G, defaultBrush.Color.B));
#endif
            }

            resetDefaults = true;
            return defaultBrush;
        }
    }
}
