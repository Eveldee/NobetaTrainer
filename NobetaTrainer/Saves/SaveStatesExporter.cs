using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Cpp2IL.Core.Extensions;
using NativeFileDialogSharp;
using NobetaTrainer.Config.Serialization;

namespace NobetaTrainer.Saves;

// NSS file tools
public class NobetaSaveStatesArchive
{
    public const string ArchiveFileExtension = ".nss";
    public const string InfoEntryName = "info.json";

    public static string GetEntryNameFromId(Guid id)
    {
        return $"{id}.dat";
    }

    public void Export(string groupName, IEnumerable<SaveState> saveStates)
    {
        // Choose destination
        var result = Dialog.FileSave(ArchiveFileExtension[1..]);

        if (!result.IsOk)
        {
            return;
        }

        string savePath = result.Path;
        if (!savePath.EndsWith(ArchiveFileExtension))
        {
            savePath = $"{savePath}{ArchiveFileExtension}";
        }

        Plugin.Log.LogInfo($"Saving save states '{groupName}' to '{savePath}'");

        // Generate info in json file
        var saveStatesInfo = Encoding.UTF8.GetBytes(SerializeUtils.SerializeIndented(saveStates));

        // Create zip with json info and each save file
        using var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Write info json
            var infoEntry = archive.CreateEntry(InfoEntryName);
            using (var infoEntryStream = infoEntry.Open())
            {
                infoEntryStream.Write(saveStatesInfo);
            }

            // Add each game save file
            foreach (var saveState in saveStates)
            {
                archive.CreateEntryFromFile(SavesManager.GetGameSaveStatePathFromGuid(saveState.Id), GetEntryNameFromId(saveState.Id));
            }
        }

        File.WriteAllBytes(savePath, memoryStream.ToArray());
    }

    public void Import(List<SaveState> saveStates)
    {
        // Open archive
        var result = Dialog.FileOpenMultiple(ArchiveFileExtension[1..]);

        if (!result.IsOk)
        {
            return;
        }

        foreach (var archivePath in result.Paths)
        {
            Plugin.Log.LogInfo($"Loading save states from '{archivePath}'");

            // Check file
            if (!File.Exists(archivePath))
            {
                continue;
            }

            // Open archive
            using var archive = ZipFile.OpenRead(archivePath);

            // Read info
            var infoEntry = archive.GetEntry(InfoEntryName);

            if (infoEntry is null)
            {
                Plugin.Log.LogWarning($"Skipping '{archivePath}' because no info file could be find in the archive");
                continue;
            }

            var saveStatesInfo = SerializeUtils.Deserialize<IEnumerable<SaveState>>(Encoding.UTF8.GetString(infoEntry.ReadBytes()));

            // Add each save state
            foreach (var saveState in saveStatesInfo)
            {
                // Check that a corresponding entry exists in the archive
                var saveStateEntry = archive.GetEntry($"{saveState.Id}.dat");

                if (saveStateEntry is null)
                {
                    continue;
                }

                // Add save state and copy game save file
                var destination = SavesManager.GetGameSaveStatePathFromGuid(saveState.Id);

                saveStateEntry.ExtractToFile(destination!, true);
                saveStates.Add(saveState);
            }
        }
    }
}