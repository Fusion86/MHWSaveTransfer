using Cirilla.Core.Models;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace MHWSaveTransfer.Helpers
{
    // Mostly copy-pasted from https://github.com/Fusion86/MHWAppearanceEditor/blob/master/src/MHWAppearanceEditor/Services/BackupService.cs
    internal class BackupHelper
    {
        private static readonly int MaxBackupCount = 10;
        private static readonly ILogger log = Log.ForContext<BackupHelper>();

        private readonly string backupDir;

        public BackupHelper()
        {
            backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MHWSaveTransfer", "backup");

            try
            {
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);

                    string readme = "MHWSaveTransfer uses this folder for its automated backup feature.\n";
                    readme += "You can disable this feature by setting ' \"MaxBackupCount\": 0 ' in ../config.json\n\n";
                    readme += "Do NOT manually add files to this folder, MHWSaveTransfer may delete them at any time to make more space!";
                    File.WriteAllText(Path.Combine(backupDir, "README.txt"), readme);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
        }

        public void CreateBackup(SaveData saveData)
        {
            if (MaxBackupCount == 0) return;

            string fileName = "SAVEDATA_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string backupPath = Path.Combine(backupDir, fileName);
            string infoPath = Path.Combine(backupDir, fileName + ".txt");

            // Write SAVEDATA
            File.Copy(saveData.Filepath, backupPath);

            // Write info
            string info = "";
            for (int i = 0; i < saveData.SaveSlots.Length; i++)
            {
                info += $"SaveSlot {i + 1}:\n";

                var timespan = TimeSpan.FromSeconds(saveData.SaveSlots[i].PlayTime);
                string timeStr = (int)timespan.TotalHours + ":" + timespan.ToString(@"mm\:ss");

                // TODO: This used the **edited** huntername, rank, etc but the actual backup contains the **unedited** values.
                info += $"TimePlayed: {timeStr}\n";
                info += $"HunterName: {saveData.SaveSlots[i].HunterName}\n";
                info += $"HunterRank: {saveData.SaveSlots[i].HunterRank}\n";
                info += $"MasterRank: {saveData.SaveSlots[i].MasterRank}\n";
                info += $"HunterXp: {saveData.SaveSlots[i].HunterXp}\n";
                info += $"MasterXp: {saveData.SaveSlots[i].MasterXp}\n";
                info += $"PalicoName: {saveData.SaveSlots[i].PalicoName}\n\n";
            }

            File.WriteAllText(infoPath, info);

            log.Information($"Created a backup at '{backupPath}'");

            RemoveExcessiveBackups();
        }

        public void RemoveExcessiveBackups()
        {
            var files = Directory.GetFiles(backupDir, "*")
                .Where(x => !x.EndsWith(".txt"))
                .OrderByDescending(x => x)
                .Skip(MaxBackupCount)
                .ToList();

            if (files.Count > 0)
            {
                log.Information($"Deleting {files.Count} backup(s) because they exceed the MaxBackupCount");

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                        File.Delete(file + ".txt");
                    }
                    catch (Exception ex)
                    {
                        log.Error("Couldn't delete backup: " + ex.Message);
                    }
                }
            }
        }
    }
}
