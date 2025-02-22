﻿// <copyright file="UpdateWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2022-2024 Peter Kirmeier

namespace SystemTrayMenu.UserInterface
{
#if !AVALONIA
    using System.Windows;
#else
    using Avalonia.Interactivity;
    using SystemTrayMenu.Utilities;
#endif
    using SystemTrayMenu.Helpers.Updater;

    /// <summary>
    /// Logic of Update window.
    /// </summary>
    public partial class UpdateWindow : Window
    {
        public UpdateWindow()
        {
            InitializeComponent();

            label.Content = ((string?)label.Content) + " " + GitHubUpdate.LatestVersionName;
        }

        private void ButtonGoToDownloadPage_Click(object sender, RoutedEventArgs e)
        {
            GitHubUpdate.WebOpenLatestRelease();
            Close();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
