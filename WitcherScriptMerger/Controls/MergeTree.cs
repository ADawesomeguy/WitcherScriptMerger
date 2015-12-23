﻿using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WitcherScriptMerger.FileIndex;

namespace WitcherScriptMerger.Controls
{
    public class MergeTree : SMTree
    {
        #region Context Menu Members

        private ToolStripMenuItem _contextOpenMergedFile = new ToolStripMenuItem();
        private ToolStripMenuItem _contextOpenMergedFileDir = new ToolStripMenuItem();
        private ToolStripMenuItem _contextDeleteAssociatedMerges = new ToolStripMenuItem();
        private ToolStripMenuItem _contextDeleteMerge = new ToolStripMenuItem();
        private ToolStripSeparator _contextDeleteSeparator = new ToolStripSeparator();

        #endregion

        public MergeTree()
        {
            FileNodeForeColor = Color.Blue;

            ContextOpenRegion.Items.AddRange(new ToolStripItem[]
            {
                _contextOpenMergedFile, _contextOpenMergedFileDir,
                _contextDeleteSeparator,
                _contextDeleteMerge,
                _contextDeleteAssociatedMerges,
            });
            BuildContextMenu();
            
            // 
            // contextOpenMergedFile
            // 
            _contextOpenMergedFile.Name = "contextOpenMergedFile";
            _contextOpenMergedFile.Size = new Size(225, 22);
            _contextOpenMergedFile.Text = "Open Merged File";
            _contextOpenMergedFile.Click += ContextOpenScript_Click;
            // 
            // contextOpenMergedFileDir
            // 
            _contextOpenMergedFileDir.Name = "contextOpenMergedFileDir";
            _contextOpenMergedFileDir.Size = new Size(225, 22);
            _contextOpenMergedFileDir.Text = "Open Merged File Directory";
            _contextOpenMergedFileDir.Click += ContextOpenDirectory_Click;
            // 
            // contextDeleteMerge
            // 
            _contextDeleteMerge.Name = "contextDeleteMerge";
            _contextDeleteMerge.Size = new Size(225, 22);
            _contextDeleteMerge.Text = "Delete This Merge";
            _contextDeleteMerge.Click += ContextDeleteMerge_Click;
            // 
            // contextDeleteAssociatedMerges
            // 
            _contextDeleteAssociatedMerges.Name = "contextDeleteAssociatedMerges";
            _contextDeleteAssociatedMerges.Size = new Size(225, 22);
            _contextDeleteAssociatedMerges.Text = "Delete All {0} Merges";
            _contextDeleteAssociatedMerges.Click += ContextDeleteAssociatedMerges_Click;
            // 
            // contextDeleteSeparator
            // 
            _contextDeleteSeparator.Name = "contextDeleteSeparator";
            _contextDeleteSeparator.Size = new Size(235, 6);
        }

        protected override void HandleCheckedChange()
        {
            if (IsCategoryNode(ClickedNode))
            {
                foreach (var fileNode in ClickedNode.GetTreeNodes())
                    fileNode.Checked = ClickedNode.Checked;
            }
            else if (IsFileNode(ClickedNode))
            {
                var catNode = ClickedNode.Parent;
                catNode.Checked = catNode.GetTreeNodes().All(node => node.Checked);
            }
            Program.MainForm.EnableUnmergeIfValidSelection();
        }

        protected override void OnLeftMouseUp(MouseEventArgs e)
        {
            if (ClickedNode != null && IsModNode(ClickedNode))
                ClickedNode = ClickedNode.Parent;

            base.OnLeftMouseUp(e);
        }

        protected override void SetAllChecked(bool isChecked)
        {
            foreach (var catNode in CategoryNodes)
            {
                catNode.Checked = isChecked;
                foreach (var fileNode in catNode.GetTreeNodes())
                    fileNode.Checked = isChecked;
            }
            Program.MainForm.EnableUnmergeIfValidSelection();
        }

        private void ContextDeleteMerge_Click(object sender, EventArgs e)
        {
            if (ClickedNode == null)
                return;

            if (IsModNode(ClickedNode))
                Program.MainForm.DeleteMerges(new TreeNode[] { ClickedNode.Parent });
            else
                Program.MainForm.DeleteMerges(new TreeNode[] { ClickedNode });
        }

        private void ContextDeleteAssociatedMerges_Click(object sender, EventArgs e)
        {
            if (ClickedNode == null || !IsModNode(ClickedNode))
                return;

            // Find all file nodes that contain a node matching the clicked node
            var fileNodes = FileNodes.Where(node =>
                node.GetTreeNodes().Any(modNode =>
                    modNode.Text == ClickedNode.Text));

            Program.MainForm.DeleteMerges(fileNodes);
        }

        protected override void SetContextItemAvailability()
        {
            base.SetContextItemAvailability();

            if (ClickedNode != null)
            {
                if (ClickedNode.Tag != null && IsFileNode(ClickedNode))
                {
                    string filePath = ClickedNode.Tag as string;
                    if (ModFile.IsScript(filePath))
                        _contextOpenMergedFile.Available = _contextOpenMergedFileDir.Available = true;
                    else
                        _contextOpenMergedFile.Available = _contextOpenMergedFileDir.Available = true;
                }

                if (!IsCategoryNode(ClickedNode))
                {
                    _contextDeleteMerge.Available = _contextDeleteSeparator.Available = true;
                    if (IsModNode(ClickedNode))
                    {
                        _contextDeleteAssociatedMerges.Available = true;
                        _contextDeleteAssociatedMerges.Text = string.Format("Delete All {0} Merges", ClickedNode.Text);
                    }
                }
            }

            if (!this.IsEmpty())
            {
                if (CategoryNodes.Any(catNode => !catNode.Checked))
                    ContextSelectAll.Available = true;
                if (FileNodes.Any(fileNode => fileNode.Checked))
                    ContextDeselectAll.Available = true;
            }
        }
    }
}
