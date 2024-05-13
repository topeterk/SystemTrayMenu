// <copyright file="SettingsWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Versioning;
#if WINDOWS
    using SystemTrayMenu.Helpers;
    using Windows.ApplicationModel;
    using StartupTaskState = Windows.ApplicationModel.StartupTaskState;
#endif
#if !AVALONIA
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
#else
    using Avalonia.Controls;
    using Avalonia.Input;
    using Avalonia.Interactivity;
    using Window = SystemTrayMenu.Utilities.Window;
#endif
    using Microsoft.Win32;
    using SystemTrayMenu.Properties;
    using SystemTrayMenu.UserInterface.FolderBrowseDialog;
    using SystemTrayMenu.Utilities;

    /// <summary>
    /// Logic of SettingsWindow.xaml .
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private const string MenuName = @"Software\Classes\directory\shell\SystemTrayMenu_SetAsRootFolder";
        private const string Command = @"Software\Classes\directory\shell\SystemTrayMenu_SetAsRootFolder\command";

        private static SettingsWindow? singletonWindow;

        public SettingsWindow()
        {
            InitializeComponent();

            // TODO: Find a way to escape ' within inline single quotes markup string in XAML
            buttonAddSampleStartMenuFolder.Content = Translator.GetText("Add sample directory 'Start Menu'");
            checkBoxShowFunctionKeyOpenFolder.Content = Translator.GetText("Show function key 'Open Folder'");
            checkBoxShowFunctionKeyPinMenu.Content = Translator.GetText("Show function key 'Pin menu'");
            checkBoxShowFunctionKeySettings.Content = Translator.GetText("Show function key 'Settings'");
            checkBoxShowFunctionKeyRestart.Content = Translator.GetText("Show function key 'Restart'");

            switch (GetAutostartMode())
            {
                case AutostartMode.StartupTask:
                    groupBoxAutostart.Content = $"{(string)groupBoxAutostart.Content} ({Translator.GetText("Task Manager")})";
                    checkBoxAutostart.SetVisibility(Visibility.Collapsed);
                    labelStartupStatus.Content = string.Empty;
                    break;
                case AutostartMode.Win32Registry:
                    checkBoxAutostart.IsChecked = Settings.Default.IsAutostartActivated;
                    buttonAddStartup.SetVisibility(Visibility.Collapsed);
                    labelStartupStatus.SetVisibility(Visibility.Collapsed);
                    break;
                case AutostartMode.None:
                default:
                    checkBoxAutostart.SetVisibility(Visibility.Collapsed);
                    buttonAddStartup.SetVisibility(Visibility.Collapsed);
                    labelStartupStatus.SetVisibility(Visibility.Collapsed);
                    break;
            }

            textBoxFolder.Text = Config.Path;
            checkBoxSetFolderByWindowsContextMenu.IsChecked = Settings.Default.SetFolderByWindowsContextMenu;
            checkBoxSaveConfigInApplicationDirectory.IsChecked = CustomSettingsProvider.IsActivatedConfigPathAssembly();
            checkBoxSaveLogFileInApplicationDirectory.IsChecked = Settings.Default.SaveLogFileInApplicationDirectory;

            checkBoxCheckForUpdates.IsChecked = Settings.Default.CheckForUpdates;

#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                textBoxHotkey.SetHotkeyRegistration(GlobalHotkeys.GetLastCreatedHotkeyFunction());
            }
#endif

            List<LanguageID> languages = new()
            {
                new ("Afrikaans", "af"),
                new ("Azərbaycan", "az"),
                new ("bahasa Indonesia", "id"),
                new ("català", "ca"),
                new ("čeština", "cs"),
                new ("Cymraeg", "cy"),
                new ("dansk", "da"),
                new ("Deutsch", "de"),
                new ("eesti keel", "et"),
                new ("English", "en"),
                new ("English (United Kingdom)", "en-GB"),
                new ("Español", "es"),
                new ("Esperanto", "eo"),
                new ("euskara", "eu"),
                new ("Filipino", "fil"),
                new ("Français", "fr"),
                new ("Italian", "it"),
                new ("galego", "gl"),
                new ("Hrvatski", "hr"),
                new ("Gaeilge", "ga"),
                new ("íslenskur", "is"),
                new ("kiswahili", "sw"),
                new ("Kreyòl ayisyen", "ht"),
                new ("Latinus", "la"),
                new ("latviski", "lv"),
                new ("lietuvių", "lt"),
                new ("Magyar", "hu"),
                new ("Malti", "mt"),
                new ("Melayu", "ms"),
                new ("Nederlands", "nl"),
                new ("norsk", "nb"),
                new ("Polski", "pl"),
                new ("Português (Brasil)", "pt-BR"),
                new ("português (Portugal)", "pt-PT"),
                new ("Română", "ro"),
                new ("shqiptare", "sq"),
                new ("Slovenščina", "sl"),
                new ("slovenský", "sk"),
                new ("Suorittaa loppuun", "fi"),
                new ("svenska", "sv"),
                new ("Tiếng Việt", "vi"),
                new ("Türkçe ", "tr"),
                new ("Ελληνικά", "el"),
                new ("беларуская", "bg"),
                new ("македонски", "mk"),
                new ("русский", "ru"),
                new ("Српски", "sr"),
                new ("український", "uk"),
                new ("ქართული", "ka"),
                new ("հայերեն", "hy"),
                new ("יידיש", "yi"),
                new ("עִברִית", "he"),
                new ("اردو", "ur"),
                new ("عربي", "ar"),
                new ("فارسی", "fa"),
                new ("हिन्दी", "hi"),
                new ("ગુજરાતી", "gu"),
                new ("தமிழ்", "ta"),
                new ("తెలుగు", "te"),
                new ("ಕನ್ನಡ", "kn"),
                new ("ไทย", "th"),
                new ("ພາສາລາວ", "lo"),
                new ("ខ្មែរ", "km"),
                new ("한국어", "ko"),
                new ("中文(正體)", "zh-TW"),
                new ("中文(简体)", "zh-CN"),
                new ("日本語", "ja"),
            };
            comboBoxLanguage.ItemsSource = languages;
            comboBoxLanguage.SelectedValue = Settings.Default.CurrentCultureInfoName;
            comboBoxLanguage.SelectedValue ??= "en";

            numericUpDownSizeInPercent.Value = Settings.Default.SizeInPercent;
            numericUpDownIconSizeInPercent.Value = Settings.Default.IconSizeInPercent;
            if (DllImports.NativeMethods.IsTouchEnabled())
            {
                numericUpDownRowHeightInPercentage.Value = Settings.Default.RowHeighteInPercentageTouch;
            }
            else
            {
                numericUpDownRowHeightInPercentage.Value = Settings.Default.RowHeighteInPercentage;
            }

            numericUpDownMenuWidth.Value = Settings.Default.WidthMaxInPercent;
            numericUpDownMenuHeight.Value = Settings.Default.HeightMaxInPercent;

            if (Settings.Default.UseCustomLocation)
            {
                radioButtonUseCustomLocation.IsChecked = true;
            }
            else if (Settings.Default.AppearAtMouseLocation)
            {
                radioButtonAppearAtMouseLocation.IsChecked = true;
            }
            else if (Settings.Default.AppearAtTheBottomLeft)
            {
                radioButtonAppearAtTheBottomLeft.IsChecked = true;
            }
            else
            {
                radioButtonAppearAtTheBottomRight.IsChecked = true;
            }

            numericUpDownOverlappingOffsetPixels.Value = Settings.Default.OverlappingOffsetPixels;
            if (Settings.Default.AppearNextToPreviousMenu)
            {
                radioButtonNextToPreviousMenu.IsChecked = true;
                numericUpDownOverlappingOffsetPixels.IsEnabled = false;
            }
            else
            {
                radioButtonOverlapping.IsChecked = true;
                numericUpDownOverlappingOffsetPixels.IsEnabled = true;
            }

            checkBoxResolveLinksToFolders.IsChecked = Settings.Default.ResolveLinksToFolders;
            checkBoxShowInTaskbar.IsChecked = Settings.Default.ShowInTaskbar;
            checkBoxSendHotkeyInsteadKillOtherInstances.IsChecked = Settings.Default.SendHotkeyInsteadKillOtherInstances;
            checkBoxSupportGamepad.IsChecked = Settings.Default.SupportGamepad;
            checkBoxOpenItemWithOneClick.IsChecked = Settings.Default.OpenItemWithOneClick;
            checkBoxOpenDirectoryWithOneClick.IsChecked = Settings.Default.OpenDirectoryWithOneClick;

            if (DllImports.NativeMethods.IsTouchEnabled())
            {
                checkBoxDragDropItems.IsChecked = Settings.Default.DragDropItemsEnabledTouch;
                checkBoxSwipeScrolling.IsChecked = Settings.Default.SwipeScrollingEnabledTouch;
            }
            else
            {
                checkBoxDragDropItems.IsChecked = Settings.Default.DragDropItemsEnabled;
                checkBoxSwipeScrolling.IsChecked = Settings.Default.SwipeScrollingEnabled;
            }

            textBoxIcoFolder.Text = Settings.Default.PathIcoDirectory;
            if (Settings.Default.SortByTypeAndNameWindowsExplorerSort)
            {
                radioButtonSortByTypeAndName.IsChecked = true;
            }
            else if (Settings.Default.SortByTypeAndDate)
            {
                radioButtonSortByTypeAndDate.IsChecked = true;
            }
            else if (Settings.Default.SortByFileExtensionAndName)
            {
                radioButtonSortByFileExtensionAndName.IsChecked = true;
            }
            else if (Settings.Default.SortByDate)
            {
                radioButtonSortByDate.IsChecked = true;
            }
            else
            {
                // default: Settings.Default.SortByName
                radioButtonSortByName.IsChecked = true;
            }

            if (Settings.Default.NeverShowHiddenFiles)
            {
                radioButtonNeverShowHiddenFiles.IsChecked = true;
            }
            else if (Settings.Default.AlwaysShowHiddenFiles)
            {
                radioButtonAlwaysShowHiddenFiles.IsChecked = true;
            }
            else
            {
                // default: Settings.Default.SystemSettingsShowHiddenFiles
                radioButtonSystemSettingsShowHiddenFiles.IsChecked = true;
            }

            checkBoxShowOnlyAsSearchResult.IsChecked = Settings.Default.ShowOnlyAsSearchResult;
            try
            {
                foreach (string pathAndRecursivString in Settings.Default.PathsAddToMainMenu.Split(@"|"))
                {
                    if (string.IsNullOrEmpty(pathAndRecursivString))
                    {
                        continue;
                    }

                    string pathAddToMainMenu = pathAndRecursivString.Split("recursiv:")[0].Trim();
                    bool recursive = pathAndRecursivString.Split("recursiv:")[1].StartsWith("True");
                    bool onlyFiles = pathAndRecursivString.Split("onlyFiles:")[1].StartsWith("True");
                    dataGridViewFolders.Items.Add(new ListViewItemData(pathAddToMainMenu, recursive, onlyFiles));
                }
            }
            catch (Exception ex)
            {
                Log.Warn("PathsAddToMainMenu", ex);
            }

            EnableButtonAddStartMenu();

            checkBoxGenerateShortcutsToDrives.IsChecked = Settings.Default.GenerateShortcutsToDrives;

            checkBoxStayOpenWhenItemClicked.IsChecked = Settings.Default.StaysOpenWhenItemClicked;
            checkBoxStayOpenWhenFocusLost.IsChecked = Settings.Default.StaysOpenWhenFocusLost;
            numericUpDownTimeUntilClose.Value = Settings.Default.TimeUntilCloses;

            numericUpDownTimeUntilOpens.Value = Settings.Default.TimeUntilOpens;

            checkBoxStayOpenWhenFocusLostAfterEnterPressed.IsChecked = Settings.Default.StaysOpenWhenFocusLostAfterEnterPressed;

            numericUpDownTimeUntilClosesAfterEnterPressed.Value = Settings.Default.TimeUntilClosesAfterEnterPressed;

            numericUpDownClearCacheIfMoreThanThisNumberOfItems.Value = Settings.Default.ClearCacheIfMoreThanThisNumberOfItems;

            textBoxSearchPattern.Text = Settings.Default.SearchPattern;

            checkBoxUseIconFromRootFolder.IsChecked = Settings.Default.UseIconFromRootFolder;
            checkBoxRoundCorners.IsChecked = Settings.Default.RoundCorners;
            checkBoxDarkModeAlwaysOn.IsChecked = Settings.Default.IsDarkModeAlwaysOn;
            checkBoxUseFading.IsChecked = Settings.Default.UseFading;
            checkBoxShowLinkOverlay.IsChecked = Settings.Default.ShowLinkOverlay;
            checkBoxShowDirectoryTitleAtTop.IsChecked = Settings.Default.ShowDirectoryTitleAtTop;
            checkBoxShowSearchBar.IsChecked = Settings.Default.ShowSearchBar;
            checkBoxShowCountOfElementsBelow.IsChecked = Settings.Default.ShowCountOfElementsBelow;
            checkBoxShowFunctionKeyOpenFolder.IsChecked = Settings.Default.ShowFunctionKeyOpenFolder;
            checkBoxShowFunctionKeyPinMenu.IsChecked = Settings.Default.ShowFunctionKeyPinMenu;
            checkBoxShowFunctionKeySettings.IsChecked = Settings.Default.ShowFunctionKeySettings;
            checkBoxShowFunctionKeyRestart.IsChecked = Settings.Default.ShowFunctionKeyRestart;

            textBoxColorSelectedItem.Text = Settings.Default.ColorSelectedItem;
            textBoxColorSelectedItemDarkMode.Text = Settings.Default.ColorDarkModeSelecetedItem;
            textBoxColorSelectedItemBorder.Text = Settings.Default.ColorSelectedItemBorder;
            textBoxColorSelectedItemBorderDarkMode.Text = Settings.Default.ColorDarkModeSelectedItemBorder;
            textBoxColorOpenFolder.Text = Settings.Default.ColorOpenFolder;
            textBoxColorOpenFolderDarkMode.Text = Settings.Default.ColorDarkModeOpenFolder;
            textBoxColorOpenFolderBorder.Text = Settings.Default.ColorOpenFolderBorder;
            textBoxColorOpenFolderBorderDarkMode.Text = Settings.Default.ColorDarkModeOpenFolderBorder;
            textBoxColorIcons.Text = Settings.Default.ColorIcons;
            textBoxColorIconsDarkMode.Text = Settings.Default.ColorDarkModeIcons;
            textBoxColorBackground.Text = Settings.Default.ColorBackground;
            textBoxColorBackgroundDarkMode.Text = Settings.Default.ColorDarkModeBackground;
            textBoxColorBackgroundBorder.Text = Settings.Default.ColorBackgroundBorder;
            textBoxColorBackgroundBorderDarkMode.Text = Settings.Default.ColorDarkModeBackgroundBorder;
            textBoxColorSearchField.Text = Settings.Default.ColorSearchField;
            textBoxColorSearchFieldDarkMode.Text = Settings.Default.ColorDarkModeSearchField;

            textBoxColorScrollbarBackground.Text = Settings.Default.ColorScrollbarBackground;
            textBoxColorSlider.Text = Settings.Default.ColorSlider;
            textBoxColorSliderDragging.Text = Settings.Default.ColorSliderDragging;
            textBoxColorSliderHover.Text = Settings.Default.ColorSliderHover;
            textBoxColorArrow.Text = Settings.Default.ColorArrow;
            textBoxColorArrowClick.Text = Settings.Default.ColorArrowClick;
            textBoxColorArrowClickBackground.Text = Settings.Default.ColorArrowClickBackground;
            textBoxColorArrowHover.Text = Settings.Default.ColorArrowHover;
            textBoxColorArrowHoverBackground.Text = Settings.Default.ColorArrowHoverBackground;
            textBoxColorScrollbarBackgroundDarkMode.Text = Settings.Default.ColorScrollbarBackgroundDarkMode;
            textBoxColorSliderDarkMode.Text = Settings.Default.ColorSliderDarkMode;
            textBoxColorSliderDraggingDarkMode.Text = Settings.Default.ColorSliderDraggingDarkMode;
            textBoxColorSliderHoverDarkMode.Text = Settings.Default.ColorSliderHoverDarkMode;
            textBoxColorArrowDarkMode.Text = Settings.Default.ColorArrowDarkMode;
            textBoxColorArrowClickDarkMode.Text = Settings.Default.ColorArrowClickDarkMode;
            textBoxColorArrowClickBackgroundDarkMode.Text = Settings.Default.ColorArrowClickBackgroundDarkMode;
            textBoxColorArrowHoverDarkMode.Text = Settings.Default.ColorArrowHoverDarkMode;
            textBoxColorArrowHoverBackgroundDarkMode.Text = Settings.Default.ColorArrowHoverBackgroundDarkMode;

            Closed += (_, _) => singletonWindow = null;

#if !AVALONIA
            PreviewKeyDown += HandlePreviewKeyDown;
#endif
        }

        private enum AutostartMode
        {
            /// <summary>
            /// No Autostart support available.
            /// </summary>
            None,

            /// <summary>
            /// Windows registry entry.
            /// </summary>
            Win32Registry,

            /// <summary>
            /// Windows UWP or Desktop application startup entry.
            /// </summary>
            StartupTask,
        }

        public static void ShowSingleInstance()
        {
            if (IsOpen())
            {
                singletonWindow!.HandleInvoke(() => singletonWindow?.Activate());
            }
            else
            {
                singletonWindow = new();
                singletonWindow.Show();
            }
        }

        public static bool IsOpen() => singletonWindow != null;

        private static T? GetSettingsDefaultValue<T>(string name)
        {
            return (T?)Convert.ChangeType(Settings.Default.Properties[name].DefaultValue, typeof(T));
        }

        [SupportedOSPlatform("Windows")]
        private static void AddSetFolderByWindowsContextMenu()
        {
            RegistryKey? registryKeyContextMenu = null;
            RegistryKey? registryKeyContextMenuCommand = null;

            try
            {
                registryKeyContextMenu = Registry.CurrentUser.CreateSubKey(MenuName);
                string? binLocation = Environment.ProcessPath;
                if ((registryKeyContextMenu != null) && (binLocation != null))
                {
                    registryKeyContextMenu.SetValue(string.Empty, Translator.GetText("Set as directory"));
                    registryKeyContextMenu.SetValue("Icon", binLocation);
                }

                registryKeyContextMenuCommand = Registry.CurrentUser.CreateSubKey(Command);
                registryKeyContextMenuCommand?.SetValue(string.Empty, binLocation + " \"%1\"");

                Settings.Default.SetFolderByWindowsContextMenu = true;
            }
            catch (Exception ex)
            {
                Log.Warn("SaveSetFolderByWindowsContextMenu failed", ex);
            }
            finally
            {
                registryKeyContextMenu?.Close();
                registryKeyContextMenuCommand?.Close();
            }
        }

        [SupportedOSPlatform("Windows")]
        private static void RemoveSetFolderByWindowsContextMenu()
        {
            try
            {
                RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey(Command);
                if (registryKey != null)
                {
                    registryKey.Close();
                    Registry.CurrentUser.DeleteSubKey(Command);
                }

                registryKey = Registry.CurrentUser.OpenSubKey(MenuName);
                if (registryKey != null)
                {
                    registryKey.Close();
                    Registry.CurrentUser.DeleteSubKey(MenuName);
                }

                Settings.Default.SetFolderByWindowsContextMenu = false;
            }
            catch (Exception ex)
            {
                Log.Warn("DeleteSetFolderByWindowsContextMenu failed", ex);
            }
        }

        private static AutostartMode GetAutostartMode()
        {
            if (OperatingSystem.IsWindows())
            {
#if RELEASEPACKAGE // Windows app package
                return AutostartMode.StartupTask;
#else
                return AutostartMode.Win32Registry;
#endif
            }

            return AutostartMode.None;
        }

        private void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
#if WINDOWS
                // When hotkeys are not enabled, we are not allowed to handle it
                if (OperatingSystem.IsWindows() && !GlobalHotkeys.IsEnabled)
                {
                    return;
                }
#endif
                Close();
            }
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            if (OperatingSystem.IsWindows())
            {
                if (checkBoxSetFolderByWindowsContextMenu.IsChecked ?? false)
                {
                    AddSetFolderByWindowsContextMenu();
                }
                else
                {
                    RemoveSetFolderByWindowsContextMenu();
                }
            }

            Settings.Default.SaveLogFileInApplicationDirectory = checkBoxSaveLogFileInApplicationDirectory.IsChecked ?? false;
            if (Settings.Default.SaveLogFileInApplicationDirectory)
            {
                try
                {
                    string fileNameToCheckWriteAccess = "CheckWriteAccess";
                    File.WriteAllText(fileNameToCheckWriteAccess, fileNameToCheckWriteAccess);
                    File.Delete(fileNameToCheckWriteAccess);
                    Settings.Default.SaveLogFileInApplicationDirectory = true;
                }
                catch (Exception ex)
                {
                    Settings.Default.SaveLogFileInApplicationDirectory = false;
                    Log.Warn($"Failed to save log file in application folder {Log.GetLogFilePath()}", ex);
                }
            }

            if ((GetAutostartMode() == AutostartMode.Win32Registry) && OperatingSystem.IsWindows())
            {
                if (checkBoxAutostart.IsChecked ?? false)
                {
                    RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    key?.SetValue(
                            Assembly.GetExecutingAssembly().GetName().Name,
                            Environment.ProcessPath!);

                    Settings.Default.IsAutostartActivated = true;
                }
                else
                {
                    RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                    key?.DeleteValue("SystemTrayMenu", false);

                    Settings.Default.IsAutostartActivated = false;
                }
            }

            Settings.Default.CheckForUpdates = checkBoxCheckForUpdates.IsChecked ?? false;

#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                if (textBoxHotkey.WasHotkeyChanged)
                {
                    Settings.Default.HotKey = textBoxHotkey.HotkeyFunction?.GetHotkeyInvariantString() ?? string.Empty;
                }
            }
#endif

            Settings.Default.CurrentCultureInfoName = comboBoxLanguage.SelectedValue?.ToString() ?? string.Empty;
            if (numericUpDownSizeInPercent.Value.HasValue)
            {
                Settings.Default.SizeInPercent = (int)numericUpDownSizeInPercent.Value;
            }

            if (numericUpDownIconSizeInPercent.Value.HasValue)
            {
                Settings.Default.IconSizeInPercent = (int)numericUpDownIconSizeInPercent.Value;
            }

            if (numericUpDownRowHeightInPercentage.Value.HasValue)
            {
                if (DllImports.NativeMethods.IsTouchEnabled())
                {
                    Settings.Default.RowHeighteInPercentageTouch = (int)numericUpDownRowHeightInPercentage.Value;
                }
                else
                {
                    Settings.Default.RowHeighteInPercentage = (int)numericUpDownRowHeightInPercentage.Value;
                }
            }

            if (numericUpDownMenuWidth.Value.HasValue)
            {
                Settings.Default.WidthMaxInPercent = (int)numericUpDownMenuWidth.Value;
            }

            if (numericUpDownMenuHeight.Value.HasValue)
            {
                Settings.Default.HeightMaxInPercent = (int)numericUpDownMenuHeight.Value;
            }

            if (radioButtonUseCustomLocation.IsChecked ?? true)
            {
                Settings.Default.UseCustomLocation = true;
                Settings.Default.AppearAtMouseLocation = false;
                Settings.Default.AppearAtTheBottomLeft = false;
            }
            else if (radioButtonAppearAtMouseLocation.IsChecked ?? true)
            {
                Settings.Default.UseCustomLocation = false;
                Settings.Default.AppearAtMouseLocation = true;
                Settings.Default.AppearAtTheBottomLeft = false;
            }
            else if (radioButtonAppearAtTheBottomLeft.IsChecked ?? true)
            {
                Settings.Default.UseCustomLocation = false;
                Settings.Default.AppearAtMouseLocation = false;
                Settings.Default.AppearAtTheBottomLeft = true;
            }
            else
            {
                Settings.Default.UseCustomLocation = false;
                Settings.Default.AppearAtMouseLocation = false;
                Settings.Default.AppearAtTheBottomLeft = false;
            }

            if (numericUpDownOverlappingOffsetPixels.Value.HasValue)
            {
                Settings.Default.OverlappingOffsetPixels = (int)numericUpDownOverlappingOffsetPixels.Value;
            }

            if (radioButtonNextToPreviousMenu.IsChecked ?? true)
            {
                Settings.Default.AppearNextToPreviousMenu = true;
            }
            else
            {
                Settings.Default.AppearNextToPreviousMenu = false;
            }

            Settings.Default.ResolveLinksToFolders = checkBoxResolveLinksToFolders.IsChecked ?? true;
            Settings.Default.ShowInTaskbar = checkBoxShowInTaskbar.IsChecked ?? true;
            Settings.Default.SendHotkeyInsteadKillOtherInstances = checkBoxSendHotkeyInsteadKillOtherInstances.IsChecked ?? false;
            Settings.Default.SupportGamepad = checkBoxSupportGamepad.IsChecked ?? false;
            Settings.Default.OpenItemWithOneClick = checkBoxOpenItemWithOneClick.IsChecked ?? true;
            Settings.Default.OpenDirectoryWithOneClick = checkBoxOpenDirectoryWithOneClick.IsChecked ?? false;

            if (DllImports.NativeMethods.IsTouchEnabled())
            {
                Settings.Default.DragDropItemsEnabledTouch = checkBoxDragDropItems.IsChecked ?? false;
                Settings.Default.SwipeScrollingEnabledTouch = checkBoxSwipeScrolling.IsChecked ?? true;
            }
            else
            {
                Settings.Default.DragDropItemsEnabled = checkBoxDragDropItems.IsChecked ?? false;
                Settings.Default.SwipeScrollingEnabled = checkBoxSwipeScrolling.IsChecked ?? true;
            }

            Settings.Default.PathIcoDirectory = textBoxIcoFolder.Text;
            Settings.Default.SortByTypeAndNameWindowsExplorerSort = radioButtonSortByTypeAndName.IsChecked ?? false;
            Settings.Default.SortByTypeAndDate = radioButtonSortByTypeAndDate.IsChecked ?? false;
            Settings.Default.SortByFileExtensionAndName = radioButtonSortByFileExtensionAndName.IsChecked ?? false;
            Settings.Default.SortByName = radioButtonSortByName.IsChecked ?? true;
            Settings.Default.SortByDate = radioButtonSortByDate.IsChecked ?? false;

            Settings.Default.SystemSettingsShowHiddenFiles = radioButtonSystemSettingsShowHiddenFiles.IsChecked ?? true;
            Settings.Default.AlwaysShowHiddenFiles = radioButtonAlwaysShowHiddenFiles.IsChecked ?? false;
            Settings.Default.NeverShowHiddenFiles = radioButtonNeverShowHiddenFiles.IsChecked ?? false;

            Settings.Default.ShowOnlyAsSearchResult = checkBoxShowOnlyAsSearchResult.IsChecked ?? false;

            string pathsAddToMainMenu = string.Empty;
#if !AVALONIA
            foreach (ListViewItemData itemData in dataGridViewFolders.Items)
            {
                pathsAddToMainMenu += $"{itemData.ColumnFolder} recursiv:{itemData.ColumnRecursiveLevel} onlyFiles:{itemData.ColumnOnlyFiles}|";
            }
#else
            List<ListViewItemData>? list = (List<ListViewItemData>?)dataGridViewFolders.ItemsSource;
            if (list is not null)
            {
                foreach (ListViewItemData itemData in list)
                {
                    pathsAddToMainMenu += $"{itemData.ColumnFolder} recursiv:{itemData.ColumnRecursiveLevel} onlyFiles:{itemData.ColumnOnlyFiles}|";
                }
            }
#endif

            Settings.Default.PathsAddToMainMenu = pathsAddToMainMenu;

            Settings.Default.GenerateShortcutsToDrives = checkBoxGenerateShortcutsToDrives.IsChecked ?? false;

            Settings.Default.StaysOpenWhenItemClicked = checkBoxStayOpenWhenItemClicked.IsChecked ?? true;
            Settings.Default.StaysOpenWhenFocusLost = checkBoxStayOpenWhenFocusLost.IsChecked ?? true;
            if (numericUpDownTimeUntilClose.Value.HasValue)
            {
                Settings.Default.TimeUntilCloses = (int)numericUpDownTimeUntilClose.Value;
            }

            if (numericUpDownTimeUntilOpens.Value.HasValue)
            {
                Settings.Default.TimeUntilOpens = (int)numericUpDownTimeUntilOpens.Value;
            }

            Settings.Default.StaysOpenWhenFocusLostAfterEnterPressed = checkBoxStayOpenWhenFocusLostAfterEnterPressed.IsChecked ?? true;
            if (numericUpDownTimeUntilClosesAfterEnterPressed.Value.HasValue)
            {
                Settings.Default.TimeUntilClosesAfterEnterPressed = (int)numericUpDownTimeUntilClosesAfterEnterPressed.Value;
            }

            if (numericUpDownClearCacheIfMoreThanThisNumberOfItems.Value.HasValue)
            {
                Settings.Default.ClearCacheIfMoreThanThisNumberOfItems = (int)numericUpDownClearCacheIfMoreThanThisNumberOfItems.Value;
            }

            Settings.Default.SearchPattern = textBoxSearchPattern.Text;

            Settings.Default.UseIconFromRootFolder = checkBoxUseIconFromRootFolder.IsChecked ?? false;
            Settings.Default.RoundCorners = checkBoxRoundCorners.IsChecked ?? false;
            Settings.Default.IsDarkModeAlwaysOn = checkBoxDarkModeAlwaysOn.IsChecked ?? true;
            Settings.Default.UseFading = checkBoxUseFading.IsChecked ?? false;
            Settings.Default.ShowLinkOverlay = checkBoxShowLinkOverlay.IsChecked ?? false;
            Settings.Default.ShowDirectoryTitleAtTop = checkBoxShowDirectoryTitleAtTop.IsChecked ?? false;
            Settings.Default.ShowSearchBar = checkBoxShowSearchBar.IsChecked ?? true;
            Settings.Default.ShowCountOfElementsBelow = checkBoxShowCountOfElementsBelow.IsChecked ?? false;
            Settings.Default.ShowFunctionKeyOpenFolder = checkBoxShowFunctionKeyOpenFolder.IsChecked ?? false;
            Settings.Default.ShowFunctionKeyPinMenu = checkBoxShowFunctionKeyPinMenu.IsChecked ?? false;
            Settings.Default.ShowFunctionKeySettings = checkBoxShowFunctionKeySettings.IsChecked ?? false;
            Settings.Default.ShowFunctionKeyRestart = checkBoxShowFunctionKeyRestart.IsChecked ?? false;

            if (checkBoxSaveConfigInApplicationDirectory.IsChecked ?? false)
            {
                CustomSettingsProvider.ActivateConfigPathAssembly();
                TrySettingsDefaultSave();
            }
            else
            {
                TrySettingsDefaultSave();
                CustomSettingsProvider.DeactivateConfigPathAssembly();
            }

            static void TrySettingsDefaultSave()
            {
                try
                {
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    Log.Warn($"Failed to save configuration file in application folder {CustomSettingsProvider.ConfigPathAssembly}", ex);
                }
            }

            AppRestart.ByConfigChange();

            Close();
        }

#if !WINDOWS
        private void ButtonAddStartup_Click(object sender, RoutedEventArgs e)
        {
        }
#else
        [SupportedOSPlatform("Windows")]
        private async void ButtonAddStartup_Click(object sender, RoutedEventArgs e)
        {
            // Pass the task ID you specified in the appxmanifest file
            StartupTask startupTask = await StartupTask.GetAsync("MyStartupId");
            StartupTaskState startupState = startupTask.State;

            Log.Info($"Autostart {startupState}.");

            if (startupState == StartupTaskState.Disabled)
            {
                // Task is disabled but can be enabled.
                startupState = await startupTask.RequestEnableAsync();
            }

            if ((startupState == StartupTaskState.Enabled) || (startupState == StartupTaskState.EnabledByPolicy))
            {
                labelStartupStatus.Content = Translator.GetText("Activated");
            }
            else
            {
                labelStartupStatus.Content = Translator.GetText("Deactivated");
            }
        }
#endif

        private async void ButtonChange_Click(object sender, RoutedEventArgs e)
        {
            if (await Config.SetFolderByUser(this, false))
            {
                textBoxFolder.Text = Config.Path;
            }
        }

        private void ButtonOpenFolder_Click(object sender, RoutedEventArgs e)
        {
#if TODO_LINUX
            Log.ProcessStart("explorer.exe", Config.Path, true);
#endif
        }

        private void ButtonChangeRelativeFolder_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Config.Path))
            {
                string? location = Assembly.GetEntryAssembly()?.Location;
                if (!string.IsNullOrEmpty(location))
                {
                    string? parentPath = Directory.GetParent(location)?.FullName;
                    if (!string.IsNullOrEmpty(parentPath))
                    {
                        Settings.Default.PathDirectory = Path.GetRelativePath(parentPath, Config.Path);
                        textBoxFolder.Text = Config.Path;
                    }
                }
            }
        }

        private void ButtonOpenAssemblyLocation_Click(object sender, RoutedEventArgs e)
        {
            string? location = Assembly.GetEntryAssembly()?.Location;
            if (!string.IsNullOrEmpty(location))
            {
                string? parentPath = Directory.GetParent(location)?.FullName;
                if (!string.IsNullOrEmpty(parentPath))
                {
                    Log.ProcessStart(parentPath);
                }
            }
        }

        private void ButtonHotkeyDefault_Click(object sender, RoutedEventArgs e)
        {
#if WINDOWS
            if (OperatingSystem.IsWindows())
            {
                textBoxHotkey.ChangeHotkey(GetSettingsDefaultValue<string>(nameof(Settings.Default.HotKey)));
            }
#endif
        }

        private void ButtonGeneralDefault_Click(object sender, RoutedEventArgs e)
        {
            checkBoxSetFolderByWindowsContextMenu.IsChecked = false;
            checkBoxSaveConfigInApplicationDirectory.IsChecked = false;
            checkBoxSaveLogFileInApplicationDirectory.IsChecked = false;
            checkBoxAutostart.IsChecked = false;
            checkBoxCheckForUpdates.IsChecked = false;
        }

        private async void ButtonChangeIcoFolder_Click(object sender, RoutedEventArgs e)
        {
            if (await Config.SetFolderIcoByUser(this))
            {
                textBoxIcoFolder.Text = Settings.Default.PathIcoDirectory;
            }
        }

        private void ButtonAddSampleStartMenuFolder_Click(object sender, RoutedEventArgs e)
        {
            string folderPathCommonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            dataGridViewFolders.Items.Add(new ListViewItemData(folderPathCommonStartMenu, true, true));
            EnableButtonAddStartMenu();
        }

        private void ButtonClearFolders_Click(object sender, RoutedEventArgs e)
        {
            checkBoxShowOnlyAsSearchResult.IsChecked = false;
            dataGridViewFolders.Items.Clear();
            EnableButtonAddStartMenu();
            checkBoxGenerateShortcutsToDrives.IsChecked = false;
        }

        private async void ButtonAddFolderToRootFolder_Click(object sender, RoutedEventArgs e)
        {
            using FolderDialog dialog = new();
            dialog.InitialFolder = Config.Path;

            if (await dialog.ShowDialog(this))
            {
                if (!string.IsNullOrEmpty(dialog.Folder))
                {
#if AVALONIA
                    List<ListViewItemData> newList = [];
                    List<ListViewItemData>? list = (List<ListViewItemData>?)dataGridViewFolders.ItemsSource;
                    if (list is not null)
                    {
                        foreach (ListViewItemData itemData in list)
                        {
                            newList.Add(itemData);
                        }
                    }

                    newList.Add(new (dialog.Folder, false, true));
                    dataGridViewFolders.ItemsSource = null;
                    dataGridViewFolders.ItemsSource = newList;
#else
                    dataGridViewFolders.Items.Add(new ListViewItemData(dialog.Folder, false, true));
#endif
                    EnableButtonAddStartMenu();
                }
            }

            dataGridViewFolders.SelectedItem = null;
        }

        private void ButtonRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridViewFolders.SelectedItems.Count > 0)
            {
                Array items = Array.CreateInstance(typeof(object), dataGridViewFolders.SelectedItems.Count);
                dataGridViewFolders.SelectedItems.CopyTo(items, 0);

                foreach (object item in items)
                {
                    dataGridViewFolders.Items.Remove(item);
                }

                EnableButtonAddStartMenu();
            }
        }

        private void DataGridViewFolders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            buttonRemoveFolder.IsEnabled = dataGridViewFolders.SelectedItems.Count > 0;
        }

        private void EnableButtonAddStartMenu()
        {
            string folderPathCommonStartMenu = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            bool doesStartMenuFolderExist = false;
#if AVALONIA
            List<ListViewItemData>? list = (List<ListViewItemData>?)dataGridViewFolders.ItemsSource;
            if (list is not null)
            {
                foreach (ListViewItemData itemData in list)
                {
                    // TODO: Check: Is RecursiveLevel and OnlyFiles really important to be the StartMenu folder entry? (Remove in version 1?)
                    if (folderPathCommonStartMenu.Equals(itemData.ColumnFolder))
                    {
                        doesStartMenuFolderExist = true;
                        break;
                    }
                }
            }
#else
            foreach (ListViewItemData itemData in dataGridViewFolders.Items)
            {
                // TODO: Check: Is RecursiveLevel and OnlyFiles really important to be the StartMenu folder entry? (Remove in version 1?)
                if (folderPathCommonStartMenu.Equals(itemData.ColumnFolder))
                {
                    doesStartMenuFolderExist = true;
                    break;
                }
            }
#endif

            buttonAddSampleStartMenuFolder.IsEnabled = !doesStartMenuFolderExist;
        }

        private void ButtonSizeAndLocationDefault_Click(object sender, RoutedEventArgs e)
        {
            numericUpDownSizeInPercent.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.SizeInPercent));
            numericUpDownIconSizeInPercent.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.IconSizeInPercent));
            if (DllImports.NativeMethods.IsTouchEnabled())
            {
                numericUpDownRowHeightInPercentage.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.RowHeighteInPercentageTouch));
            }
            else
            {
                numericUpDownRowHeightInPercentage.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.RowHeighteInPercentage));
            }

            numericUpDownMenuWidth.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.WidthMaxInPercent));
            numericUpDownMenuHeight.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.HeightMaxInPercent));

            radioButtonAppearAtTheBottomLeft.IsChecked = true;

            radioButtonNextToPreviousMenu.IsChecked = true;
            numericUpDownOverlappingOffsetPixels.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.OverlappingOffsetPixels));
        }

        private void ButtonAdvancedDefault_Click(object sender, RoutedEventArgs e)
        {
            checkBoxResolveLinksToFolders.IsChecked = true;
            checkBoxShowInTaskbar.IsChecked = true;
            checkBoxSendHotkeyInsteadKillOtherInstances.IsChecked = false;
            checkBoxSupportGamepad.IsChecked = false;
            checkBoxOpenItemWithOneClick.IsChecked = true;
            checkBoxOpenDirectoryWithOneClick.IsChecked = false;
            if (DllImports.NativeMethods.IsTouchEnabled())
            {
                checkBoxDragDropItems.IsChecked = false;
                checkBoxSwipeScrolling.IsChecked = true;
            }
            else
            {
                checkBoxDragDropItems.IsChecked = true;
                checkBoxSwipeScrolling.IsChecked = false;
            }

            textBoxIcoFolder.Text = Path.Combine(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    $"SystemTrayMenu"),
                "ico");

            if (!Directory.Exists(Settings.Default.PathIcoDirectory))
            {
                Directory.CreateDirectory(Settings.Default.PathIcoDirectory);
            }

            radioButtonSortByName.IsChecked = true;
            radioButtonSystemSettingsShowHiddenFiles.IsChecked = true;
        }

        private void CheckBoxStayOpenWhenFocusLost_CheckedChanged(object sender, RoutedEventArgs e)
        {
            numericUpDownTimeUntilClose.IsEnabled = checkBoxStayOpenWhenFocusLost.IsChecked ?? true;
        }

        private void CheckBoxStayOpenWhenFocusLostAfterEnterPressed_CheckedChanged(object sender, RoutedEventArgs e)
        {
            numericUpDownTimeUntilClosesAfterEnterPressed.IsEnabled = checkBoxStayOpenWhenFocusLostAfterEnterPressed.IsChecked ?? true;
        }

        private void ButtonExpertDefault_Click(object sender, RoutedEventArgs e)
        {
            checkBoxStayOpenWhenItemClicked.IsChecked = true;
            checkBoxStayOpenWhenFocusLost.IsChecked = true;
            numericUpDownTimeUntilClose.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.TimeUntilCloses));
            numericUpDownTimeUntilOpens.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.TimeUntilOpens));
            checkBoxStayOpenWhenFocusLostAfterEnterPressed.IsChecked = true;
            numericUpDownTimeUntilClosesAfterEnterPressed.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.TimeUntilClosesAfterEnterPressed));
            numericUpDownClearCacheIfMoreThanThisNumberOfItems.Value = GetSettingsDefaultValue<decimal>(nameof(Settings.Default.ClearCacheIfMoreThanThisNumberOfItems));
            textBoxSearchPattern.Text = string.Empty;
        }

        private void SaveColorsTemporarily()
        {
            if (this.GetVisibility() == Visibility.Visible)
            {
                Settings.Default.ColorSelectedItem = textBoxColorSelectedItem.Text;
                Settings.Default.ColorDarkModeSelecetedItem = textBoxColorSelectedItemDarkMode.Text;
                Settings.Default.ColorSelectedItemBorder = textBoxColorSelectedItemBorder.Text;
                Settings.Default.ColorDarkModeSelectedItemBorder = textBoxColorSelectedItemBorderDarkMode.Text;
                Settings.Default.ColorOpenFolder = textBoxColorOpenFolder.Text;
                Settings.Default.ColorDarkModeOpenFolder = textBoxColorOpenFolderDarkMode.Text;
                Settings.Default.ColorOpenFolderBorder = textBoxColorOpenFolderBorder.Text;
                Settings.Default.ColorDarkModeOpenFolderBorder = textBoxColorOpenFolderBorderDarkMode.Text;
                Settings.Default.ColorIcons = textBoxColorIcons.Text;
                Settings.Default.ColorDarkModeIcons = textBoxColorIconsDarkMode.Text;
                Settings.Default.ColorBackground = textBoxColorBackground.Text;
                Settings.Default.ColorDarkModeBackground = textBoxColorBackgroundDarkMode.Text;
                Settings.Default.ColorBackgroundBorder = textBoxColorBackgroundBorder.Text;
                Settings.Default.ColorDarkModeBackgroundBorder = textBoxColorBackgroundBorderDarkMode.Text;
                Settings.Default.ColorSearchField = textBoxColorSearchField.Text;
                Settings.Default.ColorDarkModeSearchField = textBoxColorSearchFieldDarkMode.Text;
                Settings.Default.ColorScrollbarBackground = textBoxColorScrollbarBackground.Text;
                Settings.Default.ColorSlider = textBoxColorSlider.Text;
                Settings.Default.ColorSliderDragging = textBoxColorSliderDragging.Text;
                Settings.Default.ColorSliderHover = textBoxColorSliderHover.Text;
                Settings.Default.ColorArrow = textBoxColorArrow.Text;
                Settings.Default.ColorArrowClick = textBoxColorArrowClick.Text;
                Settings.Default.ColorArrowClickBackground = textBoxColorArrowClickBackground.Text;
                Settings.Default.ColorArrowHover = textBoxColorArrowHover.Text;
                Settings.Default.ColorArrowHoverBackground = textBoxColorArrowHoverBackground.Text;
                Settings.Default.ColorScrollbarBackgroundDarkMode = textBoxColorScrollbarBackgroundDarkMode.Text;
                Settings.Default.ColorSliderDarkMode = textBoxColorSliderDarkMode.Text;
                Settings.Default.ColorSliderDraggingDarkMode = textBoxColorSliderDraggingDarkMode.Text;
                Settings.Default.ColorSliderHoverDarkMode = textBoxColorSliderHoverDarkMode.Text;
                Settings.Default.ColorArrowDarkMode = textBoxColorArrowDarkMode.Text;
                Settings.Default.ColorArrowClickDarkMode = textBoxColorArrowClickDarkMode.Text;
                Settings.Default.ColorArrowClickBackgroundDarkMode = textBoxColorArrowClickBackgroundDarkMode.Text;
                Settings.Default.ColorArrowHoverDarkMode = textBoxColorArrowHoverDarkMode.Text;
                Settings.Default.ColorArrowHoverBackgroundDarkMode = textBoxColorArrowHoverBackgroundDarkMode.Text;

                AppColors.Initialize(false);
            }
        }

        private void CheckBoxDarkModeAlwaysOnCheckedChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.IsDarkModeAlwaysOn = checkBoxDarkModeAlwaysOn.IsChecked ?? true;
            Config.ResetReadDarkModeDone();
            SaveColorsTemporarily();
        }

        private void ShowHowToOpenSettings(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.ShowHintYouCanOpenSettingsInSystemtrayIconRightClick)
            {
                HowToOpenSettingsWindow dialog = new();
                dialog.ShowDialog();
                Settings.Default.ShowHintYouCanOpenSettingsInSystemtrayIconRightClick = !dialog.DoNotShowAgain;
            }
        }

        private void ButtonAppearanceDefault_Click(object sender, RoutedEventArgs e)
        {
            checkBoxUseIconFromRootFolder.IsChecked = false;
            checkBoxRoundCorners.IsChecked = false;
            checkBoxUseFading.IsChecked = false;
            checkBoxDarkModeAlwaysOn.IsChecked = true;
            checkBoxShowLinkOverlay.IsChecked = false;
            checkBoxShowDirectoryTitleAtTop.IsChecked = false;
            checkBoxShowSearchBar.IsChecked = true;
            checkBoxShowCountOfElementsBelow.IsChecked = false;
            checkBoxShowFunctionKeyOpenFolder.IsChecked = false;
            checkBoxShowFunctionKeyPinMenu.IsChecked = false;
            checkBoxShowFunctionKeySettings.IsChecked = false;
            checkBoxShowFunctionKeyRestart.IsChecked = false;
        }

        private void ButtonDefaultColors_Click(object sender, RoutedEventArgs e)
        {
            textBoxColorIcons.Text = "#95a0a6";
            textBoxColorOpenFolder.Text = "#C2F5DE";
            textBoxColorOpenFolderBorder.Text = "#99FFA5";
            textBoxColorBackground.Text = "#ffffff";
            textBoxColorBackgroundBorder.Text = "#000000";
            textBoxColorSearchField.Text = "#ffffff";
            textBoxColorSelectedItem.Text = "#CCE8FF";
            textBoxColorSelectedItemBorder.Text = "#99D1FF";
            textBoxColorArrow.Text = "#606060";
            textBoxColorArrowHoverBackground.Text = "#dadada";
            textBoxColorArrowHover.Text = "#000000";
            textBoxColorArrowClick.Text = "#ffffff";
            textBoxColorArrowClickBackground.Text = "#606060";
            textBoxColorSlider.Text = "#cdcdcd";
            textBoxColorSliderHover.Text = "#a6a6a6";
            textBoxColorSliderDragging.Text = "#606060";
            textBoxColorScrollbarBackground.Text = "#f0f0f0";
        }

        private void ButtonDefaultColorsDark_Click(object sender, RoutedEventArgs e)
        {
            textBoxColorIconsDarkMode.Text = "#95a0a6";
            textBoxColorOpenFolderDarkMode.Text = "#14412A";
            textBoxColorOpenFolderBorderDarkMode.Text = "#144B55";
            textBoxColorBackgroundDarkMode.Text = "#202020";
            textBoxColorBackgroundBorderDarkMode.Text = "#000000";
            textBoxColorSearchFieldDarkMode.Text = "#191919";
            textBoxColorSelectedItemDarkMode.Text = "#333333";
            textBoxColorSelectedItemBorderDarkMode.Text = "#141D4B";
            textBoxColorArrowDarkMode.Text = "#676767";
            textBoxColorArrowHoverBackgroundDarkMode.Text = "#373737";
            textBoxColorArrowHoverDarkMode.Text = "#676767";
            textBoxColorArrowClickDarkMode.Text = "#171717";
            textBoxColorArrowClickBackgroundDarkMode.Text = "#a6a6a6";
            textBoxColorSliderDarkMode.Text = "#4d4d4d";
            textBoxColorSliderHoverDarkMode.Text = "#7a7a7a";
            textBoxColorSliderDraggingDarkMode.Text = "#a6a6a6";
            textBoxColorScrollbarBackgroundDarkMode.Text = "#171717";
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Reload();
            Close();
        }

        private void RadioButtonOverlapping_Checked(object sender, RoutedEventArgs e)
        {
            numericUpDownOverlappingOffsetPixels.IsEnabled = true;
        }

        private void RadioButtonOverlapping_Unchecked(object sender, RoutedEventArgs e)
        {
            numericUpDownOverlappingOffsetPixels.IsEnabled = false;
        }

        /// <summary>
        /// Type for ListView items.
        /// </summary>
        private class ListViewItemData
        {
            public ListViewItemData(string folder, bool recursiveLevel, bool onlyFiles)
            {
                ColumnFolder = folder;
                ColumnRecursiveLevel = recursiveLevel;
                ColumnOnlyFiles = onlyFiles;
            }

            public string ColumnFolder { get; set; }

            public bool ColumnRecursiveLevel { get; set; }

            public bool ColumnOnlyFiles { get; set; }
        }

        /// <summary>
        /// Pairs of language display names and their ISO 639-1 standard language codes.
        /// </summary>
        private class LanguageID
        {
            public LanguageID(string displayName, string languageCode)
            {
                DisplayName = displayName;
                Code = languageCode;
            }

            public string DisplayName { get; set; }

            public string Code { get; set; }
        }
    }
}
