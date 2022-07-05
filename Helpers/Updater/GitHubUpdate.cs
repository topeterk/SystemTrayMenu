﻿// <copyright file="GitHubUpdate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace SystemTrayMenu.Helper.Updater
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Net.Http;
    using System.Reflection;
    using System.Windows.Forms;
    using SystemTrayMenu.Utilities;

    public class GitHubUpdate
    {
        private static List<Dictionary<string, object>> releases;
        private static Form newVersionForm;

        public static void ActivateNewVersionFormOrCheckForUpdates(bool showWhenUpToDate)
        {
            if (newVersionForm != null)
            {
                newVersionForm.Activate();
            }
            else
            {
                CheckForUpdates(showWhenUpToDate);
            }
        }

        private static void CheckForUpdates(bool showWhenUpToDate)
        {
            string urlGithubReleases = @"http://api.github.com/repos/Hofknecht/SystemTrayMenu/releases";
            HttpClient client = new();

            // https://developer.github.com/v3/#user-agent-required
            client.DefaultRequestHeaders.Add("User-Agent", "SystemTrayMenu/" + Application.ProductVersion.ToString());

            // https://developer.github.com/v3/media/#request-specific-version
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3.text+json");

            try
            {
                using HttpResponseMessage response = client.GetAsync(urlGithubReleases).Result;
                using HttpContent content = response.Content;
                string responseString = content.ReadAsStringAsync().Result;
                releases = responseString.FromJson<List<Dictionary<string, object>>>();
            }
            catch (Exception ex)
            {
                Log.Warn($"{nameof(CheckForUpdates)} failed", ex);
            }

            if (releases == null)
            {
                Log.Info($"{nameof(CheckForUpdates)} failed.");
            }
            else
            {
                RemoveCurrentAndOlderVersions();
                ShowNewVersionOrUpToDateDialog(showWhenUpToDate);
            }

            newVersionForm?.Dispose();
            newVersionForm = null;
        }

        private static void RemoveCurrentAndOlderVersions()
        {
            int releasesCount = releases.Count;
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string majorPlusMinorVersionString = $"{version.Major}.{version.Minor}.{version.Build}";
            for (int i = 0; i < releasesCount; i++)
            {
                string tag_name = releases[i]["tag_name"].ToString();
                if (tag_name.Contains($"{majorPlusMinorVersionString}."))
                {
                    releases.RemoveRange(i, releasesCount - i);
                    break;
                }
            }
        }

        private static void ShowNewVersionOrUpToDateDialog(bool showWhenUpToDate)
        {
            if (releases.Count > 0)
            {
                if (NewVersionDialog() == DialogResult.Yes)
                {
                    Log.ProcessStart("https://github.com/Hofknecht/SystemTrayMenu/releases");
                }
            }
            else if (showWhenUpToDate)
            {
                MessageBox.Show(Translator.GetText("You have the latest version of SystemTrayMenu!"));
            }
        }

        /// <summary>
        /// Creates a window to show changelog of new available versions.
        /// </summary>
        /// <param name="LatestVersionTitle">Name of latest release.</param>
        /// <param name="Changelog">Pathnotes.</param>
        /// <returns>OK = OK, Yes = Website, else = Cancel.</returns>
        private static DialogResult NewVersionDialog()
        {
            const int ClientPad = 15;
            newVersionForm = new();

            newVersionForm.StartPosition = FormStartPosition.CenterScreen;
            newVersionForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            newVersionForm.Icon = Config.GetAppIcon();
            newVersionForm.ShowInTaskbar = false;
            newVersionForm.FormBorderStyle = FormBorderStyle.Sizable;
            newVersionForm.MaximizeBox = true;
            newVersionForm.MinimizeBox = false;
            newVersionForm.ClientSize = new Size(600, 400);
            newVersionForm.MinimumSize = newVersionForm.ClientSize;
            newVersionForm.Text = Translator.GetText("New version available!");

            Label label = new();
            label.Size = new Size(newVersionForm.ClientSize.Width - ClientPad, 20);
            label.Location = new Point(ClientPad, ClientPad);
            label.Text = $"{Translator.GetText("Latest available version:")}    {GetLatestVersionName()}";
            newVersionForm.Controls.Add(label);

            Button okButton = new();
            okButton.DialogResult = DialogResult.OK;
            okButton.Name = "okButton";
            okButton.Location = new Point(
                newVersionForm.ClientSize.Width - okButton.Size.Width - ClientPad,
                newVersionForm.ClientSize.Height - okButton.Size.Height - ClientPad);
            okButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            okButton.Text = Translator.GetText("OK");
            newVersionForm.Controls.Add(okButton);

            Button wwwButton = new();
            wwwButton.DialogResult = DialogResult.Yes;
            wwwButton.Name = "wwwButton";
            wwwButton.Location = new Point(
                newVersionForm.ClientSize.Width - wwwButton.Size.Width - ClientPad - okButton.Size.Width - ClientPad,
                newVersionForm.ClientSize.Height - wwwButton.Size.Height - ClientPad);
            wwwButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            wwwButton.Text = Translator.GetText("Go to download page");
            wwwButton.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            wwwButton.AutoSize = true;
            newVersionForm.Controls.Add(wwwButton);

            TextBox textBox = new();
            textBox.Location = new Point(ClientPad, label.Location.Y + label.Size.Height + 5);
            textBox.Size = new Size(
                newVersionForm.ClientSize.Width - (ClientPad * 2),
                okButton.Location.Y - ClientPad - textBox.Location.Y);
            textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textBox.Multiline = true;
            textBox.Text = GetChangelog();
            textBox.ReadOnly = true;
            textBox.ScrollBars = ScrollBars.Both;
            textBox.BackColor = Color.FromKnownColor(KnownColor.Window);
            textBox.ForeColor = Color.FromKnownColor(KnownColor.ControlText);
            newVersionForm.Controls.Add(textBox);

            newVersionForm.AcceptButton = okButton;
            return newVersionForm.ShowDialog();
        }

        /// <summary>
        /// Returns the latest release version name.
        /// </summary>
        /// <returns>Version name.</returns>
        private static string GetLatestVersionName()
        {
            string result = "Unknown";

            if (releases == null)
            {
                return result;
            }

            try
            {
                result = releases[0]["tag_name"].ToString().Replace("v", string.Empty);
            }
            catch (Exception ex)
            {
                Log.Warn($"{nameof(GetLatestVersionName)} failed", ex);
            }

            return result;
        }

        /// <summary>
        /// Returns the change log from current version up to the latest release version.
        /// </summary>
        /// <returns>Change log summary or error text.</returns>
        private static string GetChangelog()
        {
            string result = string.Empty;
            string errorstr = "An error occurred during update check!" + Environment.NewLine;

            if (releases == null)
            {
                return errorstr + "Could not receive changelog!";
            }

            try
            {
                for (int i = 0; i < releases.Count; i++)
                {
                    Dictionary<string, object> release = releases[i];

                    result += release["name"].ToString()
                        + Environment.NewLine
                        + release["body_text"].ToString()
                        .Replace("\n\n", Environment.NewLine)
                        .Replace("\n \n", Environment.NewLine)
                        + Environment.NewLine + Environment.NewLine;
                    if (i < releases.Count)
                    {
                        result += "--------------------------------------------------" +
                            "-------------------------------------------------------"
                            + Environment.NewLine;
                    }
                }

                result = result.Replace("\n", Environment.NewLine);
            }
            catch (Exception ex)
            {
                Log.Warn($"{nameof(GetChangelog)}", ex);
                result = errorstr + ex.Message.ToString();
            }

            return result;
        }
    }
}