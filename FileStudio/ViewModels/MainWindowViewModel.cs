// ViewModels/MainWindowViewModel.cs
using FileStudio.Ai;
using FileStudio.Mvvm; // Add using for custom MVVM
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
using System.Windows.Input; // Add for ICommand
using FileStudio.Communication; // Add Mediator namespace
using FileStudio.Communication.Messages; // Add Messages namespace

namespace FileStudio.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {
        // Remove direct service dependencies
        // private readonly IAiService _aiService;
        // private readonly IFileService _fileService;
        // private readonly IPromptGenerator _promptGenerator;
        private readonly IMediator _mediator;

        // Store the currently selected folder
        private StorageFolder _currentFolder = null;

        // [ObservableProperty]
        private string _folderPathText = "No folder selected";
        public string FolderPathText
        {
            get => _folderPathText;
            set => SetProperty(ref _folderPathText, value);
        }

        // [ObservableProperty]
        private string _responseText = "AI Response will appear here...";
        public string ResponseText
        {
            get => _responseText;
            set => SetProperty(ref _responseText, value);
        }

        // [ObservableProperty]
        private bool _isBusy = false; // To indicate loading/processing states
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    // Manually trigger CanExecuteChanged for commands and dependent properties
                    (PickFolderCommand as RelayCommand)?.NotifyCanExecuteChanged(); // Use the new command property
                    (GenerateResponseCommand as RelayCommand)?.NotifyCanExecuteChanged(); // Use the new command property
                    (RenameFilesCommand as RelayCommand)?.NotifyCanExecuteChanged(); // Use the new command property
                    OnPropertyChanged(nameof(IsGenerateResponseEnabled));
                }
            }
        }

        // [ObservableProperty]
        private bool _canRename = false; // Controls Rename button enabled state
        public bool CanRename
        {
            get => _canRename;
            set
            {
                if (SetProperty(ref _canRename, value))
                {
                    (RenameFilesCommand as RelayCommand)?.NotifyCanExecuteChanged(); // Use the new command property
                }
            }
        }

        public ObservableCollection<CustomStorageFile> Files { get; } = [];

        // Store the generated AI response
        private string _generatedResponse = string.Empty;

        // Commands
        public ICommand PickFolderCommand { get; }
        public ICommand GenerateResponseCommand { get; }
        public ICommand RenameFilesCommand { get; }

        // Constructor for DI - Inject IMediator
        public MainWindowViewModel(IMediator mediator)
        {
            _mediator = mediator;

            // Initialize Commands
            PickFolderCommand = new RelayCommand(async (param) => await PickFolderAsync(param), _ => !IsBusy); // CanExecute depends on IsBusy
            GenerateResponseCommand = new RelayCommand(async _ => await GenerateResponseAsync(), _ => CanGenerateResponse());
            RenameFilesCommand = new RelayCommand(async _ => await RenameFilesAsync(), _ => CanRenameFiles());
        }

        // [RelayCommand] // Remove attribute
        private async Task PickFolderAsync(object windowObject)
        {
            if (windowObject is not Window window) return;

            var hwnd = WindowNative.GetWindowHandle(window);
            var request = new PickFolderRequest(hwnd);

            IsBusy = true;
            try
            {
                var response = await _mediator.SendAsync<PickFolderResponse>(request);

                if (response.SelectedFolder != null)
                {
                    _currentFolder = response.SelectedFolder;
                    FolderPathText = _currentFolder.Path;
                    await LoadFilesAsync();
                }
                else
                {
                    // User cancelled
                    FolderPathText = "Folder selection cancelled.";
                    Files.Clear();
                    _currentFolder = null;
                }
            }
            catch (Exception ex)
            {
                FolderPathText = $"Error picking folder: {ex.Message}";
                // Handle error appropriately, maybe show a dialog
            }
            finally
            {
                IsBusy = false;
            }
        }

        // [RelayCommand(CanExecute = nameof(CanGenerateResponse))] // Remove attribute
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
                // Prepare data for the request
                var filesForRequest = Files.ToList();
                var subFolderItems = await _currentFolder.GetFoldersAsync();
                var subFolderNames = subFolderItems.Select(f => f.Name).ToList();

                var request = new GenerateResponseRequest(_currentFolder, filesForRequest, subFolderNames);
                var response = await _mediator.SendAsync<GenerateResponseResponse>(request);

                if (response.Success)
                {
                    _generatedResponse = response.GeneratedResponse;
                    ResponseText = _generatedResponse;
                    CanRename = !string.IsNullOrEmpty(_generatedResponse); // Enable rename if response is valid
                }
                else
                {
                    ResponseText = response.ErrorMessage ?? "Failed to generate AI response.";
                    _generatedResponse = string.Empty;
                    CanRename = false;
                }
            }
            catch (Exception ex)
            {
                ResponseText = $"Error generating AI response: {ex.Message}";
                _generatedResponse = string.Empty;
                CanRename = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // [RelayCommand(CanExecute = nameof(CanRenameFiles))] // Remove attribute
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
                var request = new RenameFilesRequest(_currentFolder, _generatedResponse);
                var response = await _mediator.SendAsync<RenameFilesResponse>(request);

                ResponseText = response.Message;
                if (response.Success)
                {
                    _generatedResponse = string.Empty; // Clear response after use
                    CanRename = false; // Disable rename after completion
                    // Reload files to show changes
                    await LoadFilesAsync();
                }
                else
                {
                    // Keep CanRename true if renaming failed but response is still valid?
                    // Or clear response? For now, keep CanRename as is, user might retry.
                }
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
        // Update this to check the command's CanExecute status directly if needed, or keep logic here
        // For simplicity, keeping the logic here, but ensure GenerateResponseCommand.NotifyCanExecuteChanged() is called
        public bool IsGenerateResponseEnabled => CanGenerateResponse();

        // Helper method to load files from the current folder
        private async Task LoadFilesAsync()
        {
            if (_currentFolder == null) return;

            IsBusy = true;
            Files.Clear(); // Clear existing files
            CanRename = false; // Reset rename state
            _generatedResponse = string.Empty; // Clear previous response
            ResponseText = "Loading files..."; // Update status

            try
            {
                var fileItems = await _currentFolder.GetFilesAsync();
                foreach (var file in fileItems)
                {
                    Files.Add(await CustomStorageFile.CreateAsync(file));
                }
                ResponseText = $"Loaded {Files.Count} files. Ready to generate response.";
            }
            catch (Exception ex)
            {
                ResponseText = $"Error loading files: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                // Update command states after loading
                (GenerateResponseCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (RenameFilesCommand as RelayCommand)?.NotifyCanExecuteChanged();
                OnPropertyChanged(nameof(IsGenerateResponseEnabled));
            }
        }
    }
}
