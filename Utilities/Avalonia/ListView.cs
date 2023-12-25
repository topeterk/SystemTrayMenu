// <copyright file="ListView.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using Avalonia.Controls;

    public class ListView : Avalonia.Controls.ItemsRepeater
    {
        private object? selectedItem;

        internal object? SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
            }
        }

        internal double ActualHeight => DesiredSize.Height;

        internal double ActualWidth => DesiredSize.Width;

        internal IList SelectedItems
        {
            get
            {
                List<object?> list = new();
                if (selectedItem != null)
                {
                    list.Add(selectedItem);
                }

                return list;
            }
        }

        internal ItemCollection Items
        {
            get
            {
                var lv = new ItemsControl();
                lv.ItemsSource = ItemsSource;
                return lv.Items;
            }
            set => ItemsSource = value;
        }

        public void ScrollIntoView(int index)
        {
            var element = GetOrCreateElement(index);
            ((TopLevel?)VisualRoot)?.UpdateLayout();
            element.BringIntoView();
        }
    }
}
#endif
