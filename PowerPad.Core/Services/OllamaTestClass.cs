using Microsoft.Extensions.AI;
using OllamaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;

namespace PowerPad.Core.Services
{
    public static class OllamaTestClass
    {
        public static async Task<string> GetResponseAsync(string model, string systemPrompt, string chatMessage)
        {
            IChatClient client = new OllamaChatClient(new Uri("http://localhost:11434/"), model);

            return (await client.GetResponseAsync(
            [
                new ChatMessage(ChatRole.System, systemPrompt),
                new ChatMessage(ChatRole.User, chatMessage),
            ])).Message.Text!;
        }

        public static async IAsyncEnumerable<ChatResponseUpdate> GetResponseAsync(string model, IList<ChatMessage> chatMessages, CancellationToken cancellationToken)
        {
            IChatClient client = new OllamaChatClient(new Uri("http://localhost:11434/"), model);

            var result = client.GetStreamingResponseAsync(chatMessages, null, cancellationToken);

            await foreach (var update in result)
            {
                yield return update;
            }
        }
    }
}
