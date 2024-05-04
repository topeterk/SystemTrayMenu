// <copyright file="ListView.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2024 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Avalonia;
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

        // The regular method tries to take up as much space as possible,
        // but actually want to take less possible space as possible.
        protected override Size MeasureOverride(Size availableSize)
        {
            Size size = base.MeasureOverride(availableSize);

            // Find larges child
            double maxChildWidth = 0;
            foreach (var element in Children)
            {
                maxChildWidth = Math.Max(maxChildWidth, element.DesiredSize.Width);
            }

            // Shrink until largest child fits
            if (maxChildWidth < size.Width)
            {
                size = size.WithWidth(maxChildWidth);
            }

            return size;
        }
    }
}
#endif
