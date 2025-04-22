using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Threading;
using Microsoft.UI.Xaml.Media;
using ColorCode.Compilation.Languages;

namespace PowerPad.WinUI.Dialogs
{
    public partial class OllamaDownloadHelper : ContentDialog
    {
        private const string OLLAMA_DOWNLOAD_URL = "https://ollama.com/download/OllamaSetup.exe";
        private const int BUFFER_SIZE = 8192;

        private CancellationTokenSource? _cts;
        private bool _isDownloading;
        private bool _downloadCompleted;
        private readonly string _tempFilePath;

        private OllamaDownloadHelper(XamlRoot xamlRoot)
        {
            InitializeComponent();

            _tempFilePath = Path.Combine(Path.GetTempPath(), "OllamaSetup.exe");

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
                        await DownloadFileAsync(OLLAMA_DOWNLOAD_URL, _tempFilePath, ProgressBar, _cts.Token);

                        // Run the installer
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = _tempFilePath,
                            UseShellExecute = true
                        });

                        this.Message.Text = "Descarga completa. La instalación de Ollama se iniciará en otra ventana.";

                        Reset();

                        this.PrimaryButtonText = "Volver a comprobar";
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
                Hide();
            }
        }

        private static async Task DownloadFileAsync(string url, string destinationPath, ProgressBar progressBar, CancellationToken cancellationToken)
        {
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0L;
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[BUFFER_SIZE];
            long totalRead = 0;
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    progressBar.Value = (double)totalRead / totalBytes * 100;
                }
            }
        }
    }
}