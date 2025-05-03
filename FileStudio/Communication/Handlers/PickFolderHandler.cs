using System;
using System.Threading.Tasks;
using FileStudio.Communication.Messages;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace FileStudio.Communication.Handlers
{
    /// <summary>
    /// Handles the PickFolderRequest.
    /// </summary>
    public class PickFolderHandler : IRequestHandler<PickFolderRequest, PickFolderResponse>
    {
        public async Task<PickFolderResponse> HandleAsync(PickFolderRequest request)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add("*");

            InitializeWithWindow.Initialize(folderPicker, request.WindowHandle);

            var folder = await folderPicker.PickSingleFolderAsync();

            return new PickFolderResponse(folder);
        }
    }
}

