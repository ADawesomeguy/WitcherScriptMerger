﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Inventory;

namespace WitcherScriptMerger.Forms
{
    public partial class MainForm : Form
    {
        #region Members

        public const string InventoryPath = "MergeInventory.xml";
        public static string ModScriptBase = Path.Combine("content", "scripts");
        public static string VanillaScriptBase = Path.Combine("content", "content0", "scripts");

        private MergeInventory _inventory = null;
        private TreeView _clickedTree = null;
        private TreeNode _clickedNode = null;
        private int _mergesToDo = 0;

        public string GameDirectory
        {
            get { return txtGameDir.Text; }
        }
        
        private string _scriptsDirSetting = Program.Settings.Get("ScriptsDirectory");
        public string ScriptsDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_scriptsDirSetting))
                    return _scriptsDirSetting;
                return Path.Combine(txtGameDir.Text, VanillaScriptBase);
            }
        }

        private string _modsDirSetting = Program.Settings.Get("ModsDirectory");
        public string ModsDirectory
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_modsDirSetting))
                    return _modsDirSetting;
                return Path.Combine(txtGameDir.Text, "Mods");
            }
        }

        #endregion

        #region Form Operations

        public MainForm()
        {
            InitializeComponent();
            this.Text += " v" + Application.ProductVersion;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Program.Settings.StartBatch();
            txtGameDir.Text = Program.Settings.Get("GameDirectory");
            chkReviewEachMerge.Checked = Program.Settings.Get<bool>("ReviewEachMerge");

            LoadLastWindowConfiguration();
            Program.Settings.EndBatch();
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            this.Update();

            if ((!string.IsNullOrWhiteSpace(txtGameDir.Text) || !string.IsNullOrWhiteSpace(_modsDirSetting)))
                RefreshMergeInventory();

            if (!string.IsNullOrWhiteSpace(txtGameDir.Text) ||
                (!string.IsNullOrWhiteSpace(_scriptsDirSetting) && !string.IsNullOrWhiteSpace(_modsDirSetting)))
                RefreshConflicts();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Settings.StartBatch();
            Program.Settings.Set("GameDirectory", txtGameDir.Text);
            Program.Settings.Set("ReviewEachMerge", chkReviewEachMerge.Checked);

            if (WindowState == FormWindowState.Maximized)
                Program.Settings.Set("StartMaximized", true);
            else
            {
                Program.Settings.Set("StartMaximized", false);
                Program.Settings.Set("StartWidth", Width);
                Program.Settings.Set("StartHeight", Height);
                Program.Settings.Set("StartPosTop", Top);
                Program.Settings.Set("StartPosLeft", Left);
            }
            Program.Settings.Set("StartSplitterPosPct", (int)((float)splitContainer.SplitterDistance / splitContainer.Width * 100f));
            Program.Settings.EndBatch();
        }

        private void LoadLastWindowConfiguration()
        {
            int top = Program.Settings.Get<int>("StartPosTop");
            int left = Program.Settings.Get<int>("StartPosLeft");
            if (top > 0)
                Top = top;
            if (left > 0)
                Left = left;
            if (Top > 0 || Left > 0)
                StartPosition = FormStartPosition.Manual;
            
            int startWidth = Program.Settings.Get<int>("StartWidth");
            int startHeight = Program.Settings.Get<int>("StartHeight");
            if (startWidth > 0)
                Width = startWidth;
            if (startHeight > 0)
                Height = startHeight;

            if (Program.Settings.Get<bool>("StartMaximized"))
                WindowState = FormWindowState.Maximized;

            int splitterPosPct = Program.Settings.Get<int>("StartSplitterPosPct");
            if (splitterPosPct > 0)
                splitContainer.SplitterDistance = (int)(splitterPosPct / 100f * splitContainer.Width);
        }

        private void RefreshMergeInventory()
        {
            btnDeleteMerges.Enabled = false;
            treMerges.Nodes.Clear();
            bool changed = false;
            _inventory = MergeInventory.Load(InventoryPath);
            for (int i = _inventory.MergedScripts.Count - 1; i >= 0; --i)
            {
                var script = _inventory.MergedScripts[i];
                var mergedFile = GetModFile(ModsDirectory, script.MergedModName, script.RelativePath);
                if (!mergedFile.Exists &&
                    ConfirmPruneMissingMergeFile(Path.GetFileName(script.RelativePath)))
                {
                    _inventory.MergedScripts.RemoveAt(i);
                    changed = true;
                    continue;
                }
                var scriptNode = new TreeNode(script.RelativePath);
                scriptNode.Tag = mergedFile;

                scriptNode.ForeColor = Color.Blue;
                treMerges.Nodes.Add(scriptNode);

                foreach (var modName in script.IncludedMods)
                {
                    var modFile = GetModFile(ModsDirectory, modName, script.RelativePath);
                    var modNode = new TreeNode(modName);
                    modNode.Tag = modFile;
                    scriptNode.Nodes.Add(modNode);
                }
            }
            if (changed)
                _inventory.Save(InventoryPath);
            treMerges.Sort();
            treMerges.ExpandAll();
            var modNodes = treMerges.GetTreeNodes().SelectMany(node => node.GetTreeNodes()).ToList();
            foreach (var modNode in modNodes)
                modNode.HideCheckBox();
        }

        private bool ConfirmPruneMissingMergeFile(string mergedFileName)
        {
            return (DialogResult.Yes == MessageBox.Show(
                string.Format("Can't find the merged version of {0}.\n\nRemove from Merged Scripts list?", mergedFileName),
                "Missing Merge Inventory File",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question));
        }

        private void RefreshConflicts()
        {
            if (!ValidateDirectories())
                return;

            btnMergeScripts.Enabled = false;
            treConflicts.Nodes.Clear();

            var modIndex = new ModFileIndex()
                .Build(_inventory);
            if (modIndex == null)
                return;

            if (!modIndex.HasConflict)
                MessageBox.Show("No conflicts found.");
            else
            {
                foreach (var conflict in modIndex.Conflicts)
                {
                    var fileNode = new TreeNode(conflict.RelativePath);
                    ////fileNode.Tag = new FileInfo(vanillaPath);
                    fileNode.ForeColor = Color.Red;
                    foreach (string modName in conflict.ModNames)
                    {
                        var modNode = new TreeNode(modName);
                        fileNode.Nodes.Add(modNode);
                    }
                    treConflicts.Nodes.Add(fileNode);
                }
                treConflicts.Sort();
                treConflicts.ExpandAll();
                treConflicts.Select();
            }
        }

        private bool ValidateDirectories()
        {
            if (!Directory.Exists(ModsDirectory))
            {
                MessageBox.Show(!string.IsNullOrWhiteSpace(_modsDirSetting)
                    ? "Can't find the Mods directory specified in the config file."
                    : "Can't find Mods directory in the specified game directory.");
                return false;
            }
            if (!Directory.Exists(ScriptsDirectory))
            {
                MessageBox.Show(!string.IsNullOrWhiteSpace(_scriptsDirSetting)
                    ? "Can't find the Scripts directory specified in the config file."
                    : "Can't find \\content\\content0\\scripts directory in the specified game directory.\n\n" +
                      "It was added in patch 1.08.1 and should contain the game's vanilla scripts. If you don't have it, try this workaround:\n\n" +
                      "1) Download the official mod tools from Nexus Mods and install them.\n" +
                      "2) In the mod tools folder, open the 'r4data' folder.\n" +
                      "3) Copy the 'scripts' folder to " + Path.Combine(GameDirectory, "content\\content0") + ".\n" +
                      "4) Optional: Uninstall the mod tools.");
                return false;
            }
            return true;
        }

        private FileInfo GetModFile(string root, string modName, string relPath)
        {
            return new FileInfo(Path.Combine(root, modName, ModScriptBase, relPath));
        }

        private void txtGameDir_TextChanged(object sender, EventArgs e)
        {
            Program.Settings.Set("GameDirectory", txtGameDir.Text);
        }

        #endregion

        #region Button Clicks

        private void btnSelectGameDirectory_Click(object sender, EventArgs e)
        {
            string dirChoice = GetUserDirectoryChoice();
            if (!string.IsNullOrWhiteSpace(dirChoice))
            {
                txtGameDir.Text = dirChoice;
                RefreshTrees();
            }
        }

        private string GetUserDirectoryChoice()
        {
            FolderBrowserDialog dlgSelectRoot = new FolderBrowserDialog();
            if (Directory.Exists(txtGameDir.Text))
                dlgSelectRoot.SelectedPath = txtGameDir.Text;
            if (DialogResult.OK == dlgSelectRoot.ShowDialog())
                return dlgSelectRoot.SelectedPath;
            else
                return null;
        }

        private void btnRefreshMerged_Click(object sender, EventArgs e)
        {
            RefreshMergeInventory();
        }

        private void btnRefreshConflicts_Click(object sender, EventArgs e)
        {
            RefreshTrees();
        }

        private void btnMergeScripts_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(txtGameDir.Text))
            {
                MessageBox.Show("Can't find the specified Witcher 3 directory.");
                return;
            }

            string mergedModName = Program.Settings.Get("MergedModName");
            if (string.IsNullOrWhiteSpace(mergedModName))
            {
                MessageBox.Show("The MergedModName setting isn't configured in the .config file.");
                return;
            }
            if (mergedModName.Length > 64)
                mergedModName = mergedModName.Substring(0, 64);
            if (!mergedModName.IsAlphaNumeric() || !mergedModName.StartsWith("mod"))
            {
                if (!ConfirmInvalidModName(mergedModName))
                    return;
            }

            _inventory = MergeInventory.Load(InventoryPath);
            bool updatedInventory = false;
            
            var scriptNodes = treConflicts.GetTreeNodes().Where(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1);
            foreach (var scriptNode in scriptNodes)
            {
                var modNodes = scriptNode.GetTreeNodes().Where(modNode => modNode.Checked).ToList();

                if (modNodes.Any(node => mergedModName.CompareTo(node.Text) > 0) &&
                    !ConfirmRemainingConflict(mergedModName))
                    continue;

                var file1 = modNodes[0].Tag as FileInfo;

                string outputPath = Path.Combine(
                        ModsDirectory,
                        mergedModName,
                        ModHelpers.GetRelativePath(file1.FullName, false, false));
                
                if (File.Exists(outputPath) && !ConfirmOutputOverwrite(outputPath))
                    continue;

                var vanillaFile = scriptNode.Tag as FileInfo;
                _mergesToDo = modNodes.Count - 1;

                bool isNewScript = false;
                var mergedScript = _inventory.MergedScripts.FirstOrDefault(ms => ms.RelativePath == scriptNode.Text);
                if (mergedScript == null)
                {
                    isNewScript = true;
                    mergedScript = new MergedScript
                    {
                        RelativePath = scriptNode.Text,
                        MergedModName = mergedModName,
                        IncludedMods = new List<string>()
                    };
                }

                for (int i = 1; i < modNodes.Count; ++i)
                {
                    var file2 = modNodes[i].Tag as FileInfo;
                    var mergedFile = MergePair(i, vanillaFile, file1, file2, outputPath, mergedScript);
                    if (mergedFile != null)
                    {
                        updatedInventory = true;
                        file1 = mergedFile;
                    }
                    else
                    {
                        string msg = string.Format("Merge was canceled for {0}.", vanillaFile.Name);
                        var buttons = MessageBoxButtons.OK;
                        if (_mergesToDo > 1 || scriptNodes.Count() > 1)
                        {
                            if (_mergesToDo > 1)
                            {
                                msg = string.Format("Merge {0} of {1} was canceled for {2}.", i, _mergesToDo, vanillaFile.Name);
                                if (i < _mergesToDo)
                                {
                                    msg += "\n\nContinue with the remaining merges for this file?";
                                    buttons = MessageBoxButtons.YesNo;
                                }
                            }
                        }
                        this.Activate(); // Focus window
                        var result = MessageBox.Show(msg, "Skipped Merge", buttons, MessageBoxIcon.Information);
                        if (result == DialogResult.No)
                        {
                            if (isNewScript && mergedScript.IncludedMods.Count > 1)
                                _inventory.MergedScripts.Add(mergedScript);
                            if (updatedInventory)
                                _inventory.Save(InventoryPath);
                            RefreshTrees();
                            return;
                        }
                    }
                }
                if (isNewScript && mergedScript.IncludedMods.Count > 1)
                    _inventory.MergedScripts.Add(mergedScript);
            }
            if (updatedInventory)
                _inventory.Save(InventoryPath);
            RefreshTrees();
        }

        private bool ConfirmRemainingConflict(string mergedModName)
        {
            return (DialogResult.Yes == MessageBox.Show(
                "There will still be a conflict if you use the merged mod name " + mergedModName + ".\n\n" +
                    "The Witcher 3 loads mods in alphabetical order, so this merged mod name will load after one of the original mods and the merged file will be ignored.\n\n" +
                    "Use this name anyway?",
                "Merged Mod Name Conflict",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation));
        }

        private bool ConfirmOutputOverwrite(string outputPath)
        {
            return (DialogResult.Yes == MessageBox.Show(
                "The output file below already exists! Overwrite?\n\n" + outputPath,
                "Overwrite?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation));
        }

        private bool ConfirmInvalidModName(string mergedModName)
        {
            return (DialogResult.Yes == MessageBox.Show(
                "The Witcher 3 won't load the merged script if the mod name isn't \"mod\" followed by numbers, letters, or underscores.\n\nUse this name anyway?\n" + mergedModName
                + "\n\nTo change the name: Click No, then edit \"MergedModName\" in the .config file.",
                "Warning",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation));
        }

        private FileInfo MergePair(
            int mergeNum,
            FileInfo vanillaFile, FileInfo file1, FileInfo file2,
            string outputPath,
            MergedScript mergedScript)
        {
            string outputDir = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            string modName1 = ModHelpers.GetModName(file1);
            string modName2 = ModHelpers.GetModName(file2);

            string args = string.Format(
                "\"{0}\" \"{1}\" \"{2}\" -o \"{3}\" " +
                "--cs \"WhiteSpace3FileMergeDefault=2\"",
                vanillaFile.FullName, file1.FullName, file2.FullName, outputPath);

            if (!chkReviewEachMerge.Checked)
                args += " --auto";
            
            string kdiff3Path = (Path.IsPathRooted(Program.Kdiff3Path)
                ? Program.Kdiff3Path
                : Path.Combine(Environment.CurrentDirectory, Program.Kdiff3Path));
            
            var kdiff3Proc = Process.Start(kdiff3Path, args);
            kdiff3Proc.WaitForExit();

            if (kdiff3Proc.ExitCode == 0)
            {
                if (file1.FullName != outputPath)
                    mergedScript.IncludedMods.Add(ModHelpers.GetModName(file1));

                if (file2.FullName != outputPath)
                    mergedScript.IncludedMods.Add(ModHelpers.GetModName(file2));

                if (Program.Settings.Get<bool>("ReportAfterMerge"))
                {
                    using (var reportForm = new ReportForm(
                        mergeNum, _mergesToDo,
                        file1.FullName, file2.FullName, outputPath,
                        modName1, modName2))
                    {
                        reportForm.ShowDialog();
                    }
                }
                return new FileInfo(outputPath);
            }
            else
                return null;
        }

        private void btnDeleteMerges_Click(object sender, EventArgs e)
        {
            var scriptNodes = treMerges.GetTreeNodes().Where(node => node.Checked);
            DeleteMerges(scriptNodes);
        }

        private void DeleteMerges(IEnumerable<TreeNode> scriptNodes)
        {
            _inventory = MergeInventory.Load(InventoryPath);
            foreach (var scriptNode in scriptNodes)
            {
                var mergedFile = scriptNode.Tag as FileInfo;

                if (mergedFile.Exists)
                {
                    mergedFile.Delete();
                    DeleteEmptyDirs(Path.GetDirectoryName(mergedFile.FullName), ScriptsDirectory);
                }

                var mergedScript = _inventory.MergedScripts.FirstOrDefault(ms =>
                    mergedFile.FullName.Contains(ms.MergedModName) &&
                    mergedFile.FullName.Contains(ms.RelativePath));

                if (mergedScript != null)
                    _inventory.MergedScripts.Remove(mergedScript);
            }
            _inventory.Save(InventoryPath);
            RefreshTrees();
        }

        private void RefreshTrees()
        {
            RefreshMergeInventory();
            RefreshConflicts();
        }

        #endregion

        #region TreeView

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            (sender as TreeView).SelectedNode = null;
        }

        private void tree_MouseDown(object sender, MouseEventArgs e)
        {
            var tree = sender as TreeView;
            tree.EndUpdate();
            tree.BeginUpdate();
            _clickedNode = tree.GetNodeAt(e.Location);
            if (_clickedNode != null && !_clickedNode.Bounds.Contains(e.Location))
                _clickedNode = null;
        }

        private void tree_MouseUp(object sender, MouseEventArgs e)
        {
            var tree = sender as TreeView;
            _clickedNode = tree.GetNodeAt(e.Location);
            if (_clickedNode != null && !_clickedNode.Bounds.Contains(e.Location))
                _clickedNode = null;

            if (e.Button == MouseButtons.Left)
            {
                if (_clickedNode != null)
                {
                    if (tree == treMerges && _clickedNode.IsLeaf())
                        _clickedNode = _clickedNode.Parent;
                    
                    _clickedNode.Checked = !_clickedNode.Checked;
                    HandleCheckedChange(sender);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                SetContextMenuItems(tree);

                if (_clickedNode != null)
                    _clickedNode.BackColor = Color.Gainsboro;

                if (treeContextMenu.Items.OfType<ToolStripMenuItem>().Any(item => item.Available))
                {
                    _clickedTree = tree;
                    treeContextMenu.Show(tree, e.X, e.Y);
                }
            }
            tree.EndUpdate();
        }

        private void SetContextMenuItems(TreeView tree)
        {
            foreach (var menuItem in treeContextMenu.Items.OfType<ToolStripItem>())
                menuItem.Available = false;
            
            if (_clickedNode != null)
            {
                contextCopyPath.Available = true;
                if (_clickedNode.IsLeaf())
                    contextOpenModScript.Available = contextOpenModDir.Available = true;
                if (!_clickedNode.IsLeaf() && tree == treConflicts)
                    contextOpenVanillaScript.Available = contextOpenVanillaDir.Available = true;
                if (!_clickedNode.IsLeaf() && tree == treMerges)
                    contextOpenMergedScript.Available = contextOpenMergedDir.Available = true;

                if (tree == treMerges)
                {
                    contextDeleteMerge.Available = contextDeleteSeparator.Available = true;
                    if (_clickedNode.IsLeaf())
                    {
                        contextDeleteAssociatedMerges.Available = true;
                        contextDeleteAssociatedMerges.Text = string.Format("Delete All {0} Merges", _clickedNode.Text);
                    }
                }
            }

            // If can copy path, need separator above Select/Deselect All
            if (contextCopyPath.Available)
                contextOpenSeparator.Visible = true;

            if (!tree.IsEmpty())
            {
                if (tree.GetTreeNodes().Any(node => !node.Checked))     contextSelectAll.Available = true;
                if (tree.GetTreeNodes().Any(node =>  node.Checked))     contextDeselectAll.Available = true;
                if (tree.GetTreeNodes().Any(node => !node.IsExpanded))  contextExpandAll.Available = true;
                if (tree.GetTreeNodes().Any(node =>  node.IsExpanded))  contextCollapseAll.Available = true;
            }

            if (treeContextMenu.Items.OfType<ToolStripItem>().Any(item => item.Available))
            {
                int width = treeContextMenu.Items.OfType<ToolStripMenuItem>().Where(item => item.Available)
                    .Max(item => TextRenderer.MeasureText(item.Text, item.Font).Width);
                int height = treeContextMenu.GetAvailableItems()
                    .Sum(item => item.Height);
                treeContextMenu.Width = width + 45;
                treeContextMenu.Height = height + 5;
            }
        }

        private void tree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.Unknown)  // Event was triggered programmatically
                return;

            _clickedNode = e.Node;
            HandleCheckedChange(sender);
        }

        private void HandleCheckedChange(object sender)
        {
            TreeView tree = sender as TreeView;
            TreeNode scriptNode;
            if (!_clickedNode.IsLeaf())  // Script node
            {
                scriptNode = _clickedNode;
                if (tree == treConflicts)
                {
                    foreach (var modNode in _clickedNode.GetTreeNodes())
                        modNode.Checked = _clickedNode.Checked;
                }
            }
            else  // Mod node
            {
                scriptNode = _clickedNode.Parent;
                scriptNode.Checked = scriptNode.GetTreeNodes().All(node => node.Checked);
            }
            if (sender as TreeView == treConflicts)
                EnableMergeIfValidSelection();
            else
                EnableUnmergeIfValidSelection();
        }

        private void ClearCheckedNodes(TreeNode subTreeToSkip = null)
        {
            foreach (var scriptNode in treConflicts.GetTreeNodes())
            {
                if (subTreeToSkip != null && scriptNode.Text == subTreeToSkip.Text)
                    continue;
                scriptNode.Checked = false;
                foreach (var modNode in scriptNode.GetTreeNodes())
                    modNode.Checked = false;
            }
        }

        private void EnableMergeIfValidSelection()
        {
            int validScriptNodes = treConflicts.GetTreeNodes().Count(node => node.GetTreeNodes().Count(modNode => modNode.Checked) > 1);
            btnMergeScripts.Enabled = (validScriptNodes > 0);
            btnMergeScripts.Text = (validScriptNodes > 1
                ? "&Merge Selected Files"
                : "&Merge Selected File");
        }

        private void EnableUnmergeIfValidSelection()
        {
            int selectedNodes = treMerges.GetTreeNodes().Count(node => node.Checked);
            btnDeleteMerges.Enabled = (selectedNodes > 0);
            btnDeleteMerges.Text = (selectedNodes > 1
                ? "&Delete Selected Merges"
                : "&Delete Selected Merge");
        }

        #endregion

        #region TreeView Context Menu

        private void contextOpenScript_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null)
                return;
            var file = _clickedNode.Tag as FileInfo;
            if (!file.Exists)
                MessageBox.Show("Can't find file: " + file.FullName);
            else
                Process.Start(file.FullName);
        }

        private void contextOpenDirectory_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null)
                return;
            var path = GetPathFromNode(_clickedNode, false);
            if (!Directory.Exists(path))
                MessageBox.Show("Can't find directory: " + path);
            else
                Process.Start(path);
        }

        private void contextCopyPath_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null)
                return;
            Clipboard.SetText(GetPathFromNode(_clickedNode));
        }

        private void contextDeleteMerge_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null || _clickedTree != treMerges)
                return;

            if (_clickedNode.IsLeaf())
                DeleteMerges(new TreeNode[] { _clickedNode.Parent });
            else
                DeleteMerges(new TreeNode[] { _clickedNode });
        }

        private void contextDeleteAssociatedMerges_Click(object sender, EventArgs e)
        {
            if (_clickedNode == null || _clickedTree != treMerges || !_clickedNode.IsLeaf())
                return;

            // Find all script nodes that contain a node matching the clicked node
            var scriptNodes = treMerges.GetTreeNodes().Where(scrNode =>
                scrNode.GetTreeNodes().Any(modNode =>
                    modNode.Text == _clickedNode.Text));

            DeleteMerges(scriptNodes);
        }

        private void contextSelectAll_Click(object sender, EventArgs e)
        {
            foreach (var scriptNode in _clickedTree.Nodes.Cast<TreeNode>())
            {
                scriptNode.Checked = true;
                if (_clickedTree == treConflicts)
                {
                    foreach (var modNode in scriptNode.Nodes.Cast<TreeNode>())
                        modNode.Checked = true;
                }
            }
            if (_clickedTree == treConflicts)
                EnableMergeIfValidSelection();
            else
                EnableUnmergeIfValidSelection();
        }

        private void contextDeselectAll_Click(object sender, EventArgs e)
        {
            foreach (var scriptNode in _clickedTree.Nodes.Cast<TreeNode>())
            {
                scriptNode.Checked = false;
                if (_clickedTree == treConflicts)
                {
                    foreach (var modNode in scriptNode.Nodes.Cast<TreeNode>())
                        modNode.Checked = false;
                }
            }

            if (_clickedTree == treConflicts)
                EnableMergeIfValidSelection();
            else
                EnableUnmergeIfValidSelection();
        }

        private void contextExpandAll_Click(object sender, EventArgs e)
        {
            _clickedTree.ExpandAll();
        }

        private void contextCollapseAll_Click(object sender, EventArgs e)
        {
            _clickedTree.CollapseAll();
        }

        private string GetPathFromNode(TreeNode node, bool includeFile = true)
        {
            if (node.Tag is FileInfo)
            {
                if (includeFile)
                    return (node.Tag as FileInfo).FullName;
                else
                    return Path.GetDirectoryName((node.Tag as FileInfo).FullName);
            }
            else
                return (node.Tag as DirectoryInfo).FullName;
        }

        private void treeContextMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (_clickedNode != null)
            {
                _clickedNode.BackColor = Color.Transparent;
                _clickedNode.TreeView.Update();
            }
        }

        #endregion

        #region Key Input

        private void txt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                (sender as TextBox).SelectAll();
        }

        private void treConflicts_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                contextSelectAll_Click(null, null);
        }

        private void treMergeInventory_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && btnDeleteMerges.Enabled)
                btnDeleteMerges_Click(null, null);
        }

        private void splitContainer_Panel1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && btnMergeScripts.Enabled)
                btnMergeScripts_Click(null, null);
        }

        #endregion

        #region File/Dir Operations

        private void DeleteEmptyDirs(string dirPath, string stopPath)
        {
            if (dirPath == stopPath)
                return;
            var dirInfo = new DirectoryInfo(dirPath);
            if (dirInfo.GetFiles().Length > 0 || dirInfo.GetDirectories().Length > 0)
                return;
            Directory.Delete(dirPath);
            DeleteEmptyDirs(dirInfo.Parent.FullName, stopPath);
        }

        #endregion
    }
}