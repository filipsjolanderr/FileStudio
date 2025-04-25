using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Text;

namespace FileStudio.FileManagement
{
    public class CustomStorageFile : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _name;
        private string _path;
        private string _type;
        private string _size;
        private string _date;
        private string _contentType;
        private string _byteContent;
        private string _textContent;
        private string _textContentTrimmed;
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

        public string ContentType
        {
            get => _contentType;
            set
            {
                _contentType = value;
                OnPropertyChanged();
            }
        }

        public string ByteContent
        {
            get => _byteContent;
            set
            {
                _byteContent = value;
                OnPropertyChanged();
            }
        }

        public string TextContent
        {
            get => _textContent;
            set
            {
                _textContent = value;
                OnPropertyChanged();
            }
        }

        public string TextContentTrimmed
        {
            get => _textContentTrimmed;
            set
            {
                _textContentTrimmed = value;
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

        // Make the constructor private
        public CustomStorageFile(StorageFile file)
        {
            Name = file.Name;
            Path = file.Path;
            Type = file.FileType;
            Size = "Calculating...";
            Date = "Calculating...";
            ContentType = file.ContentType;
            Icon = new BitmapImage();
        }

        // Static async method to create FileInfoProperty instance
        public static async Task<CustomStorageFile> CreateAsync(StorageFile file)
        {
            var fileInfo = new CustomStorageFile(file);
            await fileInfo.LoadPropertiesAsync(file);
            return fileInfo;
        }


        private async Task LoadPropertiesAsync(StorageFile file)
        {
            var properties = await file.GetBasicPropertiesAsync();
            Size = GetSize(properties.Size);
            Date = GetDate(properties.DateModified);
            Icon = await GetIcon(file);

            if (file.ContentType.StartsWith("text/"))
            {
                using var stream = await file.OpenReadAsync();
                using var reader = new DataReader(stream);
                await reader.LoadAsync((uint)stream.Size);
                var bytes = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(bytes);
                ByteContent = Convert.ToBase64String(bytes);
                TextContent = Encoding.UTF8.GetString(bytes);
            }
            else switch (file.FileType)
            {
                //pdf
                case ".pdf":
                    try
                    {
                        TextContent = await Task.Run(() => ReadPdfText(file.Path));
                        ByteContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(TextContent));
                    }
                    catch (FileNotFoundException)
                    {
                        ByteContent = "PDF Not Found";
                        TextContent = "PDF Not Found";
                        // Optionally log the error
                    }
                    catch (Exception ex)
                    {
                        ByteContent = "Error reading PDF";
                        TextContent = $"Error reading PDF: {ex.Message}";
                        // Optionally log the detailed error
                    }

                    break;
                default:
                    ByteContent = "Not supported";
                    TextContent = "Not supported";
                    break;
            }

            // Trimmed content for display
            if (TextContent.Length > 200)
            {
                TextContentTrimmed = TextContent[..200] + "...";
            }
            else
            {
                TextContentTrimmed = TextContent;
            }
        }

        public static string ReadPdfText(string pdfFilePath)
        {
            if (!File.Exists(pdfFilePath))
            {
                throw new FileNotFoundException($"Could not find file '{pdfFilePath}'");
            }

            var text = string.Empty;
            PdfReader reader = null;

            try
            {
                reader = new PdfReader(pdfFilePath);
                var pdfDoc = new PdfDocument(reader);

                for (var i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                    text += pageText + Environment.NewLine; // Add a newline after each page
                }

                pdfDoc.Close();
            }
            finally
            {
                reader?.Close();
            }

            return text;
        }

        private static string GetSize(ulong size)
        {
            string[] sizes = ["B", "KB", "MB", "GB", "TB"];
            var order = 0;
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
