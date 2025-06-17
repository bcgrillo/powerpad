using Microsoft.Extensions.AI;
using Moq;
using PowerPad.Core.Contracts;
using PowerPad.Core.Models.AI;
using PowerPad.Core.Services.AI;
using System.Text;

namespace PowerPad.Core.Tests.Services.AI
{
    [TestClass()]
    public class ChatServiceTests
    {
        private Mock<IAIService>? _mockAIService;
        private Mock<IChatClient>? _mockChatClient;
        private ChatService? _chatService;

        [TestInitialize]
        public void Setup()
        {
            _mockAIService = new Mock<IAIService>();
            _mockChatClient = new Mock<IChatClient>();

            var aiServices = new Dictionary<ModelProvider, IAIService>
            {
                { ModelProvider.OpenAI, _mockAIService.Object }
            };

            _chatService = new ChatService(aiServices);
        }

        /// <summary>
        /// Tests that the default model is correctly set and retrieved.
        /// </summary>
        [TestMethod()]
        public void SetDefaultModelTest()
        {
            // Arrange
            var model = new AIModel("test-model", ModelProvider.OpenAI, null);

            // Act
            _chatService!.SetDefaultModel(model);

            // Assert
            Assert.AreEqual(model, typeof(ChatService).GetField("_defaultModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_chatService));
        }

        /// <summary>
        /// Tests that the default parameters are correctly set and retrieved.
        /// </summary>
        [TestMethod()]
        public void SetDefaultParametersTest()
        {
            // Arrange
            var parameters = new AIParameters { Temperature = 0.7f };

            // Act
            _chatService!.SetDefaultParameters(parameters);

            // Assert
            Assert.AreEqual(parameters, typeof(ChatService).GetField("_defaultParameters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(_chatService));
        }

        /// <summary>
        /// Tests that GetChatResponse calls the correct methods and returns the expected result.
        /// </summary>
        [TestMethod()]
        public void GetChatResponseTest()
        {
            // Arrange
            var messages = new List<ChatMessage> { new(ChatRole.User, "Hello") };
            var model = new AIModel("test-model", ModelProvider.OpenAI, null);
            var parameters = new AIParameters { Temperature = 0.7f };

            _mockAIService!.Setup(s => s.ChatClient(It.IsAny<AIModel>(), out It.Ref<IEnumerable<string>>.IsAny!))
                .Returns(_mockChatClient!.Object);

            _mockChatClient.Setup(c => c.GetStreamingResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncEnumerable.Empty<ChatResponseUpdate>());

            // Act
            var result = _chatService!.GetChatResponse(messages, model, parameters);

            // Assert
            Assert.IsNotNull(result);
            _mockAIService.Verify(s => s.ChatClient(It.IsAny<AIModel>(), out It.Ref<IEnumerable<string>>.IsAny!), Times.Once);
            _mockChatClient.Verify(c => c.GetStreamingResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAgentResponse calls the correct methods and returns the expected result.
        /// </summary>
        [TestMethod()]
        public void GetAgentResponseTest()
        {
            // Arrange
            var messages = new List<ChatMessage> { new(ChatRole.User, "Hello") };
            var agent = new Agent { Name = "TestAgent", Prompt = "TestPrompt", AIModel = new AIModel("test-model", ModelProvider.OpenAI, null) };

            _mockAIService!.Setup(s => s.ChatClient(It.IsAny<AIModel>(), out It.Ref<IEnumerable<string>>.IsAny!))
                .Returns(_mockChatClient!.Object);

            _mockChatClient.Setup(c => c.GetStreamingResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .Returns(AsyncEnumerable.Empty<ChatResponseUpdate>());

            // Act
            var result = _chatService!.GetAgentResponse(messages, agent);

            // Assert
            Assert.IsNotNull(result);
            _mockAIService.Verify(s => s.ChatClient(It.IsAny<AIModel>(), out It.Ref<IEnumerable<string>>.IsAny!), Times.Once);
            _mockChatClient.Verify(c => c.GetStreamingResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that GetAgentSingleResponse processes the input and appends the correct response to the output.
        /// </summary>
        [TestMethod()]
        public async Task GetAgentSingleResponseTest()
        {
            // Arrange
            var input = "Hello";
            var output = new StringBuilder();
            var agent = new Agent { Name = "TestAgent", Prompt = "TestPrompt", AIModel = new AIModel("test-model", ModelProvider.OpenAI, null) };
            var responseText = "Response";

            _mockAIService!.Setup(s => s.ChatClient(It.IsAny<AIModel>(), out It.Ref<IEnumerable<string>>.IsAny!))
                .Returns(_mockChatClient!.Object);

            _mockChatClient.Setup(c => c.GetResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatResponse(message: new(ChatRole.User, responseText)));

            // Act
            await _chatService!.GetAgentSingleResponse(input, output, agent, null, null);

            // Assert
            Assert.AreEqual(responseText, output.ToString());
            _mockAIService.Verify(s => s.ChatClient(It.IsAny<AIModel>(), out It.Ref<IEnumerable<string>>.IsAny!), Times.Once);
            _mockChatClient.Verify(c => c.GetResponseAsync(It.IsAny<IList<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}