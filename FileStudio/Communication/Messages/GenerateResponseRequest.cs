using System.Collections.Generic;
using System.Threading.Tasks;
using FileStudio.Communication;
using FileStudio.FileManagement; // For CustomStorageFile
using Windows.Storage; // For StorageFolder

namespace FileStudio.Communication.Messages
{
    /// <summary>
    /// Request to generate an AI response based on folder contents.
    /// </summary>
    public class GenerateResponseRequest : IRequest<GenerateResponseResponse>
    {
        public StorageFolder CurrentFolder { get; }
        public List<CustomStorageFile> Files { get; }
        public List<string> SubFolders { get; }

        public GenerateResponseRequest(StorageFolder currentFolder, List<CustomStorageFile> files, List<string> subFolders)
        {
            CurrentFolder = currentFolder;
            Files = files;
            SubFolders = subFolders;
        }
    }

    /// <summary>
    /// Response containing the generated AI response.
    /// </summary>
    public class GenerateResponseResponse
    {
        public string GeneratedResponse { get; }
        public bool Success { get; }
        public string ErrorMessage { get; }

        public GenerateResponseResponse(string generatedResponse, bool success = true, string errorMessage = null)
        {
            GeneratedResponse = generatedResponse;
            Success = success;
            ErrorMessage = errorMessage;
        }
    }
}
