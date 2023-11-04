// <copyright file="ColorSelector.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
#if WINDOWS
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
#else
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Media;
    using SystemTrayMenu.Utilities;
    using UserControl = SystemTrayMenu.Utilities.UserControl;
#endif

    /// <summary>
    /// Logic of ColorSelector .
    /// </summary>
    public partial class ColorSelector : UserControl
    {
        public ColorSelector()
        {
            InitializeComponent();
            label.Content = string.Empty;
        }

        public event Action<ColorSelector>? ColorChanged;

        public string Text
        {
            get
            {
                try
                {
                    Color color = (Color)ColorConverter.ConvertFromString(txtbox.Text.Trim());
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
            try
            {
                Color color = (Color)ColorConverter.ConvertFromString(txtbox.Text.Trim());
                pane.Background = new SolidColorBrush(color);
            }
            catch
            {
                return;
            }

            ColorChanged?.Invoke(this);
        }

#if WINDOWS
        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
#else
        private void Shape_PointerReleased(object sender, PointerReleasedEventArgs e)
#endif
        {
#if WINDOWS
            if (e.LeftButton == MouseButtonState.Pressed)
#else
            if (e.InitialPressMouseButton == MouseButton.Left)
#endif
            {
                ColorPickerWindow dialog = new(Description, Colors.LightYellow);
                if (dialog.ShowDialog() ?? false)
                {
                    Text = dialog.SelectedColor.ToString();
                }

                e.Handled = true;
            }
        }
    }
}
