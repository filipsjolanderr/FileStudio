using System;
using System.Threading.Tasks;
using FileStudio.Communication.Messages;
using FileStudio.FileManagement; // For IFileService

namespace FileStudio.Communication.Handlers
{
    /// <summary>
    /// Handles the RenameFilesRequest.
    /// </summary>
    public class RenameFilesHandler : IRequestHandler<RenameFilesRequest, RenameFilesResponse>
    {
        private readonly IFileService _fileService;

        // Inject necessary services
        public RenameFilesHandler(IFileService fileService)
        {
            _fileService = fileService;
        }

        public async Task<RenameFilesResponse> HandleAsync(RenameFilesRequest request)
        {
            if (request.CurrentFolder == null || string.IsNullOrEmpty(request.GeneratedResponse))
            {
                return new RenameFilesResponse(false, "Cannot rename: Folder not selected or no AI response provided.");
            }

            try
            {
                // Pass the current folder and the generated response to the service
                await _fileService.RenameFilesAsync(request.CurrentFolder, request.GeneratedResponse);
                return new RenameFilesResponse(true, "Renamed files based on the generated response.");
            }
            catch (Exception ex)
            {
                // Log the exception details here if a logging service exists
                return new RenameFilesResponse(false, $"Error renaming files: {ex.Message}");
            }
        }
    }
}
