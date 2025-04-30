using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using PowerPad.WinUI.Dialogs;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace PowerPad.WinUI.Helpers
{
    public static class Base64ImageHelper
    {
        public static BitmapImage LoadImageFromBase64(string base64String)
        {
            var imageBytes = Convert.FromBase64String(base64String);

            using var stream = new MemoryStream(imageBytes);

            var bitmapImage = new BitmapImage();
            bitmapImage.SetSource(stream.AsRandomAccessStream());
            return bitmapImage;
        }

        public static async Task<string?> PickImageToBase64(XamlRoot xamlRoot)
        {
            var filePicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter =
                {
                    ".png", ".jpg", ".jpeg"
                }
            };

            var window = App.MainWindow;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

            while (true)
            {
                var file = await filePicker.PickSingleFileAsync();

                if (file == null) return string.Empty;

                var properties = await file.GetBasicPropertiesAsync();
                if (properties.Size > 200 * 1024) // More than 200 KB
                {
                    await DialogHelper.Alert(xamlRoot, "Error", "El archivo seleccionado supera los 200 KB. Por favor, seleccione otro archivo.");
                    continue;
                }

                using (var stream = await file.OpenReadAsync())
                {
                    var decoder = await Windows.Graphics.Imaging.BitmapDecoder.CreateAsync(stream);

                    double scale = 50.0 / Math.Max(decoder.PixelWidth, decoder.PixelHeight);
                    uint newWidth = (uint)(decoder.PixelWidth * scale);
                    uint newHeight = (uint)(decoder.PixelHeight * scale);

                    using (var resizedStream = new InMemoryRandomAccessStream())
                    {
                        var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                        encoder.BitmapTransform.ScaledWidth = newWidth;
                        encoder.BitmapTransform.ScaledHeight = newHeight;
                        await encoder.FlushAsync();

                        resizedStream.Seek(0);
                        using (var dataReader = new DataReader(resizedStream.GetInputStreamAt(0)))
                        {
                            var bytes = new byte[resizedStream.Size];
                            await dataReader.LoadAsync((uint)resizedStream.Size);
                            dataReader.ReadBytes(bytes);
                            return Convert.ToBase64String(bytes);
                        }
                    }
                }
            }
        }
    }
}