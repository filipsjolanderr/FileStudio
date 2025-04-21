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
using FileStudio.ViewModels; // Add ViewModel namespace

namespace FileStudio
{
    public sealed partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel { get; }

        public MainWindow(MainWindowViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = viewModel;
        }

    }
}
