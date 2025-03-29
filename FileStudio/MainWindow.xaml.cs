using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;
using FileStudio.File;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FileStudio
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<FileInfo> Files { get; } = [];

        public MainWindow()
        {
            this.InitializeComponent();
            GetItemsAsync();
        }

        private async Task GetItemsAsync()
        {
            // get downloads folder
            var downloadFolder = await GetDownloadsFolder();

            var result = downloadFolder.CreateFileQueryWithOptions(new QueryOptions());

            var files = await result.GetFilesAsync();
            foreach (var file in files)
            {
                Files.Add(new FileInfo(file));
            }

            FileList.ItemsSource = Files;
        }

        public static async Task<StorageFolder> GetDownloadsFolder()
        {
            try
            {
                var downloadsFolder = await StorageFolder.GetFolderFromPathAsync(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads");

                // Alternatively, for modern UWP apps, you can try this (less reliable for desktop apps):
                // StorageFolder downloadsFolder = KnownFolders.Downloads;
                return downloadsFolder != null ? downloadsFolder : null; // Downloads folder not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting downloads folder: {ex.Message}");
                return null; // Handle any exceptions
            }
        }
    }
}
