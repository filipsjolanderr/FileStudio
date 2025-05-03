using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using FileStudio.Repositories; // Added repository namespace

namespace FileStudio.FileManagement;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;

    // Constructor injection for IFileRepository
    public FileService(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;
    }

    // Delegate RenameFilesAsync to the repository
    public async Task RenameFilesAsync(StorageFolder folder, string jsonFileInfo)
    {
        await _fileRepository.RenameFilesAsync(folder, jsonFileInfo);
    }

    // Delegate GetFilesAsync to the repository
    public async Task<IEnumerable<StorageFile>> GetFilesAsync(StorageFolder folder)
    {
        return await _fileRepository.GetFilesAsync(folder);
    }
}
