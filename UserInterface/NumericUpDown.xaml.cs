// <copyright file="NumericUpDown.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#if !AVALONIA
namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

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

        // For WPF "int" would be better type
        // Use "decimal?" for easier compatibility with Avalonia
        public decimal? Value
        {
            get => decimal.Parse(txtbox.Text);
            set
            {
                txtbox.Text = value.ToString();
            }
        }

        public decimal Minimum { get; set; } = decimal.MinValue;

        public decimal Maximum { get; set; } = decimal.MaxValue;

        public decimal Increment { get; set; } = 1;

        private static bool IsTextAllowed(string text) => !RegexNonNumeric.IsMatch(text);

        private void Txtbox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

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

        private void Txtbox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Value.HasValue)
            {
                if (e.Delta > 0)
                {
                    Value = Math.Min(Value.Value + Increment, Maximum);
                }
                else
                {
                    Value = Math.Max(Value.Value - Increment, Minimum);
                }
            }

            e.Handled = true;
        }

        private void Txtbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtbox.Text, out decimal result))
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

        private void ButtonUp_Click(object sender, RoutedEventArgs e) => Value = Value.HasValue ? Math.Min(Value.Value + Increment, Maximum) : null;

        private void ButtonDown_Click(object sender, RoutedEventArgs e) => Value = Value.HasValue ? Math.Max(Value.Value - Increment, Minimum) : null;
    }
}
#endif
