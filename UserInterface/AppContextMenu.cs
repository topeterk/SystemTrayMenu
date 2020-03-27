﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using SystemTrayMenu.UserInterface;
using SystemTrayMenu.Utilities;

namespace SystemTrayMenu.Helper
{
    internal class AppContextMenu
    {
        public event EventHandler ClickedChangeFolder;
        public event EventHandler ClickedOpenLog;
        public event EventHandler ClickedRestart;
        public event EventHandler ClickedExit;

        public ContextMenuStrip Create()
        {
            ContextMenuStrip menu = new ContextMenuStrip
            {
                BackColor = SystemColors.Control
            };
            ToolStripMenuItem changeFolder = new ToolStripMenuItem
            {
                Text = Language.Translate("Folder")
            };
            changeFolder.Click += ChangeFolder_Click;
            void ChangeFolder_Click(object sender, EventArgs e)
            {
                ClickedChangeFolder.Invoke();
            }
            menu.Items.Add(changeFolder);

            ToolStripMenuItem changeLanguage = new ToolStripMenuItem()
            {
                Name = "changeLanguage",
                Text = Language.Translate("Language")
            };
            foreach (CultureInfo cultureInfo in
                GetCultureList(CultureTypes.AllCultures))
            {
                if (MenuDefines.Languages.Contains(cultureInfo.Name))
                {
                    ToolStripItem language = changeLanguage.DropDownItems.
                        Add(Language.Translate(cultureInfo.EnglishName));
                    language.Click += Language_Click;
                    void Language_Click(object sender, EventArgs e)
                    {
                        string twoLetter = cultureInfo.Name.Substring(0, 2);
                        Properties.Settings.Default.CurrentCultureInfoName =
                               twoLetter;
                        Properties.Settings.Default.Save();
                        ClickedRestart.Invoke();
                    }
                    if (cultureInfo.Name == Properties.Settings.Default.
                        CurrentCultureInfoName)
                    {
                        language.Image = Properties.Resources.Selected;
                        language.ImageScaling = ToolStripItemImageScaling.None;
                        language.Image = ResizeImage(language.Image);
                    }
                }
            }
            menu.Items.Add(changeLanguage);

            ToolStripMenuItem autostart = new ToolStripMenuItem
            {
                Text = Language.Translate("Autostart")
            };
            //autostart.Image.HorizontalResolution.wi.c.sc.Select .ImageScaling = ToolStripItemImageScaling.None;
            if (Properties.Settings.Default.IsAutostartActivated)
            {
                autostart.Image = Properties.Resources.Selected;
            }
            else
            {
                autostart.Image = Properties.Resources.NotSelected;
            }
            autostart.ImageScaling = ToolStripItemImageScaling.None;
            autostart.Image = ResizeImage(autostart.Image);
            autostart.Click += Autostart_Click;
            void Autostart_Click(object sender, EventArgs e)
            {
                if (Properties.Settings.Default.IsAutostartActivated)
                {
                    Microsoft.Win32.RegistryKey key =
                        Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                        true);
                    key.DeleteValue("SystemTrayMenu", false);
                    Properties.Settings.Default.IsAutostartActivated = false;
                }
                else
                {
                    Microsoft.Win32.RegistryKey key =
                        Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run",
                            true);
                    key.SetValue(
                        Assembly.GetExecutingAssembly().GetName().Name,
                        Assembly.GetEntryAssembly().Location);
                    Properties.Settings.Default.IsAutostartActivated = true;
                }
                Properties.Settings.Default.Save();
                ClickedRestart.Invoke();
            }
            menu.Items.Add(autostart);

            ToolStripMenuItem hotKey = new ToolStripMenuItem();
            string hotKeyText =
                $"{Language.Translate("CTRL")} + " +
                $"{Language.Translate("ALT")} + ";

            hotKey.ImageScaling = ToolStripItemImageScaling.SizeToFit;
            if (string.IsNullOrEmpty(Properties.Settings.Default.HotKey))
            {
                hotKey.Image = Properties.Resources.NotSelected;
                hotKey.Text = hotKeyText + "? ";
            }
            else
            {
                hotKey.Image = Properties.Resources.Selected;
                hotKey.Text = hotKeyText +
                    $"{Properties.Settings.Default.HotKey}";
            }
            hotKey.ImageScaling = ToolStripItemImageScaling.None;
            hotKey.Image = ResizeImage(hotKey.Image);
            hotKey.Click += HotKey_Click;
            void HotKey_Click(object sender, EventArgs e)
            {
                AskHotKeyForm askHotKey = new AskHotKeyForm();
                if (askHotKey.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default.HotKey = askHotKey.NewHotKey;
                    Properties.Settings.Default.Save();
                    ClickedRestart?.Invoke();
                }
            }
            menu.Items.Add(hotKey);

            ToolStripSeparator seperator = new ToolStripSeparator
            {
                BackColor = SystemColors.Control
            };
            menu.Items.Add(seperator);

            ToolStripMenuItem openLog = new ToolStripMenuItem
            {
                Text = Language.Translate("Log File")
            };
            openLog.Click += OpenLog_Click;
            void OpenLog_Click(object sender, EventArgs e)
            {
                ClickedOpenLog.Invoke();
            }
            menu.Items.Add(openLog);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem about = new ToolStripMenuItem
            {
                Text = Language.Translate("About")
            };
            about.Click += About_Click;
            void About_Click(object sender, EventArgs e)
            {
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(
                    Assembly.GetEntryAssembly().Location);
                AboutBox ab = new AboutBox
                {
                    AppTitle = versionInfo.ProductName,
                    AppDescription = versionInfo.FileDescription,
                    AppVersion = $"Version {versionInfo.FileVersion}",
                    AppCopyright = versionInfo.LegalCopyright,
                    AppMoreInfo = versionInfo.LegalCopyright
                };
                ab.AppMoreInfo += Environment.NewLine;
                ab.AppMoreInfo += "Markus Hofknecht (mailto:Markus@Hofknecht.eu)";
                ab.AppMoreInfo += Environment.NewLine;
                ab.AppMoreInfo += "Tanja Kauth (mailto:Tanja@Hofknecht.eu)";
                ab.AppMoreInfo += Environment.NewLine;
                ab.AppMoreInfo += "http://www.hofknecht.eu/systemtraymenu/";
                ab.AppMoreInfo += Environment.NewLine;
                ab.AppMoreInfo += "https://github.com/Hofknecht/SystemTrayMenu";
                ab.AppMoreInfo += Environment.NewLine;
                ab.AppMoreInfo += Environment.NewLine;
                ab.AppMoreInfo += "GNU GENERAL PUBLIC LICENSE" + Environment.NewLine;
                ab.AppMoreInfo += "(Version 3, 29 June 2007)" + Environment.NewLine;
                ab.AppDetailsButton = true;
                ab.ShowDialog();
            }
            menu.Items.Add(about);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem restart = new ToolStripMenuItem
            {
                Text = Language.Translate("Restart")
            };
            restart.Click += Restart_Click;
            void Restart_Click(object sender, EventArgs e)
            {
                ClickedRestart.Invoke();
            }
            menu.Items.Add(restart);

            ToolStripMenuItem exit = new ToolStripMenuItem
            {
                Text = Language.Translate("Exit")
            };
            exit.Click += Exit_Click;
            void Exit_Click(object sender, EventArgs e)
            {
                ClickedExit.Invoke();
            }
            menu.Items.Add(exit);

            return menu;
        }

        /// <summary>
        /// https://www.codeproject.com/Tips/744914/
        /// Sorted-list-of-available-cultures-in-NET
        /// </summary>
        private static IEnumerable<CultureInfo> GetCultureList(
            CultureTypes cultureType = CultureTypes.SpecificCultures)
        {
            List<CultureInfo> cultureList = CultureInfo.GetCultures(cultureType).ToList();
            cultureList.Sort((p1, p2) => string.Compare(
                p1.NativeName, p2.NativeName, true));
            return cultureList;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image)
        {
            int length = (int)Math.Round(
                16 * Scaling.Factor, 0,
                MidpointRounding.AwayFromZero);
            return ResizeImage(image, length, length);
        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            Rectangle destRect = new Rectangle(0, 0, width, height);
            Bitmap destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}