// <copyright file="AboutBox.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2022-2024 Peter Kirmeier

namespace SystemTrayMenu.UserInterface
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Versioning;
    using System.Text.RegularExpressions;
    using Microsoft.Win32;
#if WINDOWS
    using System.Windows;
#endif
#if !AVALONIA
    using System.ComponentModel;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Threading;
#else
    using System.Diagnostics.CodeAnalysis;
    using Avalonia.Controls;
    using Avalonia.Controls.Documents;
    using Avalonia.Controls.Templates;
    using Avalonia.Input;
    using Avalonia.Threading;
    using RoutedEventArgs = Avalonia.Interactivity.RoutedEventArgs;
    using SizeToContent = Avalonia.Controls.SizeToContent;
    using Visibility = SystemTrayMenu.Utilities.Visibility;
    using Window = SystemTrayMenu.Utilities.Window;
#endif
    using SystemTrayMenu.Utilities;

    /// <summary>
    /// Logic of About window.
    /// </summary>
    public partial class AboutBox : Window
    {
        private static AboutBox? singletonWindow;

        private string? entryAssemblyName;
        private string? callingAssemblyName;
        private string? executingAssemblyName;
        private NameValueCollection entryAssemblyAttribCollection = new();

        public AboutBox()
        {
            InitializeComponent();

#if AVALONIA // seems to be called during InitializeComponent when set in XAML directly, so we add it here instead
            TabPanelDetails.SelectionChanged += TabPanelDetails_SelectedIndexChanged;
#endif
            Loaded += AboutBox_Load;
            Closed += (_, _) => singletonWindow = null;

            TabPanelDetails.SetVisibility(Visibility.Collapsed);
#if !AVALONIA
            buttonSystemInfo.SetVisibility(Visibility.Collapsed);
#endif
        }

        // <summary>
        // single line of text to show in the application title section of the about box dialog
        // </summary>
        // <remarks>
        // defaults to "%title%"
        // %title% = Assembly: AssemblyTitle
        // </remarks>
        public string AppTitle
        {
            get => (string)AppTitleLabel.Content ?? string.Empty;
            set => AppTitleLabel.Content = value ?? string.Empty;
        }

        // <summary>
        // single line of text to show in the description section of the about box dialog
        // </summary>
        // <remarks>
        // defaults to "%description%"
        // %description% = Assembly: AssemblyDescription
        // </remarks>
        public string AppDescription
        {
            get => (string)AppDescriptionLabel.Content ?? string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    AppDescriptionLabel.SetVisibility(Visibility.Collapsed);
                    AppDescriptionLabel.Content = string.Empty;
                }
                else
                {
                    AppDescriptionLabel.SetVisibility(Visibility.Visible);
                    AppDescriptionLabel.Content = value;
                }
            }
        }

        // <summary>
        // single line of text to show in the version section of the about dialog
        // </summary>
        // <remarks>
        // defaults to "Version %version%"
        // %version% = Assembly: AssemblyVersion
        // </remarks>
        public string AppVersion
        {
            get => (string)AppVersionLabel.Content ?? string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    AppVersionLabel.SetVisibility(Visibility.Collapsed);
                    AppVersionLabel.Content = string.Empty;
                }
                else
                {
                    AppVersionLabel.SetVisibility(Visibility.Visible);
                    AppVersionLabel.Content = value;
                }
            }
        }

        // <summary>
        // single line of text to show in the copyright section of the about dialog
        // </summary>
        // <remarks>
        // defaults to "Copyright © %year%, %company%"
        // %company% = Assembly: AssemblyCompany
        // %year% = current 4-digit year
        // </remarks>
        public string AppCopyright
        {
            get => (string)AppCopyrightLabel.Content ?? string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    AppCopyrightLabel.SetVisibility(Visibility.Collapsed);
                    AppCopyrightLabel.Content = string.Empty;
                }
                else
                {
                    AppCopyrightLabel.SetVisibility(Visibility.Visible);
                    AppCopyrightLabel.Content = value;
                }
            }
        }

        // <summary>
        // multiple lines of miscellaneous text to show in rich text box
        // </summary>
        // <remarks>
        // defaults to "%product% is %copyright%, %trademark%"
        // %product% = Assembly: AssemblyProduct
        // %copyright% = Assembly: AssemblyCopyright
        // %trademark% = Assembly: AssemblyTrademark
        // </remarks>
        public string AppMoreInfo
        {
#if TODO_AVALONIA
            get => new TextRange(MoreRichTextBox.Document.ContentStart, MoreRichTextBox.Document.ContentEnd).Text;
#else
            get => AppMoreInfoLabel.Text ?? string.Empty;
#endif
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    MoreRichTextBox.SetVisibility(Visibility.Collapsed);
                }
                else
                {
                    MoreRichTextBox.SetVisibility(Visibility.Visible);

#if AVALONIA
                    string text = ReplaceTokens(value);
                    AppMoreInfoLabel.Text = null;
                    AppMoreInfoLabel.Inlines.Clear();

                    // Parse string to detect hyperlinks and add handlers to them
                    // See: https://mycsharp.de/forum/threads/97560/erledigt-dynamische-hyperlinks-in-wpf-flowdocument?page=1
                    int lastPos = 0;
                    foreach (Match match in (IEnumerable<Match>)RegexUrl().Matches(text))
                    {
                        if (match.Index != lastPos)
                        {
                            AppMoreInfoLabel.Inlines.Add(text[lastPos..match.Index]);
                        }

                        AppMoreInfoLabel.Inlines.Add(CreateInlineHyperlink(match.Value, match.Value));

                        lastPos = match.Index + match.Length;
                    }

                    if (lastPos < text.Length)
                    {
                        AppMoreInfoLabel.Inlines.Add(text[lastPos..]);
                    }
#else
                    MoreRichTextBox.Document.Blocks.Clear();

                    Paragraph para = new ();

                    // Parse string to detect hyperlinks and add handlers to them
                    // See: https://mycsharp.de/forum/threads/97560/erledigt-dynamische-hyperlinks-in-wpf-flowdocument?page=1
                    int lastPos = 0;
                    foreach (Match match in (IEnumerable<Match>)RegexUrl().Matches(value))
                    {
                        if (match.Index != lastPos)
                        {
                            para.Inlines.Add(value[lastPos..match.Index]);
                        }

                        var link = new Hyperlink(new Run(match.Value))
                        {
                            NavigateUri = new Uri(match.Value),
                        };
                        link.Click += MoreRichTextBox_LinkClicked;

                        para.Inlines.Add(link);

                        lastPos = match.Index + match.Length;
                    }

                    if (lastPos < value.Length)
                    {
                        para.Inlines.Add(value[lastPos..]);
                    }

                    MoreRichTextBox.Document.Blocks.Add(para);
#endif
                }
            }
        }

        // <summary>
        // returns the entry assembly for the current application domain
        // </summary>
        // <remarks>
        // This is usually read-only, but in some weird cases (Smart Client apps)
        // you won't have an entry assembly, so you may want to set this manually.
        // </remarks>
        private Assembly? AppEntryAssembly { get; set; }

        public static bool IsOpen() => singletonWindow != null;

        public static void ShowSingleInstance()
        {
            if (IsOpen())
            {
                singletonWindow!.HandleInvoke(() => singletonWindow?.Activate());
            }
            else
            {
                singletonWindow = CreateAbout();
                singletonWindow.Show();
            }
        }

        [GeneratedRegex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?")]
        private static partial Regex RegexUrl();

        [GeneratedRegex("(\\.Assembly|\\.)(?<ColumnText>[^.]*)Attribute$", RegexOptions.IgnoreCase)]
        private static partial Regex RegexAssemblyAttrib();

#if AVALONIA
        private static InlineUIContainer CreateInlineHyperlink(string linkName, string navigateUri)
        {
            // Workaround until it might become an Avalonia feature: Custom UI Inline control to implement inline hyperlink
            //   See: https://github.com/AvaloniaUI/Avalonia/discussions/8818
            //   See: https://github.com/AvaloniaUI/Avalonia/discussions/6664
            //   See: https://github.com/AvaloniaUI/Avalonia/blob/9849da209c1c958e65fe4f9adc1657e0264ca515/tests/Avalonia.Controls.UnitTests/TextBlockTests.cs#L282-L295
            Button button = new()
            {
                Content = linkName,
                Template = new FuncControlTemplate<Button>((parent, scope) =>
                    new TextBlock
                    {
                        Name = "PART_ContentPresenter",

                        // ! is Avlonia property operator, See: public static IndexerDescriptor operator !(AvaloniaProperty property)
                        [!TextBlock.TextProperty] = parent[!ContentProperty],
                        TextDecorations = Avalonia.Media.TextDecorationCollection.Parse("Underline"),
                        Foreground = AppColors.Hyperlink,
                    }.RegisterInNameScope(scope)),
                Command = new ActionCommand((_) => Log.ProcessStart(navigateUri)),
                Cursor = new (StandardCursorType.Hand),
            };
            ToolTip.SetTip(button, navigateUri);
            return new (button);
        }
#endif

        private static AboutBox CreateAbout()
        {
            string copyright = string.Empty;
            string productName = string.Empty;
            string fileDescription = string.Empty;
            string fileVersion = string.Empty;
            string? location = Assembly.GetEntryAssembly()?.Location;
            if (location != null)
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(location);
                if (versionInfo.LegalCopyright != null)
                {
                    copyright = versionInfo.LegalCopyright;
                }

                if (versionInfo.ProductName != null)
                {
                    productName = versionInfo.ProductName;
                }

                if (versionInfo.FileDescription != null)
                {
                    fileDescription = versionInfo.FileDescription;
                }

                if (versionInfo.FileVersion != null)
                {
                    fileVersion = versionInfo.FileVersion;
                }
            }

            string moreInfo = copyright + Environment.NewLine;
            moreInfo += "Markus Hofknecht (mailto:Markus@Hofknecht.eu)" + Environment.NewLine;

            // Thanks for letting me being part of this project and that I am allowed to be listed here :-)
            // Made most of WPF port and whole Avalonia and initial Linux support
            moreInfo += "Peter Kirmeier (https://github.com/topeterk/)" + Environment.NewLine;

            moreInfo += "http://www.hofknecht.eu/systemtraymenu/" + Environment.NewLine;
            moreInfo += "https://github.com/Hofknecht/SystemTrayMenu" + Environment.NewLine;
            moreInfo += Environment.NewLine;
            moreInfo += "GNU GENERAL PUBLIC LICENSE" + Environment.NewLine;
            moreInfo += "(Version 3, 29 June 2007)" + Environment.NewLine;

            moreInfo += "Thanks for ideas, reporting issues and contributing!" + Environment.NewLine;
            moreInfo +=
                "#123 Mordecai00, " +
                "#125 Holgermh, " +
                "#135 #153 #154 #164 jakkaas, " +
                "#145 Pascal Aloy, " +
                "#153 #158 #160 blackcrack, " +
                "#162 HansieNL, " +
                "#163 igorruckert, " +
                "#171 kehoen, " +
                "#186 Dtrieb, " +
                "#188 #189 #191 #195 iJahangard, " +
                "#195 #197 #225 #238 the-phuctran, " +
                "#205 kristofzerbe, " +
                "#209 jonaskohl, " +
                "#211 blacksparrow15, " +
                "#220 #403 Yavuz E., " +
                "#229 #230 #239 Peter O., " +
                "#231 Ryonez, " +
                "#235 #242 243 #247, #271 Tom, " +
                "#237 Torsten S., " +
                "#240 video Patrick, " +
                "#244 Gunter D., " +
                "#246 #329 MACE4GITHUB, " +
                "#259 #310 vanjac, " +
                "#262 terencemcdonnell, " +
                "#269 petersnows25, " +
                "#272 Peter M., " +
                "#273 #274 ParasiteDelta, " +
                "#275 #276 #278 donaldaken, " +
                "#277 Jan S., " +
                "#282 akuznets, " +
                "#283 #284 #289 RuSieg, " +
                "#285 #286 dao-net, " +
                "#288 William P., " +
                "#294 #295 #296 Stefan Mahrer, " +
                "#225 #297 #299 #317 #321 #324 #330 #386 #390 #401 #402 #407 #409 #414 #416 #418 #428 #430 #443 chip33, " +
                "#298 phanirithvij, " +
                "#306 wini2, " +
                "#370 dna5589, " +
                "#372 not-nef, " +
                "#376 Michelle H., " +
                "#377 SoenkeHob, " +
                "#380 #394 TransLucida, " +
                "#384 #434 #435 boydfields, " +
                "#386 visusys, " +
                "#387 #411 #444 yrctw, " +
                "#446 timinformatica, " +
                "#450 ppt-oldoerp, " +
                "#453 fubaWoW, " +
                "#454 WouterVanGoey, " +
                "#462 verdammt89x, " +
                "#463 Dirk S., " +
                "#466 Dean-Corso, " +
                "#488 DailenG, " +
                "#490 TrampiPW, " +
                "#497 Aziz, " +
                "#499 spitzlbergerj, " +
                Environment.NewLine +
                Environment.NewLine;
            moreInfo += "Sponsors - Thank you!" + Environment.NewLine;
            moreInfo +=
                "Stefan Mahrer, " +
                "boydfields, " +
                "RuSieg, " +
                "igor-davidov, " +
                "Ralf K., " +
                "Tim K., " +
                "Georg W., " +
                "donaldaken, " +
                "Marc Speer, " +
                "Cito, " +
                "Peter G., " +
                "Traditional_Tap3954, " +
                "Maximilian H., " +
                "Jens B., " +
                "spitzlbergerj, " +
                Environment.NewLine;
            return new()
            {
                AppTitle = productName,
                AppDescription = fileDescription,
                AppVersion = $"Version {fileVersion}",
                AppCopyright = copyright,
                AppMoreInfo = moreInfo,
            };
        }

        // <summary>
        // exception-safe retrieval of LastWriteTime for this assembly.
        // </summary>
        // <returns>File.GetLastWriteTime, or DateTime.MaxValue if exception was encountered.</returns>
        private static DateTime AssemblyLastWriteTime(Assembly a)
        {
            DateTime assemblyLastWriteTime = DateTime.MaxValue;

            // Location property not available for dynamic assemblies
            if (!a.IsDynamic)
            {
                if (!string.IsNullOrEmpty(a.Location))
                {
                    assemblyLastWriteTime = File.GetLastWriteTime(a.Location);
                }
            }

            return assemblyLastWriteTime;
        }

        // <summary>
        // returns DateTime this Assembly was last built. Will attempt to calculate from build number, if possible.
        // If not, the actual LastWriteTime on the assembly file will be returned.
        // </summary>
        // <param name="a">Assembly to get build date for</param>
        // <param name="ForceFileDate">Don't attempt to use the build number to calculate the date</param>
        // <returns>DateTime this assembly was last built</returns>
        private static DateTime AssemblyBuildDate(Assembly a, bool forceFileDate)
        {
            Version? assemblyVersion = a.GetName().Version;
            DateTime dt;

            if (forceFileDate || assemblyVersion == null)
            {
                dt = AssemblyLastWriteTime(a);
            }
            else
            {
                dt = DateTime.Parse("01/01/2000", CultureInfo.InvariantCulture).AddDays(assemblyVersion.Build).AddSeconds(assemblyVersion.Revision * 2);
#pragma warning disable CS0618
                if (TimeZone.IsDaylightSavingTime(dt, TimeZone.CurrentTimeZone.GetDaylightChanges(dt.Year)))
#pragma warning restore CS0618
                {
                    dt = dt.AddHours(1);
                }

                if (dt > DateTime.Now || assemblyVersion.Build < 730 || assemblyVersion.Revision == 0)
                {
                    dt = AssemblyLastWriteTime(a);
                }
            }

            return dt;
        }

        // <summary>
        // returns string name / string value pair of all attribs
        // for specified assembly
        // </summary>
        // <remarks>
        // note that Assembly* values are pulled from AssemblyInfo file in project folder
        //
        // Trademark       = AssemblyTrademark string
        // Debuggable      = true
        // GUID            = 7FDF68D5-8C6F-44C9-B391-117B5AFB5467
        // CLSCompliant    = true
        // Product         = AssemblyProduct string
        // Copyright       = AssemblyCopyright string
        // Company         = AssemblyCompany string
        // Description     = AssemblyDescription string
        // Title           = AssemblyTitle string
        // </remarks>
        private static NameValueCollection AssemblyAttribs(Assembly a)
        {
            string typeName;
            string name;
            string value;
            NameValueCollection nvc = new();
            Regex r = RegexAssemblyAttrib();

            foreach (object attrib in a.GetCustomAttributes(false))
            {
                typeName = attrib.GetType().ToString();
                name = r.Match(typeName).Groups["ColumnText"].ToString();
                value = string.Empty;
                switch (typeName)
                {
                    case "System.CLSCompliantAttribute":
                        value = ((CLSCompliantAttribute)attrib).IsCompliant.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Diagnostics.DebuggableAttribute":
                        value = ((System.Diagnostics.DebuggableAttribute)attrib).IsJITTrackingEnabled.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyCompanyAttribute":
                        value = ((AssemblyCompanyAttribute)attrib).Company.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyConfigurationAttribute":
                        value = ((AssemblyConfigurationAttribute)attrib).Configuration.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyCopyrightAttribute":
                        value = ((AssemblyCopyrightAttribute)attrib).Copyright.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyDefaultAliasAttribute":
                        value = ((AssemblyDefaultAliasAttribute)attrib).DefaultAlias.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyDelaySignAttribute":
                        value = ((AssemblyDelaySignAttribute)attrib).DelaySign.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyDescriptionAttribute":
                        value = ((AssemblyDescriptionAttribute)attrib).Description.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyInformationalVersionAttribute":
                        value = ((AssemblyInformationalVersionAttribute)attrib).InformationalVersion.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyKeyFileAttribute":
                        value = ((AssemblyKeyFileAttribute)attrib).KeyFile.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyProductAttribute":
                        value = ((AssemblyProductAttribute)attrib).Product.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyTrademarkAttribute":
                        value = ((AssemblyTrademarkAttribute)attrib).Trademark.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Reflection.AssemblyTitleAttribute":
                        value = ((AssemblyTitleAttribute)attrib).Title.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Resources.NeutralResourcesLanguageAttribute":
                        value = ((System.Resources.NeutralResourcesLanguageAttribute)attrib).CultureName.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Resources.SatelliteContractVersionAttribute":
                        value = ((System.Resources.SatelliteContractVersionAttribute)attrib).Version.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Runtime.InteropServices.ComCompatibleVersionAttribute":
                        {
                            System.Runtime.InteropServices.ComCompatibleVersionAttribute x;
                            x = (System.Runtime.InteropServices.ComCompatibleVersionAttribute)attrib;
                            value = x.MajorVersion + "." + x.MinorVersion + "." + x.RevisionNumber + "." + x.BuildNumber;
                            break;
                        }

                    case "System.Runtime.InteropServices.ComVisibleAttribute":
                        value = ((System.Runtime.InteropServices.ComVisibleAttribute)attrib).Value.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Runtime.InteropServices.GuidAttribute":
                        value = ((System.Runtime.InteropServices.GuidAttribute)attrib).Value.ToString(CultureInfo.InvariantCulture); break;
                    case "System.Runtime.InteropServices.TypeLibVersionAttribute":
                        {
                            System.Runtime.InteropServices.TypeLibVersionAttribute x;
                            x = (System.Runtime.InteropServices.TypeLibVersionAttribute)attrib;
                            value = x.MajorVersion + "." + x.MinorVersion;
                            break;
                        }

                    case "System.Security.AllowPartiallyTrustedCallersAttribute":
                        value = "(Present)"; break;
                    default:
                        // debug.writeline("** unknown assembly attribute '" + TypeName + "'")
                        value = typeName; break;
                }

                if (nvc[name] == null)
                {
                    nvc.Add(name, value);
                }
            }

            // add some extra values that are not in the AssemblyInfo, but nice to have
            // codebase
            try
            {
                if (!a.IsDynamic)
                {
                    nvc.Add("CodeBase", a.Location.Replace("file:///", string.Empty, StringComparison.InvariantCulture));
                }
            }
            catch (NotSupportedException)
            {
                nvc.Add("CodeBase", "(not supported)");
            }

            // build date
            DateTime dt = AssemblyBuildDate(a, false);
            if (dt == DateTime.MaxValue)
            {
                nvc.Add("BuildDate", "(unknown)");
            }
            else
            {
                nvc.Add("BuildDate", dt.ToString("yyyy-MM-dd hh:mm tt", CultureInfo.InvariantCulture));
            }

            // location
            try
            {
                if (!a.IsDynamic)
                {
                    nvc.Add("Location", a.Location);
                }
            }
            catch (NotSupportedException)
            {
                nvc.Add("Location", "(not supported)");
            }

            string version = "(unknown)";
            AssemblyName assemblyName = a.GetName();
            if (assemblyName.Version != null &&
                (assemblyName.Version.Major != 0 || assemblyName.Version.Minor != 0))
            {
                if (!a.IsDynamic)
                {
                    version = a.GetName().Version?.ToString() ?? version;
                }
            }

            nvc.Add("Version", version);

            if (!a.IsDynamic)
            {
                nvc.Add("FullName", a.FullName);
            }

            return nvc;
        }

        // <summary>
        // reads an HKLM Windows Registry key value
        // </summary>
        [SupportedOSPlatform("Windows")]
        private static string RegistryHklmValue(string keyName, string subKeyRef)
        {
            string strSysInfoPath = string.Empty;
            try
            {
                RegistryKey? rk = Registry.LocalMachine.OpenSubKey(keyName);
                if (rk != null)
                {
                    strSysInfoPath = (string)rk.GetValue(subKeyRef, string.Empty)!;
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"KeyName:'{keyName}' SubKeyRef:'{subKeyRef}'", ex);
            }

            return strSysInfoPath;
        }

        // <summary>
        // populate details for a single assembly
        // </summary>
#if !AVALONIA
        private static void PopulateAssemblyDetails(Assembly? a, ListView lvw)
#else
        private static void PopulateAssemblyDetails(Assembly? a, DataGrid lvw)
#endif
        {
            lvw.ItemsSource = null;

            if (a != null)
            {
                NameValueCollection nvc = AssemblyAttribs(a);
                List<AssemblyDetailsListViewItem> items = new(nvc.Count + 1)
                {
                    new ()
                    {
                        Key = "Image Runtime Version",
                        Value = a.ImageRuntimeVersion,
                    },
                };

                foreach (string strKey in nvc)
                {
                    string? value = nvc[strKey];
                    if (!string.IsNullOrEmpty(value))
                    {
                        items.Add(new()
                        {
                            Key = strKey,
                            Value = value,
                        });
                    }
                }

                lvw.ItemsSource = items;
            }
        }

        // <summary>
        // matches assembly by Assembly.GetName.ColumnText; returns nothing if no match
        // </summary>
        private static Assembly? MatchAssemblyByName(string assemblyName)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.GetName().Name == assemblyName)
                {
                    return a;
                }
            }

            return null;
        }

        private void TabPanelDetails_SelectedIndexChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (TabPanelDetails.SelectedItem == TabPageAssemblyDetails)
            {
                AssemblyNamesComboBox.Focus();
            }
        }

        // <summary>
        // launch the MSInfo "system information" application (works on XP, 2003, and Vista)
        // </summary>
        [SupportedOSPlatform("Windows")]
        private void ShowSysInfo()
        {
            string strSysInfoPath = RegistryHklmValue(@"SOFTWARE\Microsoft\Shared Tools Location", "MSINFO");
            if (string.IsNullOrEmpty(strSysInfoPath))
            {
                strSysInfoPath = RegistryHklmValue(@"SOFTWARE\Microsoft\Shared Tools\MSINFO", "PATH");
            }

            if (string.IsNullOrEmpty(strSysInfoPath))
            {
                MessageBox.Show(
                    "System Information is unavailable at this time." +
                    Environment.NewLine + Environment.NewLine +
                    "(couldn't find path for Microsoft System Information Tool in the registry.)",
                    Title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Log.ProcessStart(strSysInfoPath);
        }

        // <summary>
        // populates the Application Information listview
        // </summary>
        private void PopulateAppInfo()
        {
            AppDomain d = AppDomain.CurrentDomain;
            List<AssemblyDetailsListViewItem> items = new ();

            foreach ((string key, string? value) in new List<KeyValuePair<string, string?>>()
            {
                new ("Application ColumnText", Assembly.GetEntryAssembly()?.GetName().Name),
                new ("Application Base", d.SetupInformation.ApplicationBase),
                new ("Friendly ColumnText", d.FriendlyName),
                new (" ", " "),
                new ("Entry Assembly", entryAssemblyName),
                new ("Executing Assembly", executingAssemblyName),
                new ("Calling Assembly", callingAssemblyName),
            })
            {
                if (!string.IsNullOrEmpty(value))
                {
                    items.Add(new ()
                    {
                        Key = key,
                        Value = value,
                    });
                }
            }

            AppInfoListView.ItemsSource = items;
        }

        // <summary>
        // populate Assembly Information listview with ALL assemblies
        // </summary>
        private void PopulateAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<AssemblyInfoListViewItem> items = new (assemblies.Length);

            foreach (Assembly a in assemblies)
            {
                AssemblyInfoListViewItem item = CreateAssemblySummaryEntry(a);
                items.Add(item);
                AssemblyNamesComboBox.Items.Add(item.Tag);
            }

            AssemblyInfoListView.ItemsSource = items;
            AssemblyNamesComboBox.SelectedIndex = AssemblyNamesComboBox.Items.IndexOf(entryAssemblyName);
        }

        // <summary>
        // populate Assembly Information listview with summary view for a specific assembly
        // </summary>
        private AssemblyInfoListViewItem CreateAssemblySummaryEntry(Assembly a)
        {
            NameValueCollection nvc = AssemblyAttribs(a);

            string strAssemblyName = a.GetName().Name ?? "?";
            string strAssemblyNameFull = strAssemblyName;
            if (strAssemblyName == callingAssemblyName)
            {
                strAssemblyNameFull += " (calling)";
            }
            else if (strAssemblyName == executingAssemblyName)
            {
                strAssemblyNameFull += " (executing)";
            }
            else if (strAssemblyName == entryAssemblyName)
            {
                strAssemblyNameFull += " (entry)";
            }

            return new ()
            {
                Name = strAssemblyNameFull,
                Version = nvc["version"] ?? string.Empty,
                Built = nvc["builddate"] ?? string.Empty,
                CodeBase = nvc["codebase"] ?? string.Empty,
                Tag = strAssemblyName,
            };
        }

        // <summary>
        // retrieves a cached value from the entry assembly attribute lookup collection
        // </summary>
        private string? EntryAssemblyAttrib(string strName)
        {
            if (entryAssemblyAttribCollection[strName] == null)
            {
                return "<Assembly: Assembly" + strName + "(\"\")>";
            }
            else
            {
                return entryAssemblyAttribCollection[strName]?.ToString(CultureInfo.InvariantCulture);
            }
        }

        // <summary>
        // Populate all the form labels with tokenized text
        // </summary>
        private void PopulateLabels()
        {
            // get entry assembly attribs
            entryAssemblyAttribCollection = AssemblyAttribs(AppEntryAssembly!);

            // replace all labels and window title
            Title = ReplaceTokens(Title);
            AppTitle = ReplaceTokens(AppTitle);
            if (AppDescriptionLabel.GetVisibility() == Visibility.Visible)
            {
                AppDescription = ReplaceTokens(AppDescription);
            }

            if (AppCopyrightLabel.GetVisibility() == Visibility.Visible)
            {
                AppCopyright = ReplaceTokens(AppCopyright);
            }

            if (AppVersionLabel.GetVisibility() == Visibility.Visible)
            {
                AppVersion = ReplaceTokens(AppVersion);
            }

            if (AppDateLabel.GetVisibility() == Visibility.Visible)
            {
                AppDateLabel.Content = ReplaceTokens((string)AppDateLabel.Content);
            }

#if !AVALONIA
            if (MoreRichTextBox.GetVisibility() == Visibility.Visible)
            {
                AppMoreInfo = ReplaceTokens(AppMoreInfo);
            }
#endif
        }

        // <summary>
        // perform assemblyinfo to string replacements on labels
        // </summary>
#if AVALONIA
        [return: NotNullIfNotNull(nameof(s))]
        private string? ReplaceTokens(string? s) => s?
#else
        private string ReplaceTokens(string s) => s
#endif
                .Replace("%title%", EntryAssemblyAttrib("title"), StringComparison.InvariantCulture)
                .Replace("%copyright%", EntryAssemblyAttrib("copyright"), StringComparison.InvariantCulture)
                .Replace("%description%", EntryAssemblyAttrib("description"), StringComparison.InvariantCulture)
                .Replace("%company%", EntryAssemblyAttrib("company"), StringComparison.InvariantCulture)
                .Replace("%product%", EntryAssemblyAttrib("product"), StringComparison.InvariantCulture)
                .Replace("%trademark%", EntryAssemblyAttrib("trademark"), StringComparison.InvariantCulture)
                .Replace("%year%", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture), StringComparison.InvariantCulture)
                .Replace("%version%", EntryAssemblyAttrib("version"), StringComparison.InvariantCulture)
                .Replace("%builddate%", EntryAssemblyAttrib("builddate"), StringComparison.InvariantCulture);

        // <summary>
        // things to do when form is loaded
        // </summary>
        private void AboutBox_Load(object? sender, RoutedEventArgs e)
        {
            // if the user didn't provide an assembly, try to guess which one is the entry assembly
            AppEntryAssembly ??= Assembly.GetEntryAssembly()!;
            AppEntryAssembly ??= Assembly.GetExecutingAssembly();

            executingAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            callingAssemblyName = Assembly.GetCallingAssembly().GetName().Name;

            // for web hosted apps, GetEntryAssembly = nothing
            entryAssemblyName = Assembly.GetEntryAssembly()?.GetName().Name;

            TabPanelDetails.SetVisibility(Visibility.Collapsed);
            if (MoreRichTextBox.GetVisibility() != Visibility.Visible)
            {
                Height -= MoreRichTextBox.Height;
            }

            WPFExtensions.CurrentDispatcher.Invoke(
                DispatcherPriority.Loaded,
                new Action(delegate
                {
#if !AVALONIA
                    Cursor = Cursors.Wait;
#else
                    Cursor = new Cursor(StandardCursorType.Wait);
#endif
                    PopulateLabels();
#if !AVALONIA
                    Cursor = null;
#else
                    Cursor = Cursor.Default;
#endif
                }));
        }

        // <summary>
        // expand about dialog to show additional advanced details
        // </summary>
        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
#if !AVALONIA
            Cursor = Cursors.Wait;
#else
            Cursor = new Cursor(StandardCursorType.Wait);
#endif
            MoreRichTextBox.SetVisibility(Visibility.Collapsed);
            TabPanelDetails.SetVisibility(Visibility.Visible);
            if (OperatingSystem.IsWindows())
            {
                buttonSystemInfo.SetVisibility(Visibility.Visible);
            }

            buttonDetails.SetVisibility(Visibility.Collapsed);
            UpdateLayout(); // Force AutoSize to update the height before switching to manual mode
            SizeToContent = SizeToContent.Manual;
#if !AVALONIA
            ResizeMode = ResizeMode.CanResizeWithGrip;
#else
            CanResize = true;
#endif
            TabPanelDetails.Height = double.NaN;
#if !AVALONIA
            if (Width < 580)
            {
                Width = 580;
            }
#endif

            PopulateAssemblies();
            PopulateAppInfo();
#if !AVALONIA
            Cursor = null;
#else
            Cursor = Cursor.Default;
#endif
        }

        // <summary>
        // for detailed system info, launch the external Microsoft system info app
        // </summary>
        [SupportedOSPlatform("Windows")]
        private void SysInfoButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSysInfo();
        }

        /// <summary>
        /// Closes the window.
        /// </summary>
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // <summary>
        // if an assembly is double-clicked, go to the detail page for that assembly
        // </summary>
#if AVALONIA
        private void AssemblyInfoListView_DoubleClick(object sender, TappedEventArgs e)
#else
        private void AssemblyInfoListView_DoubleClick(object sender, MouseButtonEventArgs e)
#endif
        {
            if (AssemblyInfoListView.SelectedItems.Count > 0)
            {
                string? strAssemblyName = Convert.ToString(((AssemblyInfoListViewItem?)AssemblyInfoListView.SelectedItems[0])?.Tag, CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(strAssemblyName))
                {
                    AssemblyNamesComboBox.SelectedIndex = AssemblyNamesComboBox.Items.IndexOf(strAssemblyName);
                    TabPanelDetails.SelectedItem = TabPageAssemblyDetails;
                }
            }
        }

        // <summary>
        // if a new assembly is selected from the combo box, show details for that assembly
        // </summary>
        private void AssemblyNamesComboBox_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            string? strAssemblyName = Convert.ToString(AssemblyNamesComboBox.SelectedItem, CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(strAssemblyName))
            {
                PopulateAssemblyDetails(MatchAssemblyByName(strAssemblyName), AssemblyDetailsListView);
            }
        }

#if !AVALONIA
        // <summary>
        // sort the assembly list by column
        // </summary>
        private void AssemblyInfoListView_ColumnClick(object sender, RoutedEventArgs e)
        {
            AssemblyInfoListView.Items.SortDescriptions.Clear();
            AssemblyInfoListView.Items.SortDescriptions.Add(new SortDescription(
                ((GridViewColumnHeader)e.OriginalSource).Column.Header.ToString(),
                ListSortDirection.Ascending));
            AssemblyInfoListView.Items.Refresh();
        }
#endif

#if !AVALONIA
        // <summary>
        // launch any http:// or mailto: links clicked in the body of the rich text box
        // </summary>
        private void MoreRichTextBox_LinkClicked(object sender, RoutedEventArgs e)
        {
            Log.ProcessStart(((Hyperlink)sender).NavigateUri.ToString());
        }
#endif

        /// <summary>
        /// Type for ListView items.
        /// </summary>
        private class AssemblyDetailsListViewItem
        {
            public string Key { get; set; } = string.Empty;

            public string Value { get; set; } = string.Empty;
        }

        /// <summary>
        /// Type for ListView items.
        /// </summary>
        private class AssemblyInfoListViewItem
        {
            public string Name { get; set; } = string.Empty;

            public string Version { get; set; } = string.Empty;

            public string Built { get; set; } = string.Empty;

            public string CodeBase { get; set; } = string.Empty;

            public string Tag { get; set; } = string.Empty;
        }
    }
}
