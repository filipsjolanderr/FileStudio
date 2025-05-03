using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace FileStudio.FileManagement;

/// <summary>
/// Defines the contract for services that retrieve file information.
/// </summary>
public interface IFileService
{

    /// <summary>
    /// Asynchronously retrieves file information from a specified StorageFolder.
    /// </summary>
    /// <param name="folder">The StorageFolder to query.</param>
    /// <returns>A task representing the asynchronous operation, containing an enumerable collection of FileInfoProperty objects.</returns>
    Task<IEnumerable<StorageFile>> GetFilesAsync(StorageFolder folder);

    /// <summary>
    /// Asynchronously renames files based on the provided new name.
    /// </summary>
    /// <param name="files">The collection of FileInfoProperty objects to rename.</param>
    /// <param name="jsonFileInfo">Json file info to rename files.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RenameFilesAsync(StorageFolder folder, string jsonFileInfo);
}