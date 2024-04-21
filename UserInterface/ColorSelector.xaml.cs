// <copyright file="ColorSelector.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
#if !AVALONIA
    using System.Runtime.Versioning;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
#else
    using Avalonia.Controls;
    using Avalonia.Controls.Converters;
    using Avalonia.Controls.Presenters;
    using Avalonia.Media;
    using SystemTrayMenu.Utilities;
    using UserControl = SystemTrayMenu.Utilities.UserControl;
#endif

    /// <summary>
    /// Logic of ColorSelector .
    /// </summary>
    public partial class ColorSelector : UserControl
    {
#if AVALONIA
        private bool isChanging;
#endif

        public ColorSelector()
        {
            InitializeComponent();
            label.Content = string.Empty;

#if AVALONIA
            // Do some programmatic overrides as they cannot be overriden via Styles as values are set locally
            picker.TemplateApplied += (_, _) =>
            {
                // Search for: https://github.com/AvaloniaUI/Avalonia/blob/326ef7c9/src/Avalonia.Controls.ColorPicker/Themes/Fluent/ColorPicker.xaml#L40
                DropDownButton? dropdownbutton = picker.FindVisualChildOfType<DropDownButton>();
                if (dropdownbutton is not null)
                {
                    dropdownbutton.Padding = new(0);

                    // As we removed the PathIcon via Styles we have adjust some more settings
                    dropdownbutton.TemplateApplied += (_, _) =>
                    {
                        // Search for: https://github.com/AvaloniaUI/Avalonia/blob/326ef7c9/src/Avalonia.Themes.Fluent/Controls/DropDownButton.xaml#L42
                        ContentPresenter? presenter = dropdownbutton.FindVisualChildOfType<ContentPresenter>(); // PART_ContentPresenter
                        if (presenter is not null)
                        {
                            if (presenter.Content is Panel panel)
                            {
                                // https://github.com/AvaloniaUI/Avalonia/blob/326ef7c9/src/Avalonia.Controls.ColorPicker/Themes/Fluent/ColorPicker.xaml#L22-L31
                                Border? border = panel.FindVisualChildOfType<Border>(0);
                                if (border is not null)
                                {
                                    border.Margin = new(1);
                                    border.CornerRadius = new(3);
                                }

                                border = panel.FindVisualChildOfType<Border>(1);
                                if (border is not null)
                                {
                                    border.Margin = new(1);
                                    border.CornerRadius = new(3);
                                }
                            }
                        }
                    };
                }
            };
#endif
        }

        public event Action<ColorSelector>? ColorChanged;

        public string Text
        {
            get
            {
                try
                {
#if AVALONIA
                    Color color = Color.Parse(txtbox.Text.Trim());
#else
                    Color color = (Color)ColorConverter.ConvertFromString(txtbox.Text.Trim());
#endif

                    // Convertion passed, return string as it is valid
                    return txtbox.Text.Trim();
                }
                catch
                {
                    return Colors.White.ToString();
                }
            }

            set
            {
                txtbox.Text = value;
            }
        }

        public string Description
        {
            get => (string)label.Content;
            set => label.Content = value ?? string.Empty;
        }

        private void Txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
#if AVALONIA
            if (isChanging)
            {
                return;
            }

            Color color;
            try
            {
                color = Color.Parse(txtbox.Text ?? string.Empty);
            }
            catch
            {
                return;
            }

            isChanging = true;
            picker.Color = color;
            isChanging = false;
#else
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(txtbox.Text.Trim());
                pane.Background = new SolidColorBrush(color);
            }
            catch
            {
                return;
            }
#endif

            ColorChanged?.Invoke(this);
        }

#if AVALONIA
        private void Picker_ColorChanged(object? sender, ColorChangedEventArgs e)
        {
            if (isChanging)
            {
                return;
            }

            isChanging = true;
            Text = ColorToHexConverter.ToHexString(e.NewColor, AlphaComponentPosition.Leading, false, true);
            isChanging = false;

            ColorChanged?.Invoke(this);
        }
#endif

#if !AVALONIA
        [SupportedOSPlatform("windows")]
        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ColorPickerWindow dialog = new(Description, Colors.LightYellow);
                if (dialog.ShowDialog() ?? false)
                {
                    Text = dialog.SelectedColor.ToString();
                }

                e.Handled = true;
            }
        }
#endif
    }
}
