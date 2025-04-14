using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.IO;

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
    }
}
