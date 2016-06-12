﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WitcherScriptMerger.LoadOrder
{
    class CustomLoadOrder
    {
        public const int MinPriority = 1;
        public readonly string FilePath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "The Witcher 3",
                "mods.settings");

        public List<ModLoadSetting> Mods = new List<ModLoadSetting>();
        
        public CustomLoadOrder()
        {
            if (!File.Exists(FilePath))
                return;

            ModLoadSetting currModSetting = null;

            var lines =
                File.ReadAllLines(FilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim());

            if (!lines.Any())
                return;

            foreach (string line in lines)
            {
                if (line.StartsWith("["))
                {
                    if (currModSetting != null)
                    {
                        Mods.Add(currModSetting);
                    }
                    string modName = line.Substring(1, line.Length - 2);  // Trim brackets
                    currModSetting = new ModLoadSetting(modName);
                }
                else if (line.StartsWith("Enabled"))
                {
                    currModSetting.IsEnabled = line.EndsWith("1");
                }
                else if (line.StartsWith("Priority"))
                {
                    string priorityString = line.Substring(line.IndexOf('=') + 1);
                    currModSetting.Priority = int.Parse(priorityString);
                }
            }
            if (currModSetting != null)
                Mods.Add(currModSetting); // Final item

            Mods.OrderBy(m => m.Priority)
                .ThenBy(m => m.ModName);
        }

        public void Save()
        {
            var builder = new StringBuilder();

            foreach (var modSetting in Mods)
            {
                builder
                    .Append("[").Append(modSetting.ModName).AppendLine("]")
                    .Append("Enabled=").AppendLine(Convert.ToInt32(modSetting.IsEnabled).ToString())
                    .Append("Priority=").AppendLine(modSetting.Priority.ToString())
                    .AppendLine();
            }

            File.WriteAllText(FilePath, builder.ToString());
        }

        public void AddMergedModIfMissing()
        {
            var mergedModName = Paths.RetrieveMergedModName();

            if (!Mods.Any(setting => setting.ModName.EqualsIgnoreCase(mergedModName)))
            {
                Mods.Insert(0,
                    new ModLoadSetting
                    {
                        ModName = mergedModName,
                        IsEnabled = true,
                        Priority = MinPriority
                    });
            }
        }

        public ModLoadSetting GetTopPriorityEnabledMod()
        {
            return Mods
                .OrderBy(setting => setting, new LoadOrderComparer())
                .FirstOrDefault();
        }

        public string GetTopPriorityEnabledMod(IEnumerable<string> conflictMods)
        {
            var conflictModSettings = Mods.Where(setting => conflictMods.Any(modName => modName.EqualsIgnoreCase(setting.ModName)));
            var enabledModSettings = conflictModSettings.Where(setting => setting.IsEnabled);

            if (!conflictModSettings.Any())
                return conflictMods
                    .OrderBy(name => name, new LoadOrderComparer())
                    .FirstOrDefault();

            if (!enabledModSettings.Any())
                return conflictMods
                    .Except(conflictModSettings.Select(setting => setting.ModName))
                    .OrderBy(name => name, new LoadOrderComparer())
                    .FirstOrDefault();

            return enabledModSettings
                .OrderBy(setting => setting, new LoadOrderComparer())
                .ThenBy(setting => setting.ModName, new LoadOrderComparer())
                .FirstOrDefault()
                ?.ModName;
        }

        public ModLoadSetting GetModLoadSettingByName(string modName)
        {
            return Mods.FirstOrDefault(setting => setting.ModName.EqualsIgnoreCase(modName));
        }

        public bool IsModDisabledByName(string modName)
        {
            var mod = GetModLoadSettingByName(modName);

            return (mod != null && !mod.IsEnabled);
        }

        public void ToggleModByName(string modName)
        {
            var mod = GetModLoadSettingByName(modName);

            if (mod != null)
                mod.IsEnabled = !mod.IsEnabled;
            else
            {
                int bottomPriority =
                    Mods.Any()
                    ? Mods.Max(setting => setting.Priority)
                    : MinPriority;

                Mods.Add(new ModLoadSetting
                {
                    ModName = modName,
                    IsEnabled = false,
                    Priority = bottomPriority + 1
                });
            }
        }

        public int GetPriorityByName(string modName)
        {
            var mod = GetModLoadSettingByName(modName);

            return
                mod != null
                ? mod.Priority
                : -1;
        }

        public void SetPriorityByName(string modName, int priority)
        {
            var mod = GetModLoadSettingByName(modName);

            if (mod != null)
            {
                mod.Priority = priority;
            }
            else
            {
                Mods.Add(new ModLoadSetting
                {
                    ModName = modName,
                    IsEnabled = true,
                    Priority = priority
                });
            }
        }
    }
}
