// <copyright file="AppColors.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu
{
    using System;
#if !AVALONIA
    using System.Windows.Media;
#else
    using Avalonia.Media;
#endif
    using SystemTrayMenu.Properties;
    using SystemTrayMenu.Utilities;

    internal static class AppColors
    {
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
#if AVALONIA
            object converter = new(); // TODO: Remove
#else
            ColorConverter converter = new();
#endif
            ColorAndCode colorAndCode = default;
            bool resetDefaults = false;

            colorAndCode.HtmlColorCode = Settings.Default.ColorSelectedItem;
            colorAndCode.Color = AppColors.SelectedItem.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSelectedItem = colorAndCode.HtmlColorCode;
            AppColors.SelectedItem = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeSelecetedItem;
            colorAndCode.Color = AppColors.DarkModeSelecetedItem.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeSelecetedItem = colorAndCode.HtmlColorCode;
            AppColors.DarkModeSelecetedItem = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSelectedItemBorder;
            colorAndCode.Color = AppColors.SelectedItemBorder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSelectedItemBorder = colorAndCode.HtmlColorCode;
            AppColors.SelectedItemBorder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeSelectedItemBorder;
            colorAndCode.Color = AppColors.DarkModeSelectedItemBorder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeSelectedItemBorder = colorAndCode.HtmlColorCode;
            AppColors.DarkModeSelectedItemBorder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorOpenFolder;
            colorAndCode.Color = AppColors.OpenFolder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorOpenFolder = colorAndCode.HtmlColorCode;
            AppColors.OpenFolder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeOpenFolder;
            colorAndCode.Color = AppColors.DarkModeOpenFolder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeOpenFolder = colorAndCode.HtmlColorCode;
            AppColors.DarkModeOpenFolder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorOpenFolderBorder;
            colorAndCode.Color = AppColors.OpenFolderBorder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorOpenFolderBorder = colorAndCode.HtmlColorCode;
            AppColors.OpenFolderBorder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeOpenFolderBorder;
            colorAndCode.Color = AppColors.DarkModeOpenFolderBorder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeOpenFolderBorder = colorAndCode.HtmlColorCode;
            AppColors.DarkModeOpenFolderBorder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorIcons;
            colorAndCode.Color = AppColors.Icons.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorIcons = colorAndCode.HtmlColorCode;
            AppColors.Icons = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeIcons;
            colorAndCode.Color = AppColors.DarkModeIcons.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeIcons = colorAndCode.HtmlColorCode;
            AppColors.DarkModeIcons = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSearchField;
            colorAndCode.Color = AppColors.SearchField.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSearchField = colorAndCode.HtmlColorCode;
            AppColors.SearchField = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeSearchField;
            colorAndCode.Color = AppColors.DarkModeSearchField.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeSearchField = colorAndCode.HtmlColorCode;
            AppColors.DarkModeSearchField = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorBackground;
            colorAndCode.Color = AppColors.Background.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorBackground = colorAndCode.HtmlColorCode;
            AppColors.Background = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeBackground;
            colorAndCode.Color = AppColors.DarkModeBackground.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeBackground = colorAndCode.HtmlColorCode;
            AppColors.DarkModeBackground = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorBackgroundBorder;
            colorAndCode.Color = AppColors.BackgroundBorder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorBackgroundBorder = colorAndCode.HtmlColorCode;
            AppColors.BackgroundBorder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorDarkModeBackgroundBorder;
            colorAndCode.Color = AppColors.DarkModeBackgroundBorder.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorDarkModeBackgroundBorder = colorAndCode.HtmlColorCode;
            AppColors.DarkModeBackgroundBorder = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrow;
            colorAndCode.Color = AppColors.Arrow.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrow = colorAndCode.HtmlColorCode;
            AppColors.Arrow = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowHoverBackground;
            colorAndCode.Color = AppColors.ArrowHoverBackground.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowHoverBackground = colorAndCode.HtmlColorCode;
            AppColors.ArrowHoverBackground = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowHover;
            colorAndCode.Color = AppColors.ArrowHover.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowHover = colorAndCode.HtmlColorCode;
            AppColors.ArrowHover = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowClick;
            colorAndCode.Color = AppColors.ArrowClick.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowClick = colorAndCode.HtmlColorCode;
            AppColors.ArrowClick = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowClickBackground;
            colorAndCode.Color = AppColors.ArrowClickBackground.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowClickBackground = colorAndCode.HtmlColorCode;
            AppColors.ArrowClickBackground = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSlider;
            colorAndCode.Color = AppColors.Slider.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSlider = colorAndCode.HtmlColorCode;
            AppColors.Slider = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSliderHover;
            colorAndCode.Color = AppColors.SliderHover.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSliderHover = colorAndCode.HtmlColorCode;
            AppColors.SliderHover = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSliderDragging;
            colorAndCode.Color = AppColors.SliderDragging.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSliderDragging = colorAndCode.HtmlColorCode;
            AppColors.SliderDragging = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorScrollbarBackground;
            colorAndCode.Color = AppColors.ScrollbarBackground.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorScrollbarBackground = colorAndCode.HtmlColorCode;
            AppColors.ScrollbarBackground = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowDarkMode;
            colorAndCode.Color = AppColors.ArrowDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowDarkMode = colorAndCode.HtmlColorCode;
            AppColors.ArrowDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowHoverBackgroundDarkMode;
            colorAndCode.Color = AppColors.ArrowHoverBackgroundDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowHoverBackgroundDarkMode = colorAndCode.HtmlColorCode;
            AppColors.ArrowHoverBackgroundDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowHoverDarkMode;
            colorAndCode.Color = AppColors.ArrowHoverDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowHoverDarkMode = colorAndCode.HtmlColorCode;
            AppColors.ArrowHoverDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowClickDarkMode;
            colorAndCode.Color = AppColors.ArrowClickDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowClickDarkMode = colorAndCode.HtmlColorCode;
            AppColors.ArrowClickDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorArrowClickBackgroundDarkMode;
            colorAndCode.Color = AppColors.ArrowClickBackgroundDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorArrowClickBackgroundDarkMode = colorAndCode.HtmlColorCode;
            AppColors.ArrowClickBackgroundDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSliderDarkMode;
            colorAndCode.Color = AppColors.SliderDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSliderDarkMode = colorAndCode.HtmlColorCode;
            AppColors.SliderDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSliderHoverDarkMode;
            colorAndCode.Color = AppColors.SliderHoverDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSliderHoverDarkMode = colorAndCode.HtmlColorCode;
            AppColors.SliderHoverDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorSliderDraggingDarkMode;
            colorAndCode.Color = AppColors.SliderDraggingDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorSliderDraggingDarkMode = colorAndCode.HtmlColorCode;
            AppColors.SliderDraggingDarkMode = new SolidColorBrush(colorAndCode.Color);

            colorAndCode.HtmlColorCode = Settings.Default.ColorScrollbarBackgroundDarkMode;
            colorAndCode.Color = AppColors.ScrollbarBackgroundDarkMode.Color;
            ProcessColorAndCode(converter, ref colorAndCode, ref resetDefaults);
            Settings.Default.ColorScrollbarBackgroundDarkMode = colorAndCode.HtmlColorCode;
            AppColors.ScrollbarBackgroundDarkMode = new SolidColorBrush(colorAndCode.Color);

            if (save && resetDefaults)
            {
                Settings.Default.Save();
            }
        }

#if AVALONIA
        private static void ProcessColorAndCode(
            object obsolete, // TODO: Remove argument
            ref ColorAndCode colorAndCode,
            ref bool resetDefaults)
#else
        private static void ProcessColorAndCode(
            ColorConverter colorConverter,
            ref ColorAndCode colorAndCode,
            ref bool resetDefaults)
#endif
        {
            try
            {
#if AVALONIA
                Color? color = Color.Parse(colorAndCode.HtmlColorCode);
#else
                Color? color = (Color?)colorConverter.ConvertFromInvariantString(colorAndCode.HtmlColorCode);
#endif
                if (color != null)
                {
                    colorAndCode.Color = color.Value;
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"HtmlColorCode {colorAndCode.HtmlColorCode}", ex);
                colorAndCode.HtmlColorCode = System.Drawing.ColorTranslator.ToHtml(
                    System.Drawing.Color.FromArgb(colorAndCode.Color.R, colorAndCode.Color.G, colorAndCode.Color.B));
                resetDefaults = true;
            }
        }

        /// <summary>
        /// Helper class to process color settings.
        /// </summary>
        internal struct ColorAndCode
        {
            public Color Color { get; set; }

            public string HtmlColorCode { get; set; }
        }
    }
}
