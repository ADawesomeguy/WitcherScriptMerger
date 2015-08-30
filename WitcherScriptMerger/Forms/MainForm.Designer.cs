﻿namespace WitcherScriptMerger
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.txtGameDir = new System.Windows.Forms.TextBox();
            this.lblGameDir = new System.Windows.Forms.Label();
            this.btnSelectGameDir = new System.Windows.Forms.Button();
            this.treConflicts = new System.Windows.Forms.TreeView();
            this.treeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.contextVanillaScript = new System.Windows.Forms.ToolStripMenuItem();
            this.contextVanillaDir = new System.Windows.Forms.ToolStripMenuItem();
            this.contextModScript = new System.Windows.Forms.ToolStripMenuItem();
            this.contextModDir = new System.Windows.Forms.ToolStripMenuItem();
            this.contextCopyPath = new System.Windows.Forms.ToolStripMenuItem();
            this.contextExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.contextCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCheckForConflicts = new System.Windows.Forms.Button();
            this.btnTryMergeSelected = new System.Windows.Forms.Button();
            this.lblMergedModName = new System.Windows.Forms.Label();
            this.txtMergedModName = new System.Windows.Forms.TextBox();
            this.chkMoveToBackup = new System.Windows.Forms.CheckBox();
            this.chkCheckAtLaunch = new System.Windows.Forms.CheckBox();
            this.btnSelectBackupDir = new System.Windows.Forms.Button();
            this.lblBackupDir = new System.Windows.Forms.Label();
            this.txtBackupDir = new System.Windows.Forms.TextBox();
            this.chkLineBreakSymbol = new System.Windows.Forms.CheckBox();
            this.treeContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtGameDir
            // 
            this.txtGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtGameDir.Location = new System.Drawing.Point(116, 14);
            this.txtGameDir.Name = "txtGameDir";
            this.txtGameDir.Size = new System.Drawing.Size(259, 20);
            this.txtGameDir.TabIndex = 0;
            this.txtGameDir.TextChanged += new System.EventHandler(this.txtGameDirectory_TextChanged);
            // 
            // lblGameDir
            // 
            this.lblGameDir.AutoSize = true;
            this.lblGameDir.Location = new System.Drawing.Point(9, 17);
            this.lblGameDir.Name = "lblGameDir";
            this.lblGameDir.Size = new System.Drawing.Size(101, 13);
            this.lblGameDir.TabIndex = 1;
            this.lblGameDir.Text = "Witcher 3 Directory:";
            // 
            // btnSelectGameDir
            // 
            this.btnSelectGameDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectGameDir.Location = new System.Drawing.Point(381, 12);
            this.btnSelectGameDir.Name = "btnSelectGameDir";
            this.btnSelectGameDir.Size = new System.Drawing.Size(26, 23);
            this.btnSelectGameDir.TabIndex = 1;
            this.btnSelectGameDir.Text = "...";
            this.btnSelectGameDir.UseVisualStyleBackColor = true;
            this.btnSelectGameDir.Click += new System.EventHandler(this.btnSelectGameDirectory_Click);
            // 
            // treConflicts
            // 
            this.treConflicts.AllowDrop = true;
            this.treConflicts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treConflicts.CheckBoxes = true;
            this.treConflicts.Location = new System.Drawing.Point(12, 90);
            this.treConflicts.Name = "treConflicts";
            this.treConflicts.ShowNodeToolTips = true;
            this.treConflicts.ShowRootLines = false;
            this.treConflicts.Size = new System.Drawing.Size(505, 387);
            this.treConflicts.TabIndex = 4;
            this.treConflicts.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treConflicts_AfterCheck);
            this.treConflicts.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treConflicts_AfterSelect);
            this.treConflicts.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treConflicts_MouseDown);
            this.treConflicts.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treConflicts_MouseUp);
            // 
            // treeContextMenu
            // 
            this.treeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextVanillaScript,
            this.contextVanillaDir,
            this.contextModScript,
            this.contextModDir,
            this.contextCopyPath,
            this.contextExpandAll,
            this.contextCollapseAll});
            this.treeContextMenu.Name = "treeContextMenu";
            this.treeContextMenu.Size = new System.Drawing.Size(226, 158);
            // 
            // contextVanillaScript
            // 
            this.contextVanillaScript.Name = "contextVanillaScript";
            this.contextVanillaScript.Size = new System.Drawing.Size(225, 22);
            this.contextVanillaScript.Text = "Open Vanilla Script";
            this.contextVanillaScript.Click += new System.EventHandler(this.contextOpenScript_Click);
            // 
            // contextVanillaDir
            // 
            this.contextVanillaDir.Name = "contextVanillaDir";
            this.contextVanillaDir.Size = new System.Drawing.Size(225, 22);
            this.contextVanillaDir.Text = "Open Vanilla Script Directory";
            this.contextVanillaDir.Click += new System.EventHandler(this.contextOpenDirectory_Click);
            // 
            // contextModScript
            // 
            this.contextModScript.Name = "contextModScript";
            this.contextModScript.Size = new System.Drawing.Size(225, 22);
            this.contextModScript.Text = "Open Mod Script";
            this.contextModScript.Click += new System.EventHandler(this.contextOpenScript_Click);
            // 
            // contextModDir
            // 
            this.contextModDir.Name = "contextModDir";
            this.contextModDir.Size = new System.Drawing.Size(225, 22);
            this.contextModDir.Text = "Open Mod Script Directory";
            this.contextModDir.Click += new System.EventHandler(this.contextOpenDirectory_Click);
            // 
            // contextCopyPath
            // 
            this.contextCopyPath.Name = "contextCopyPath";
            this.contextCopyPath.Size = new System.Drawing.Size(225, 22);
            this.contextCopyPath.Text = "Copy Path";
            this.contextCopyPath.Click += new System.EventHandler(this.contextCopyPath_Click);
            // 
            // contextExpandAll
            // 
            this.contextExpandAll.Name = "contextExpandAll";
            this.contextExpandAll.Size = new System.Drawing.Size(225, 22);
            this.contextExpandAll.Text = "Expand All";
            this.contextExpandAll.Click += new System.EventHandler(this.contextExpandAll_Click);
            // 
            // contextCollapseAll
            // 
            this.contextCollapseAll.Name = "contextCollapseAll";
            this.contextCollapseAll.Size = new System.Drawing.Size(225, 22);
            this.contextCollapseAll.Text = "Collapse All";
            this.contextCollapseAll.Click += new System.EventHandler(this.contextCollapseAll_Click);
            // 
            // btnCheckForConflicts
            // 
            this.btnCheckForConflicts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckForConflicts.Location = new System.Drawing.Point(12, 41);
            this.btnCheckForConflicts.Name = "btnCheckForConflicts";
            this.btnCheckForConflicts.Size = new System.Drawing.Size(505, 43);
            this.btnCheckForConflicts.TabIndex = 3;
            this.btnCheckForConflicts.Text = "&Check for Script Conflicts";
            this.btnCheckForConflicts.UseVisualStyleBackColor = true;
            this.btnCheckForConflicts.Click += new System.EventHandler(this.btnCheckForConflicts_Click);
            // 
            // btnTryMergeSelected
            // 
            this.btnTryMergeSelected.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTryMergeSelected.Enabled = false;
            this.btnTryMergeSelected.Location = new System.Drawing.Point(351, 483);
            this.btnTryMergeSelected.Name = "btnTryMergeSelected";
            this.btnTryMergeSelected.Size = new System.Drawing.Size(166, 93);
            this.btnTryMergeSelected.TabIndex = 7;
            this.btnTryMergeSelected.Text = "Try to &Merge Selected Scripts";
            this.btnTryMergeSelected.UseVisualStyleBackColor = true;
            this.btnTryMergeSelected.Click += new System.EventHandler(this.btnTryMergeSelected_Click);
            // 
            // lblMergedModName
            // 
            this.lblMergedModName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblMergedModName.AutoSize = true;
            this.lblMergedModName.Location = new System.Drawing.Point(9, 486);
            this.lblMergedModName.Name = "lblMergedModName";
            this.lblMergedModName.Size = new System.Drawing.Size(151, 13);
            this.lblMergedModName.TabIndex = 9;
            this.lblMergedModName.Text = "Mod Name for Merged Scripts:";
            // 
            // txtMergedModName
            // 
            this.txtMergedModName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMergedModName.Location = new System.Drawing.Point(166, 483);
            this.txtMergedModName.Name = "txtMergedModName";
            this.txtMergedModName.Size = new System.Drawing.Size(179, 20);
            this.txtMergedModName.TabIndex = 5;
            this.txtMergedModName.TextChanged += new System.EventHandler(this.txtMergedModName_TextChanged);
            // 
            // chkMoveToBackup
            // 
            this.chkMoveToBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMoveToBackup.AutoSize = true;
            this.chkMoveToBackup.Location = new System.Drawing.Point(12, 537);
            this.chkMoveToBackup.Name = "chkMoveToBackup";
            this.chkMoveToBackup.Size = new System.Drawing.Size(335, 17);
            this.chkMoveToBackup.TabIndex = 6;
            this.chkMoveToBackup.Text = "After successful merge, move obsolete scripts to &backup directory";
            this.chkMoveToBackup.UseVisualStyleBackColor = true;
            this.chkMoveToBackup.CheckedChanged += new System.EventHandler(this.chkMoveToBackup_CheckedChanged);
            // 
            // chkCheckAtLaunch
            // 
            this.chkCheckAtLaunch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCheckAtLaunch.AutoSize = true;
            this.chkCheckAtLaunch.Location = new System.Drawing.Point(413, 16);
            this.chkCheckAtLaunch.Name = "chkCheckAtLaunch";
            this.chkCheckAtLaunch.Size = new System.Drawing.Size(104, 17);
            this.chkCheckAtLaunch.TabIndex = 2;
            this.chkCheckAtLaunch.Text = "Check at &launch";
            this.chkCheckAtLaunch.UseVisualStyleBackColor = true;
            this.chkCheckAtLaunch.CheckedChanged += new System.EventHandler(this.chkCheckAtLaunch_CheckedChanged);
            // 
            // btnSelectBackupDir
            // 
            this.btnSelectBackupDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBackupDir.Location = new System.Drawing.Point(319, 509);
            this.btnSelectBackupDir.Name = "btnSelectBackupDir";
            this.btnSelectBackupDir.Size = new System.Drawing.Size(26, 23);
            this.btnSelectBackupDir.TabIndex = 12;
            this.btnSelectBackupDir.Text = "...";
            this.btnSelectBackupDir.UseVisualStyleBackColor = true;
            this.btnSelectBackupDir.Click += new System.EventHandler(this.btnSelectBackupDir_Click);
            // 
            // lblBackupDir
            // 
            this.lblBackupDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblBackupDir.AutoSize = true;
            this.lblBackupDir.Location = new System.Drawing.Point(9, 514);
            this.lblBackupDir.Name = "lblBackupDir";
            this.lblBackupDir.Size = new System.Drawing.Size(92, 13);
            this.lblBackupDir.TabIndex = 11;
            this.lblBackupDir.Text = "Backup Directory:";
            // 
            // txtBackupDir
            // 
            this.txtBackupDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBackupDir.Location = new System.Drawing.Point(107, 511);
            this.txtBackupDir.Name = "txtBackupDir";
            this.txtBackupDir.Size = new System.Drawing.Size(206, 20);
            this.txtBackupDir.TabIndex = 10;
            this.txtBackupDir.TextChanged += new System.EventHandler(this.txtBackupDir_TextChanged);
            // 
            // chkLineBreakSymbol
            // 
            this.chkLineBreakSymbol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkLineBreakSymbol.AutoSize = true;
            this.chkLineBreakSymbol.Location = new System.Drawing.Point(12, 559);
            this.chkLineBreakSymbol.Name = "chkLineBreakSymbol";
            this.chkLineBreakSymbol.Size = new System.Drawing.Size(283, 17);
            this.chkLineBreakSymbol.TabIndex = 13;
            this.chkLineBreakSymbol.Text = "Resolve Conflict screen: &Show line breaks as ¶ symbol";
            this.chkLineBreakSymbol.UseVisualStyleBackColor = true;
            this.chkLineBreakSymbol.CheckedChanged += new System.EventHandler(this.chkLineBreakSymbol_CheckedChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(529, 588);
            this.Controls.Add(this.chkLineBreakSymbol);
            this.Controls.Add(this.btnSelectBackupDir);
            this.Controls.Add(this.lblBackupDir);
            this.Controls.Add(this.txtBackupDir);
            this.Controls.Add(this.chkCheckAtLaunch);
            this.Controls.Add(this.chkMoveToBackup);
            this.Controls.Add(this.lblMergedModName);
            this.Controls.Add(this.txtMergedModName);
            this.Controls.Add(this.btnTryMergeSelected);
            this.Controls.Add(this.btnCheckForConflicts);
            this.Controls.Add(this.treConflicts);
            this.Controls.Add(this.btnSelectGameDir);
            this.Controls.Add(this.lblGameDir);
            this.Controls.Add(this.txtGameDir);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(545, 280);
            this.Name = "MainForm";
            this.Text = "Script Merger";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.treeContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtGameDir;
        private System.Windows.Forms.Label lblGameDir;
        private System.Windows.Forms.Button btnSelectGameDir;
        private System.Windows.Forms.TreeView treConflicts;
        private System.Windows.Forms.Button btnCheckForConflicts;
        private System.Windows.Forms.ContextMenuStrip treeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem contextModDir;
        private System.Windows.Forms.ToolStripMenuItem contextCopyPath;
        private System.Windows.Forms.ToolStripMenuItem contextExpandAll;
        private System.Windows.Forms.ToolStripMenuItem contextCollapseAll;
        private System.Windows.Forms.ToolStripMenuItem contextModScript;
        private System.Windows.Forms.ToolStripMenuItem contextVanillaScript;
        private System.Windows.Forms.ToolStripMenuItem contextVanillaDir;
        private System.Windows.Forms.Button btnTryMergeSelected;
        private System.Windows.Forms.Label lblMergedModName;
        private System.Windows.Forms.TextBox txtMergedModName;
        private System.Windows.Forms.CheckBox chkMoveToBackup;
        private System.Windows.Forms.CheckBox chkCheckAtLaunch;
        private System.Windows.Forms.Button btnSelectBackupDir;
        private System.Windows.Forms.Label lblBackupDir;
        private System.Windows.Forms.TextBox txtBackupDir;
        private System.Windows.Forms.CheckBox chkLineBreakSymbol;
    }
}

