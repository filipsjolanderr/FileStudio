namespace FileStudio.FileManagement;

using FileStudio.Ai;
using System;
using System.Collections.Generic;
using System.IO; // Required for FileInfoProperty, Path, Directory, File
using System.Linq;
using System.Text.RegularExpressions; // Required for Regex
using System.Threading.Tasks; // Required for Task
using Windows.Storage;

// Assume the following DTOs and Parser class are defined elsewhere or above:
// public class OutputFileResult { /* OriginalName, NewName, Summary */ }
// public class OutputFolderResult { /* FolderName, Files */ }
// public class AnalysisOutputPayload { /* Folders */ }
// public class AnalysisResultParser { /* DeserializeAnalysisResult method */ }

public class FileRenamer
{
    // Method to sanitize folder and file names suggested by the AI
    private string SanitizeName(string name, bool isDirectory = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty; // Return empty if input is bad
        }

        // Define invalid characters
        char[] invalidChars = isDirectory
            ? Path.GetInvalidPathChars() // Use broader set for directory parts
            : Path.GetInvalidFileNameChars(); // Use standard invalid file chars

        // Replace invalid characters with an underscore (or remove them)
        string sanitized = string.Join("_", name.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).Trim();

        // Additional checks (optional):
        // - Limit length?
        // - Check for reserved names (CON, PRN, AUX, NUL, COM1-9, LPT1-9)?

        // Ensure it's not empty after sanitization
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            // Maybe generate a fallback name like "InvalidName_[guid]"
            return $"SanitizedName_{Guid.NewGuid().ToString().Substring(0, 8)}";
        }

        // replace _ with space
        sanitized = sanitized.Replace("_", " ");

        return sanitized;
    }

    // Method to extract JSON potentially wrapped in Markdown code fences
    private string ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;

        // Regex to find ```json ... ``` block, capturing the content inside
        // Handles optional whitespace and potential leading/trailing text
        var match = Regex.Match(
            text,
            @"```json\s*([\s\S]*?)\s*```", // Match ```json, capture content (non-greedy), match ```
            RegexOptions.Multiline
        );

        if (match.Success && match.Groups.Count > 1)
        {
            // Return the captured JSON content, trimmed
            return match.Groups[1].Value.Trim();
        }

        // Fallback: If no fences found, assume the entire trimmed string is the JSON
        // This might be risky if the AI added explanatory text without fences.
        Console.WriteLine("Warning: JSON code fences ```json ... ``` not found. Attempting to parse the entire input.");
        return text.Trim();
    }


    /// <summary>
    /// Renames/moves files based on AI analysis JSON and creates categorical folders.
    /// Assumes input files share a common base directory.
    /// </summary>
    /// <param name="files">The collection of System.IO.FileInfoProperty objects representing original files.</param>
    /// <param name="jsonFileInfo">The JSON string output from the AI containing folder/rename instructions.</param>
    /// <returns>A Task representing the asynchronous operation completion.</returns>
    public async Task RenameFilesAsync(IEnumerable<StorageFile> files, string jsonFileInfo)
    {
        var fileInfos = files?.ToList(); // Enumerate once

        // --- Input Validation ---
        if (fileInfos == null || !fileInfos.Any())
        {
            Console.Error.WriteLine("No files provided to rename.");
            // throw new ArgumentException("No files to rename."); // Or just return
            return;
        }

        if (string.IsNullOrWhiteSpace(jsonFileInfo))
        {
            Console.Error.WriteLine("Invalid or empty JSON file info provided.");
            // throw new ArgumentException("Invalid JSON file info."); // Or just return
            return;
        }

        // Determine base directory (assuming all files share one)
        var path = fileInfos.First().Path;

        var baseDirectory = Path.GetDirectoryName(path); // Get the directory of the first file


        if (string.IsNullOrEmpty(baseDirectory))
        {
            Console.Error.WriteLine("Could not determine the base directory of the input files.");
            // throw new InvalidOperationException("Cannot determine base directory."); // Or return
            return;
        }

        // Create a lookup map for original files by name for efficient access
        // Using OrdinalIgnoreCase for case-insensitive matching on Windows/macOS
        var originalFileMap = fileInfos.ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);

        // --- Parse AI Output ---
        string cleanedJson = ExtractJson(jsonFileInfo); // Extract JSON from potential Markdown fences

        var parser = new AnalysisResultParser(); // Assuming this class exists
        var analysisResult = parser.DeserializeAnalysisResult(cleanedJson); // Assuming this method exists

        if (analysisResult?.Folders == null)
        {
            Console.Error.WriteLine("Failed to parse JSON or 'folders' array is missing/invalid.");
            // throw new ArgumentException("Invalid JSON file info structure."); // Or just return
            return;
        }

        // --- Perform File Operations ---
        Console.WriteLine($"Starting file processing based on AI analysis in base directory: {baseDirectory}");

        // Use Task.Run to offload synchronous I/O, keeping the method async
        // If File.MoveAsync were readily available and suitable, it could be used directly.
        await Task.Run(() =>
        {
            foreach (var folderResult in analysisResult.Folders)
            {
                string sanitizedFolderName = SanitizeName(folderResult.FolderName, isDirectory: true);
                if (string.IsNullOrWhiteSpace(sanitizedFolderName))
                {
                    Console.WriteLine($"Warning: Skipping folder with invalid/empty sanitized name derived from: {folderResult.FolderName}");
                    continue; // Skip this folder entry
                }

                string targetFolderPath = Path.Combine(baseDirectory, sanitizedFolderName);

                try
                {
                    // Create the target directory. Does nothing if it already exists.
                    Directory.CreateDirectory(targetFolderPath);
                    Console.WriteLine($"Ensured directory exists: {targetFolderPath}");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"ERROR: Failed to create directory '{targetFolderPath}'. Skipping files for this folder. Reason: {ex.Message}");
                    continue; // Skip processing files for this folder if directory creation fails
                }

                if (folderResult.Files == null) continue; // Skip if no files listed for this folder

                foreach (var fileResult in folderResult.Files)
                {
                    // Find the original file object using the map
                    if (!originalFileMap.TryGetValue(fileResult.OriginalName, out var originalFileInfo))
                    {
                        Console.WriteLine($"Warning: Original file '{fileResult.OriginalName}' mentioned in JSON not found in the provided file list. Skipping.");
                        continue;
                    }

                    string sanitizedNewFileName = SanitizeName(fileResult.NewName, isDirectory: false);
                    if (string.IsNullOrWhiteSpace(sanitizedNewFileName))
                    {
                        Console.WriteLine($"Warning: Skipping file '{fileResult.OriginalName}' due to invalid/empty sanitized new name derived from: {fileResult.NewName}");
                        continue;
                    }


                    string sourcePath = originalFileInfo.Path;
                    string destinationPath = Path.Combine(targetFolderPath, sanitizedNewFileName);

                    try
                    {
                        // Check if destination already exists to avoid accidental overwrite
                        // (File.Move throws IOException if destination exists)
                        if (File.Exists(destinationPath))
                        {
                            // Optional: Implement conflict resolution (e.g., rename with suffix, skip, overwrite)
                            Console.WriteLine($"Warning: Destination file '{destinationPath}' already exists. Skipping move for '{sourcePath}'.");
                            continue;
                        }

                        // Check if source and destination are effectively the same
                        if (string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Info: Source and destination are the same for '{sourcePath}'. No move needed.");
                            continue;
                        }


                        // Perform the move/rename operation
                        File.Move(sourcePath, destinationPath);
                        Console.WriteLine($"Moved: '{Path.GetFileName(sourcePath)}' -> '{Path.Combine(sanitizedFolderName, Path.GetFileName(destinationPath))}'");

                        // Remove processed file from map to prevent accidental reprocessing if listed twice? Optional.
                        // originalFileMap.Remove(fileResult.OriginalName);
                    }
                    catch (IOException ioEx)
                    {
                        Console.Error.WriteLine($"ERROR: I/O error moving '{sourcePath}' to '{destinationPath}'. Reason: {ioEx.Message}");
                        // Example: File might be locked
                    }
                    catch (UnauthorizedAccessException authEx)
                    {
                        Console.Error.WriteLine($"ERROR: Permission denied moving '{sourcePath}' to '{destinationPath}'. Reason: {authEx.Message}");
                    }
                    catch (Exception ex) // Catch other potential file system errors
                    {
                        Console.Error.WriteLine($"ERROR: Failed to move file '{sourcePath}' to '{destinationPath}'. Reason: {ex.Message}");
                        // Decide how to proceed: continue with next file, stop?
                    }
                } // End loop through files in folder
            } // End loop through folders
        }); // End Task.Run

        Console.WriteLine("File processing completed.");
        // Method completes when Task.Run finishes. The 'await' ensures this.
    }
}