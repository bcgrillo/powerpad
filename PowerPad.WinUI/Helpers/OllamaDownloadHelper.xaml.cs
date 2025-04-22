using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Threading;
using CommunityToolkit.WinUI.Converters;
using System.Diagnostics;

namespace PowerPad.WinUI.Dialogs
{
    public partial class OllamaDownloadHelper : ContentDialog
    {
        private const string OLLAMA_DOWNLOAD_URL = "https://github.com/ip7z/7zip/releases/download/24.09/7z2409-x64.exe";
        private const int BUFFER_SIZE = 8192;

        private CancellationTokenSource? _cts;
        private bool _isDownloading;
        private bool _downloadCompleted;
        private readonly string _tempFilePath;
        private readonly FileSizeToFriendlyStringConverter _fileSizeConverter;

        private OllamaDownloadHelper(XamlRoot xamlRoot)
        {
            InitializeComponent();

            _tempFilePath = Path.Combine(Path.GetTempPath(), "OllamaSetup.exe");
            _fileSizeConverter = new FileSizeToFriendlyStringConverter();

            XamlRoot = xamlRoot;
            Title = "Descarga e instalación de Ollama";

            Reset();

            DefaultButton = ContentDialogButton.Primary;
        }

        private void Reset()
        {
            PrimaryButtonText = "Descargar Ollama";
            SecondaryButtonText = "Revisar configuración";
            CloseButtonText = "No volver a comprobar";
            MessageAux.Visibility = Visibility.Visible;
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.Value = 0;
            DownloadInfoText.Visibility = Visibility.Collapsed;
            DownloadInfoText.Text = null;
            _isDownloading = false;
        }

        public static async Task<ContentDialogResult> ShowAsync(XamlRoot xamlRoot)
        {
            var dialog = new OllamaDownloadHelper(xamlRoot);
            return await dialog.ShowAsync();
        }

        private async void OnPrimaryButtonClick(object _, ContentDialogButtonClickEventArgs eventArgs)
        {
            if (!_isDownloading && !_downloadCompleted)
            {
                eventArgs.Cancel = true;

                Message.Text = "Descargando Ollama, espere...";
                MessageAux.Visibility = Visibility.Collapsed;

                PrimaryButtonText = "Cancelar";
                SecondaryButtonText = null;
                CloseButtonText = null;
                ProgressBar.Visibility = Visibility.Visible;
                DownloadInfoText.Visibility = Visibility.Visible;

                _isDownloading = true;
                _cts = new();

                try
                {
                    // Get file size
                    using var httpClient = new HttpClient();
                    var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, OLLAMA_DOWNLOAD_URL));
                    response.EnsureSuccessStatusCode();
                    var fileSize = response.Content.Headers.ContentLength ?? 0L;

                    // Check available space
                    var driveInfo = new DriveInfo(Path.GetPathRoot(_tempFilePath)!);
                    if (driveInfo.AvailableFreeSpace < fileSize)
                    {
                        Message.Text = "No hay suficiente espacio en disco para descargar el instalador .";
                        
                        Reset();
                        PrimaryButtonText = "Volver a intentar";
                    }
                    else
                    {
                        await DownloadFileAsync();

                        // Run the installer
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = _tempFilePath,
                            UseShellExecute = true
                        });

                        Message.Text = "Descarga completa. La instalación de Ollama se iniciará en otra ventana.";

                        Reset();

                        PrimaryButtonText = "Volver a comprobar";
                        _downloadCompleted = true;
                    }
                }
                catch (OperationCanceledException)
                {
                    //Try delete the temp file if it exists
                    try { if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath); }
                    catch { }

                    Message.Text = "Descarga cancelada.";

                    Reset();
                    PrimaryButtonText = "Volver a intentar";
                }
                catch (Exception ex)
                {
                    Message.Text = $"Ocurrió un error al descargar Ollama: {ex.Message}";

                    Reset();
                    PrimaryButtonText = "Volver a intentar";
                }
            }
            else if (_isDownloading)
            {
                eventArgs.Cancel = true;

                // Cancel the download
                _cts?.Cancel();
            }
            else //Download completed
            {
                //Try delete the temp file if it exists
                try { if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath); }
                catch { }
                
                //Return to check Ollama status again
            }
        }

        private async Task DownloadFileAsync()
        {
            var cancellationToken = _cts?.Token ?? default;

            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(OLLAMA_DOWNLOAD_URL, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0L;
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(_tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[BUFFER_SIZE];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    ProgressBar.Value = (double)totalRead / totalBytes * 100;

                    DownloadInfoText.Text = "Descargado: " +
                        _fileSizeConverter.Convert(totalRead, typeof(string), null!, null!) +
                        " de " +
                        _fileSizeConverter.Convert(totalBytes, typeof(string), null!, null!);
                }
            }
        }
    }
}