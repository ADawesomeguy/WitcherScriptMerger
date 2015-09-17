﻿using System.IO;
using System.Xml.Serialization;
using WitcherScriptMerger.FileIndex;

namespace WitcherScriptMerger.Inventory
{
    [XmlRoot]
    public class Merge : ModFile
    {
        [XmlElement]
        public string MergedModName;

        [XmlIgnore]
        public new ModFileType Type
        {
            get
            {
                return (ModFile.IsScriptPath(RelativePath)
                    ? ModFileType.Script
                    : ModFileType.BundleContent);
            }
        }

        public string GetMergedFile()
        {
            if (Type == ModFileType.Script)
                return Path.Combine(Paths.ModsDirectory, MergedModName, Paths.ModScriptBase, RelativePath);
            else
                return Path.Combine(Paths.ModsDirectory, MergedModName, Paths.ModBundleBase, BundleName);
        }
    }
}
