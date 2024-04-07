﻿// <copyright file="Menu.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
#if !AVALONIA
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
#else
    using System.Drawing;
    using Avalonia;
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Avalonia.Media;
    using ReactiveUI;
    using FrameworkElement = Avalonia.Controls.Control;
    using ModifierKeys = Avalonia.Input.KeyModifiers;
    using MouseEventArgs = Avalonia.Input.PointerEventArgs;
    using Point = Avalonia.Point;
    using RoutingStrategy = Avalonia.Interactivity.RoutingStrategies;
    using Size = Avalonia.Size;
    using Window = SystemTrayMenu.Utilities.Window;
#endif
    using SystemTrayMenu.Business;
    using SystemTrayMenu.DataClasses;
    using SystemTrayMenu.DllImports;
    using SystemTrayMenu.Helpers;
    using SystemTrayMenu.Properties;
    using SystemTrayMenu.Utilities;

    /// <summary>
    /// Logic of Menu window.
    /// </summary>
    public partial class Menu : Window
    {
        private const int CornerRadiusConstant = 10;
#if TODO_AVALONIA // Fade Events
        private static readonly RoutedEvent FadeToTransparentEvent = EventManager.RegisterRoutedEvent(
            nameof(FadeToTransparent), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Menu));

        private static readonly RoutedEvent FadeInEvent = EventManager.RegisterRoutedEvent(
            nameof(FadeIn), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Menu));

        private static readonly RoutedEvent FadeOutEvent = EventManager.RegisterRoutedEvent(
            nameof(FadeOut), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Menu));
#endif

        private readonly string? folderPath;

        private int countLeftMouseButtonClicked;
        private bool isShellContextMenuOpen;
        private bool directionToRight;
        private Point lastLocation;

#if AVALONIA
        public Menu()
        {
            // Dummy constructor to resolve this issue:
            // Avalonia warning AVLN:0005: XAML resource "avares://SystemTrayMenu/UserInterface/Menu.axaml" won't be reachable via runtime loader, as no public constructor was found
        }
#endif

        internal Menu(RowData? rowDataParent, string? path)
        {
            InitializeComponent();

            if (!Config.ShowDirectoryTitleAtTop)
            {
                txtTitle.SetVisibility(Visibility.Collapsed);
            }

            if (!Config.ShowSearchBar)
            {
                searchPanel.SetVisibility(Visibility.Collapsed);
            }

            buttonOpenFolder.SetVisibility(Visibility.Collapsed);

            if (!Config.ShowFunctionKeySettings)
            {
                buttonSettings.SetVisibility(Visibility.Collapsed);
            }

            if (!Config.ShowFunctionKeyRestart)
            {
                buttonRestart.SetVisibility(Visibility.Collapsed);
            }

            folderPath = path;
            RowDataParent = rowDataParent;
            if (RowDataParent == null)
            {
                // This will be a main menu
                Level = 0;
                MainMenu = this;

                // Use Main Menu DPI for all further calculations
                Scaling.CalculateFactorByDpi(this);

                // Moving the window is only supported for the main menu
                MouseDown += MainMenu_MoveStart;

                if (!Config.ShowFunctionKeyPinMenu)
                {
                    buttonMenuAlwaysOpen.SetVisibility(Visibility.Collapsed);
                }
            }
            else
            {
                // This will be a sub menu
                if (ParentMenu == null)
                {
                    // Should never happen as each parent menu must have a valid entry which's owner is set
                    throw new ArgumentNullException(new (nameof(ParentMenu)));
                }

                Level = RowDataParent.Level + 1;
                MainMenu = ParentMenu.MainMenu;
                RowDataParent.SubMenu = this;

                buttonMenuAlwaysOpen.SetVisibility(Visibility.Collapsed);
                buttonSettings.SetVisibility(Visibility.Collapsed);
                buttonRestart.SetVisibility(Visibility.Collapsed);
            }

            string title;
            if (string.IsNullOrEmpty(path))
            {
                title = Translator.GetText("Directory empty");
            }
            else
            {
                title = new DirectoryInfo(path).Name;
                if (title.Length > MenuDefines.LengthMax)
                {
                    title = $"{title[..MenuDefines.LengthMax]}...";
                }
            }

#if DEBUG
            txtTitle.Text = Title = "v"
                + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.Major.ToString()
                + "."
                + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.Minor.ToString()
                + " - " + title;
#else
            txtTitle.Text = Title = title;
#endif

            foreach (FrameworkElement control in
                new List<FrameworkElement>()
                {
                    buttonMenuAlwaysOpen,
                    buttonOpenFolder,
                    buttonSettings,
                    buttonRestart,
                    pictureBoxSearch,
                    pictureBoxMenuAlwaysOpen,
                    pictureBoxOpenFolder,
                    pictureBoxSettings,
                    pictureBoxRestart,
                    pictureBoxLoading,
                })
            {
                control.Width = Scaling.Scale(control.Width);
                control.Height = Scaling.Scale(control.Height);
            }

            labelTitle.FontSize = Scaling.ScaleFontByPoints(8.25F);
            textBoxSearch.FontSize = Scaling.ScaleFontByPoints(8.25F);
            labelStatus.FontSize = Scaling.ScaleFontByPoints(7F);
#if TODO_AVALONIA
            dgv.FontSize = Scaling.ScaleFontByPoints(9F);
#endif

            textBoxSearch.TextChanged += (_, _) => TextBoxSearch_TextChanged(false);
            textBoxSearch.ContextMenu = new()
            {
                Background = MenuDefines.ColorSystemControlDefault,
            };
            textBoxSearch.ContextMenu.Items.Add(new MenuItem()
            {
                Header = Translator.GetText("To cut out"),
                Command = new ActionCommand((_) => textBoxSearch.Cut()),
            });
            textBoxSearch.ContextMenu.Items.Add(new MenuItem()
            {
                Header = Translator.GetText("Copy"),
#if !AVALONIA
                Command = new ActionCommand((_) => Clipboard.SetData(DataFormats.Text, textBoxSearch.SelectedText)),
#else
                Command = ReactiveCommand.CreateFromTask(async () =>
                    {
                        if (GetTopLevel(this)?.Clipboard is { } clipboard)
                        {
                            await clipboard.SetTextAsync(textBoxSearch.SelectedText);
                        }
                    }),
#endif
            });
            textBoxSearch.ContextMenu.Items.Add(new MenuItem()
            {
                Header = Translator.GetText("To paste"),
#if !AVALONIA
                Command = new ActionCommand((_) =>
                    {
                        if (Clipboard.ContainsText(TextDataFormat.Text))
                        {
                            textBoxSearch.SelectedText = Clipboard.GetData(DataFormats.Text).ToString();
                        }
                    }),
#else
                Command = new ActionCommand((_) => textBoxSearch.Paste()),
#endif
            });
            textBoxSearch.ContextMenu.Items.Add(new MenuItem()
            {
                Header = Translator.GetText("Undo"),
                Command = new ActionCommand((_) => textBoxSearch.Undo()),
            });
            textBoxSearch.ContextMenu.Items.Add(new MenuItem()
            {
                Header = Translator.GetText("Selecting All"),
                Command = new ActionCommand((_) => textBoxSearch.SelectAll()),
            });

            Loaded += (_, _) =>
            {
                // This will remove the outer padding from the ListView's Control Template
                Border? dgv_border = dgv.FindVisualChildOfType<Border>(0);
                if (dgv_border != null)
                {
                    dgv_border.Padding = new(0);
                }

                NativeMethods.HideFromAltTab(this);

#if TODO_AVALONIA // Fade Events
                RaiseEvent(new(routedEvent: FadeInEvent));
#endif
                FocusTextBox();
            };

            Closed += (_, _) =>
            {
                if (RowDataParent?.SubMenu == this)
                {
                    RowDataParent.SubMenu = null;
                }

#if !AVALONIA
                foreach (RowData item in dgv.Items.SourceCollection)
#else
                // TODO: SourceCollection
                foreach (RowData item in dgv.Items)
#endif
                {
                    item.SubMenu?.Close();
                }
            };

#if !AVALONIA
            IsVisibleChanged += (_, _) => VisibilityChanged?.Invoke(this);
#endif

            bool isTouchEnabled = NativeMethods.IsTouchEnabled();
            if ((isTouchEnabled && Settings.Default.DragDropItemsEnabledTouch) ||
                (!isTouchEnabled && Settings.Default.DragDropItemsEnabled))
            {
#if !AVALONIA
                AllowDrop = true;
                DragEnter += DragDropHelper.DragEnter;
                Drop += DragDropHelper.DragDrop;
#else
                // TODO: AllowDrop = true;
                AddHandler(DragDrop.DragOverEvent, DragDropHelper.DragEnter);
                AddHandler(DragDrop.DropEvent, DragDropHelper.DragDrop);
#endif
            }
        }

        internal event Action<RowData>? StartLoadSubMenu;

        internal event Action? MenuScrolled;

        internal event Action<Menu, Key, ModifierKeys>? CmdKeyProcessed;

        internal event Action? SearchTextChanging;

        internal event Action<Menu, bool, bool>? SearchTextChanged;

        internal event Action<RowData>? RowSelectionChanged;

        internal event Action<RowData>? CellMouseEnter;

        internal event Action? CellMouseLeave;

        internal event Action<RowData>? CellMouseDown;

        internal event Action<RowData>? CellOpenOnClick;

        internal event Action<Menu>? VisibilityChanged;

#if TODO_AVALONIA // Fade Events
        internal event RoutedEventHandler FadeToTransparent
        {
            add { AddHandler(FadeToTransparentEvent, value); }
            remove { RemoveHandler(FadeToTransparentEvent, value); }
        }

        internal event RoutedEventHandler FadeIn
        {
            add { AddHandler(FadeInEvent, value); }
            remove { RemoveHandler(FadeInEvent, value); }
        }

        internal event RoutedEventHandler FadeOut
        {
            add { AddHandler(FadeOutEvent, value); }
            remove { RemoveHandler(FadeOutEvent, value); }
        }
#endif

        internal enum StartLocation
        {
            Point,
            Predecessor,
            BottomLeft,
            BottomRight,
            TopRight,
        }

        internal Point Location => new (Left, Top); // TODO WPF Replace Forms wrapper

        internal int Level { get; set; }

        internal RowData? RowDataParent { get; set; }

        internal RowData? SelectedItem
        {
            get => (RowData?)dgv.SelectedItem;
            set => dgv.SelectedItem = value;
        }

        internal Menu MainMenu { get; private set; }

        internal Menu? ParentMenu => RowDataParent?.Owner;

        internal Menu? SubMenu
        {
            get
            {
#if !AVALONIA
                foreach (RowData rowData in dgv.Items.SourceCollection)
#else
                // TODO: SourceCollection
                foreach (RowData rowData in dgv.Items)
#endif
                {
                    if (rowData.SubMenu != null)
                    {
                        return rowData.SubMenu;
                    }
                }

                return null;
            }
        }

        internal bool RelocateOnNextShow { get; set; } = true;

        public override string ToString() => nameof(Menu) + " L" + Level.ToString() + ": " + Title;

        internal void RiseItemExecuted(RowData rowData)
        {
#if TODO_AVALONIA
            ListViewItem? lvi;
            int i = 0;
            while ((lvi = dgv.FindVisualChildOfType<ListViewItem>(i++)) != null)
            {
                if (lvi.Content == rowData)
                {
                    Border? border_outer = lvi.FindVisualChildOfType<Border>();
                    Border? border_inner = border_outer?.FindVisualChildOfType<Border>();
                    border_inner?.BeginStoryboard((Storyboard)dgv.FindResource("OpenAnimationStoryboard"));

                    return;
                }
            }
#endif
        }

        internal void RiseItemOpened(RowData rowData) => CellOpenOnClick?.Invoke(rowData);

        internal void RiseStartLoadSubMenu(RowData rowData) => StartLoadSubMenu?.Invoke(rowData);

        internal void ResetSearchText()
        {
            textBoxSearch.Text = string.Empty;
            if (dgv.Items.Count > 0)
            {
#if !AVALONIA
                dgv.ScrollIntoView(dgv.Items[0]);
#else
                dgv.ScrollIntoView(0);
#endif
            }
        }

        internal void OnWatcherUpdate()
        {
            TextBoxSearch_TextChanged(true);
            if (dgv.Items.Count > 0)
            {
#if !AVALONIA
                dgv.ScrollIntoView(dgv.Items[0]);
#else
                dgv.ScrollIntoView(0);
#endif
            }
        }

        internal void FocusTextBox(bool selectAll = false)
        {
            if (selectAll)
            {
                textBoxSearch.SelectAll();
            }
            else
            {
                textBoxSearch.CaretIndex = string.IsNullOrEmpty(textBoxSearch.Text) ? 0 : textBoxSearch.Text.Length; // On Linux: Text may be null?!
            }

            textBoxSearch.Focus();
        }

        // TODO: Check if we can just use original IsMouseOver instead?  (Check if it requires Mouse.Capture(this))
#if !AVALONIA
        internal new bool IsMouseOver()
#else
        internal bool IsMouseOver()
#endif
        {
            Point mousePos = NativeMethods.Screen.CursorPosition;
            bool isMouseOver = this.GetVisibility() == Visibility.Visible &&
                mousePos.X >= 0 && mousePos.X < Width &&
                mousePos.Y >= 0 && mousePos.Y < Height;
            return isMouseOver;
        }

        internal ListView GetDataGridView() => dgv; // TODO WPF Replace Forms wrapper

        // Not used as refreshing should be done automatically due to databinding
        // TODO: As long as WPF transition from Forms is incomplete, keep it for testing.
        internal void RefreshDataGridView()
        {
#if TODO_AVALONIA // maybe not required any more?
            ((CollectionView)CollectionViewSource.GetDefaultView(dgv.ItemsSource)).Refresh();
#endif
        }

        // TODO: Check if it is implicitly already running due to SelectionChanged event
        //       In case it is needed, run it within HideWithFade/ShowWithFade?
        internal void RefreshSelection() => ListView_SelectionChanged(GetDataGridView(), null);

        internal bool TrySelectAt(int index, int indexAlternative = -1)
        {
            RowData itemData;
            if (index >= 0 && dgv.Items.Count > index)
            {
                itemData = (RowData)dgv.Items[index];
            }
            else if (indexAlternative >= 0 && dgv.Items.Count > indexAlternative)
            {
                itemData = (RowData)dgv.Items[indexAlternative];
#if AVALONIA
                index = indexAlternative;
#endif
            }
            else
            {
                return false;
            }

            dgv.SelectedItem = itemData;
#if !AVALONIA
            dgv.ScrollIntoView(itemData);
#else
            dgv.ScrollIntoView(index);
#endif

            RowSelectionChanged?.Invoke(itemData);

            return true;
        }

        internal void AddItemsToMenu(List<RowData> data, MenuDataDirectoryState? state)
        {
            for (int index = 0; index < data.Count; index++)
            {
                RowData rowData = data[index];
                rowData.RowIndex = index;
                rowData.Owner = this;
                rowData.SortIndex = rowData.IsAdditionalItem && Settings.Default.ShowOnlyAsSearchResult ? 99 : 0;
            }

            dgv.ItemsSource = data;

            // TODO: Replace filter logic?
            // See: https://learn.microsoft.com/en-us/windows/apps/design/controls/items-repeater#sorting-filtering-and-resetting-the-data
            // See: https://www.answeroverflow.com/m/1090360510458888252
#if TODO_AVALONIA
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(dgv.ItemsSource);
            view.Filter = (object item) => Filter_Default((RowData)item);
#endif

            if (state != null)
            {
                SetSubMenuState(state.Value);
            }
        }

        internal void ActivateWithFade(bool recursive)
        {
            if (recursive)
            {
                SubMenu?.ActivateWithFade(true);
            }

            if (Opacity != 1D)
            {
#if TODO_AVALONIA // Fade Events
                if (Settings.Default.UseFading)
                {
                    RaiseEvent(new(routedEvent: FadeInEvent));
                }
                else
#endif
                {
                    Opacity = 1D;
                }
            }
        }

        internal void ShowWithFade(bool transparency, bool recursive)
        {
            if (recursive)
            {
                SubMenu?.ShowWithFade(transparency, true);
            }

            if (Level > 0)
            {
                ShowActivated = false;
            }

            Opacity = 0D;
            Show();

            if (!Settings.Default.UseFading)
            {
                Opacity = transparency ? 0.80D : 1D;
#if TODO_AVALONIA // Fade Events
            }
            else if (transparency)
            {
                RaiseEvent(new(routedEvent: FadeToTransparentEvent));
            }
            else
            {
                RaiseEvent(new(routedEvent: FadeInEvent));
#else
            }
            else
            {
                Opacity = transparency ? 0.80D : 1D;
#endif
            }
        }

        internal void HideAllMenus()
        {
            // Find main menu and close/hide all
            Menu menu = this;
            while (menu.ParentMenu != null)
            {
                menu = menu.ParentMenu;
            }

            menu.HideWithFade(true);
        }

        internal void HideWithFade(bool recursive)
        {
            if (recursive)
            {
                SubMenu?.HideWithFade(true);
            }

            if (RowDataParent != null)
            {
                RowDataParent.SubMenu = null;
            }

#if TODO_AVALONIA // Fade Events
            if (Settings.Default.UseFading)
            {
                RaiseEvent(new(routedEvent: FadeOutEvent));
            }
            else
#endif
            {
                FadeOut_Completed(this, new());
            }
        }

#if TODO_AVALONIA // Fade Events
        internal void StartFadeIn()
        {
            if (Settings.Default.UseFading)
            {
                RaiseEvent(new(routedEvent: FadeInEvent));
            }
        }
#endif

        /// <summary>
        /// Update the position and size of the menu.
        /// </summary>
        /// <param name="bounds">Screen coordinates where the menu is allowed to be drawn in.</param>
        /// <param name="menuPredecessor">Predecessor menu (when available).</param>
        /// <param name="startLocation">Defines where the first menu is drawn (when no predecessor is set).</param>
        /// <param name="useCustomLocation">Use CustomLocation as start position.</param>
        internal void AdjustSizeAndLocation(
#if !AVALONIA
            Rect bounds,
#else
            Rectangle bounds,
#endif
            Menu? menuPredecessor,
            StartLocation startLocation,
            bool useCustomLocation)
        {
            Point originLocation = new(0D, 0D);

            // Update the height and width
            AdjustDataGridViewHeight(menuPredecessor, bounds.Height);
            AdjustDataGridViewWidth();

            if (Level > 0)
            {
                if (menuPredecessor == null)
                {
                    // should never happen
                    return;
                }

                // Sub Menu location depends on the location of its predecessor
                startLocation = StartLocation.Predecessor;
                originLocation = menuPredecessor.Location;
            }
            else if (useCustomLocation)
            {
                if (!RelocateOnNextShow)
                {
                    return;
                }

                RelocateOnNextShow = false;
                startLocation = StartLocation.Point;
                originLocation = new(Settings.Default.CustomLocationX, Settings.Default.CustomLocationY);
            }
            else if (Settings.Default.AppearAtMouseLocation)
            {
                if (!RelocateOnNextShow)
                {
                    return;
                }

                RelocateOnNextShow = false;
                startLocation = StartLocation.Point;
                originLocation = NativeMethods.Screen.CursorPosition;
            }

            if (IsLoaded)
            {
                AdjustWindowPositionInternal(originLocation);
            }
            else
            {
                // Layout cannot be calculated during loading, postpone this event
                Loaded += (_, _) => AdjustWindowPositionInternal(originLocation);
            }

            void AdjustWindowPositionInternal(in Point originLocation)
            {
                double scaling = Math.Round(Scaling.Factor, 0, MidpointRounding.AwayFromZero);
                double overlappingOffset = 0D;
                double predecessorFrameWidth = 0D;

                // Make sure we have latest values of own window size
                UpdateLayout();

#if !AVALONIA
                double menuFrameWidth = windowFrame.ActualWidth;
                double menuFrameHeight = windowFrame.ActualHeight;
#else
                double menuFrameWidth = windowFrame.DesiredSize.Width;
                double menuFrameHeight = windowFrame.DesiredSize.Height;
#endif

                // Prepare parameters
                if (startLocation == StartLocation.Predecessor)
                {
                    if (menuPredecessor == null)
                    {
                        // should never happen
                        return;
                    }

                    // When (later in calculation) a list item is not found,
                    // its values might be invalidated due to resizing or moving.
                    // After updating the layout the location should be available again.
                    menuPredecessor.UpdateLayout();

#if !AVALONIA
                    predecessorFrameWidth = menuPredecessor.windowFrame.ActualWidth;
#else
                    predecessorFrameWidth = menuPredecessor.windowFrame.DesiredSize.Width;
#endif

                    directionToRight = menuPredecessor.directionToRight; // try keeping same direction from predecessor

                    if (!Settings.Default.AppearNextToPreviousMenu &&
                        predecessorFrameWidth > Settings.Default.OverlappingOffsetPixels)
                    {
                        if (directionToRight)
                        {
                            overlappingOffset = Settings.Default.OverlappingOffsetPixels - predecessorFrameWidth;
                        }
                        else
                        {
                            overlappingOffset = predecessorFrameWidth - Settings.Default.OverlappingOffsetPixels;
                        }
                    }
                }
                else
                {
                    directionToRight = true; // use right as default direction
                }

                // Calculate X position
                double x;
                switch (startLocation)
                {
                    case StartLocation.Point:
                        x = originLocation.X;
                        break;
                    case StartLocation.Predecessor:
                        if (menuPredecessor == null)
                        {
                            // should never happen
                            return;
                        }

                        if (directionToRight)
                        {
                            x = originLocation.X + predecessorFrameWidth - scaling;

                            // Change direction when out of bounds (predecessor only)
                            if (startLocation == StartLocation.Predecessor &&
                                bounds.X + bounds.Width <= x + menuFrameWidth - scaling)
                            {
                                x = originLocation.X - menuFrameWidth + scaling;
                                if (x < bounds.X &&
                                    originLocation.X + predecessorFrameWidth < bounds.X + bounds.Width &&
                                    bounds.X + (bounds.Width / 2) > originLocation.X + (menuFrameWidth / 2))
                                {
                                    x = bounds.X + bounds.Width - menuFrameWidth + scaling;
                                }
                                else
                                {
                                    if (x < bounds.X)
                                    {
                                        x = bounds.X;
                                    }

                                    directionToRight = !directionToRight;
                                }
                            }
                        }
                        else
                        {
                            x = originLocation.X - menuFrameWidth + scaling;

                            // Change direction when out of bounds (predecessor only)
                            if (startLocation == StartLocation.Predecessor &&
                                x < bounds.X)
                            {
                                x = originLocation.X + predecessorFrameWidth - scaling;
                                if (x + menuFrameWidth > bounds.X + bounds.Width &&
                                    originLocation.X > bounds.X &&
                                    bounds.X + (bounds.Width / 2) < originLocation.X + (menuFrameWidth / 2))
                                {
                                    x = bounds.X;
                                }
                                else
                                {
                                    if (x + menuFrameWidth > bounds.X + bounds.Width)
                                    {
                                        x = bounds.X + bounds.Width - menuFrameWidth + scaling;
                                    }

                                    directionToRight = !directionToRight;
                                }
                            }
                        }

                        break;
                    case StartLocation.BottomLeft:
                        x = bounds.X;
                        directionToRight = true;
                        break;
                    case StartLocation.TopRight:
                    case StartLocation.BottomRight:
                    default:
                        x = bounds.Width - menuFrameWidth;
                        directionToRight = false;
                        break;
                }

                // Besides overlapping setting we need to subtract the left margin from x as it was not part of x calculation
                x += overlappingOffset - windowFrame.Margin.Left;

                // Calculate Y position
                double y;
                switch (startLocation)
                {
                    case StartLocation.Point:
                        y = originLocation.Y;
                        if (Settings.Default.AppearAtMouseLocation)
                        {
#if !AVALONIA
                            y -= labelTitle.ActualHeight; // Mouse should point below title
#else
                            y -= labelTitle.Height; // Mouse should point below title
#endif
                        }

                        break;
                    case StartLocation.Predecessor:
                        if (menuPredecessor == null)
                        {
                            // should never happen
                            return;
                        }

                        y = originLocation.Y;

                        // Set position on same height as the selected row from predecessor
                        RowData? trigger = RowDataParent;
                        if (trigger != null)
                        {
                            double offset = menuPredecessor.GetDataGridViewChildRect(trigger).Top;

                            if (offset < 0)
                            {
                                // Do not allow to show window higher than previous window
                                offset = 0;
                            }
                            else
                            {
                                ListView dgv = menuPredecessor.GetDataGridView();
                                double offsetList = menuPredecessor.GetRelativeChildPositionTo(dgv).Y;
                                offsetList += dgv.ActualHeight;
                                if (offsetList < offset)
                                {
                                    // Do not allow to show window below last entry position of list
                                    offset = offsetList;
                                }
                            }

                            y += offset;
                        }

                        if (searchPanel.GetVisibility() == Visibility.Collapsed)
                        {
#if !AVALONIA
                            y += menuPredecessor.searchPanel.ActualHeight;
#else
                            y += menuPredecessor.searchPanel.Height;
#endif
                        }

                        break;
                    case StartLocation.TopRight:
                        y = bounds.Y;
                        break;
                    case StartLocation.BottomLeft:
                    case StartLocation.BottomRight:
                    default:
                        y = bounds.Height - menuFrameHeight;
                        break;
                }

                // Move vertically when out of bounds
                // Besides that we need to subtract the top margin from y as it was not part of y calculation
                if (bounds.Y + bounds.Height < y + menuFrameHeight)
                {
                    y = bounds.Y + bounds.Height - menuFrameHeight - windowFrame.Margin.Top;
                }
                else if (y < bounds.Y)
                {
                    y = bounds.Y - windowFrame.Margin.Top;
                }

                // Update position
                Left = x;
                Top = y;

                if (Settings.Default.RoundCorners)
                {
                    windowFrame.CornerRadius = new CornerRadius(CornerRadiusConstant);
                }

                UpdateLayout();
            }
        }

        internal Rect GetDataGridViewChildRect(RowData rowData)
        {
#if TODO_AVALONIA
            // When scrolled, we have to reduce the index number as we calculate based on visual tree
            int rowIndex = rowData.RowIndex;
            int startIndex = 0;
            double offsetY = 0D;
            if (VisualTreeHelper.GetChild(dgv, 0) is Decorator { Child: ScrollViewer scrollViewer })
            {
                startIndex = (int)scrollViewer.VerticalOffset;
                if (rowIndex < startIndex)
                {
                    // calculate position above starting point
                    for (int i = rowIndex; i < startIndex; i++)
                    {
                        ListViewItem? item = dgv.FindVisualChildOfType<ListViewItem>(i);
                        if (item != null)
                        {
                            offsetY -= item.ActualHeight;
                        }
                    }
                }
            }

            if (startIndex < rowIndex)
            {
                // calculate position below starting point
                // outer loop check for max RowIndex, independend of currently active filter
                // inner loop check for filtered and shown items
                for (int i = startIndex; i < rowIndex; i++)
                {
                    ListViewItem? item = dgv.FindVisualChildOfType<ListViewItem>(i);
                    if (item != null)
                    {
                        if (((RowData)item.Content).RowIndex >= rowIndex)
                        {
                            break;
                        }

                        offsetY += item.ActualHeight;
                    }
                }
            }
#else
            double offsetY = 0D;
#endif

            // All childs are using same width and height, so we simply fill in values from parent instead of individual child
            return new(0D, offsetY, dgv.ActualWidth, (double)Resources["RowHeight"]);
        }

#if AVALONIA
        protected override void IsVisibleChanged(AvaloniaPropertyChangedEventArgs e)
        {
            VisibilityChanged?.Invoke(this);

            base.IsVisibleChanged(e);
        }
#endif

#if TODO_AVALONIA
        private static bool Filter_Default(RowData itemData)
        {
            if (Settings.Default.ShowOnlyAsSearchResult && itemData.IsAdditionalItem)
            {
                return false;
            }

            return true;
        }

        private static bool Filter_ByUserPattern(RowData itemData, string userPattern)
        {
            // Instead implementing in-string wildcards, simply split into multiple search pattersy
            // Look for each space separated string if it is part of an entry's text (case insensitive)
            foreach (string pattern in userPattern.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (!itemData.ColumnText.ToLower().Contains(pattern))
                {
                    return false;
                }
            }

            return true;
        }
#endif

        private void SetSubMenuState(MenuDataDirectoryState state)
        {
            if (Config.ShowFunctionKeyOpenFolder)
            {
                buttonOpenFolder.SetVisibility(Visibility.Visible);
            }

            pictureBoxLoading.SetVisibility(Visibility.Collapsed);

            switch (state)
            {
                case MenuDataDirectoryState.Valid:
                    if (Config.ShowCountOfElementsBelow)
                    {
                        ((INotifyCollectionChanged)dgv.Items).CollectionChanged += ListView_CollectionChanged;
                        ListView_CollectionChanged(this, new(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        labelStatus.SetVisibility(Visibility.Collapsed);
                    }

                    break;
                case MenuDataDirectoryState.Empty:
                    searchPanel.SetVisibility(Visibility.Collapsed);
                    labelStatus.Content = Translator.GetText("Directory empty");
                    break;
                case MenuDataDirectoryState.NoAccess:
                    searchPanel.SetVisibility(Visibility.Collapsed);
                    labelStatus.Content = Translator.GetText("Directory inaccessible");
                    break;
                default:
                    break;
            }
        }

        private void FadeOut_Completed(object sender, EventArgs e) => Hide();

        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            searchPanel.SetVisibility(Visibility.Visible);

#if !AVALONIA
            ModifierKeys modifiers = Keyboard.Modifiers;
#else
            ModifierKeys modifiers = e.KeyModifiers; // TODO: Check if ok?
#endif
            switch (e.Key)
            {
                case Key.F4:
                    if (modifiers != ModifierKeys.Alt)
                    {
                        return;
                    }

                    break;
                case Key.F:
                    if (modifiers != ModifierKeys.Control)
                    {
                        return;
                    }

                    break;
                case Key.Tab:
                    if ((modifiers != ModifierKeys.Shift) && (modifiers != ModifierKeys.None))
                    {
                        return;
                    }

                    break;
                case Key.Enter:
                case Key.Home:
                case Key.End:
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.Escape:
                case Key.Apps:
                    if (modifiers != ModifierKeys.None)
                    {
                        return;
                    }

                    break;
                default:
                    return;
            }

            CmdKeyProcessed?.Invoke(this, e.Key, modifiers);
            e.Handled = true;
        }

        private void AdjustDataGridViewHeight(Menu? menuPredecessor, double screenHeightMax)
        {
            double factor = Settings.Default.RowHeighteInPercentage / 100f;
            if (NativeMethods.IsTouchEnabled())
            {
                factor = Settings.Default.RowHeighteInPercentageTouch / 100f;
            }

            if (menuPredecessor == null)
            {
                if (dgv.Tag == null && dgv.Items.Count > 0)
                {
                    // dgv.AutoResizeRows(); slightly incorrect depending on dpi
                    // 100% = 20 instead 21
                    // 125% = 23 instead 27, 150% = 28 instead 32
                    // 175% = 33 instead 37, 200% = 35 instead 42
                    // #418 use 21 as default and scale it manually
                    double rowHeightDefault = 21.24d * Scaling.FactorByDpi;
                    Resources["RowHeight"] = Math.Round(rowHeightDefault * factor * Scaling.Factor);
                    dgv.Tag = true;
                }
            }
            else
            {
                // Take over the height from predecessor menu
                Resources["RowHeight"] = menuPredecessor.Resources["RowHeight"];
                dgv.Tag = true;
            }

            double heightMaxByOptions = Scaling.Factor * Scaling.FactorByDpi *
                450f * (Settings.Default.HeightMaxInPercent / 100f);

            // Margin of the windowFrame is allowed to exceed the boundaries, so we just add them afterwards
            MaxHeight = Math.Min(screenHeightMax, heightMaxByOptions)
                + windowFrame.Margin.Top + windowFrame.Margin.Bottom;
        }

        private void AdjustDataGridViewWidth()
        {
            if (!string.IsNullOrEmpty(textBoxSearch.Text))
            {
                return;
            }

            double factorIconSizeInPercent = Settings.Default.IconSizeInPercent / 100f;

            // IcoWidth 100% = 21px, 175% is 33
            double icoWidth = 16 * Scaling.FactorByDpi;
            Resources["ColumnIconWidth"] = Math.Ceiling(icoWidth * factorIconSizeInPercent * Scaling.Factor);

            // Margin of the windowFrame is allowed to exceed the boundaries, so we just add them afterwards
            Resources["ColumnTextMaxWidth"] = Math.Ceiling(
                ((double)Scaling.Factor * Scaling.FactorByDpi * 400D * (Settings.Default.WidthMaxInPercent / 100D))
                + windowFrame.Margin.Left + windowFrame.Margin.Right);
        }

        private void HandleScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (IsLoaded)
            {
                MenuScrolled?.Invoke();
            }
        }

        private void TextBoxSearch_TextChanged(bool causedByWatcherUpdate)
        {
            SearchTextChanging?.Invoke();

            string? userPattern = textBoxSearch.Text?.Replace("%", " ").Replace("*", " ").ToLower();
#if TODO_AVALONIA
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(dgv.ItemsSource);
            if (string.IsNullOrEmpty(userPattern))
            {
                SizeToContent = SizeToContent.WidthAndHeight;
                view.Filter = (object item) => Filter_Default((RowData)item);
            }
            else
            {
                SizeToContent = SizeToContent.Manual;
                view.Filter = (object item) => Filter_ByUserPattern((RowData)item, userPattern);
            }
#endif

            SearchTextChanged?.Invoke(this, string.IsNullOrEmpty(userPattern), causedByWatcherUpdate);
        }

        private void PictureBoxOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            Menus.OpenFolder(folderPath);
        }

        private void PictureBoxMenuAlwaysOpen_Click(object sender, RoutedEventArgs e)
        {
            if (Config.AlwaysOpenByPin = !Config.AlwaysOpenByPin)
            {
                pictureBoxMenuAlwaysOpen.Source = (DrawingImage)Resources["ic_fluent_pin_48_filledDrawingImage"];
            }
            else
            {
                pictureBoxMenuAlwaysOpen.Source = (DrawingImage)Resources["ic_fluent_pin_48_regularDrawingImage"];
            }
        }

        private void PictureBoxSettings_MouseClick(object sender, RoutedEventArgs e)
        {
            SettingsWindow.ShowSingleInstance();
        }

        private void PictureBoxRestart_MouseClick(object sender, RoutedEventArgs e)
        {
            AppRestart.ByMenuButton();
        }

        private void MainMenu_MoveStart(object sender, EventArgs e)
        {
            // Hide all sub menus to clear the view for repositioning of the main menu
            if (SubMenu != null)
            {
                SubMenu?.HideWithFade(true);
                RefreshSelection();
            }

            lastLocation = NativeMethods.Screen.CursorPosition;
            MouseMove += MainMenu_MoveRelocate;
            MouseUp += MainMenu_MoveEnd;
            Deactivated += MainMenu_MoveEnd;
#if TODO_AVALONIA
            Mouse.Capture(this);
#endif
        }

        private void MainMenu_MoveRelocate(object sender, MouseEventArgs e)
        {
            Point mousePos = NativeMethods.Screen.CursorPosition;
            Left = Left + mousePos.X - lastLocation.X;
            Top = Top + mousePos.Y - lastLocation.Y;
            lastLocation = mousePos;

            Settings.Default.CustomLocationX = (int)Left;
            Settings.Default.CustomLocationY = (int)Top;
        }

        private void MainMenu_MoveEnd(object? sender, EventArgs? e)
        {
#if TODO_AVALONIA
            Mouse.Capture(null);
#endif
            MouseMove -= MainMenu_MoveRelocate;
            MouseUp -= MainMenu_MoveEnd;
            Deactivated -= MainMenu_MoveEnd;

            if (Settings.Default.UseCustomLocation)
            {
                if (!SettingsWindow.IsOpen())
                {
                    Settings.Default.Save();
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs? e)
        {
            if (e != null)
            {
                foreach (RowData itemData in e.AddedItems)
                {
                    itemData.IsSelected = true;
                }

                foreach (RowData itemData in e.RemovedItems)
                {
                    itemData.IsSelected = false;
                }
            }
            else
            {
                // TODO: Refactor item selection to prevent running this loop
                ListView lv = (ListView)sender;
#if !AVALONIA
                foreach (RowData itemData in lv.Items.SourceCollection)
#else
                // TODO: SourceCollection
                foreach (RowData itemData in lv.Items)
#endif
                {
                    itemData.IsSelected = lv.SelectedItem == itemData;
                }
            }
        }

        private void ListView_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            int count = dgv.Items.Count;
            labelStatus.Content = count.ToString() + " " + Translator.GetText(count == 1 ? "element" : "elements");
        }

#if AVALONIA
        internal void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
#else
        private void ListViewItem_MouseEnter(object sender, MouseEventArgs e)
#endif
        {
            if (!isShellContextMenuOpen)
            {
                // "DisconnectedItem" protection
#if !AVALONIA
                if (((ListViewItem)sender).Content is RowData rowData)
#else
                if (((StyledElement)sender).DataContext is RowData rowData)
#endif
                {
                    CellMouseEnter?.Invoke(rowData);
                }
            }
        }

#if AVALONIA
        internal void ListViewItem_MouseLeave(object sender, MouseEventArgs e)
#else
        private void ListViewItem_MouseLeave(object sender, MouseEventArgs e)
#endif
        {
#if !AVALONIA
            // "DisconnectedItem" protection
            if (((ListViewItem)sender).Content is RowData rowData)
#else
            if (((StyledElement)sender).DataContext is RowData rowData)
#endif
            {
#if !AVALONIA
                rowData.IsClicked = false;
#endif
                countLeftMouseButtonClicked = 0;
                if (!isShellContextMenuOpen)
                {
                    CellMouseLeave?.Invoke();
#if TODO_AVALONIA
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        string[] files = new string[] { rowData.Path };
                        DragDrop.DoDragDrop(this, new DataObject(DataFormats.FileDrop, files), DragDropEffects.Copy);
                    }
#endif
                }
            }
        }

#if AVALONIA
        private void ListViewItem_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (((StyledElement)sender).DataContext is RowData rowData)
            {
                CellMouseDown?.Invoke(rowData);
            }
        }

        private void ListViewItem_SingleTapped(object sender, TappedEventArgs e)
        {
            if (((StyledElement)sender).DataContext is RowData rowData)
            {
                rowData.OpenItem(1);
            }
        }

        private void ListViewItem_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (((StyledElement)sender).DataContext is RowData rowData)
            {
                rowData.OpenItem(2);
            }
        }
#else  // rowData.IsClicked only visible in the moment of clicking, so should be replaced with OpenAnimationBorder/OpenAnimationStoryboard
        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // "DisconnectedItem" protection
            if (((ListViewItem)sender).Content is RowData rowData)
            {
                CellMouseDown?.Invoke(rowData);
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // "DisconnectedItem" protection
            if (((ListViewItem)sender).Content is RowData rowData)
            {
                rowData.IsClicked = true;
#if TODO_AVALONIA
                countLeftMouseButtonClicked = e.ClickCount;
#else
                countLeftMouseButtonClicked = 1; // TODO: Fix double click
#endif
            }
        }

        private void ListViewItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // "DisconnectedItem" protection
            if (((ListViewItem)sender).Content is RowData rowData)
            {
                if (rowData.IsClicked)
                {
                    // Same row has been called with PreviewMouseLeftButtonDown without leaving the item, so we can call it a "click".
                    // The click count is also taken from Down event as it seems not being correct in Up event.
                    rowData.OpenItem(countLeftMouseButtonClicked);
                }

                rowData.IsClicked = false;
            }

            countLeftMouseButtonClicked = 0;
        }
#endif

#if AVALONIA
        private void ListViewItem_PointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.GetCurrentPoint(null).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonReleased &&
                ((StyledElement)sender).DataContext is RowData rowData)
#else
                private void ListViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // "DisconnectedItem" protection
            if (((ListViewItem)sender).Content is RowData rowData)
#endif
            {
                // At mouse location
                Point position = Mouse.GetPosition(this);
                position.Offset(Left, Top);

                isShellContextMenuOpen = true;
                rowData.OpenShellContextMenu(position);
                isShellContextMenuOpen = false;
            }
        }
    }
}
