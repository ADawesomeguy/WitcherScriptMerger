﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using WitcherScriptMerger.Inventory;

namespace WitcherScriptMerger.FileIndex
{
    public class ModFile
    {
        #region Members

        [XmlElement]
        public string RelativePath { get; set; }

        [XmlElement("IncludedMod")]
        public List<FileHash> Mods { get; private set; }

        [XmlElement]
        public string BundleName { get; set; }

        [XmlIgnore]
        public ModFileCategory Category
        {
            get
            {
                if (BundleName != null)
                {
                    if (IsTextFile(RelativePath))
                        return Categories.BundleText;
                    else
                        return Categories.BundleNotMergeable;
                }
                else if (IsScript(RelativePath))
                    return Categories.Script;
                else if (IsXml(RelativePath))
                    return Categories.Xml;
                else
                    return Categories.FlatNotMergeable;
            }
        }

        [XmlIgnore]
        public bool IsBundleContent => (BundleName != null);

        [XmlIgnore]
        public bool HasConflict => (Mods.Count > 1);

        #endregion

        public ModFile(string relPath, string bundlePath = null)
        {
            RelativePath = relPath;
            Mods = new List<FileHash>();
            if (bundlePath != null)
                BundleName = Path.GetFileName(bundlePath);
        }

        public ModFile()
        {
            Mods = new List<FileHash>();
        }

        public bool ContainsMod(string modName)
        {
            return Mods.Any(mod => mod.Name.EqualsIgnoreCase(modName));
        }

        public string GetVanillaFile()
        {
            if (Category == Categories.Script)
                return Path.Combine(Paths.ScriptsDirectory, RelativePath);
            else if (Category == Categories.Xml)
                return Path.Combine(Paths.GameDirectory, RelativePath);
            else
                throw new Exception($"Can't get vanilla file for category '{Category.DisplayName}'.");
        }

        public string GetModFile(string modName)
        {
            if (Category == Categories.Script)
                return Path.Combine(Paths.ModsDirectory, modName, Paths.ModScriptBase, RelativePath);
            else if (Category == Categories.Xml)
                return Path.Combine(Paths.ModsDirectory, modName, RelativePath);
            else if (Category.IsBundled)
                return Path.Combine(Paths.ModsDirectory, modName, Paths.BundleBase, BundleName);
            else
                throw new NotImplementedException();
        }

        public static string GetModNameFromPath(string modFilePath)
        {
            if (!modFilePath.StartsWithIgnoreCase(Paths.ModsDirectory))  // Merged bundle content has internal path, not derived from mod folder
                return Paths.MergedBundleContent;

            var nameStart = Paths.ModsDirectory.Length + 1;
            var name = modFilePath.Substring(nameStart);
            return name.Substring(0, name.IndexOf('\\'));
        }

        public static bool IsScript(string path) => path.EndsWithIgnoreCase(".ws");

        public static bool IsXml(string path) => path.EndsWithIgnoreCase(".xml");

        public static bool IsFlatFile(string path) => (IsScript(path) || IsXml(path));

        public static bool IsBundle(string path) => path.EndsWithIgnoreCase(".bundle");

        public static bool IsTextFile(string path) => (path.EndsWithIgnoreCase(".ws") || path.EndsWithIgnoreCase(".xml") || path.EndsWithIgnoreCase(".txt") || path.EndsWithIgnoreCase(".csv"));

        public override string ToString()
        {
            return $"({Mods.Count} mod{Mods.Count.GetPluralS()}) {RelativePath}";
        }
    }
}