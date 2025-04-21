// ViewModels/MainWindowViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileStudio.Ai;
using FileStudio.FileManagement;
using Microsoft.UI.Xaml;
using System; 
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace FileStudio.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly IAiService _aiService;
        private readonly IFileService _fileService;
        private readonly IPromptGenerator _promptGenerator;

        // Store the currently selected folder
        private StorageFolder _currentFolder = null;

        [ObservableProperty]
        private string _folderPathText = "No folder selected";

        [ObservableProperty]
        private string _responseText = "AI Response will appear here...";

        [ObservableProperty]
        private bool _isBusy = false; // To indicate loading/processing states

        [ObservableProperty]
        private bool _canRename = false; // Controls Rename button enabled state

        public ObservableCollection<CustomStorageFile> Files { get; } = [];

        // Store the generated AI response
        private string _generatedResponse = string.Empty;

        // Constructor for DI
        public MainWindowViewModel(IAiService aiService, IFileService fileService, IPromptGenerator promptGenerator)
        {
            _aiService = aiService;
            _fileService = fileService;
            _promptGenerator = promptGenerator;
        }

        [RelayCommand]
        private async Task PickFolderAsync(object windowObject)
        {
            if (windowObject is not Window window) return;

            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(folderPicker, hwnd);

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                _currentFolder = folder;
                FolderPathText = folder.Path;
                await LoadFilesAsync();
            }
            else
            {
                // User cancelled
            }
        }

        [RelayCommand(CanExecute = nameof(CanGenerateResponse))] 
        private async Task GenerateResponseAsync()
        {
            if (_currentFolder == null || !Files.Any()) 
            {
                ResponseText = "Please select a folder and ensure files are loaded.";
                return;
            }

            IsBusy = true;
            ResponseText = "Generating...";
            CanRename = false; // Disable rename while generating

            try
            {
                // Re-fetch file details if necessary, or use existing Files collection directly
                // Assuming Files collection is up-to-date enough for the prompt
                var filesForPrompt = Files.ToList(); // Create a copy if needed
                var subFolders = await _currentFolder.GetFoldersAsync();
                var folders = subFolders.Select(f => f.Name).ToList();

                var prompt = _promptGenerator.GeneratePrompt(filesForPrompt, folders);
                _generatedResponse = await _aiService.GenerateResponseAsync(prompt);
                ResponseText = _generatedResponse;
                CanRename = !string.IsNullOrEmpty(_generatedResponse); // Enable rename if response is valid
            }
            catch (Exception ex)
            { 
                ResponseText = $"Error generating AI response: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanRenameFiles))] 
        private async Task RenameFilesAsync()
        { 
            if (_currentFolder == null || !Files.Any() || string.IsNullOrEmpty(_generatedResponse))
            {
                ResponseText = "Cannot rename: Folder not selected, no files loaded, or no AI response generated.";
                return;
            }

            IsBusy = true;
            ResponseText = "Renaming files...";

            try
            {
                // Pass the current folder and the generated response to the service
                await _fileService.RenameFilesAsync(_currentFolder, _generatedResponse);
                ResponseText = $"Renamed files based on the generated response.";
                _generatedResponse = string.Empty; // Clear response after use
                CanRename = false; // Disable rename after completion
                // Reload files to show changes
                await LoadFilesAsync(); 
            }
            catch (Exception ex)
            {
                ResponseText = $"Error renaming files: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        // CanExecute conditions
        private bool CanGenerateResponse() => !IsBusy && _currentFolder != null && Files.Any();
        private bool CanRenameFiles() => !IsBusy && _currentFolder != null && Files.Any() && !string.IsNullOrEmpty(_generatedResponse) && CanRename;

        // Public property for XAML binding to GenerateResponseCommand's CanExecute status
        public bool IsGenerateResponseEnabled => CanGenerateResponse();

        // Need to notify property changed for IsGenerateResponseEnabled when dependencies change
        partial void OnIsBusyChanged(bool value)
        {
            GenerateResponseCommand.NotifyCanExecuteChanged();
            RenameFilesCommand.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(IsGenerateResponseEnabled)); // Notify UI about the change
        }

        // We also need to notify when Files collection or _currentFolder changes.
        // LoadFilesAsync already calls NotifyCanExecuteChanged, which is good.
        // Let's ensure PickFolderAsync also triggers the notification indirectly or directly.
        // Modifying LoadFilesAsync to notify the property change is safer.

        private async Task LoadFilesAsync()
        {
            Files.Clear();
            _generatedResponse = string.Empty; // Clear previous response
            CanRename = false; // Reset rename state

            if (_currentFolder == null)
            {
                FolderPathText = "Please select a folder first.";
                ResponseText = "";
                // Ensure state is updated even if no folder
                GenerateResponseCommand.NotifyCanExecuteChanged();
                RenameFilesCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(IsGenerateResponseEnabled));
                return;
            }

            IsBusy = true;
            ResponseText = $"Loading files from {_currentFolder.Name}...";

            try
            {
                var filesFromService = await _fileService.GetFilesAsync(_currentFolder);

                if (filesFromService != null)
                {
                    foreach (var fileInfo in filesFromService)
                    {
                        var customFile = await CustomStorageFile.CreateAsync(fileInfo);
                        Files.Add(customFile);
                    }
                    ResponseText = !Files.Any() ? $"No files found in {_currentFolder.Name}." : $"Loaded {Files.Count} files from {_currentFolder.Name}. Ready for prompts.";
                }
                else
                {
                    FolderPathText = $"Failed to load files from '{_currentFolder.Name}'. Check permissions.";
                    ResponseText = $"Error loading files from '{_currentFolder.Name}'.";
                }
            }
            catch (Exception ex)
            {
                FolderPathText = $"Error accessing folder '{_currentFolder.Name}'.";
                ResponseText = $"Error loading files: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading files: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
                // Update CanExecute for commands after loading
                GenerateResponseCommand.NotifyCanExecuteChanged();
                RenameFilesCommand.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(IsGenerateResponseEnabled)); // Notify UI about the change
            }
        }
    }
}
