using System.Threading.Tasks;
using FileStudio.Communication;
using Windows.Storage; // For StorageFolder

namespace FileStudio.Communication.Messages
{
    /// <summary>
    /// Request to rename files based on a generated response.
    /// </summary>
    public class RenameFilesRequest : IRequest<RenameFilesResponse>
    {
        public StorageFolder CurrentFolder { get; }
        public string GeneratedResponse { get; }

        public RenameFilesRequest(StorageFolder currentFolder, string generatedResponse)
        {
            CurrentFolder = currentFolder;
            GeneratedResponse = generatedResponse;
        }
    }

    /// <summary>
    /// Response indicating the result of the file renaming operation.
    /// </summary>
    public class RenameFilesResponse
    {
        public bool Success { get; }
        public string Message { get; }

        public RenameFilesResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}
