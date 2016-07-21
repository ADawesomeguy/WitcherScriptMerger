﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WitcherScriptMerger.FileIndex;
using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.LoadOrder;

namespace WitcherScriptMerger
{
    public class FileMerger
    {
        #region Types

        public struct MergeSource
        {
            public FileInfo TextFile;
            public string Name;
            public FileInfo Bundle;

            public static MergeSource FromFlatFile(FileInfo file)
                => Create(file, false);

            public static MergeSource FromBundle(FileInfo file)
                => Create(file, true);

            static MergeSource Create(FileInfo file, bool isBundle)
                => new MergeSource
                {
                    TextFile = isBundle ? null : file,
                    Bundle   = isBundle ? file : null,
                    Name     = ModFile.GetModNameFromPath(file.FullName)
                };
        }

        #endregion

        #region Members

        public MergeProgressInfo ProgressInfo { get; private set; }

        MergeInventory _inventory;
        TreeNode[] _checkedFileNodes;
        FileInfo _vanillaFile;
        string _mergedModName;
        string _outputPath;
        
        bool _bundleChanged;
        List<Merge> _pendingBundleMerges = new List<Merge>();

        BackgroundWorker _bgWorker;

        #endregion

        public FileMerger(
            MergeInventory inventory,
            ProgressChangedEventHandler progressHandler,
            RunWorkerCompletedEventHandler completedHandler)
        {
            _inventory = inventory;

            _bgWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            _bgWorker.ProgressChanged += progressHandler;
            ProgressInfo = new MergeProgressInfo();
            ProgressInfo.PropertyChanged += (sender, e) =>
            {
                _bgWorker.ReportProgress(0, ProgressInfo);
            };
            _bgWorker.RunWorkerCompleted += completedHandler;
        }

        ~FileMerger()
        {
            if (_bgWorker != null)
                _bgWorker.Dispose();
        }

        public void MergeByTreeNodesAsync(
            IEnumerable<TreeNode> fileNodesToMerge,
            string mergedModName)
        {
            _bgWorker.DoWork += (sender, e) =>
            {
                _checkedFileNodes = fileNodesToMerge.ToArray();
                _mergedModName = mergedModName;

                var checkedModNodesForFile =
                    _checkedFileNodes.Select(
                        fileNode =>
                        fileNode.GetTreeNodes().Where(
                            modNode =>
                            modNode.Checked
                        ).ToArray()
                    ).ToArray();

                ProgressInfo.TotalMergeCount = checkedModNodesForFile.Sum(modNodes => modNodes.Length - 1);
                ProgressInfo.TotalFileCount = _checkedFileNodes.Length;

                for (int i = 0; i < _checkedFileNodes.Length; ++i)
                {
                    var fileNode = _checkedFileNodes[i];

                    ProgressInfo.CurrentFileName = Path.GetFileName(fileNode.Text);
                    ProgressInfo.CurrentFileNum = i + 1;

                    var checkedModNodes = checkedModNodesForFile[i];

                    ProgressInfo.CurrentAction = "Starting merge";

                    if (checkedModNodes.Any(node => (new LoadOrderComparer()).Compare(node.Text, _mergedModName) < 0) &&
                        !ConfirmRemainingConflict(_mergedModName))
                        continue;

                    var isNew = false;
                    var merge = _inventory.Merges.FirstOrDefault(m => m.RelativePath.EqualsIgnoreCase(fileNode.Text));
                    if (merge == null)
                    {
                        isNew = true;
                        merge = new Merge
                        {
                            RelativePath = fileNode.Text,
                            MergedModName = _mergedModName
                        };
                    }

                    if ((ModFileCategory)fileNode.Parent.Tag == Categories.BundleText)
                    {
                        merge.BundleName = Path.GetFileName(Paths.RetrieveMergedBundlePath());
                        MergeBundleFileNode(fileNode, checkedModNodes, merge, isNew);
                    }
                    else
                        MergeFlatFileNode(fileNode, checkedModNodes, merge, isNew);
                }
                if (_bundleChanged)
                {
                    var newBundlePath = PackNewBundle(Paths.RetrieveMergedBundlePath());
                    if (newBundlePath != null)
                    {
                        ProgressInfo.CurrentAction = "Adding bundle merge to inventory";
                        foreach (var bundleMerge in _pendingBundleMerges)
                            _inventory.Merges.Add(bundleMerge);

                        if (Program.MainForm.CompletionSoundsSetting)
                        {
                            System.Media.SystemSounds.Asterisk.Play();
                        }
                        if (Program.MainForm.PackReportSetting)
                        {
                            using (var reportForm = new PackReportForm(newBundlePath))
                            {
                                ProgressInfo.CurrentAction = "Showing pack report";
                                Program.MainForm.ShowModal(reportForm);
                            }
                        }
                    }
                }
                CleanUpTempFiles();
                CleanUpEmptyDirectories();
            };
            _bgWorker.RunWorkerAsync();
        }

        void MergeFlatFileNode(TreeNode fileNode, TreeNode[] checkedModNodes, Merge merge, bool isNew)
        {
            var source1 = MergeSource.FromFlatFile(new FileInfo(checkedModNodes[0].Tag as string));
            
            var relPath = Paths.GetRelativePath(
                source1.TextFile.FullName,
                Path.Combine(Paths.ModsDirectory, source1.Name));

            _outputPath = Path.Combine(Paths.ModsDirectory, _mergedModName, relPath);

            if (File.Exists(_outputPath) && !ConfirmOutputOverwrite(_outputPath))
                return;

            _vanillaFile = new FileInfo(fileNode.Tag as string);

            for (int i = 1; i < checkedModNodes.Length; ++i)
            {
                ++ProgressInfo.CurrentMergeNum;

                var source2 = MergeSource.FromFlatFile(new FileInfo(checkedModNodes[i].Tag as string));

                var mergedFile = MergeText(merge, source1, source2);
                if (mergedFile != null)
                {
                    source1 = MergeSource.FromFlatFile(mergedFile);
                }
                else if (DialogResult.Abort == HandleCanceledMerge(checkedModNodes.Length - i - 1, merge))
                    break;
            }

            if (isNew && merge.Mods.Count > 1)
            {
                ProgressInfo.CurrentAction = "Adding script merge to inventory";
                _inventory.Merges.Add(merge);
            }
        }

        void MergeBundleFileNode(TreeNode fileNode, TreeNode[] checkedModNodes, Merge merge, bool isNew)
        {
            _outputPath = Path.Combine(Paths.MergedBundleContent, fileNode.Text);

            if (File.Exists(_outputPath) && !ConfirmOutputOverwrite(_outputPath))
                return;

            _vanillaFile = null;

            var source1 = MergeSource.FromBundle(new FileInfo(checkedModNodes[0].Tag as string));

            for (int i = 1; i < checkedModNodes.Length; ++i)
            {
                ++ProgressInfo.CurrentMergeNum;

                var source2 = MergeSource.FromBundle(new FileInfo(checkedModNodes[i].Tag as string));

                if (!GetUnpackedFiles(fileNode.Text, ref source1, ref source2))
                {
                    if (DialogResult.Abort != HandleCanceledMerge(checkedModNodes.Length - i - 1, merge))
                        continue;
                    break;
                }

                var mergedFile = MergeText(merge, source1, source2);
                if (mergedFile != null)
                {
                    source1 = MergeSource.FromFlatFile(mergedFile);
                }
                else if (DialogResult.Abort == HandleCanceledMerge(checkedModNodes.Length - i - 1, merge))
                    break;
            }

            if (merge.BundleName != null && isNew && merge.Mods.Count > 1)
            {
                _bundleChanged = true;
                _pendingBundleMerges.Add(merge);
            }
        }

        FileInfo MergeText(Merge merge, MergeSource source1, MergeSource source2)
        {
            ProgressInfo.CurrentAction = $"Merging {source1.Name} && {source2.Name} — waiting for KDiff3 to close";

            var outputDir = Path.GetDirectoryName(_outputPath);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var hasVanillaVersion = (_vanillaFile != null && _vanillaFile.Exists);

            var args = (hasVanillaVersion
                ? "\"" + _vanillaFile.FullName + "\" "
                : "");

            args +=
                $"\"{source1.TextFile.FullName}\" \"{source2.TextFile.FullName}\" " +
                $"-o \"{_outputPath}\" " +
                "--cs \"WhiteSpace3FileMergeDefault=2\" " +
                "--cs \"CreateBakFiles=0\" " +
                "--cs \"LineEndStyle=1\" " +
                "--cs \"FollowFileLinks=1\" " +
                "--cs \"FollowDirLinks=1\"";

            if (!Program.MainForm.PathsInKdiff3Setting)
            {
                if (hasVanillaVersion)
                    args += $" --L1 Vanilla --L2 \"{source1.Name}\" --L3 \"{source2.Name}\"";
                else
                    args += $" --L1 \"{source1.Name}\" --L2 \"{source2.Name}\"";
            }

            if (!Program.MainForm.ReviewEachMergeSetting && hasVanillaVersion)
                args += " --auto";

            var kdiff3Path = (Path.IsPathRooted(Paths.KDiff3)
                ? Paths.KDiff3
                : Path.Combine(Environment.CurrentDirectory, Paths.KDiff3));

            var kdiff3Proc = Process.Start(kdiff3Path, args);
            kdiff3Proc.WaitForExit();

            if (kdiff3Proc.ExitCode == 0)
            {
                if (!source1.TextFile.FullName.EqualsIgnoreCase(_outputPath)
                    && !source1.TextFile.FullName.StartsWithIgnoreCase(Paths.MergedBundleContentAbsolute))
                {
                    _inventory.AddModToMerge(source1, merge);
                }

                if (!source2.TextFile.FullName.EqualsIgnoreCase(_outputPath)
                    && !source2.TextFile.FullName.StartsWithIgnoreCase(Paths.MergedBundleContentAbsolute))
                {
                    _inventory.AddModToMerge(source2, merge);
                }

                if (Program.MainForm.CompletionSoundsSetting)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
                if (Program.MainForm.MergeReportSetting)
                {
                    using (var reportForm = new MergeReportForm(
                        ProgressInfo.CurrentMergeNum, ProgressInfo.TotalMergeCount,
                        source1.TextFile.FullName, source2.TextFile.FullName, _outputPath,
                        source1.Name, source2.Name))
                    {
                        ProgressInfo.CurrentAction = "Showing merge report";
                        Program.MainForm.ShowModal(reportForm);
                    }
                }
                return new FileInfo(_outputPath);
            }
            else
                return null;
        }

        bool ConfirmRemainingConflict(string mergedModName)
        {
            return (DialogResult.Yes == Program.MainForm.ShowMessage(
                "There will still be a conflict if you use the merged mod name " + mergedModName + ".\n\n" +
                    "The Witcher 3 loads mods in case-insensitive ASCII order, " +
                    "so this merged mod name will load after one of the original mods, " +
                    "and the merged file will be ignored.\n\n" +
                    "Use this name anyway?",
                "Merged Mod Name Conflict",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation));
        }

        bool ConfirmOutputOverwrite(string outputPath)
        {
            return (DialogResult.Yes == Program.MainForm.ShowMessage(
                "The output file below already exists! Overwrite?\n\n" + outputPath,
                "Overwrite?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation));
        }

        DialogResult HandleCanceledMerge(int remainingMergesForFile, Merge merge)
        {
            var msg = $"Merge {ProgressInfo.CurrentMergeNum} of {ProgressInfo.TotalMergeCount} was canceled.";
            var buttons = MessageBoxButtons.OK;
            if (remainingMergesForFile > 0)
            {
                var fileName = Path.GetFileName(merge.RelativePath);
                msg += $"\n\nContinue with {remainingMergesForFile} remaining merge{remainingMergesForFile.GetPluralS()} for file {fileName}?";
                buttons = MessageBoxButtons.YesNo;
            }
            var result = Program.MainForm.ShowMessage(msg, "Skipped Merge", buttons, MessageBoxIcon.Information);
            if (result == DialogResult.No)
            {
                ProgressInfo.CurrentMergeNum += remainingMergesForFile;
                return DialogResult.Abort;
            }
            return DialogResult.OK;
        }

        bool GetUnpackedFiles(string contentRelativePath, ref MergeSource source1, ref MergeSource source2)
        {
            if (_vanillaFile == null)
            {
                ProgressInfo.CurrentAction = "Searching for corresponding vanilla bundle";
                var bundleDirs = Directory.GetDirectories(Paths.BundlesDirectory)
                    .Select(path => Path.Combine(path, "bundles"))
                    .Where(path => Directory.Exists(path))
                    .ToList();
                for (int i = bundleDirs.Count - 1; i >= 0; --i)  // Search vanilla directories in reverse
                {                                                // order, as patches override content.
                    var bundleFiles = Directory.GetFiles(bundleDirs[i], "*.bundle");
                    foreach (var bundle in bundleFiles)
                    {
                        var contentPaths = ModFileIndex.GetBundleContentPaths(bundle);
                        if (contentPaths.Any(path => path.EqualsIgnoreCase(contentRelativePath)))
                        {
                            _vanillaFile = new FileInfo(bundle);
                            break;
                        }
                    }
                    if (_vanillaFile != null)
                        break;
                }
                if (_vanillaFile != null)
                {
                    ProgressInfo.CurrentAction = "Unpacking vanilla bundle content file";
                    var vanillaContentPath = UnpackFile(_vanillaFile.FullName, contentRelativePath, "Vanilla");
                    _vanillaFile = new FileInfo(vanillaContentPath);
                }
            }

            if (source1.TextFile == null)
            {
                ProgressInfo.CurrentAction = $"Unpacking bundle content file for {source1.Name}";
                var modContentFile1 = UnpackFile(source1.Bundle.FullName, contentRelativePath, "Mod 1");
                if (modContentFile1 == null)
                    return false;
                source1.TextFile = new FileInfo(modContentFile1);
            }
            ProgressInfo.CurrentAction = $"Unpacking bundle content file for {source2.Name}";
            var modContentFile2 = UnpackFile(source2.Bundle.FullName, contentRelativePath, "Mod 2");
            if (modContentFile2 == null)
                return false;
            source2.TextFile = new FileInfo(modContentFile2);
            return true;
        }

        string UnpackFile(string bundlePath, string contentRelativePath, string outputDirName)
        {
            if (!ValidateBmsResources(bundlePath))
                return null;

            var outputDir = Path.Combine(Paths.TempBundleContent, outputDirName);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            var procInfo = new ProcessStartInfo
            {
                FileName = Paths.Bms,
                Arguments = $"-Y -f \"{contentRelativePath}\" \"{Paths.BmsPlugin}\" \"{bundlePath}\" \"{outputDir}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var bmsProc = new Process { StartInfo = procInfo })
            {
                bmsProc.Start();
                var output = bmsProc.StandardError.ReadToEnd();  // QuickBMS prints results to std error, even if successful

                if (output.Contains("- 0 files found"))
                {
                    var errorMsg = "Error unpacking bundle content file using QuickBMS.\nIts output is below.";
                    var outputStart = output.IndexOf("- filter string");
                    if (outputStart != -1)
                    {
                        output = output.Substring(outputStart);
                        errorMsg += "\n\n" + output;
                    }
                    Program.MainForm.ShowError(errorMsg);
                    return null;
                }

                return Path.Combine(outputDir, contentRelativePath);
            }
        }

        public void RepackBundleAsync(string bundlePath)
        {
            if (_bgWorker.IsBusy)
                throw new Exception("BackgroundWorker can't run 2 tasks concurrently.");
            _bgWorker.DoWork += (sender, e) =>
            {
                var newBundlePath = PackNewBundle(bundlePath, true);
                if (newBundlePath == null)
                    return;

                if (Program.MainForm.CompletionSoundsSetting)
                {
                    System.Media.SystemSounds.Asterisk.Play();
                }
                if (Program.MainForm.PackReportSetting)
                {
                    using (var reportForm = new PackReportForm(bundlePath))
                    {
                        Program.MainForm.ShowModal(reportForm);
                    }
                }
            };
            _bgWorker.RunWorkerAsync();
        }

        string PackNewBundle(string bundlePath, bool isRepack = false)
        {
            ProgressInfo.CurrentPhase = (!isRepack ? "Packing Bundle" : "Repacking Bundle");
            ProgressInfo.CurrentAction = "Packing merged bundle content into new blob0.bundle";

            var contentDir = Path.Combine(Environment.CurrentDirectory, Paths.MergedBundleContent);
            
            if (!ValidateWccLiteResources(contentDir))
                return null;

            var outputDir = Path.GetDirectoryName(bundlePath);
            var procInfo = new ProcessStartInfo
            {
                FileName = Paths.WccLite,
                Arguments = $"pack -dir=\"{contentDir}\" -outdir=\"{outputDir}\"",
                WorkingDirectory = Path.GetDirectoryName(Paths.WccLite),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            using (var wccLiteProc = new Process { StartInfo = procInfo })
            {
                wccLiteProc.Start();
                var stdOutput = wccLiteProc.StandardOutput.ReadToEnd().Trim();
                var stdError = wccLiteProc.StandardError.ReadToEnd().Trim();

                string errorMsg = null;
                if (!string.IsNullOrWhiteSpace(stdError))
                    errorMsg = stdError;
                else if (stdOutput.EndsWith("Wcc operation failed"))
                    errorMsg = stdOutput;
                if (errorMsg != null)
                {
                    Program.MainForm.ShowError("Error packing merged content into a new bundle using wcc_lite.\nIts error output is below.\n\n" + errorMsg);
                    return null;
                }
            }
            ProgressInfo.CurrentAction = "Generating metadata.store for new blob0.bundle";
            procInfo.Arguments = $"metadatastore -path=\"{outputDir}\"";
            using (var wccLiteProc = new Process { StartInfo = procInfo })
            {
                wccLiteProc.Start();
                var stdOutput = wccLiteProc.StandardOutput.ReadToEnd();
                var stdError = wccLiteProc.StandardError.ReadToEnd();

                string errorMsg = null;
                if (!string.IsNullOrWhiteSpace(stdError))
                    errorMsg = stdError;
                else if (stdOutput.Contains("wcc operation failed"))
                    errorMsg = stdOutput;
                if (errorMsg != null)
                {
                    Program.MainForm.ShowError("Error generating metadata.store for new merged bundle using wcc_lite.\nIts error output is below.\n\n" + errorMsg);
                    return null;
                }
            }
            return bundlePath;
        }

        bool ValidateBmsResources(string bundlePath)
        {
            if (!File.Exists(bundlePath))
            {
                Program.MainForm.ShowError("Can't find bundle file:\n\n" + bundlePath, "Missing Bundle");
                return false;
            }
            if (!File.Exists(Paths.Bms))
            {
                Program.MainForm.ShowError("Can't find QuickBMS at this location:\n\n" + Paths.Bms, "Missing QuickBMS");
                return false;
            }
            if (!File.Exists(Paths.BmsPlugin))
            {
                Program.MainForm.ShowError("Can't find QuickBMS plugin at this location:\n\n" + Paths.BmsPlugin, "Missing QuickBMS Plugin");
                return false;
            }
            return true;
        }

        bool ValidateWccLiteResources(string contentDir)
        {
            if (!Directory.Exists(contentDir))
            {
                Program.MainForm.ShowError("Can't find Merged Bundle Content directory:\n\n" + contentDir, "Missing Directory");
                return false;
            }
            if (!File.Exists(Paths.WccLite))
            {
                Program.MainForm.ShowError("Can't find wcc_lite at this location:\n\n" + Paths.WccLite, "Missing wcc_lite");
                return false;
            }
            return true;
        }

        void CleanUpTempFiles()
        {
            if (!Directory.Exists(Paths.TempBundleContent))
                return;

            try
            {
                ProgressInfo.CurrentAction = "Deleting temporary unpacked bundle content";
                DeleteDirectory(Paths.TempBundleContent);
            }
            catch (Exception ex)
            {
                Program.MainForm.ShowMessage(
                    "Non-critical error: Failed to delete temporary unpacked bundle content.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        void CleanUpEmptyDirectories()
        {
            if (!Directory.Exists(Paths.MergedBundleContent))
                return;

            try
            {
                ProgressInfo.CurrentAction = "Deleting empty Merged Bundle Content directories";
                DeleteEmptyDirectories(Paths.MergedBundleContent);
            }
            catch (Exception ex)
            {
                Program.MainForm.ShowMessage(
                        "Non-critical error: Failed to delete empty Merged Bundle Content directories.\n\n" + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Depth-first recursive delete, with handling for descendant 
        /// directories open in Windows Explorer.
        /// </summary>
        void DeleteDirectory(string path)
        {
            foreach (var subdirPath in Directory.GetDirectories(path))
            {
                System.Threading.Thread.Sleep(1);
                DeleteDirectory(subdirPath);
            }

            try
            {
                System.Threading.Thread.Sleep(1);
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                System.Threading.Thread.Sleep(1);
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                System.Threading.Thread.Sleep(1);
                Directory.Delete(path, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Deletes any subdirectories of the root that are empty, AS WELL AS the root itself, if it's empty.
        /// </summary>
        void DeleteEmptyDirectories(string rootPath)
        {
            foreach (string directory in Directory.GetDirectories(rootPath))
            {
                System.Threading.Thread.Sleep(1);
                DeleteEmptyDirectories(directory);
            }

            if (Directory.GetFiles(rootPath).Any() || Directory.GetDirectories(rootPath).Any())
                return;
            
            try
            {
                System.Threading.Thread.Sleep(1);
                DeleteDirectory(rootPath);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}