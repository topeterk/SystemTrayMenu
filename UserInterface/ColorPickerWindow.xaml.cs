// <copyright file="ColorPickerWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

namespace SystemTrayMenu.UserInterface
{
#if !AVALONIA
    using System;
    using System.Windows;
    using System.Windows.Media;
#else
    using Avalonia.Interactivity;
    using Avalonia.Media;
    using Window = SystemTrayMenu.Utilities.Window;
#endif

    /// <summary>
    /// Logic of ColorPickerWindow.xaml .
    /// </summary>
    public partial class ColorPickerWindow : Window
    {
#if AVALONIA
        public ColorPickerWindow()
        {
            // Dummy constructor to resolve this issue:
            // Avalonia warning AVLN:0005: XAML resource "avares://SystemTrayMenu/UserInterface/ColorPickerWindow.axaml" won't be reachable via runtime loader, as no public constructor was found
        }
#endif

        public ColorPickerWindow(string description, Color initialColor)
        {
            InitializeComponent();
#if TODO_AVALONIA
            if (Config.IsDarkMode())
            {
                ResourceDictionary resDict = new ();
                resDict.Source = new Uri("pack://application:,,,/ColorPicker;component/Styles/DefaultColorPickerStyle.xaml", UriKind.RelativeOrAbsolute);
                picker.Style = (Style)resDict["DefaultColorPickerStyle"];
            }
#endif

            Loaded += (_, _) =>
            {
                MinWidth = ActualWidth;
                MinHeight = ActualHeight;

                // Issue: Placement of picker child elements incorrect.
                //        Beyond initial layout updates it requires to have a fixed size set.
                //        But this will force us to manually update on resize events.
                // Workaround: Remove the fixed values and witch back to automatic size calculation afterwards.
                SizeChanged += UnsetSize;
                void UnsetSize(object sender, RoutedEventArgs e)
                {
                    SizeChanged -= UnsetSize;
                    picker.Width = double.NaN;
                    picker.Height = double.NaN;
                }
            };

#if !AVALONIA
            picker.SelectedColor = picker.SecondaryColor = initialColor;
#else
            picker.Color = initialColor;
#endif
            lblDescription.Content = description;
        }

#if !AVALONIA
        public Color SelectedColor => picker.SelectedColor;
#else
        public Color SelectedColor => picker.Color;
#endif

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) => Close();
    }
}
