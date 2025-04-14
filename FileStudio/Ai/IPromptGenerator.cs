using System.Collections.Generic;
using System.IO;
using FileStudio.FileManagement;

namespace FileStudio.Ai;

/// <summary>
/// Defines the contract for generating prompts based on file information.
/// </summary>
public interface IPromptGenerator
{
    /// <summary>
    /// Generates a prompt string using the provided file name and content.
    /// </summary>
    /// <param name="fileInfoList"> An array of FileInfoProperty objects representing the files to be processed.</param>
    /// <returns>A formatted prompt string.</returns>
    string GeneratePrompt(List<CustomStorageFile> fileInfoList);
}