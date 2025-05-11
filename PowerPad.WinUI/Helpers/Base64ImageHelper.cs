using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using PowerPad.WinUI.Dialogs;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace PowerPad.WinUI.Helpers
{
    /// <summary>
    /// Provides helper methods for handling Base64 image encoding and decoding.
    /// </summary>
    public static class Base64ImageHelper
    {
        private const ulong DEFAULT_SIZE = 48;

        /// <summary>
        /// Converts a Base64 string to a <see cref="BitmapImage"/> and optionally resizes it.
        /// </summary>
        /// <param name="base64String">The Base64 string representing the image.</param>
        /// <param name="size">The desired size for the image. Defaults to 48.</param>
        /// <returns>A <see cref="BitmapImage"/> created from the Base64 string.</returns>
        public static BitmapImage LoadImageFromBase64(string base64String, double size = DEFAULT_SIZE)
        {
            var imageBytes = Convert.FromBase64String(base64String);

            using var stream = new MemoryStream(imageBytes);

            var bitmapImage = new BitmapImage();

            if (Convert.ToUInt64(size) != DEFAULT_SIZE)
            {
                using var randomAccessStream = stream.AsRandomAccessStream();
                var decoder = BitmapDecoder.CreateAsync(randomAccessStream).GetAwaiter().GetResult();

                double scale = size / Math.Max(decoder.PixelWidth, decoder.PixelHeight);
                uint newWidth = (uint)(decoder.PixelWidth * scale);
                uint newHeight = (uint)(decoder.PixelHeight * scale);

                using var resizedStream = new InMemoryRandomAccessStream();
                var encoder = BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder).GetAwaiter().GetResult();
                encoder.BitmapTransform.ScaledWidth = newWidth;
                encoder.BitmapTransform.ScaledHeight = newHeight;
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                encoder.FlushAsync().Wait();

                resizedStream.Seek(0);
                bitmapImage.SetSource(resizedStream);
            }
            else
            {
                bitmapImage.SetSource(stream.AsRandomAccessStream());
            }

            return bitmapImage;
        }

        /// <summary>
        /// Opens a file picker to select an image and converts it to a Base64 string.
        /// </summary>
        /// <param name="xamlRoot">The <see cref="XamlRoot"/> of the current UI context.</param>
        /// <returns>A Base64 string representation of the selected image, or null if no image is selected.</returns>
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

                if (file is null) return string.Empty;

                var properties = await file.GetBasicPropertiesAsync();
                if (properties.Size > 200 * 1024) // More than 200 KB
                {
                    await DialogHelper.Alert(xamlRoot, "Error", "El archivo seleccionado supera los 200 KB. Por favor, seleccione otro archivo.");
                    continue;
                }

                using var stream = await file.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(stream);

                double scale = Convert.ToDouble(DEFAULT_SIZE) / Math.Max(decoder.PixelWidth, decoder.PixelHeight);
                uint newWidth = (uint)(decoder.PixelWidth * scale);
                uint newHeight = (uint)(decoder.PixelHeight * scale);

                using var resizedStream = new InMemoryRandomAccessStream();
                var encoder = await Windows.Graphics.Imaging.BitmapEncoder.CreateForTranscodingAsync(resizedStream, decoder);
                encoder.BitmapTransform.ScaledWidth = newWidth;
                encoder.BitmapTransform.ScaledHeight = newHeight;
                await encoder.FlushAsync();

                resizedStream.Seek(0);
                using var dataReader = new DataReader(resizedStream.GetInputStreamAt(0));
                var bytes = new byte[resizedStream.Size];
                await dataReader.LoadAsync((uint)resizedStream.Size);
                dataReader.ReadBytes(bytes);
                return Convert.ToBase64String(bytes);
            }
        }
    }
}