## E.1. ViewModels

En el contexto de la **arquitectura MVVM**, los **ViewModels** constituyen una pieza central al actuar como puente entre la interfaz de usuario y la lógica de negocio. Su función principal es gestionar el estado, la lógica y los comandos necesarios para que la vista interactúe de forma desacoplada con los modelos de datos y los servicios subyacentes. En PowerPad, los ViewModels permiten mantener una interfaz reactiva y coherente, facilitando la actualización automática de la UI y simplificando la gestión de eventos y acciones del usuario. En este apartado se revisan los ViewModels implementados en la aplicación, describiendo su papel y cómo contribuyen a la estructura y funcionamiento general del software.

### E.1.1 Gestión de conversaciones

#### ChatViewModel

##### Descripción general:

`ChatViewModel` es un ViewModel encargado de gestionar la funcionalidad de chat, incluyendo la colección de mensajes, los parámetros de IA, el modelo de IA asociado y los comandos para manipular los mensajes. Supervisa los cambios en los mensajes y actualiza el estado de error del chat según corresponda.

##### Código simplificado:

```csharp
public class ChatViewModel : ObservableObject
{
    public AIModelViewModel? Model { get; set; }
    public AIParametersViewModel? Parameters { get; set; }
    public Guid? AgentId { get; set; }
    public bool ChatError { get; set; }
    public required ObservableCollection<MessageViewModel> Messages { get; set; }
    public IRelayCommand RemoveLastMessageCommand { get; }
    public IRelayCommand ClearMessagesCommand { get; }

    public ChatViewModel() { ... }

    private void RemoveLastMessage() { ...  }
    private void ClearMessages() { ...  }
    private void MessageCollectionChangedHandler( ... ) { ...  }
}
```

##### Propiedades observables:

- `Model`: Modelo de IA asociado al chat.
- `Parameters`: Parámetros de IA utilizados para configurar el comportamiento del chat.
- `AgentId`: Identificador único del agente asociado al chat.
- `ChatError`: Indica si existe un error en el chat.
- `Messages`: Colección observable de mensajes en el chat. Se inicializa con un manejador para supervisar cambios.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Comandos:

- `RemoveLastMessageCommand`: Comando para eliminar el último mensaje del chat.
- `ClearMessagesCommand`: Comando para limpiar todos los mensajes del chat.

##### Otros métodos relevantes:

- `MessageCollectionChangedHandler(NotifyCollectionChangedEventArgs eventArgs)`: Maneja los cambios en la colección de mensajes y actualiza la propiedad `ChatError` según el tipo de cambio realizado.

##### Nota importante:

- La colección `Messages` se inicializa con un manejador de eventos para detectar cambios y actualizar el estado de error del chat automáticamente.

#### MessageViewModel

##### Descripción general:

La clase `MessageViewModel` representa un mensaje dentro del chat, incluyendo su contenido, fecha y hora de creación, el rol del remitente, razonamientos opcionales, mensajes de error y estados de carga. Es utilizada para modelar cada mensaje en la interfaz de usuario de un chat, permitiendo el enlace de propiedades y la notificación de cambios.

##### Código simplificado:

```csharp
public class MessageViewModel : ObservableObject
{
    public string? Content { get; set; }
    public DateTime DateTime { get; private init; }
    public ChatRole Role { get; private init; }
    public string? Reasoning { get; set; }
    public bool Loading { get; set; }
    public string LoadingMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
```

##### Propiedades observables:

- `Content`: Contenido del mensaje.
- `Reasoning`: Razonamiento o explicación asociada al mensaje.
- `Loading`: Indica si el mensaje está en estado de carga.
- `LoadingMessage`: Mensaje mostrado durante el estado de carga.
- `ErrorMessage`: Mensaje de error si el mensaje tuvo algún problema.

##### Otras propiedades:

- `DateTime`: Fecha y hora de creación del mensaje (solo lectura).
- `Role`: Rol del remitente del mensaje (solo lectura).

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Notas adicionales:

- Las propiedades `Loading` y `LoadingMessage` están decoradas con `[JsonIgnore]`, por lo que no se serializan ni deserializan en operaciones JSON.


### E.1.2 Gestión de modelos y parámetros

#### AIModelViewModel

##### Descripción general:

ViewModel que representa y gestiona el estado de un modelo de inteligencia artificial (AIModel) en la aplicación. Permite controlar su disponibilidad, habilitación, descarga, progreso y errores asociados, además de exponer información relevante del modelo para la interfaz de usuario.

##### Código simplificado:

```csharp
public class AIModelViewModel : ObservableObject
{
    private readonly AIModel _aiModel;

    public string Name => _aiModel.Name;
    public ModelProvider ModelProvider => _aiModel.ModelProvider;
    public string? InfoUrl => _aiModel.InfoUrl;
    public bool Enabled { get; set; }
    public bool Available { get; set; }
    public bool Downloading { get; set; }
    public double Progress { get; set; }
    public bool DownloadError { get; set; }
    public CancellationTokenSource? DownloadCancelationToken { get; set; }
    public bool IsSizeTooLargeForExecution { get; set; }
    public long? Size => _aiModel.Size;
    public string? DisplayName => _aiModel.DisplayName;
    public bool CanAdd => !Available && !Downloading && !IsSizeTooLargeForExecution;

    public AIModel GetRecord() => _aiModel;
    public string CardName => DisplayName ?? Name;
    public override bool Equals(object? obj) { ... }
    public override int GetHashCode() { ... }
    public static bool operator ==( ... ) { ... }
    public static bool operator !=( ... ) { ... }

    public void OnAvailableChanged(bool value) { ... }
    public void OnDownloadingChanged(bool value) { ... }
    public void UpdateDownloadProgress(double progress) { ... }
    public void SetDownloadError() { ... }
}
```

##### Propiedades observables:

- `Enabled`: Indica si el modelo de IA está habilitado.
- `Available`: Indica si el modelo de IA está disponible para su uso.
- `Downloading`: Indica si el modelo de IA se está descargando actualmente.
- `Progress`: Progreso de la descarga del modelo de IA.
- `DownloadError`: Indica si ha habido un error durante la descarga.

##### Otras propiedades:

- `_aiModel`: Modelo `AIModel` subyacente.
- `Name`: Nombre único del modelo de IA.
- `ModelProvider`: Proveedor del modelo de IA (por ejemplo, Ollama, OpenAI).
- `InfoUrl`: URL opcional con más información sobre el modelo.
- `DownloadCancelationToken`: Token de cancelación para la operación de descarga.
- `IsSizeTooLargeForExecution`: Indica si el tamaño del modelo es demasiado grande para su ejecución local en el equipo.
- `Size`: Tamaño opcional del modelo en bytes.
- `DisplayName`: Nombre para mostrar opcional del modelo.
- `CanAdd`: Indica si el modelo puede ser añadido (no disponible, no descargando y tamaño permitido).
- `CardName`: Devuelve el nombre para mostrar o, en su defecto, el nombre único.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables y en algunas propiedades públicas modificadas explícitamente.

##### Métodos públicos:

- `AIModel GetRecord()`: Recupera el registro subyacente del modelo de IA.
- `override bool Equals(object? obj)`: Determina si el objeto especificado es igual al actual.
- `override int GetHashCode()`: Devuelve el código hash para el objeto actual.
- `static bool operator ==(AIModelViewModel? left, AIModelViewModel? right)`: Determina si dos instancias son iguales (sobrecarga del operador de igualdad).
- `static bool operator !=(AIModelViewModel? left, AIModelViewModel? right)`: Determina si dos instancias no son iguales (sobrecarga del operador de desigualdad).
- `void UpdateDownloadProgress(double progress)`: Actualiza el progreso de descarga del modelo de IA.
- `void SetDownloadError()`: Establece el estado de error de descarga a verdadero.

##### Otros métodos relevantes:

- `void OnAvailableChanged(bool value)`: Notifica el cambio de la propiedad `Available` y actualiza `CanAdd`.
- `void OnDownloadingChanged(bool value)`: Notifica el cambio de la propiedad `Downloading` y actualiza `CanAdd`.

##### Notas adicionales:

- Algunas propiedades están marcadas con `[JsonIgnore]` para evitar su persistencia.
- La clase implementa operadores de igualdad y desigualdad para facilitar la comparación entre instancias teniendo en cuenta los valores del modelo subyacente.
- Posee un constructor específico para deserialización decorado con `[JsonConstructor]`.

#### AIParameterViewModel

##### Descripción general:

Clase ViewModel para gestionar los parámetros de IA, proporcionando enlace de datos y notificación de cambios. Permite encapsular y modificar los parámetros de IA de manera reactiva, facilitando la interacción con la interfaz de usuario.

##### Código simplificado:

```csharp
public class AIParametersViewModel : ObservableObject
{
    private readonly AIParameters _aiParameters;

    public string? SystemPrompt
        { get => _aiParameters.SystemPrompt; set => Set(...); }
    public float? Temperature
        { get => _aiParameters.Temperature; set => Set(...); }
    public float? TopP
        { get => _aiParameters.TopP; set => Set(...); }
    public int? MaxOutputTokens
        { get => _aiParameters.MaxOutputTokens; set => Set(...); }
    public int? MaxConversationLength
        { get => _aiParameters.MaxConversationLength; set => Set(...); }

    public AIParameters GetRecord() => _aiParameters;
    public void SetRecord(AIParameters parameters) { Set(...); }
    public override bool Equals(object? obj) { ... }
    public override int GetHashCode() { ... }
    public static bool operator ==( ... ) { ... }
    public static bool operator !=( ... ) { ... }
    public AIParametersViewModel Copy() { ... }
}
```

##### Propiedades observables:

- No hay propiedades marcadas explícitamente con `[ObservableProperty]`.

##### Otras propiedades:

- `_aiParameters`: Modelo `AIParameters` subyacente.
- `SystemPrompt`: Indica el prompt del sistema que sirve como instrucción inicial para la IA.
- `Temperature`: Controla la aleatoriedad de las respuestas de la IA.
- `TopP`: Controla la probabilidad acumulada para el muestreo de tokens.
- `MaxOutputTokens`: Número máximo de tokens permitidos en la respuesta de salida.
- `MaxConversationLength`: Longitud máxima de una conversación en número de mensajes.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en algunas propiedades públicas modificadas explícitamente.

##### Métodos públicos:

- `AIParameters GetRecord()`: Devuelve el registro subyacente de parámetros de IA.
- `void SetRecord(AIParameters parameters)`: Actualiza el ViewModel con un nuevo registro de parámetros de IA.
- `override bool Equals(object? obj)`: Determina si el objeto especificado es igual a la instancia actual.
- `override int GetHashCode()`: Devuelve el código hash de la instancia actual.
- `static bool operator ==(AIParametersViewModel? left, AIParametersViewModel? right)`: Determina si dos instancias son iguales (sobrecarga del operador de igualdad).
- `static bool operator !=(AIParametersViewModel? left, AIParametersViewModel? right)`: Determina si dos instancias no son iguales (sobrecarga del operador de desigualdad).
- `AIParametersViewModel Copy()`: Crea una copia superficial de la instancia actual duplicando el modelo subyacente.

##### Notas adicionales:

- La clase implementa operadores de igualdad y desigualdad para facilitar la comparación entre instancias teniendo en cuenta los valores del modelo subyacente.
- Posee un constructor específico para deserialización decorado con `[JsonConstructor]`.

#### AIModelsViewModelBase

##### Descripción general:

Clase base abstracta para la gestión de modelos de IA en la aplicación. Proporciona funcionalidad para filtrar, buscar, añadir y eliminar modelos, así como gestionar el estado de disponibilidad de los proveedores de modelos de IA. Está diseñada para ser utilizada en el patrón MVVM a través de observables y comandos, facilitando la interacción con los modelos de IA desde la interfaz.

##### Código simplificado:

```csharp
public abstract class AIModelsViewModelBase : ObservableObject, IDisposable
{
    private bool _searchCompleted;
    protected readonly IAIService _aiService;
    protected readonly SettingsViewModel _settings;
    protected readonly ModelProvider _modelProvider;

    public IRelayCommand<AIModelViewModel> SetDefaultModelCommand { get; }
    public IAsyncRelayCommand<AIModelViewModel> RemoveModelCommand { get; }
    public IAsyncRelayCommand<AIModelViewModel> AddModelCommand { get; }
    public IAsyncRelayCommand<string> SearchModelCommand { get; }
    public ObservableCollection<AIModelViewModel> FilteredModels { get; }
    public ObservableCollection<AIModelViewModel> SearchResultModels { get; }
    public bool Searching { get; set; }
    public bool FilteredModelsEmpty => !FilteredModels.Any();
    public bool SearchResultModelsEmpty => _searchCompleted && !SearchResultModels.Any();
    public bool RepeaterEnabled { get; protected set; }

    protected AIModelsViewModelBase(ModelProvider modelProvider) { ... }
    protected void FilterModels( ... ) { ... }
    protected void SetDefaultModel(AIModelViewModel? aiModel) { ... }
    protected virtual async Task SearchModels(string? query) { ... }
    protected virtual Task AddModel(AIModelViewModel? aiModel) { ... }
    protected virtual Task RemoveModel(AIModelViewModel? aiModel) { ... }
    protected void UpdateRepeaterState( ... ) { ... }
    public void Dispose() { ... }
    protected virtual void Dispose(bool disposing) { ... }
}
```

##### Propiedades observables:

- `FilteredModels`: Colección de modelos de IA filtrados según el proveedor actual.
- `SearchResultModels`: Colección de modelos de IA resultado de una búsqueda.
- `Searching`: Indica si se está realizando una operación de búsqueda de modelos.

##### Otras propiedades:

- `FilteredModelsEmpty`: Indica si la colección de modelos filtrados está vacía.
- `SearchResultModelsEmpty`: Indica si la colección de resultados de búsqueda está vacía tras completarse la búsqueda.
- `RepeaterEnabled`: Indica si la funcionalidad de repetidor está habilitada para el proveedor actual.
- `_aiService`: Servicio de IA utilizado para buscar y gestionar modelos (protegido).
- `_settings`: ViewModel de configuración de la aplicación (protegido).
- `_modelProvider`: Proveedor de los modelos de IA (protegido).
- `_searchCompleted`: Indica si una búsqueda iniciada ha finalizado (privado).

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Comandos:

- `SetDefaultModelCommand`: Comando para establecer un modelo de IA como predeterminado.
- `RemoveModelCommand`: Comando asíncrono para eliminar un modelo de IA.
- `AddModelCommand`: Comando asíncrono para añadir un modelo de IA.
- `SearchModelCommand`: Comando asíncrono para buscar modelos de IA.

##### Métodos públicos:

- `Dispose()`: Libera los recursos utilizados por la clase.

##### Otros métodos relevantes:

- `FilterModels(NotifyCollectionChangedEventArgs eventArgs)`: Filtra los modelos según el proveedor y actualiza la colección filtrada.
- `UpdateRepeaterState()`: Actualiza el estado de la funcionalidad de repetidor según la disponibilidad del proveedor.
- `Dispose(bool disposing)`: Libera los recursos utilizados por la clase (virtual y protegido).

##### Notas adicionales:

- La clase gestiona suscripciones a eventos de cambio en la configuración y la colección de modelos, y se encarga de desuscribirse correctamente al ser liberada.

#### OllamaModelsViewModel

##### Descripción general:

`OllamaModelsViewModel` es un ViewModel especializado en la gestión de modelos de IA del proveedor Ollama. Permite refrescar la lista de modelos disponibles, buscar modelos, añadir y eliminar modelos, todo ello integrándose con un servicio Ollama y gestionando el estado de los modelos en la aplicación. Hereda de `AIModelsViewModelBase`.

##### Código simplificado:

```csharp
public class OllamaModelsViewModel : AIModelsViewModelBase
{
    private readonly IOllamaService _ollamaService;
    public IAsyncRelayCommand RefreshModelsCommand { get; }

    public OllamaModelsViewModel() : this(ModelProvider.Ollama) { ... }
    protected OllamaModelsViewModel(ModelProvider modelProvider) : base(modelProvider) { ... }

    protected async Task RefreshModels() { ... }
    protected override async Task SearchModels(string? query) { ... }
    protected override async Task AddModel(AIModelViewModel? aiModel) { ... }
    protected override async Task RemoveModel(AIModelViewModel? aiModel) { ... }
}
```

##### Propiedades observables:

- No se declaran propiedades con `[ObservableProperty]` en esta clase, pero hereda de `AIModelsViewModelBase`, que contiene propiedades observables relevantes.

##### Otras propiedades:

- `_ollamaService;`: Servicio para interactuar con Ollama.

##### Eventos:

- Al heredar de `ObservableObject` (a través de la jerarquía), notifica cambios en todas las propiedades observables.

##### Comandos:

- `RefreshModelsCommand`: Comando asíncrono para actualizar la lista de modelos disponibles utilizando el servicio de Ollama.

##### Métodos sobrecargados:

- `protected override async Task SearchModels(string? query)`: Busca modelos de IA según la consulta proporcionada y las características del equipo, deshabilitando las descargas de aquellos modelos que por tamaño no pueden ser ejecutados en el equipo local.
- `protected override async Task AddModel(AIModelViewModel? aiModel)`: Añade un modelo de IA a la colección de modelos disponibles, gestionando la descarga.
- `protected override async Task RemoveModel(AIModelViewModel? aiModel)`: Elimina un modelo de IA de la colección, cancelando la descarga si está en curso o eliminándolo físicamente a través del servicio.

##### Notas adicionales:

- La lógica de actualización y búsqueda de modelos está orientada a mantener sincronizada la colección local con el estado real del servicio Ollama.


#### HuggingFaceModelsViewModel

##### Descripción general:

ViewModel especializado en la gestión de modelos de IA de Hugging Face. Hereda de `OllamaModelsViewModel` y configura el proveedor de modelos como Hugging Face, permitiendo así funcionalidades específicas para este tipo de modelos dentro de la aplicación.

##### Código simplificado:

```csharp
public class HuggingFaceModelsViewModel : OllamaModelsViewModel
{
    public HuggingFaceModelsViewModel() : base(ModelProvider.HuggingFace) { }
}
```

##### Otras propiedades:

- Hereda todas las propiedades públicas y privadas de `OllamaModelsViewModel`.

##### Eventos:

- Al heredar de `ObservableObject` (a través de la jerarquía), notifica cambios en todas las propiedades observables.

##### Comandos:

- Hereda los comandos expuestos por `OllamaModelsViewModel`.

##### Notas adicionales:

- Este ViewModel sirve como especialización para el proveedor Hugging Face, permitiendo su identificación y gestión diferenciada respecto a otros proveedores de modelos IA.

#### GitHubModelsViewModel

##### Descripción general:

ViewModel especializado en la gestión de modelos de IA de GitHub Models. Hereda de `AIModelsViewModelBase` y configura el proveedor de modelos como GitHub, permitiendo así funcionalidades específicas para este tipo de modelos dentro de la aplicación.

##### Código simplificado:

```csharp
public class GitHubModelsViewModel : AIModelsViewModelBase
{
    public GitHubModelsViewModel() : base(ModelProvider.GitHub) { }
}
```

##### Otras propiedades:

- Hereda todas las propiedades públicas y privadas de `AIModelsViewModelBase`.

##### Eventos:

- Al heredar de `ObservableObject` (a través de la jerarquía), notifica cambios en todas las propiedades observables.

##### Comandos:

- Hereda los comandos expuestos por `AIModelsViewModelBase`.

##### Notas adicionales:

- Este ViewModel sirve como especialización para el proveedor GitHub Models, permitiendo su identificación y gestión diferenciada respecto a otros proveedores de modelos IA.

#### OpenAIModelsViewModel

##### Descripción general:

ViewModel especializado en la gestión de modelos de IA de OpenAI. Hereda de `AIModelsViewModelBase` y configura el proveedor de modelos como OpenAI, permitiendo así funcionalidades específicas para este tipo de modelos dentro de la aplicación.

##### Código simplificado:

```csharp
public class OpenAIModelsViewModel : AIModelsViewModelBase
{
    public OpenAIModelsViewModel() : base(ModelProvider.OpenAI) { ... }
}
```

##### Otras propiedades:

- Hereda todas las propiedades públicas y privadas de `AIModelsViewModelBase`.

##### Eventos:

- Al heredar de `ObservableObject` (a través de la jerarquía), notifica cambios en todas las propiedades observables.

##### Comandos:

- Hereda los comandos expuestos por `AIModelsViewModelBase`.

##### Notas adicionales:

- Este ViewModel sirve como especialización para el proveedor OpenAI, permitiendo su identificación y gestión diferenciada respecto a otros proveedores de modelos IA.

### E.1.3 Gestión de agentes

#### AgentViewModel

##### Descripción general:

Se trata de un ViewModel que representa a un agente de IA, encapsulando sus propiedades configurables y comportamientos asociados. Permite la edición y visualización de los parámetros del agente, su icono, visibilidad en distintas áreas de la aplicación y facilita la integración con el patrón MVVM. Gestiona la serialización/deserialización, notificación de cambios y operaciones de copia.

##### Código simplificado:

```csharp
public class AgentViewModel : ObservableObject
{
    private readonly Agent _agent;
    private ImageSource? _iconElementSource;

    public AgentViewModel(Agent agent, AgentIcon icon) { ... }
    public AgentViewModel(Guid id, string name, string prompt, string? promptParameterName, string? promptParameterDescription, AIModel? aiModel, float? temperature, float? topP, int? maxOutputTokens, AgentIcon icon, bool showInNotes, bool showInChats) : this(...) { ... }

    public Guid Id { get; set; }
    public string Name { get => _agent.Name; set => SetProperty(...); }
    public string Prompt { get => _agent.Prompt; set => SetProperty(...); }
    public string? PromptParameterName
	    { get => _agent.PromptParameterName; set => SetProperty(...); }
    public string? PromptParameterDescription
	    { get => _agent.PromptParameterDescription; set => SetProperty(...); }
    public AIModel? AIModel
	    { get => _agent.AIModel; set => SetProperty(...); }
    public float? Temperature
	    { get => _agent.Temperature; set => SetProperty(...); }
    public float? TopP
	    { get => _agent.TopP; set => SetProperty(...); }
    public int? MaxOutputTokens
	    { get => _agent.MaxOutputTokens; set => SetProperty(...); }

    public AgentIcon Icon { get; set; }
    public bool ShowInNotes { get; set; }
    public bool ShowInChats { get; set; }
    public bool AllowDrop { get; private init; } = false;
    public bool IsSelected { get; set; }
    public IconElement IconElement => Icon.Type switch { ... };
    public bool HasPromptParameter => !string.IsNullOrEmpty(PromptParameterName);
    public bool HasAIParameters => Temperature.HasValue;

    public Agent GetRecord() => _agent;
    public void SetRecord(Agent agent) { Set(...); }
    public void OnIconChanged(AgentIcon oldValue, AgentIcon newValue) { ... }
    public AgentViewModel Copy() { ... }
    public override bool Equals(object? obj) { ... }
    public override int GetHashCode() { ... }
    public static bool operator ==( ... ) { ... }
    public static bool operator !=( ... ) { ... }
}
```

##### Propiedades observables:

- `Icon`: Icono asociado al agente.
- `ShowInNotes`: Indica si el agente se muestra en la sección de notas.
- `ShowInChats`: Indica si el agente se muestra en la sección de chats.
- `IsSelected`: Indica si el agente está seleccionado actualmente.

##### Otras propiedades:

- `_agent`: Modelo `Agent` subyacente.
- `Id`: Identificador único del agente.
- `Name`: Nombre del agente.
- `Prompt`: Prompt utilizado por el agente.
- `PromptParameterName`: Nombre del parámetro utilizado en el prompt (opcional).
- `PromptParameterDescription`: Descripción del parámetro del prompt (opcional).
- `AIModel`: Modelo de IA asociado al agente, o nulo si se usa el modelo por defecto.
- `Temperature`: Valor de temperatura para la aleatoriedad de la IA (opcional).
- `TopP`: Valor Top-P para el muestreo de tokens (opcional).
- `MaxOutputTokens`: Número máximo de tokens en la salida (opcional).
- `IconElement`: Elemento gráfico generado a partir de `Icon` (calculado).
- `HasPromptParameter`: Indica si el agente tiene un parámetro de prompt (calculado).
- `HasAIParameters`: Indica si el agente tiene parámetros de IA configurados (calculado).

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Métodos públicos:

- `Agent GetRecord()`: Devuelve el registro subyacente del agente.
- `void SetRecord(Agent agent)`: Actualiza las propiedades del agente con los valores del registro proporcionado.
- `override bool Equals(object? obj)`: Determina si el objeto especificado es igual a la instancia actual.
- `override int GetHashCode()`: Devuelve el código hash de la instancia actual.
- `static bool operator ==(AgentViewModel? left, AgentViewModel? right)`: Determina si dos instancias son iguales (sobrecarga del operador de igualdad).
- `static bool operator !=(AgentViewModel? left, AgentViewModel? right)`: Determina si dos instancias no son iguales (sobrecarga del operador de desigualdad).
- `AgentViewModel Copy()`: Crea una copia superficial de la instancia actual duplicando el modelo subyacente.

##### Otros métodos relevantes:

- `void OnIconChanged(AgentIcon oldValue, AgentIcon newValue)`: Se ejecuta cuando cambia el icono, actualizando la representación visual y notificando el cambio de propiedad.

##### Notas adicionales:

- Algunas propiedades están marcadas con `[JsonIgnore]` para evitar su persistencia.
- La clase implementa operadores de igualdad y desigualdad para facilitar la comparación entre instancias teniendo en cuenta los valores del modelo subyacente.
- Posee un constructor específico para deserialización decorado con `[JsonConstructor]`.

#### AgentsCollectionViewModel

##### Descripción general:

Se trata del ViewModel encargado de gestionar la colección de agentes (`AgentViewModel`) en la aplicación. Se encarga de inicializar, exponer y mantener sincronizada la colección de agentes, así como de notificar cambios relevantes mediante eventos. Además, proporciona utilidades para obtener agentes por identificador y generar iconos aleatorios para los agentes, adaptados al tema de la aplicación. Gestiona la persistencia de la colección en el almacén de configuración.

##### Código simplificado:

```csharp
public class AgentsCollectionViewModel : ObservableObject
{
    private static readonly string[] RANDOM_GLYPHS = [ ... ];
    private readonly SettingsViewModel _settings;
    private readonly IConfigStore _configStore;
    private int _currentGlyphIndex = 0;

    public required ObservableCollection<AgentViewModel> Agents { get; set; }

    public event EventHandler? AgentsAvailabilityChanged;

    public AgentsCollectionViewModel() { ... }
    public AgentViewModel? GetAgent(Guid id) { ... }
    public AgentIcon GenerateIcon() { ... }

    private void CollectionChangedHandler( ... ) { ... }
    private void CollectionPropertyChangedHandler( ... ) { ... }
    private void SaveAgents() { ... }
}
```

##### Constantes:

- `RANDOM_GLYPHS`: Array de símbolos en formato unicode para iconos de agentes.

##### Propiedades observables:

- `Agents`: Colección observable de agentes gestionados por el ViewModel.

##### Otras propiedades:

- `_settings`: Referencia al ViewModel de configuración de la aplicación.
- `_configStore`: Referencia al almacén de configuración para persistencia.
- `_currentGlyphIndex`: Índice actual para seleccionar el símbolo aleatorio.

##### Métodos públicos:

- `AgentViewModel? GetAgent(Guid id)`: Devuelve el agente cuyo identificador coincide con el proporcionado, si existe.
- `AgentIcon GenerateIcon()`: Genera un icono aleatorio para un agente, adaptando el color al tema de la aplicación.

##### Eventos:

- `AgentsAvailabilityChanged`: Evento que se dispara cuando cambia la disponibilidad de los agentes (por cambios en la colección o en sus propiedades).
- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Otros métodos relevantes:

- `CollectionChangedHandler(NotifyCollectionChangedEventArgs eventArgs)`: Gestiona los cambios en la colección de agentes (altas, bajas), suscribiendo/desuscribiendo a eventos y notificando cambios.
- `CollectionPropertyChangedHandler(PropertyChangedEventArgs eventArgs)`: Gestiona los cambios en las propiedades de los agentes individuales, notificando y persistiendo los cambios.
- `SaveAgents()`: Guarda la colección actual de agentes en el almacén de configuración.

##### Notas adicionales:

- La colección `Agents` se inicializa y persiste automáticamente en el almacén de configuración (`IConfigStore`).

### E.1.4. Gestión del espacio de trabajo

#### FolderEntryViewModel

##### Descripción general:

Este ViewModel representa una entrada del sistema de archivos, ya sea una carpeta o un documento, dentro de la aplicación. Gestiona el estado visual (selección, expansión, símbolo), la jerarquía de carpetas/documentos, y expone comandos para eliminar y renombrar entradas. Además, responde a mensajes de cambios en las entradas mediante el patrón Messenger y notifica cambios de propiedades para la interfaz de usuario.

##### Código simplificado:

```csharp
public class FolderEntryViewModel : ObservableObject, IRecipient<FolderEntryChanged>
{
    private const string CLOSED_FOLDER_GLYPH = "\uE8B7";
    private const string OPEN_FOLDER_GLYPH = "\uE838";
    private readonly IFolderEntry _entry;
    private readonly DocumentType? _documentType;
    private readonly FolderEntryViewModel? _parent;

    public string Name { get => _entry.Name; }
    public string? Glyph { get; set; }
    public EntryType Type { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsSelected { get; set; }
    public ObservableCollection<FolderEntryViewModel> Children { get; }
    public bool IsFolder => Type == EntryType.Folder;
    public DocumentType? DocumentType => _documentType;
    public IFolderEntry ModelEntry => _entry;
    public int? Position { get => _entry.Position; }

    public IRelayCommand DeleteCommand { get; }
    public IRelayCommand RenameCommand { get; }

    public FolderEntryViewModel(Folder folder, FolderEntryViewModel? parent) { ... }
    public FolderEntryViewModel(Document document, FolderEntryViewModel? parent) { ... }

    public void NameChanged() { ... }
    public void Receive(FolderEntryChanged message) { ... }
    public void OnIsExpandedChanged(bool value) { ... }

    private void Delete() { ... }
    private void Rename(string? newName) { ... }
}
```

##### Constantes:

- `CLOSED_FOLDER_GLYPH`: Símbolo unicode para carpeta cerrada (`"\uE8B7"`).
- `OPEN_FOLDER_GLYPH`: Símbolo unicode para carpeta abierta (`"\uE838"`).

##### Propiedades observables:

- `Glyph`: Símbolo que representa visualmente la carpeta o documento.
- `Type`: Tipo de entrada (carpeta o documento).
- `IsExpanded`: Indica si la carpeta está expandida.
- `IsSelected`: Indica si la entrada está seleccionada.
- `Children`: Colección de entradas hijas (solo para carpetas).

##### Otras propiedades:

- `_entry`: Modelo `IFolderEntry` subyacente.
- `_documentType`: Tipo de documento privado, si aplica.
- `_parent`: Referencia privada al ViewModel padre.
- `Name`: Nombre de la carpeta o documento.
- `IsFolder`: Indica si la entrada es una carpeta.
- `DocumentType`: Tipo de documento, si aplica.
- `ModelEntry`: Propiedad pública para el modelo subyacente `_entry`.
- `Position`: Posición de la entrada dentro de su carpeta.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Comandos:

- `DeleteCommand`: Elimina la entrada (carpeta o documento).
- `RenameCommand`: Renombra la entrada.

##### Métodos públicos:

- `void NameChanged()`: Notifica que el nombre de la entrada ha cambiado.
- `void Receive(FolderEntryChanged message)`: Maneja la recepción de mensajes de cambio en la entrada.
- `void OnIsExpandedChanged(bool value)`: Actualiza el símbolo cuando cambia el estado expandido de la carpeta.

##### Otros métodos relevantes:

- `void Delete()`: Lógica para eliminar la entrada y notificar el cambio.
- `void Rename(string? newName)`: Lógica para renombrar la entrada y notificar el cambio.

##### Notas adicionales:

- Utiliza el patrón Messenger para sincronizar cambios de nombre y eliminación entre diferentes instancias del ViewModel.
#### WorkspaceViewModel

##### Descripción general:

Es el ViewModel encargado de gestionar el espacio de trabajo de la aplicación, incluyendo la administración de carpetas, documentos y la lista de espacios de trabajo abiertos recientemente. Facilita la interacción entre la interfaz de usuario y los servicios de gestión de archivos y configuración, permitiendo operaciones como mover, crear y abrir entradas (carpetas o documentos) dentro del espacio de trabajo.

##### Código simplificado:

```csharp
public class WorkspaceViewModel : ObservableObject
{
    private const int MAX_RECENTLY_WORKSPACES = 5;

    private readonly IWorkspaceService _workspaceService;
    private readonly IConfigStore _appConfigStore;

    public FolderEntryViewModel Root { get; set; }
    public ObservableCollection<string> RecentlyWorkspaces { get; }
    public string? CurrentDocumentPath { get; set; }

    public IRelayCommand OpenWorkspaceCommand { get; }
    public IRelayCommand MoveEntryCommand { get; }
    public IRelayCommand NewEntryCommand { get; }

    public WorkspaceViewModel() { ... }

    public void OnCurrentDocumentPathChanged(string? oldValue, string? newValue) { ... }
    private void MoveEntry(MoveEntryParameters? parameters) { ... }
    private void NewEntry(NewEntryParameters? parameters) { ... }
    private void OpenWorkspace(string? path) { ... }
}
```

##### Constantes:

- `MAX_RECENTLY_WORKSPACES`: Numero máximo de espacios de trabajo recientes (5).

##### Propiedades observables:

- `Root`: Representa la entrada raíz (carpeta principal) del espacio de trabajo.
- `RecentlyWorkspaces`: Colección de rutas de los espacios de trabajo abiertos recientemente.
- `CurrentDocumentPath`: Ruta del documento actualmente abierto.

##### Otras propiedades:

- `_workspaceService`: Servicio privado para la gestión del espacio de trabajo.
- `_appConfigStore`: Servicio privado para la gestión de la configuración persistente.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Comandos:

- `OpenWorkspaceCommand`: Permite abrir un espacio de trabajo a partir de una ruta.
- `MoveEntryCommand`: Permite mover una entrada a otra ubicación dentro del espacio de trabajo.
- `NewEntryCommand`: Permite crear una nueva entrada (carpeta o documento).

##### Métodos públicos:

- `void OnCurrentDocumentPathChanged(string? oldValue, string? newValue)`: Actualiza la configuración persistida cuando cambia la ruta del documento actual.

##### Notas adicionales:

- La clase utiliza servicios inyectados para la gestión del espacio de trabajo y la configuración persistente.
- Utiliza el patrón Messenger para notificar la creación de nuevas entradas.

#### DocumentViewModel

##### Descripción general:

`DocumentViewModel` es un ViewModel encargado de gestionar un documento en la aplicación, incluyendo su estado, guardado, renombrado y generación automática de nombres. Facilita la interacción entre la vista y el modelo de documento, gestionando comandos y notificando cambios relevantes. Implementa el patrón MVVM y utiliza mensajería para comunicar cambios en el nombre del documento. También contiene la lógica para invocar la generación automática de nombre.

##### Código simplificado:

```csharp
public class DocumentViewModel : ObservableObject, IRecipient<FolderEntryChanged>
{
    private const int MIN_WORDS_GENERATE_NAME = 50;
    private const int SAMPLE_LENGHT_GENERATE_NAME = 500;
    private readonly IDocumentService _documentService;
    private readonly Document _document;
    private readonly IEditorContract _editorControl;
    private DateTime _lastSaveTime;
    private bool _untitled;

    public string Name { get => _document.Name; }
    public DocumentStatus Status { get; set; }
    public bool CanSave => Status != DocumentStatus.Saved;
    public DateTime LastSaveTime { get => _lastSaveTime; }
    public string? PreviousContent { get; set; }
    public string? NextContent { get; set; }

    public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand AutosaveCommand { get; }
    public IRelayCommand RenameCommand { get; }

    public DocumentViewModel(Document document, IEditorContract editorControl) { ... }

    public void NameChanged() { ... }
    public void Receive(FolderEntryChanged message) { ... }

    private async Task Save() { ... }
    private async Task Autosave() { ... }
    private void Rename(string? newName) { ... }
    private async Task GenerateName() { ... }
}
```

##### Constantes:

- `MIN_WORDS_GENERATE_NAME`: Número mínimo de palabras requeridas en el documento para habilitar la generación automática de nombre. Valor: 50.
- `SAMPLE_LENGHT_GENERATE_NAME`: Longitud máxima de caracteres del contenido que se toma como muestra para generar el nombre automático. Valor: 500.

##### Propiedades observables:

- `PreviousContent`: Contenido anterior del documento, permite navegación o deshacer cambios.
- `NextContent`: Contenido siguiente del documento, permite rehacer cambios.

##### Otras propiedades:

- `_documentService`: Servicio para operaciones sobre documentos (guardar, cargar, etc.).
- `_document`: Instancia del documento gestionado (modelo subyacente).
- `_editorControl`: Referencia al control/editor asociado al documento.
- `_untitled`: Indica si el documento aún no ha sido titulado (nuevo o sin nombre definido).
- `Name`: Nombre actual del documento.
- `Status`: Estado del documento (ej. sin guardar, guardado).
- `CanSave`: Indica si el documento puede ser guardado.
- `LastSaveTime`: Fecha y hora de la última vez que se guardó el documento.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Comandos:

- `SaveCommand`: Comando asíncrono para guardar el documento.
- `AutosaveCommand`: Comando asíncrono para autoguardado del documento.
- `RenameCommand`: Comando para renombrar el documento.

##### Métodos públicos:

- `NameChanged()`: Notifica que el nombre del documento ha cambiado y actualiza la propiedad correspondiente.
- `Receive(FolderEntryChanged message)`: Maneja la recepción de mensajes de cambio en el documento, actualizando el nombre si procede.

##### Otros métodos relevantes:

- `GenerateName()`: Genera un nombre automático para el documento basado en su contenido.

##### Notas adicionales:

- El nombre del documento puede ser generado automáticamente si cumple ciertos criterios de contenido (al menos 50 palabras). Para ello se invoca a `NameGeneratorHelper`.
- Utiliza el patrón Messenger para sincronizar cambios de nombre y eliminación entre diferentes instancias del ViewModel.

#### DraftDocumentViewModel

##### Descripción general:

ViewModel que representa un documento borrador, gestionando su contenido actual y permitiendo la navegación entre versiones previas y siguientes del contenido. Similar a `DocumentViewModel` pero simplificado para su gestión sin persistencia en la ventana de edición rápida.

##### Código simplificado:

```csharp
public class DraftDocumentViewModel : ObservableObject
{
    public string? Content { get; set; }
    public string? PreviousContent { get; set; }
    public string? NextContent { get; set; }
}
```

##### Propiedades observables:

- `Content`: Contenido actual del documento borrador.
- `PreviousContent`: Contenido anterior del documento borrador, útil para deshacer cambios o navegar hacia atrás.
- `NextContent`: Contenido siguiente del documento borrador, útil para rehacer cambios o navegar hacia adelante.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

### E.1.5. Gestión de la configuración

#### AIServiceConfigViewModel

##### Descripción general:

ViewModel encargado de gestionar la configuración de un servicio de IA, incluyendo la URL base, la clave de API y el estado de conexión. Permite notificar cambios de configuración y estado, así como probar la conexión con el servicio de IA. Es utilizado en la capa de presentación para enlazar la configuración editable y su estado en la interfaz de usuario.

##### Código simplificado:

```csharp
public class AIServiceConfigViewModel : ObservableObject
{
    private readonly AIServiceConfig _aiServiceConfig;

    public AIServiceConfigViewModel(string? baseUrl, string? key) : this(new() { BaseUrl = baseUrl, Key = key }) { ... }

    public string? BaseUrl
        { get => _aiServiceConfig.BaseUrl; set => SetProperty(...); }
    public string? Key
        { get => _aiServiceConfig.Key; set => SetProperty(...); }

    public ServiceStatus ServiceStatus { get; private set; }
    public string? ErrorMessage { get; private set; }

    public event EventHandler<PropertyChangedEventArgs>? ConfigChanged;
    public event EventHandler? StatusChanged;

    public AIServiceConfig GetRecord() => _aiServiceConfig;
    public async Task TestConnection(IAIService aiService) { ... }
    public void ResetStatus() { ... }
    public void SetErrorStatus(string message) { ... }
}
```

##### Propiedades observables:

- No hay propiedades marcadas explícitamente con `[ObservableProperty]`.

##### Otras propiedades:

- `_aiServiceConfig`: Instancia interna que almacena la configuración real.
- `BaseUrl`: URL base del servicio de IA.
- `Key`: Clave de API para el servicio de IA.
- `ServiceStatus`: Estado actual del servicio de IA (no serializable).
- `ErrorMessage`: Mensaje de error relacionado con el servicio de IA (no serializable).

##### Eventos:

- `ConfigChanged`: Se dispara cuando cambian las propiedades de configuración.
- `StatusChanged`: Se dispara cuando cambia el estado del servicio o el mensaje de error.
- Al heredar de `ObservableObject`, notifica cambios en algunas propiedades públicas modificadas explícitamente.

##### Métodos públicos:

- `AIServiceConfig GetRecord()`: Devuelve la configuración actual del servicio de IA.
- `Task TestConnection(IAIService aiService)`: Prueba la conexión con el servicio de IA y actualiza el estado y mensaje de error.
- `void ResetStatus()`: Restablece el estado del servicio a desconocido y limpia el mensaje de error.
- `void SetErrorStatus(string message)`: Establece el estado del servicio como error y asigna un mensaje de error.

##### Notas adicionales:

- Las propiedades `ServiceStatus` y `ErrorMessage` están marcadas con `[JsonIgnore]` para evitar su persistencia.
- Posee un constructor específico para deserialización decorado con `[JsonConstructor]`.

#### SettingsViewModel

##### Descripción general:

ViewModel responsable de gestionar la configuración de la aplicación, incluyendo tanto los ajustes generales como la configuración de modelos de IA. Permite comprobar la disponibilidad de los servicios de IA y sincroniza los cambios de configuración con el almacén de configuración.

##### Código simplificado:

```csharp
public class SettingsViewModel : ObservableObject
{
    private readonly IConfigStore _configStore;

    public GeneralSettingsViewModel General { get; private init; }
    public ModelsSettingsViewModel Models { get; private init; }
    public bool? IsAIAvailable { get; set; }

    public SettingsViewModel() { ... }
    public async Task TestConnections() { ... }

    private void UpdateAIAvailability( ... ) { ... }
}
```

##### Propiedades observables:

- `IsAIAvailable`: Indica si los servicios de IA están disponibles actualmente en función de la configuración y los modelos habilitados.

##### Otras propiedades:

- `_configStore`: Referencia al almacén de configuración utilizado para persistir los cambios.
- `General`: ViewModel de configuración general de la aplicación.
- `Models`: ViewModel de configuración de modelos de IA.

##### Eventos:

- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Métodos públicos:

- `Task TestConnections()`: Prueba la conexión de todos los servicios de IA habilitados y actualiza su estado.

##### Otros métodos relevantes:

- `void UpdateAIAvailability()`: Actualiza la propiedad `IsAIAvailable` y el modelo por defecto según la disponibilidad de los modelos y proveedores de IA configurados.

##### Notas adicionales:

- Durante su construcción se realiza la carga de la configuración general y de modelos a partir del almacén de configuración, y se suscribe a los eventos necesarios para la sincronización y actualización de disponibilidad de IA.

#### GeneralSettingsViewModel

##### Descripción general:

ViewModel encargado de gestionar la configuración general de la aplicación, incluyendo la activación y configuración de servicios de IA (Ollama, AzureAI, OpenAI), preferencias de interfaz de usuario y otros ajustes globales. Permite la inicialización y control dinámico de los servicios de IA, así como la gestión de los proveedores de modelos disponibles.

##### Código simplificado:

```csharp
public class GeneralSettingsViewModel : ObservableObject
{
    public bool OllamaEnabled { get; set; }
    public bool AzureAIEnabled { get; set; }
    public bool OpenAIEnabled { get; set; }
    public bool OllamaAutostart { get; set; }
    public required AIServiceConfigViewModel OllamaConfig { get; init; }
    public required AIServiceConfigViewModel AzureAIConfig { get; init; }
    public required AIServiceConfigViewModel OpenAIConfig { get; init; }
    public required ObservableCollection<ModelProvider> AvailableProviders { get; init; }
    public ApplicationTheme? AppTheme { get; set; }
    public bool AcrylicBackground { get; set; }
    public string? AgentPrompt { get; set; }
    public bool EnableHotKeys { get; set; }

    public event EventHandler? ProviderAvailabilityChanged;
    public event EventHandler? ServiceEnablementChanged;

    public void InitializeAIServices() { ... }
    public void OnAcrylicBackgroundChanging(bool value) { ... }
    public void OnEnableHotKeysChanging(bool value) { ... }
    public void OnOllamaEnabledChanged(bool value) { ... }
    public void OnAzureAIEnabledChanged(bool value) { ... }
    public void OnOpenAIEnabledChanged(bool value) { ... }

    private void ServiceStatusChanged( ... ) { ... }
    private void ServiceConfigChanged( ... ) { ... }
    private void AvailableProvidersCollectionChangedHandler( ... ) { ... }
    private static void SetServiceConfig(bool enabled, AIServiceConfigViewModel config, ModelProvider modelProvider, bool keyIsRequired) { ... }
}
```

##### Propiedades observables:

- `OllamaEnabled`: Indica si el servicio de IA Ollama está habilitado.
- `AzureAIEnabled`: Indica si el servicio de IA Azure está habilitado.
- `OpenAIEnabled`: Indica si el servicio de IA OpenAI está habilitado.
- `OllamaAutostart`: Indica si el servicio Ollama debe iniciarse automáticamente.
- `OllamaConfig`: Configuración del servicio Ollama.
- `AzureAIConfig`: Configuración del servicio AzureAI.
- `OpenAIConfig`: Configuración del servicio OpenAI.
- `AvailableProviders`: Colección de proveedores de modelos IA disponibles.
- `AppTheme`: Tema de la aplicación (claro u oscuro).
- `AcrylicBackground`: Indica si el fondo acrílico (translúcido) está habilitado.
- `AgentPrompt`: Texto del prompt del agente.
- `EnableHotKeys`: Indica si los atajos de teclado están habilitados.

##### Eventos:

- `ProviderAvailabilityChanged`: Se dispara cuando cambia la disponibilidad de proveedores.
- `ServiceEnablementChanged`: Se dispara cuando cambia el estado de habilitación de un servicio.
- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Métodos públicos:

- `InitializeAIServices()`: Inicializa los servicios de IA según su configuración y estado de habilitación.

##### Otros métodos relevantes:

- `OnAcrylicBackgroundChanging(bool value)`: Gestiona cambios en el fondo de la interfaz.
- `OnEnableHotKeysChanging(bool value)`: Gestiona la activación de atajos de teclado.
- `OnOllamaEnabledChanged(bool value)`: Gestiona cambios en la habilitación de Ollama.
- `OnAzureAIEnabledChanged(bool value)`: Gestiona cambios en la habilitación de AzureAI.
- `OnOpenAIEnabledChanged(bool value)`: Gestiona cambios en la habilitación de OpenAI.
- `ServiceStatusChanged(object? sender)`: Gestiona los cambios de estado de los servicios de IA y actualiza la lista de proveedores disponibles.
- `ServiceConfigChanged(object? sender)`: Gestiona los cambios en la configuración de los servicios de IA.
- `SetServiceConfig(bool enabled, AIServiceConfigViewModel config, ModelProvider modelProvider, bool keyIsRequired)`: Configura un servicio de IA según su configuración.
- `AvailableProvidersCollectionChangedHandler(NotifyCollectionChangedEventArgs eventArgs)`: Gestiona los cambios en la colección de proveedores disponibles y notifica.

##### Notas adicionales:

- La inicialización de las propiedades `OllamaConfig`, `AzureAIConfig` y `OpenAIConfig` suscribe automáticamente los eventos de cambio de estado y configuración de cada servicio.

#### ModelsSettingsViewModel

##### Descripción general:

Es el ViewModel encargado de almacenar y gestionar la configuración de los modelos de IA en la aplicación. Permite seleccionar el modelo por defecto, definir parámetros predeterminados, controlar si se envían estos parámetros y gestionar la colección de modelos disponibles. Notifica cambios relevantes a través de eventos y mantiene sincronización con el servicio de chat.

##### Código simplificado:

```csharp
public class ModelsSettingsViewModel : ObservableObject
{
    public AIModelViewModel? DefaultModel { get; set; }
    public bool SendDefaultParameters { get; set; }

    public required AIParametersViewModel DefaultParameters { get; init; }
    public required ObservableCollection<AIModelViewModel> AvailableModels { get; init; }

    public event EventHandler? ModelAvailabilityChanged;
    public event EventHandler? DefaultModelChanged;

    public void OnDefaultModelChanged(AIModelViewModel? value) { ... }
    public void OnSendDefaultParametersChanged(bool value) { ... }
    public void OnDefaultParametersChanged() { ... }
    public void AvailableModelsCollectionChangedHandler( ... ) { ... }
    public void AvailableModelsCollectionPropertyChangedHandler( ... ) { ... }
}
```

##### Propiedades observables:

- `DefaultModel`: Modelo de IA seleccionado como predeterminado.
- `SendDefaultParameters`: Indica si se deben enviar los parámetros predeterminados al servicio de chat.
- `DefaultParameters`: Parámetros predeterminados para el modelo de IA.
- `AvailableModels`: Colección de modelos de IA disponibles.

##### Eventos:

- `ModelAvailabilityChanged`: Se dispara cuando cambia la disponibilidad de modelos.
- `DefaultModelChanged`: Se dispara cuando cambia el modelo predeterminado.
- Al heredar de `ObservableObject`, notifica cambios en todas las propiedades observables.

##### Métodos públicos:

- `OnDefaultModelChanged(AIModelViewModel? value)`: Se ejecuta cuando cambia el modelo predeterminado; actualiza el servicio de chat y lanza el evento correspondiente.
- `OnSendDefaultParametersChanged(bool value)`: Se ejecuta al cambiar la propiedad `SendDefaultParameters`; actualiza los parámetros en el servicio de chat.
- `OnDefaultParametersChanged()`: Actualiza los parámetros predeterminados en el servicio de chat.
- `AvailableModelsCollectionChangedHandler(NotifyCollectionChangedEventArgs eventArgs)`: Gestiona los cambios en la colección de modelos disponibles, suscribiendo o desuscribiendo a eventos de cambio de propiedad y notificando la disponibilidad.
- `AvailableModelsCollectionPropertyChangedHandler(PropertyChangedEventArgs eventArgs)`: Gestiona los cambios de propiedades dentro de los modelos disponibles, notificando si cambia la propiedad `Enabled`.

##### Notas adicionales:

- El ViewModel mantiene la sincronización con el servicio de chat (`IChatService`) al cambiar modelos o parámetros.

## E.4 Mensajes de comunicación

De forma complementaria a los ViewModels, la mensajería permiten la comunicación desacoplada entre componentes que no están relacionados directamente, permitiendo que ViewModels o controles del interfaz respondan de forma coordinada a cambios en el modelo. Ambos elementos permiten que el interfaz sea dinámico y reactivo, mejorando la experiencia del usuario.

#### FolderEntryCreated

##### Descripción general:

Clase que representa un mensaje utilizado para notificar la creación de una nueva entrada de carpeta. Se emplea para comunicar a otras partes de la aplicación que se ha creado una nueva instancia de FolderEntryViewModel.

##### Código simplificado:

```csharp
public class FolderEntryCreated : ValueChangedMessage<FolderEntryViewModel>
{
    public FolderEntryCreated(FolderEntryViewModel value) : base(value) { }
}
```

##### Propiedades principales:

- `Value`: (heredada de `ValueChangedMessage<T>`) Contiene la instancia de `FolderEntryViewModel` que ha sido creada.

##### Métodos públicos:

- `FolderEntryCreated(FolderEntryViewModel value)`: Constructor que inicializa el mensaje con la entrada de carpeta creada.

##### Notas adicionales:

- Esta clase es inmutable y se utiliza exclusivamente como contenedor de datos para el sistema de mensajería.

#### FolderEntryChanged

##### Descripción general:

Representa un mensaje que indica que una entrada de carpeta ha sido modificada. Se utiliza para notificar cambios en una instancia de `IFolderEntry`, permitiendo especificar si el nombre de la entrada ha cambiado.

##### Código simplificado:

```csharp
public class FolderEntryChanged : ValueChangedMessage<IFolderEntry>
{
    public bool NameChanged { get; set; }
    public FolderEntryChanged(IFolderEntry value) : base(value) { ... }
}
```

##### Propiedades principales:

- `NameChanged`: Indica si el nombre de la entrada de carpeta ha cambiado.
- `Value`: (heredada de `ValueChangedMessage<T>`) Contiene la instancia de `IFolderEntry` que ha sido modificada.

##### Métodos públicos:

- `FolderEntryChanged(IFolderEntry value)`: Constructor que inicializa el mensaje con la entrada de carpeta modificada.

##### Notas adicionales:

- El uso del booleano `NameChanged` está pensado para posibles ampliaciones donde se notifiquen otros cambios distintos al nombre.

#### FolderEntryDeleted

##### Descripción general:

Clase que representa un mensaje utilizado para notificar que una entrada de carpeta ha sido eliminada. Se emplea para informar sobre cambios en la colección de entradas de carpetas, transmitiendo la entrada eliminada como valor.

##### Código simplificado:

```csharp
public class FolderEntryDeleted : ValueChangedMessage<IFolderEntry>
{
    public FolderEntryDeleted(IFolderEntry value) : base(value) { }
}
```

##### Propiedades principales:

- `Value`: (heredada de `ValueChangedMessage<T>`) Contiene la instancia de `IFolderEntry` que ha sido eliminada.

##### Métodos públicos:

- `FolderEntryDeleted(IFolderEntry value)`: Constructor que inicializa el mensaje con la entrada de carpeta eliminada.