// <copyright file="NumericUpDown.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Text.RegularExpressions;
#if !AVALONIA
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
#else
    using System.Threading.Tasks;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using SystemTrayMenu.Utilities;
    using TextChangedEventArgs = Avalonia.Controls.TextChangedEventArgs;
    using UserControl = SystemTrayMenu.Utilities.UserControl;
#endif

    /// <summary>
    /// Logic of NumericUpDown .
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        // TODO: Not catched yet when something like "0--8" is entered
        private static readonly Regex RegexNonNumeric = new Regex("[^0-9-]+");
        private string lastTextOK;
        private bool withinChanged;

        public NumericUpDown()
        {
            InitializeComponent();
            lastTextOK = txtbox.Text;
        }

        public int Value
        {
            get => int.Parse(txtbox.Text);
            set
            {
                txtbox.Text = value.ToString();
            }
        }

        public int Minimum { get; set; } = int.MinValue;

        public int Maximum { get; set; } = int.MaxValue;

        public int Increment { get; set; } = 1;

        private static bool IsTextAllowed(string text) => !RegexNonNumeric.IsMatch(text);

        private void Txtbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

#if !AVALONIA
        private void Txtbox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                if (!IsTextAllowed((string)e.DataObject.GetData(typeof(string))))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
#else
        private async Task Txtbox_PastingAsync(object sender, RoutedEventArgs e)
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;

            if (clipboard != null)
            {
                string? text = await clipboard.GetTextAsync();
                if (!string.IsNullOrEmpty(text))
                {
                    if (!IsTextAllowed(text))
                    {
                        e.Handled = true;
                    }
                }
            }
        }
#endif

#if !AVALONIA
        private void Txtbox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            int deltaY = e.Delta;
#else
        private void Txtbox_PreviewMouseWheel(object sender, PointerWheelEventArgs e)
        {
            double deltaY = e.Delta.Y;
#endif
            if (deltaY > 0)
            {
                Value = Math.Min(Value + Increment, Maximum);
            }
            else
            {
                Value = Math.Max(Value - Increment, Minimum);
            }

            e.Handled = true;
        }

        private void Txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(txtbox.Text, out int result))
            {
                if (result < Minimum)
                {
                    lastTextOK = Minimum.ToString();
                }
                else if (Maximum < result)
                {
                    lastTextOK = Maximum.ToString();
                }
                else
                {
                    lastTextOK = txtbox.Text;
                    return;
                }
            }

            if (!withinChanged)
            {
                withinChanged = true;
                txtbox.Text = lastTextOK;
                withinChanged = false;
            }
        }

        private void ButtonUp_Click(object sender, RoutedEventArgs e) => Value = Math.Min(Value + Increment, Maximum);

        private void ButtonDown_Click(object sender, RoutedEventArgs e) => Value = Math.Max(Value - Increment, Minimum);
    }
}
