using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using FileStudio.Ai; // Assuming AiOutputFileInfoList is in this namespace
using FileStudio.FileManagement; // Assuming FileRenamer is in this namespace

namespace FileStudio.Repositories;

/// <summary>
/// Concrete implementation of IFileRepository for handling file operations.
/// </summary>
public class FileRepository : IFileRepository
{
    // Helper method to get files, similar to the one in FileService
    public async Task<IEnumerable<StorageFile>> GetFilesAsync(StorageFolder folder)
    {
        if (folder == null)
        {
            return Enumerable.Empty<StorageFile>();
        }

        try
        {
            return await folder.GetFilesAsync();
        }
        catch (UnauthorizedAccessException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Access denied loading files from {folder.Path}: {ex.Message}");
            return null; // Or handle appropriately
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading files from {folder.Path}: {ex.Message}");
            return null; // Or handle appropriately
        }
    }

    /// <inheritdoc />
    public async Task RenameFilesAsync(StorageFolder folder, string jsonFileInfo)
    {
        var files = await GetFilesAsync(folder);
        if (files == null || !files.Any())
        {
            // Handle case where no files are found or accessible
            System.Diagnostics.Debug.WriteLine($"No files found or accessible in {folder.Path} for renaming.");
            return;
        }

        var fileRenamer = new FileRenamer(); // Consider injecting if it has dependencies
        await fileRenamer.RenameFilesAsync(files, jsonFileInfo);
    }

    /// <inheritdoc />
    public async Task CreateSidecarFileAsync(IEnumerable<StorageFile> files, string jsonFileInfo)
    {
        if (files == null || !files.Any())
        {
            throw new ArgumentException("No files provided to create a sidecar file.", nameof(files));
        }

        if (string.IsNullOrEmpty(jsonFileInfo))
        {
            throw new ArgumentException("JSON file info cannot be null or empty.", nameof(jsonFileInfo));
        }

        // Clean up potential markdown code block fences
        if (jsonFileInfo.StartsWith("```json\n"))
        {
            jsonFileInfo = jsonFileInfo.Substring(7, jsonFileInfo.Length - 7 - 4); // Remove ```json\n and \n```
        }
        else if (jsonFileInfo.StartsWith("```json"))
        {
             jsonFileInfo = jsonFileInfo.Substring(7, jsonFileInfo.Length - 7 - 3); // Remove ```json and ```
        }
        else if (jsonFileInfo.StartsWith("```"))
        {
            jsonFileInfo = jsonFileInfo.Substring(3, jsonFileInfo.Length - 3 - 3); // Remove ``` and ```
        }

        // Ensure the content is valid JSON before proceeding
        AiOutputFileInfoList? newFileInfo;
        try
        {
            newFileInfo = JsonSerializer.Deserialize<AiOutputFileInfoList>(jsonFileInfo);
            if (newFileInfo?.Files == null)
            {
                throw new JsonException("Deserialized JSON file info or its 'Files' property is null.");
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON file info format: {ex.Message}", nameof(jsonFileInfo), ex);
        }

        // Use the first file's folder to save the sidecar file
        var firstFile = files.First();
        var folder = await firstFile.GetParentAsync();
        if (folder == null)
        {
             throw new InvalidOperationException($"Could not get parent folder for file: {firstFile.Path}");
        }

        var sidecarFileName = $"{folder.Name}_sidecar_{DateTime.Now:yyyyMMddHHmmss}.json";
        var sidecarFile = await folder.CreateFileAsync(sidecarFileName, CreationCollisionOption.GenerateUniqueName); // Use GenerateUniqueName to avoid overwriting

        // Serialize the validated object back to JSON for the sidecar file
        var sidecarContent = JsonSerializer.Serialize(newFileInfo, new JsonSerializerOptions { WriteIndented = true });

        await FileIO.WriteTextAsync(sidecarFile, sidecarContent);
    }
}
