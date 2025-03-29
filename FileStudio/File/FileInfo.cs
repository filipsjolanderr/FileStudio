using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;

namespace FileStudio.File
{
    public class FileInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _path;
        private string _type;
        private string _size;
        private string _date;
        private BitmapImage _icon;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public string Size
        {
            get => _size;
            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }

        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }

        public FileInfo(StorageFile file)
        {
            Name = file.Name;
            Path = file.Path;
            Type = file.FileType;
            Size = "Calculating...";
            Date = "Calculating...";
            Icon = new BitmapImage();
            LoadProperties(file);
        }
        private async void LoadProperties(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            Size = GetSize(properties.Size);
            Date = GetDate(properties.DateModified);
            Icon = await GetIcon(file);
        }

        private string GetSize(ulong size)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = size;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = size / 1024.0;
                size /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string GetDate(DateTimeOffset date)
        {
            return date.ToString("dd/MM/yyyy HH:mm:ss");
        }

        private static async Task<BitmapImage> GetIcon(IStorageItemProperties file)
        {
            var thumbnail = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);
            var bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(thumbnail);
            return bitmapImage;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
