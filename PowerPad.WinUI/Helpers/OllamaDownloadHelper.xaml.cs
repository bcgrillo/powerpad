using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PowerPad.Core.Models.AI;
using PowerPad.WinUI.ViewModels.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPad.WinUI.Dialogs
{
    /// <summary>
    /// A ContentDialog class that handles the download and installation process of the Ollama application.
    /// </summary>
    public partial class OllamaDownloadHelper : ContentDialog
    {
        private const string OLLAMA_DOWNLOAD_URL = "https://ollama.com/download/OllamaSetup.exe";
        private const int BUFFER_SIZE = 8192;

        private readonly string _tempFilePath;
        private readonly FileSizeToFriendlyStringConverter _fileSizeConverter;

        private CancellationTokenSource? _cts;
        private bool _isDownloading;
        private bool _downloadCompleted;
        private bool _tryInstallationAgain;
        private bool _installationCompleted;

        /// <summary>
        /// Displays the dialog asynchronously.
        /// </summary>
        /// <param name="xamlRoot">The XAML root to associate with this dialog.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the dialog result.</returns>
        public static async Task<ContentDialogResult> ShowAsync(XamlRoot xamlRoot)
        {
            var dialog = new OllamaDownloadHelper(xamlRoot);

            return await dialog.ShowAsync();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OllamaDownloadHelper"/> class.
        /// </summary>
        /// <param name="xamlRoot">The XAML root to associate with this dialog.</param>
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

        /// <summary>
        /// Resets the dialog to its initial state.
        /// </summary>
        private void Reset()
        {
            PrimaryButtonText = "Descargar Ollama";
            SecondaryButtonText = "Revisar configuración";
            CloseButtonText = "No usaré Ollama";
            MessageAux.Visibility = Visibility.Visible;
            ProgressBar.Visibility = Visibility.Collapsed;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;
            DownloadInfoText.Visibility = Visibility.Collapsed;
            DownloadInfoText.Text = null;
            _isDownloading = false;
        }

        /// <summary>
        /// Handles the primary button click event.
        /// </summary>
        /// <param name="_">The sender of the event (not used).</param>
        /// <param name="eventArgs">The event arguments.</param>
        private async void OnPrimaryButtonClick(object _, ContentDialogButtonClickEventArgs eventArgs)
        {
            if (!_installationCompleted)
            {
                eventArgs.Cancel = true;

                if ((!_isDownloading && !_downloadCompleted) || _tryInstallationAgain)
                {
                    try
                    {
                        if (!_downloadCompleted)
                        {
                            Message.Text = "Descargando Ollama, espere...";
                            MessageAux.Visibility = Visibility.Collapsed;

                            PrimaryButtonText = "Cancelar";
                            SecondaryButtonText = null;
                            CloseButtonText = null;
                            ProgressBar.Visibility = Visibility.Visible;
                            DownloadInfoText.Visibility = Visibility.Visible;

                            _isDownloading = true;
                            _cts = new();

                            // Get file size
                            using var httpClient = new HttpClient();
                            var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, OLLAMA_DOWNLOAD_URL));
                            response.EnsureSuccessStatusCode();
                            var fileSize = response.Content.Headers.ContentLength ?? 0L;

                            // Check available space
                            var driveInfo = new DriveInfo(Path.GetPathRoot(_tempFilePath)!);
                            if (driveInfo.AvailableFreeSpace < fileSize)
                            {
                                Message.Text = "No hay suficiente espacio en disco para descargar el instalador.";

                                Reset();
                                PrimaryButtonText = "Volver a intentar";
                            }
                            else
                            {
                                await DownloadFileAsync();

                                _downloadCompleted = true;
                            }
                        }

                        if (_downloadCompleted)
                        {
                            // Run the installer
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = _tempFilePath,
                                UseShellExecute = true
                            });

                            Message.Text = "Descarga completa. La instalación de Ollama se iniciará en otra ventana." +
                                "\nConfirme cuando la instalación haya finalizado.";

                            Reset();

                            PrimaryButtonText = "Instalación finalizada";
                            MessageAux.Visibility = Visibility.Collapsed;
                            _tryInstallationAgain = false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Try to delete the temp file if it exists
                        try { if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath); }
                        catch
                        {
                            // It's ok, is a temp file
                        }

                        Message.Text = "Descarga cancelada.";

                        Reset();
                        PrimaryButtonText = "Volver a intentar";
                    }
                    catch (Exception ex)
                    {
                        Message.Text = $"Ocurrió un error al descargar Ollama: {ex.Message}";

                        if (_downloadCompleted) _tryInstallationAgain = true;

                        Reset();
                        PrimaryButtonText = "Volver a intentar";
                    }
                }
                else if (_isDownloading)
                {
                    // Cancel the download
                    await _cts!.CancelAsync();
                }
                else // Download completed
                {
                    var settings = App.Get<SettingsViewModel>();

                    Message.Text = "Comprobando...";
                    ProgressBar.Visibility = Visibility.Visible;
                    ProgressBar.IsIndeterminate = true;
                    IsEnabled = false;

                    await settings.TestConnections();
                    if (settings.General.OllamaConfig.ServiceStatus == ServiceStatus.NotFound)
                    {
                        Message.Text = $"Parece que la instalación no se ha realizado correctamente.\nOllama no está disponible.";

                        Reset();
                        PrimaryButtonText = "Volver a intentar";
                        IsEnabled = true;

                        _tryInstallationAgain = true;
                    }
                    else
                    {
                        // Try to delete the temp file if it exists
                        try { if (File.Exists(_tempFilePath)) File.Delete(_tempFilePath); }
                        catch
                        {
                            // It's ok, is a temp file
                        }

                        Message.Text = $"¡Instalación finalizada con éxito!" +
                            "\nInstale algún modelo de inteligencia artificial para empezar a utilizar Ollama.";

                        Reset();
                        PrimaryButtonText = "Ir a modelos";
                        IsEnabled = true;
                        CloseButtonText = null;

                        _installationCompleted = true;
                    }
                }
            }
        }

        /// <summary>
        /// Downloads the file asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Handles the dialog closed event.
        /// </summary>
        private void ContentDialog_Closed(ContentDialog _, ContentDialogClosedEventArgs __)
        {
            _cts?.Dispose();
        }
    }
}