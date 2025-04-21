// MainWindow.xaml.cs
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
// using System.Linq; // May not be needed directly here anymore
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using FileStudio.Ai;
using Windows.Storage; // Needed for StorageFolder
using Windows.Storage.Pickers; // Needed for FolderPicker
// Required for window handle interop
using WinRT.Interop;
using System.Linq;
using FileStudio.FileManagement;

namespace FileStudio
{
    public sealed partial class MainWindow : Window
    {
        private readonly IAiService _aiService;
        private readonly IFileService _fileService;
        private readonly IPromptGenerator _promptGenerator;

        // Store the currently selected folder
        private StorageFolder _currentFolder = null;

        public ObservableCollection<CustomStorageFile> Files { get; } = [];

        // Store the generated AI response
        private string _generatedResponse = string.Empty;

        public MainWindow()
        {
            this.InitializeComponent();

            _aiService = ((App)Application.Current).ServiceProvider.GetRequiredService<IAiService>();
            _fileService = ((App)Application.Current).ServiceProvider.GetRequiredService<IFileService>();
            _promptGenerator = ((App)Application.Current).ServiceProvider.GetRequiredService<IPromptGenerator>();

        }


        // --- Folder Picker Logic ---
        private async void PickFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop // Or Downloads, etc.
            };
            // FileTypeFilter is not strictly necessary for folders but good practice
            folderPicker.FileTypeFilter.Add("*");

            // Get window handle (HWND) for the picker. IMPORTANT for WinUI Desktop.
            var hwnd = WindowNative.GetWindowHandle(this);
            InitializeWithWindow.Initialize(folderPicker, hwnd); // Associate the picker with the window

            // Show the picker and get the result
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                // Store the selected folder
                _currentFolder = folder;

                // Update the UI to show the selected folder path
                PickFolderOutputTextBlock.Text = folder.Path;

                // Load/Reload files from the newly selected folder
                await LoadFilesAsync();
            }
            else
            {
                // User cancelled the picker - optionally provide feedback
                // PickFolderOutputTextBlock.Text = "Folder selection cancelled.";
            }
        }

        // Renamed and generalized loading method
        private async Task LoadFilesAsync()
        {
            // Clear existing files before loading new ones
            Files.Clear();

            // Check if a folder has been selected
            if (_currentFolder == null)
            {
                PickFolderOutputTextBlock.Text = "Please select a folder first.";
                // Optionally disable controls that require a folder/files
                return; // Don't proceed if no folder is selected
            }

            // Display loading status (optional)
            ResponseTextBlock.Text = $"Loading files from {_currentFolder.Name}...";

            try
            {
                // Use the injected service with the currently selected folder
                var filesFromService = await _fileService.GetFilesAsync(_currentFolder);

                if (filesFromService != null)
                {
                    foreach (var fileInfo in filesFromService)
                    {
                                                // Create a CustomStorageFile instance for each file
                                                var customFile = await CustomStorageFile.CreateAsync(fileInfo);
                                                Files.Add(customFile);
                    }
                    // Update status after loading (optional)
                    ResponseTextBlock.Text = !Files.Any() ? $"No files found in {_currentFolder.Name}." : $"Loaded {Files.Count} files from {_currentFolder.Name}. Ready for prompts.";
                }
                else
                {
                    // Handle case where service returned null (e.g., access denied)
                    PickFolderOutputTextBlock.Text = $"Failed to load files from '{_currentFolder.Name}'. Check permissions.";
                    ResponseTextBlock.Text = $"Error loading files from '{_currentFolder.Name}'.";
                }
            }
            catch (Exception ex)
            {
                // General error handling during file loading
                PickFolderOutputTextBlock.Text = $"Error accessing folder '{_currentFolder.Name}'.";
                ResponseTextBlock.Text = $"Error loading files: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"Error loading files: {ex.Message}");
            }
        }


        private async void GenerateResponseButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a folder and files are loaded
            if (_currentFolder == null || !Files.Any())
            {
                ResponseTextBlock.Text = "Please select a folder and ensure files are loaded.";
                return;
            }

            ResponseTextBlock.Text = "Generating...";

            try
            {
                var files = new List<CustomStorageFile> ();
                foreach (var file in Files)
                {
                    var storageFile = await StorageFile.GetFileFromPathAsync(file.Path);
                    var fileInfo = await CustomStorageFile.CreateAsync(storageFile);
                    files.Add(fileInfo);
                }
                // get folders in the selected folder
                var subFolders = await _currentFolder.GetFoldersAsync();
                var folders = subFolders.Select(folder => folder.Name).ToList();
                var prompt = _promptGenerator.GeneratePrompt(files, folders);
                _generatedResponse = await _aiService.GenerateResponseAsync(prompt);
                ResponseTextBlock.Text = _generatedResponse;
                //CreateSidecarFile();
            }
            catch (Exception ex)
            {
                ResponseTextBlock.Text = $"Error generating AI response: {ex.Message}";
            }
        }

        private async void RenameFilesButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if a folder and files are loaded
            if (_currentFolder == null || !Files.Any())
            {
                ResponseTextBlock.Text = "Please select a folder and ensure files are loaded.";
                return;
            }

            // Check if a response has been generated
            if (string.IsNullOrEmpty(_generatedResponse))
            {
                ResponseTextBlock.Text = "Please generate an AI response first.";
                return;
            }

            ResponseTextBlock.Text = "Renaming files...";

            try
            {
                await _fileService.RenameFilesAsync(_currentFolder, _generatedResponse);
                ResponseTextBlock.Text = $"Renamed {Files.Count} files.";
                // Optionally reload files to reflect the changes in the UI
                await LoadFilesAsync();
            }
            catch (Exception ex)
            {
                ResponseTextBlock.Text = $"Error renaming files: {ex.Message}";
            }
        }

        private async void CreateSidecarFile()
        {
            try
            {
               // await _fileService.CreateSidecarFileAsync()
            }
            catch (Exception e)
            {
                ResponseTextBlock.Text = $"Error creating sidecar file: {e.Message}";
            }
        }


    }
}