﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WitcherScriptMerger
{
    internal static class Extensions
    {
        public static string ReplaceIgnoreCase(this string s, string oldValue, string newValue)
        {
            return Regex.Replace(s, Regex.Escape(oldValue), newValue.Replace("$", "$$"), RegexOptions.IgnoreCase);
        }

        public static bool EqualsIgnoreCase(this string s, string otherString)
        {
            return s.Equals(otherString, StringComparison.InvariantCultureIgnoreCase);
        }

        public static int IndexOfIgnoreCase(this string s, string value, int startIndex = 0)
        {
            return s.IndexOf(value, startIndex, StringComparison.InvariantCultureIgnoreCase);
        }

        public static int LastIndexOfIgnoreCase(this string s, string value, int startIndex = -1)
        {
            if (startIndex == -1)
                startIndex = s.Length - 1;
            return s.LastIndexOf(value, startIndex, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsAlphaNumeric(this string s)
        {
            return new Regex("^[_a-zA-Z0-9]*$").IsMatch(s);
        }

        public static IEnumerable<ToolStripItem> GetAvailableItems(this ContextMenuStrip menu)
        {
            return menu.Items.Cast<ToolStripItem>().Where(item => item.Available);
        }

        public static IEnumerable<TreeNode> GetTreeNodes(this TreeView treeView)
        {
            return treeView.Nodes.Cast<TreeNode>();
        }

        public static IEnumerable<TreeNode> GetTreeNodes(this TreeNode node)
        {
            return node.Nodes.Cast<TreeNode>();
        }

        public static IEnumerable<TreeNode> Get2ndLevelNodes(this TreeView treeView)
        {
            return treeView.GetTreeNodes().SelectMany(node => node.GetTreeNodes());
        }

        public static bool IsEmpty(this TreeView tree)
        {
            return (tree.Nodes.Count == 0);
        }

        public static bool IsLeaf(this TreeNode node)
        {
            return (node.Nodes.Count == 0);
        }

        #region Scrolling TreeView to Top

        private const int WM_VSCROLL = 0x0115;
        private const int SB_THUMBPOSITION = 0x0004;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport("User32.Dll", EntryPoint = "PostMessageA")]
        static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

        public static void ScrollToTop(this TreeView treeView)
        {
            if (SetScrollPos(treeView.Handle, WM_VSCROLL, 0, true) != -1)
                PostMessage(treeView.Handle, WM_VSCROLL, SB_THUMBPOSITION, 0);
        }

        #endregion

        #region Hiding TreeView Checkbox

        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage;
            public int cChildren;
            public IntPtr lParam;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam,
                                                 ref TVITEM lParam);

        /// <summary>
        /// Hides the checkbox for the specified node on a TreeView control.
        /// </summary>
        public static void HideCheckBox(this TreeNode node)
        {
            TVITEM tvi = new TVITEM();
            tvi.hItem = node.Handle;
            tvi.mask = TVIF_STATE;
            tvi.stateMask = TVIS_STATEIMAGEMASK;
            tvi.state = 0;
            SendMessage(node.TreeView.Handle, TVM_SETITEM, IntPtr.Zero, ref tvi);
        }

        #endregion
    }
}
