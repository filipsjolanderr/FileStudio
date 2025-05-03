using System;
using FileStudio.Communication;
using Windows.Storage; // Required for StorageFolder

namespace FileStudio.Communication.Messages
{
    /// <summary>
    /// Request to initiate the folder picking process.
    /// </summary>
    public class PickFolderRequest : IRequest<PickFolderResponse>
    {
        /// <summary>
        /// The window handle required for the folder picker.
        /// </summary>
        public IntPtr WindowHandle { get; }

        public PickFolderRequest(IntPtr windowHandle)
        {
            WindowHandle = windowHandle;
        }
    }

    /// <summary>
    /// Response containing the result of the folder picking process.
    /// </summary>
    public class PickFolderResponse
    {
        /// <summary>
        /// The selected folder, or null if cancelled.
        /// </summary>
        public StorageFolder SelectedFolder { get; }

        public PickFolderResponse(StorageFolder selectedFolder)
        {
            SelectedFolder = selectedFolder;
        }
    }
}
