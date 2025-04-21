using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using FileStudio.Ai;
using System.IO;

namespace FileStudio.FileManagement;

public class FileService : IFileService
{
    public async Task<IEnumerable<StorageFile>> GetFilesAsync(StorageFolder folder)
    {
        if (folder == null)
        {
            // Handle null folder case appropriately
            // Option 1: Return empty list
            return Enumerable.Empty<StorageFile>();
            // Option 2: Throw exception
            // throw new ArgumentNullException(nameof(folder));
        }

        try
        {
            // Get all files in the specified folder
            var storageFiles = await folder.GetFilesAsync();

            return storageFiles;
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Access denied loading files from {folder.Path}: {ex.Message}");
            // Decide how to signal this error - return null, empty list, or throw
            return null; // Example: Return null on access error
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading files from {folder.Path}: {ex.Message}");
            // Decide how to signal this error
            return null; // Example: Return null on other errors
        }
    }

    //rename files and create categorical folders
    public async Task RenameFilesAsync(StorageFolder folder, string jsonFileInfo)
    {
        var fileRenamer = new FileRenamer();
        var files = await GetFilesAsync(folder);
        await fileRenamer.RenameFilesAsync(files, jsonFileInfo);
    }
    
    //create sidecar file with summarys and original name and new name
    public async Task CreateSidecarFileAsync(IEnumerable<StorageFile> files, string jsonFileInfo)
    {
        if (files == null || !files.Any())
        {
            throw new ArgumentException("No files to rename.");
        }

        if (string.IsNullOrEmpty(jsonFileInfo))
        {
            throw new ArgumentException("Invalid JSON file info.");
        }

        var firstRow = jsonFileInfo.Split('\n')[0];
        if (firstRow == "```json")
        {
            //remove first row
            jsonFileInfo = jsonFileInfo.Remove(0, firstRow.Length + 1);

            //remove last row
            jsonFileInfo = jsonFileInfo.Remove(jsonFileInfo.Length - 3, 3);
        }

        // Parse the JSON file info to get the new names
        var newFileInfo = System.Text.Json.JsonSerializer.Deserialize<AiOutputFileInfoList>(jsonFileInfo);

        if (newFileInfo?.Files == null)
        {
            throw new ArgumentException("Invalid JSON file info.");
        }

        // Create a sidecar file with summaries and original/new names
        var sidecarContent = System.Text.Json.JsonSerializer.Serialize(newFileInfo);
        
        // Save the sidecar file in the same folder as the first file
        var firstFile = files.First();
        var folder = await StorageFolder.GetFolderFromPathAsync(firstFile.Path);
        
        var sidecarFileName = $"{folder.Name}_sidecar.json";
        var sidecarFile = await folder.CreateFileAsync(sidecarFileName, CreationCollisionOption.ReplaceExisting);
        
        await FileIO.WriteTextAsync(sidecarFile, sidecarContent);
    }


}
