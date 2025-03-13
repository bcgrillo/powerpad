using ABI.System;
using OllamaSharp;
using PowerPad.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exception = System.Exception;

namespace PowerPad.Core.Services
{
    public interface IOllamaService
    {
        Task<OllamaStatus> GetStatus();
        Task<IEnumerable<ModelInfo>> GetModels();
    }

    public class OllamaService
    {
        private OllamaApiClient _ollama;

        public OllamaService(string baseUrl)
        {
            _ollama = new OllamaApiClient(baseUrl);
        }

        public async Task<OllamaStatus> GetStatus()
        {
            bool connected;

            try
            {
                connected = await _ollama.IsRunningAsync();
            }
            catch (Exception)
            {
                connected = false;
            }

            if (connected)
            {
                return OllamaStatus.Online;
            }
            else
            {
                try
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "ollama",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(startInfo)!)
                    {
                        await process.WaitForExitAsync();

                        if (process.ExitCode == 0) return OllamaStatus.Available;
                    }
                }
                catch (Exception)
                {
                }
            }

            return OllamaStatus.Unknown;
        }

        public async Task<IEnumerable<ModelInfo>> GetModels()
        {
            var models = await _ollama.ListLocalModelsAsync();

            return models.Select(m => new ModelInfo(m.Name, string.Empty));
        }
    }
}
