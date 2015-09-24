﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace WitcherScriptMerger.Forms
{
    public partial class DependencyForm : Form
    {
        private bool AreAnyPathsChanged
        {
            get
            {
                return (!txtKDiff3Path.Text.EqualsIgnoreCase(Paths.KDiff3) ||
                        !txtBmsPath.Text.EqualsIgnoreCase(Paths.Bms) ||
                        !txtBmsPluginPath.Text.EqualsIgnoreCase(Paths.BmsPlugin) ||
                        !txtWccLitePath.Text.EqualsIgnoreCase(Paths.WccLite));
            }
        }

        public DependencyForm()
        {
            InitializeComponent();
        }

        private void DependencyForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(Paths.KDiff3))
                txtKDiff3Path.Text = Paths.KDiff3;
            if (File.Exists(Paths.Bms))
                txtBmsPath.Text = Paths.Bms;
            if (File.Exists(Paths.BmsPlugin))
                txtBmsPluginPath.Text = Paths.BmsPlugin;
            if (File.Exists(Paths.WccLite))
                txtWccLitePath.Text = Paths.WccLite;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            bool anyMissing =
                !File.Exists(txtKDiff3Path.Text) ||
                !File.Exists(txtBmsPath.Text) ||
                !File.Exists(txtBmsPluginPath.Text) ||
                !File.Exists(txtWccLitePath.Text);

            if (anyMissing &&
                DialogResult.No == MessageBox.Show(
                    "Not all the files are located and valid. Save settings anyway?",
                    "Missing Dependency",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning))
                return;

            if (AreAnyPathsChanged)
            {
                Program.Settings.StartBatch();
                Paths.KDiff3    = UpdatePathSetting(Paths.KDiff3,    txtKDiff3Path.Text,    "Kdiff3Path");
                Paths.Bms       = UpdatePathSetting(Paths.Bms,       txtBmsPath.Text,       "QuickBmsPath");
                Paths.BmsPlugin = UpdatePathSetting(Paths.BmsPlugin, txtBmsPluginPath.Text, "QuickBmsPluginPath");
                Paths.WccLite   = UpdatePathSetting(Paths.WccLite,   txtWccLitePath.Text,   "WccLitePath");
                Program.Settings.EndBatch();
            }

            DialogResult = (!anyMissing
                ? DialogResult.OK
                : DialogResult.Cancel);
        }

        private string UpdatePathSetting(string oldPath, string newPath, string settingName)
        {
            if (oldPath.EqualsIgnoreCase(newPath))
                return oldPath;
            Program.Settings.Set(settingName, newPath);
            return newPath;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #region Selecting Files

        private void btnKDiff3Path_Click(object sender, EventArgs e)
        {
            GetUserFileChoice(txtKDiff3Path, "Executables|*.exe");
        }

        private void btnBmsPath_Click(object sender, EventArgs e)
        {
            GetUserFileChoice(txtBmsPath, "Executables|*.exe");
        }

        private void btnBmsPluginPath_Click(object sender, EventArgs e)
        {
            GetUserFileChoice(txtBmsPluginPath, "QuickBMS Plugins|*.bms");
        }

        private void btnWccLitePath_Click(object sender, EventArgs e)
        {
            GetUserFileChoice(txtWccLitePath, "Executables|*.exe");
        }

        private void GetUserFileChoice(TextBox txt, string filter)
        {
            OpenFileDialog dlgSelectFile = new OpenFileDialog();
            dlgSelectFile.Filter = filter;
            if (!string.IsNullOrWhiteSpace(txt.Text) && File.Exists(txt.Text))
                dlgSelectFile.FileName = txt.Text;
            if (DialogResult.OK == dlgSelectFile.ShowDialog())
                txt.Text = dlgSelectFile.FileName.Replace(Environment.CurrentDirectory + "\\", "");
        }

        #endregion

        #region Clicking Links

        private void lnkKDiff3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://kdiff3.sourceforge.net/");
        }

        private void lnkBms_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://aluigi.altervista.org/quickbms.htm");
        }

        private void lnkWccLite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://www.nexusmods.com/witcher3/news/12550/?");
        }

        #endregion
    }
}