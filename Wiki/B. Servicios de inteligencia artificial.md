Este capítulo está dedicado a los servicios de inteligencia artificial que forman el núcleo de las capacidades avanzadas de PowerPad. En esta sección se describe cómo la aplicación integra y gestiona distintos proveedores de IA, como Ollama, Azure AI y OpenAI, permitiendo al usuario interactuar con modelos de lenguaje de manera flexible y personalizada. Se detallan las implementaciones específicas de cada servicio, los mecanismos para la gestión y búsqueda de modelos, así como la gestión de las conversaciones.

## 2.1. Implementaciones

#### OllamaService

##### Descripción general:

La clase `OllamaService` proporciona una implementación de los servicios de IA relacionados con Ollama, cumpliendo los contratos definidos por las interfaces `IAIService` e `IOllamaService`. Permite la gestión de modelos de IA, la inicialización, conexión, búsqueda, descarga, eliminación y control de procesos Ollama, así como la integración con modelos de Hugging Face cuando corresponda.

##### Código simplificado:

```csharp
public class OllamaService : IAIService, IOllamaService
{
    private const string HF_OLLAMA_PREFIX = "hf.co/";
    private const string HF_OLLAMA_PREFIX_AUX = "huggingface.co/";
    private const int TEST_CONNECTION_TIMEOUT = 5000;
    private const int DELAY_AFTER_START = 500;
    private const int DOWNLOAD_UPDATE_INTERVAL = 200;

    private OllamaApiClient? _ollama;
    private AIServiceConfig? _config;

    public void Initialize(AIServiceConfig? config) { ... }
    public async Task<TestConnectionResult> TestConnection() { ... }
    public async Task<IEnumerable<AIModel>> GetInstalledModels() { ... }
    public IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters) { ... }
    public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query) { ... }
    public async Task Start() { ... }
    public async Task Stop() { ... }
    public async Task DownloadModel(AIModel model, Action<double> updateAction, Action errorAction, CancellationToken cancellationToken) { ... }
    public async Task DeleteModel(AIModel model) { ... }

    private OllamaApiClient GetClient() { ... }
    private static AIModel CreateAIModel(Model model) { ... }
    private static IEnumerable<Process> GetProcesses() { ... }
}
```

##### Constantes:

- `HF_OLLAMA_PREFIX`: Prefijo para identificar modelos de Hugging Face.
- `HF_OLLAMA_PREFIX_AUX`: Prefijo auxiliar para identificar modelos de HuggingFace.
- `TEST_CONNECTION_TIMEOUT`: Tiempo de espera para probar la conexión con Ollama.
- `DELAY_AFTER_START`: Retardo tras iniciar Ollama.
- `DOWNLOAD_UPDATE_INTERVAL`: Intervalo de actualización del progreso de descarga.

##### Propiedades principales:

- `_ollama`: Cliente API de Ollama.
- `_config`: Configuración del servicio de IA.

##### Métodos públicos:

- `void Initialize(AIServiceConfig? config)`: Inicializa el servicio con una configuración dada.
- `Task<TestConnectionResult> TestConnection()`: Prueba la conexión con Ollama e informa del estado.
- `Task<IEnumerable<AIModel>> GetInstalledModels()`: Obtiene los modelos de IA instalados localmente.
- `IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)`: Obtiene un cliente de chat para el modelo especificado.
- `Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)`: Busca modelos disponibles según el proveedor indicado (Ollama o Hugging Face).
- `Task Start()`: Inicia el proceso de Ollama.
- `Task Stop()`: Detiene todos los procesos relacionados con Ollama.
- `Task DownloadModel(AIModel model, Action<double> updateAction, Action errorAction, CancellationToken cancellationToken)`: Descarga un modelo y reporta el progreso o error.
- `Task DeleteModel(AIModel model)`: Elimina un modelo instalado.

##### Otros métodos relevantes:

- `OllamaApiClient GetClient()`: Recupera o inicializa el cliente API de Ollama; lanza excepción si el servicio no está configurado.
- `static AIModel CreateAIModel(Model model)`: Crea una instancia de `AIModel` a partir de un modelo base, identificando el proveedor y generando la URL correspondiente.
- `static IEnumerable<Process> GetProcesses()`: Retorna los procesos activos relacionados con Ollama.

##### Nota importante:

- El servicio integra y diferencia automáticamente modelos de Ollama y Hugging Face según los prefijos de nombre, permitiendo una gestión unificada de ambos proveedores.
- El correcto funcionamiento depende de la configuración previa (`AIServiceConfig`) y la presencia de los procesos/ejecutables necesarios en el entorno del sistema operativo.

#### AzureAIService

##### Descripción general:

La clase `AzureAIService` implementa la interfaz `IAIService` y proporciona una integración para interactuar con servicios de inteligencia artificial de Azure. Permite inicializar la configuración del servicio, probar la conexión, obtener clientes de chat y buscar modelos de IA disponibles.

##### Código simplificado:

```csharp
public class AzureAIService : IAIService
{
    private const string TEST_MODEL = "openai/gpt-4.1-nano";
    private const int TEST_CONNECTION_TIMEOUT = 5000;
    private ChatCompletionsClient? _azureAI;
    private AIServiceConfig? _config;

    public void Initialize(AIServiceConfig? config) { ... }
    public async Task<TestConnectionResult> TestConnection() { ... }
    public IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters) { ... }
    public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query) { ... }
    private ChatCompletionsClient GetClient() { ... }
}
```

##### Constantes:

- `private const string TEST_MODEL`: Nombre del modelo utilizado para probar la conexión.
- `private const int TEST_CONNECTION_TIMEOUT`: Tiempo máximo, en milisegundos, para probar la conexión.

##### Propiedades principales:

- `private ChatCompletionsClient? _azureAI`: Cliente para interacciones con Azure AI.
- `private AIServiceConfig? _config`: Configuración del servicio AI.

##### Métodos públicos:

- `void Initialize(AIServiceConfig? config)`: Inicializa la configuración del servicio.
- `Task<TestConnectionResult> TestConnection()`: Prueba la conexión con el servicio Azure AI.
- `IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)`: Obtiene un cliente de chat para un modelo específico.
- `Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)`: Busca modelos de IA disponibles en el Marketplace de GitHub Models.

##### Otros métodos relevantes:

- `ChatCompletionsClient GetClient()`: Recupera o inicializa el cliente de Azure AI. Lanza una excepción si la configuración es inválida o la inicialización falla.

#### OpenAIService

##### Descripción general:

Implementación de la interfaz IAIService para interactuar con la API de OpenAI. Proporciona métodos para inicializar el servicio, probar la conexión, gestionar clientes de chat y buscar modelos de IA compatibles, asegurando la correcta configuración y filtrado de modelos según criterios específicos.

##### Código simplificado:

```csharp
public class OpenAIService : IAIService
{
    private const string GPT_MODEL_PREFIX = "gpt";
    private const string OX_MODEL_PREFIX = "o";
    private static readonly string[] EXCLUDED_WORDS = ["realtime", "image", "audio", "search", "transcribe"];
    private const string OPENAI_MODELS_BASE_URL = "https://platform.openai.com/docs/models/";
    private const int TEST_CONNECTION_TIMEOUT = 5000;
    private static readonly string[] REASONING_MODELS_NOT_ALLOWED_PARAMETERS = [nameof(IChatOptions.Temperature), nameof(IChatOptions.TopP)];

    private OpenAIClient? _openAI;
    private AIServiceConfig? _config;

    public void Initialize(AIServiceConfig? config) { ... }
    public async Task<TestConnectionResult> TestConnection() { ... }
    public IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters) { ... }
    public async Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query) { ... }
    private OpenAIClient GetClient() { ... }
    private static AIModel CreateAIModel(OpenAIModel openAIModel) { ... }
}
```

##### Constantes:

- `GPT_MODEL_PREFIX`: Prefijo para identificar modelos GPT de OpenAI.
- `OX_MODEL_PREFIX`: Prefijo para identificar otros modelos razonadores.
- `EXCLUDED_WORDS`: Palabras excluidas para filtrar modelos no compatibles.
- `OPENAI_MODELS_BASE_URL`: URL base para información de modelos.
- `TEST_CONNECTION_TIMEOUT`: Tiempo máximo para probar la conexión.
- `REASONING_MODELS_NOT_ALLOWED_PARAMETERS`: Parámetros no permitidos para modelos de razonamiento.

##### Propiedades principales:

- `_openAI`: Cliente de OpenAI, inicializado bajo demanda.
- `_config`: Configuración del servicio.

##### Métodos públicos:

- `void Initialize(AIServiceConfig? config)`: Inicializa el servicio con la configuración especificada.
- `Task<TestConnectionResult> TestConnection()`: Prueba la conexión con la API de OpenAI y devuelve el resultado de la conexión.
- `IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)`: Devuelve un cliente de chat para el modelo especificado e indica los parámetros no permitidos.
- `Task<IEnumerable<AIModel>> SearchModels(ModelProvider modelProvider, string? query)`: Busca y filtra modelos compatibles con chat según el proveedor y un término de búsqueda opcional.

##### Otros métodos relevantes:

- `OpenAIClient GetClient()`: Recupera e inicializa el cliente de OpenAI si es necesario. Lanza excepción si la configuración es inválida.
- `static AIModel CreateAIModel(OpenAIModel openAIModel)`: Crea una instancia de AIModel a partir de un modelo de OpenAI, generando su URL de información asociada.

##### Notas adicionales:

- El filtrado de modelos en `SearchModels` excluye aquellos que contienen ciertas palabras clave y solo incluye aquellos compatibles con chat completions.
- Los modelos de razonamiento tienen parámetros restringidos por limitaciones propias de la API de OpenAI.

#### ChatService

##### Descripción general:

La clase `ChatService` implementa la interfaz `IChatService` y proporciona la lógica necesaria para interactuar con modelos de inteligencia artificial y agentes conversacionales. Permite gestionar modelos y parámetros por defecto, obtener respuestas de chat y agentes, y preparar los parámetros requeridos para cada interacción, incluyendo el filtrado de contenido especial en las respuestas.

##### Código simplificado:

```csharp
public class ChatService : IChatService
{
    private static readonly string[] THINK_START_TAG = ["<think>", "<thought>"];
    private static readonly string[] THINK_END_TAG = ["</think>", "</thought>"];

    private AIModel? _defaultModel;
    private AIParameters? _defaultParameters;
    private readonly IReadOnlyDictionary<ModelProvider, IAIService> _aiServices;

    public void SetDefaultModel(AIModel? defaultModel) { ... }
    public void SetDefaultParameters(AIParameters? defaultParameters) { ... }
    public IAsyncEnumerable<ChatResponseUpdate> GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? parameters = null, CancellationToken cancellationToken = default) { ... }
    public IAsyncEnumerable<ChatResponseUpdate> GetAgentResponse(IList<ChatMessage> messages, Agent agent, CancellationToken cancellationToken = default) { ... }
    public async Task GetAgentSingleResponse(string input, StringBuilder output, Agent agent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default) { ... }

    private IChatClient ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters) { ... }
    private (IChatClient...) PrepareChatParameters(IList<ChatMessage> messages, AIModel? model, AIParameters? parameters) { ... }
    private (IChatClient...) PrepareAgentParameters(IList<ChatMessage> messages, Agent agent) { ... }
    private (IChatClient...) PrepareAgentParameters(string input, Agent agent, string? promptParameterValue, string? agentPrompt) { ... }
    private static ChatOptions PrepareChatOptions(IChatOptions chatOptions, IEnumerable<string>? notAllowedParameters) { ... }
}
```

##### Propiedades principales:

- `_defaultModel`: Modelo de IA por defecto utilizado si no se especifica uno.
- `_defaultParameters`: Parámetros de configuración por defecto para las interacciones.
- `_aiServices`: Diccionario de servicios de IA disponibles, indexados por proveedor de modelo.

##### Métodos públicos:

- `SetDefaultModel(AIModel? defaultModel)`: Establece el modelo de IA por defecto.
- `SetDefaultParameters(AIParameters? defaultParameters)`: Establece los parámetros de configuración por defecto.
- `GetChatResponse(IList<ChatMessage> messages, AIModel? model = null, AIParameters? parameters = null, CancellationToken cancellationToken = default)`: Obtiene una respuesta de chat del modelo de IA especificado o por defecto.
- `GetAgentResponse(IList<ChatMessage> messages, Agent agent, CancellationToken cancellationToken = default)`: Obtiene una respuesta de un agente de IA.
- `GetAgentSingleResponse(string input, StringBuilder output, Agent agent, string? promptParameterValue, string? agentPrompt, CancellationToken cancellationToken = default)`: Obtiene una única respuesta de un agente, filtrando contenido especial si es necesario.

##### Otros métodos relevantes:

- `ChatClient(AIModel model, out IEnumerable<string>? notAllowedParameters)`: Recupera el cliente de chat para el modelo de IA especificado y lista los parámetros no permitidos.
- `PrepareChatParameters(IList<ChatMessage> messages, AIModel? model, AIParameters? parameters)`: Prepara los parámetros necesarios para una interacción de chat, incluyendo cliente, opciones y mensajes.
- `PrepareAgentParameters(IList<ChatMessage> messages, Agent agent)`: Prepara los parámetros necesarios para una interacción con un agente, usando los valores por defecto si es necesario.
- `PrepareAgentParameters(string input, Agent agent, string? promptParameterValue, string? agentPrompt)`: Prepara los parámetros para una interacción con un agente a partir de una entrada única.
- `PrepareChatOptions(IChatOptions chatOptions, IEnumerable<string>? notAllowedParameters)`: Prepara las opciones de chat, omitiendo los parámetros no permitidos por el modelo.

##### Nota importante:

- El filtrado de contenido entre etiquetas `<think>` o `<thought>` en las respuestas únicas de los agentes asegura que la salida no contenga información interna o de razonamiento del modelo, mostrando solo la respuesta relevante para el usuario para aquellos escenarios no conversacionales (como la edición de notas).
- Esta implementación aprovecha los beneficios de las abstracciones incluidas en la biblioteca de `Microsoft.Extensions.IA`, implementadas en las bibliotecas de `OllamaSharp`, `AzureAIInference` y `OpenAI` para .NET. De modo que no accede a ninguna funcionalidad específica de los proveedores, utilizando solamente la interfaz abstracta `Microsoft.Extensions.IA.IChatClient` para la interacción con los modelos.

## 2.2. Búsqueda de Modelos

En esta sección se describe el mecanismo de búsqueda de modelos de inteligencia artificial dentro de PowerPad. Se explican las herramientas y utilidades desarrolladas para facilitar la localización, consulta y administración de modelos provenientes de la biblioteca de Ollama.com, o del API de Hugging Face o de GitHub Marketplace.

#### OllamaLibraryHelper

##### Descripción general:

La clase estática `OllamaLibraryHelper` proporciona métodos auxiliares para interactuar con la biblioteca de modelos de inteligencia artificial de Ollama. Permite buscar modelos AI en la web de Ollama y generar URLs directas a modelos específicos. Además, incluye utilidades internas para el manejo de tamaños de modelos.

##### Código simplificado:

```csharp
public static class OllamaLibraryHelper
{
    private const string OLLAMA_BASE_URL = "https://ollama.com";
    private const string OLLAMA_LIBRARY_URL = "https://ollama.com/library";
    private const string OLLAMA_SEARCH_URL = "https://ollama.com/search?q=";
    private const int MAX_RESULTS = 20;

    public static async Task<IEnumerable<AIModel>> Search(string? query) { ... }
    public static string GetModelUrl(string modelName) { ... }
    private static long ConvertSizeToBytes(string size) { ... }
}
```

##### Constantes:

- `OLLAMA_BASE_URL`: URL base del sitio de Ollama.
- `OLLAMA_LIBRARY_URL`: URL base de la biblioteca de modelos de Ollama.
- `OLLAMA_SEARCH_URL`: URL base para búsquedas de modelos en Ollama.
- `MAX_RESULTS`: Número máximo de resultados a devolver en una búsqueda.

##### Métodos públicos:

- `Task<IEnumerable<AIModel>> Search(string? query)`: Busca modelos de IA en la biblioteca de Ollama según la consulta proporcionada. Devuelve una colección de objetos `AIModel` que representan los resultados.
- `string GetModelUrl(string modelName)`: Genera y devuelve la URL correspondiente a un modelo de IA específico en la biblioteca de Ollama.

##### Otros métodos relevantes:

- `long ConvertSizeToBytes(string size)`: Convierte una cadena de texto que representa un tamaño (por ejemplo, "10MB", "2GB") a su valor equivalente en bytes. Es un método privado utilizado internamente para interpretar los tamaños de los modelos.

##### Notas adicionales:

- La clase está diseñada para ser utilizada de forma estática y no requiere instanciación.
- Utiliza `HtmlAgilityPack` para el análisis de HTML y extracción de información de la web de Ollama.
- El método `Search` excluye modelos que sean únicamente de tipo "embedding" y evita duplicados por nombre.

#### HuggingFaceLibraryHelper

##### Descripción general:

La clase estática `HuggingFaceLibraryHelper` proporciona métodos auxiliares para interactuar con la API de Hugging Face, permitiendo buscar modelos y obtener detalles de los mismos. Facilita la integración con Hugging Face para recuperar modelos compatibles con ciertas extensiones y obtener información relevante para su uso en aplicaciones de inteligencia artificial.

##### Código simplificado:

```csharp
public static class HuggingFaceLibraryHelper
{
    private const string HF_OLLAMA_PREFIX = "hf.co";
    private const string HUGGINGFACE_BASE_URL = "https://huggingface.co/";
    private const string HUGGINGFACE_SEARCH_URL = "https://huggingface.co/api/models?filter=gguf,conversational&search=";
    private const string HUGGINGFACE_MODEL_URL = "https://huggingface.co/api/models/";
    private const int MAX_RESULTS = 20;

    public static async Task<IEnumerable<AIModel>> Search(string? query) { ... }
    public static string GetModelUrl(string modelName) { ... }
    private static string ExtractTagFromFileName(string filePath) { ... }

    private sealed record HuggingFaceModel(string Id);
    private sealed record HuggingFaceFile(string Path, long Size);
}
```

##### Constantes:

- `HF_OLLAMA_PREFIX`: Prefijo utilizado para identificar modelos de Hugging Face en el contexto de la aplicación Ollama.
- `HUGGINGFACE_BASE_URL`: URL base del sitio de Hugging Face.
- `HUGGINGFACE_SEARCH_URL`: URL utilizada para buscar modelos en Hugging Face mediante la API.
- `HUGGINGFACE_MODEL_URL`: URL base para obtener detalles de un modelo específico.
- `MAX_RESULTS`: Número máximo de resultados a devolver en una búsqueda.

##### Métodos públicos:

- `Task<IEnumerable<AIModel>> Search(string? query)`: Busca modelos en la API de Hugging Face que coincidan con la consulta proporcionada y devuelve una colección de objetos `AIModel` representando los resultados.
- `string GetModelUrl(string modelName)`: Construye y devuelve la URL de un modelo específico en Hugging Face, a partir de su nombre (opcionalmente incluyendo un tag).

##### Otros métodos relevantes:

- `private static string ExtractTagFromFileName(string filePath)`: Extrae el tag de un nombre de archivo, asumiendo que el tag es la parte posterior al último guion ('-'). Si no hay guion, devuelve el nombre del archivo.
- `private sealed record HuggingFaceModel(string Id)`: Representa un modelo recuperado de la API de Hugging Face, identificándolo por su ID.
- `private sealed record HuggingFaceFile(string Path, long Size)`: Representa un archivo asociado a un modelo en la API de Hugging Face, incluyendo su ruta y tamaño.

##### Notas adicionales:

- La clase utiliza internamente `HttpClient` y deserialización JSON para interactuar con la API de Hugging Face.
- Solo se consideran archivos con extensión `.gguf`, que son los soportados por Ollama, para la creación de instancias de `AIModel`.
- El método `Search` devuelve solamente modelos con capacidad de conversación a través del filtro `conversational`.

#### GitHubMarketplaceModelsHelper

##### Descripción general:

Clase auxiliar estática para interactuar con el Marketplace de GitHub y buscar modelos de IA. Proporciona métodos para consultar y filtrar modelos según criterios específicos, excluyendo modelos restringidos y limitando la cantidad de resultados.

##### Código simplificado:

```csharp
public static class GitHubMarketplaceModelsHelper
{
    private const string GITHUB_BASE_URL = "https://github.com";
    private const string GITHUB_MARKETPLACE_SEARCH_URL = "https://github.com/marketplace?type=models&task=chat-completion&query=";
    private const string HEADER_ACCEPT = "Accept";
    private const string HEADER_APPLICATION_JSON = "application/json";
    private const string NAME_PREFIX = "/models/";
    private static readonly string[] RESTRICTED_MODEL_NAMES = ["o", "o1-mini", "o1-preview", "o3-mini", "o3", "o4-mini", "o4"];
    private const int MAX_RESULTS = 20;

    public static async Task<IEnumerable<AIModel>> Search(string? query) { ... }

    private sealed record GitHubMarketplaceResponse(List<GitHubModel> Results);
    private sealed record GitHubModel(string Name, string? Friendly_Name, string Id, string Model_Url, string Publisher);
}
```

##### Constantes:

- `GITHUB_BASE_URL`: URL base de GitHub.
- `GITHUB_MARKETPLACE_SEARCH_URL`: URL para buscar modelos en el Marketplace de GitHub.
- `HEADER_ACCEPT`: Nombre del encabezado HTTP para aceptar tipos de contenido.
- `HEADER_APPLICATION_JSON`: Valor del encabezado para aceptar JSON.
- `NAME_PREFIX`: Prefijo utilizado para identificar modelos en las URLs.
- `RESTRICTED_MODEL_NAMES`: Lista de nombres de modelos restringidos que no pueden usarse con claves gratuitas.
- `MAX_RESULTS`: Número máximo de resultados a devolver en una búsqueda.

##### Métodos públicos:

- `Task<IEnumerable<AIModel>> Search(string? query)`: Busca modelos de IA en el Marketplace de GitHub según el texto de consulta, excluyendo modelos restringidos y limitando el número de resultados.

##### Otros métodos relevantes:

- `GitHubMarketplaceResponse`: Record interno que representa la estructura de respuesta de la API del Marketplace de GitHub.
- `GitHubModel`: Record interno que representa un modelo individual en el Marketplace de GitHub.

##### Nota importante:

- Los modelos restringidos definidos en `RESTRICTED_MODEL_NAMES` no se incluyen en los resultados, ya que no funcionan con claves de desarrollador gratuitas.

