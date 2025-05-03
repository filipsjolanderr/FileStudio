using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileStudio.Repositories;

/// <summary>
/// Defines the contract for repositories that handle file operations.
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Asynchronously renames files in a specified folder based on provided JSON information.
    /// </summary>
    /// <param name="folder">The folder containing the files to rename.</param>
    /// <param name="jsonFileInfo">JSON string containing the new file names and information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RenameFilesAsync(StorageFolder folder, string jsonFileInfo);


    Task<IEnumerable<StorageFile>> GetFilesAsync(StorageFolder folder);
}
