// <copyright file="UserControl.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2023-2023 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using Avalonia.Markup.Xaml;

    public class UserControl : Avalonia.Controls.UserControl
    {
        protected void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    }
}
#endif
