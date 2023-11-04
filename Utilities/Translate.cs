// <copyright file="Translate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2022-2023 Peter Kirmeier

namespace SystemTrayMenu.Utilities
{
    using System;
#if WINDOWS
    using System.Windows.Markup;
#else
    using Avalonia.Markup.Xaml;
#endif

    public class Translate : MarkupExtension
    {
        private readonly string original;

        public Translate(string original)
        {
            this.original = original;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => Translator.GetText(original);
    }
}
