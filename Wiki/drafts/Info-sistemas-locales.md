La mayoría de las soluciones para ejecutar modelos de IA generativa en local se han construido sobre **llama.cpp**, un framework en C/C++ diseñado para correr modelos potentes---como LLaMA---en ordenadores domésticos sin depender de la nube. Esta base técnica ha permitido desarrollar múltiples herramientas que simplifican el uso de estos modelos, ofreciendo desde interfaces web hasta aplicaciones de escritorio que facilitan la interacción y gestión de la IA en el entorno local.

Evolución y Envoltorio de llama.cpp
-----------------------------------

Inicialmente, llama.cpp se creó para optimizar el uso de recursos en hardware modesto y hacer accesible la ejecución de grandes modelos de lenguaje. Sobre esta base surgieron soluciones como **Ollama**, que actúa como un "wrapper" de llama.cpp, proporcionando una gestión simplificada de modelos, una API de alto nivel y funcionalidades adicionales---como la capacidad de configurar un servidor local---que facilitan la integración con diversos clientes y entornos de usuario. De esta manera, Ollama permite a los usuarios enfocarse en la interacción con la IA sin tener que preocuparse por los detalles técnicos de bajo nivel

[vinlam.com](https://vinlam.com/posts/local-llm-options/)

.

Herramientas Web
----------------

Entre las opciones basadas en la web se encuentran varias interfaces que aprovechan los modelos ejecutados en local:

-   **Open WebUI**: Es una interfaz extensible y rica en funciones que ofrece integración con técnicas como Retrieval Augmented Generation (RAG) y web browsing. Funciona completamente offline y se conecta fácilmente a backends como Ollama u otros motores compatibles

    [openwebui.com](https://openwebui.com/)

    .
-   **Enchanted**: Un cliente nativo para macOS que se integra directamente con sistemas locales, ofreciendo una experiencia fluida en entornos Apple.
-   **Hollama, Lollms-Webui y LibreChat**: Estas herramientas proporcionan distintos niveles de personalización, desde interfaces minimalistas hasta entornos altamente configurables para interactuar con modelos locales.

Herramientas de Escritorio
--------------------------

En el ámbito de las aplicaciones de escritorio, existen soluciones que permiten tanto la ejecución como la interacción con los modelos basados en llama.cpp o sus derivados:

-   **GPT4All**: Ofrece una experiencia similar a ChatGPT, con una interfaz intuitiva que facilita la interacción con modelos de IA locales. Es ideal para usuarios que buscan una solución "todo en uno" sin complicaciones de configuración.
-   **Jan.ai**: Se presenta como una alternativa open source a LM Studio, con una UI limpia y eficiente. Jan.ai incluye un servidor ligero---basado en llama.cpp (a veces denominado "nitro")---que optimiza la inferencia local, y permite descargar modelos directamente desde repositorios como Hugging Face.
-   **LLStudio / Msty**: LLStudio es un entorno de escritorio completo para gestionar y ejecutar modelos locales, y **Msty** se posiciona a la par de LLStudio, ofreciendo funcionalidades similares. Ambas herramientas permiten utilizar cualquier modelo en formato GGUF, ya sea descargado de Hugging Face o desde la biblioteca de Ollama, y ofrecen una experiencia personalizable sin requerir configuraciones complejas.
-   **vLLM**: Se centra en ofrecer una inferencia de alta velocidad y eficiencia en el uso de memoria, aprovechando tanto la CPU como la GPU para tareas exigentes.

Paralelo con Microsoft Copilot
------------------------------

Por otro lado, Microsoft ha introducido **Copilot**, un asistente digital integrado en Windows y en diversas aplicaciones de Microsoft 365. Aunque actualmente no existe una versión completamente offline de Copilot---ya que muchas de sus funciones avanzadas requieren conectividad con la nube---su filosofía es similar a la de las herramientas locales: ofrecer ayuda integrada directamente en el entorno del usuario. Copilot responde preguntas, redacta textos, genera código e incluso crea imágenes, facilitando la productividad y la asistencia inmediata en el sistema operativo. Mientras las herramientas basadas en llama.cpp (como Ollama, Open WebUI, Jan.ai, LLStudio/Msty y vLLM) permiten un control total y privacidad al operar en local, Copilot apuesta por la integración en el sistema, facilitando una experiencia inmediata pero con dependencia de servidores externos.

Conclusión
----------

La evolución de llama.cpp ha permitido que tanto desarrolladores como usuarios finales puedan aprovechar modelos de IA generativa en sus ordenadores domésticos. Soluciones como Ollama simplifican y potencian esta experiencia, sirviendo de base para numerosas herramientas web (Open WebUI, Enchanted, Hollama, Lollms-Webui y LibreChat) y aplicaciones de escritorio (GPT4All, Jan.ai, LLStudio/Msty y vLLM). Por su parte, Microsoft Copilot representa una apuesta de integración en el sistema operativo, proporcionando asistencia directa a través de herramientas nativas, aunque requiera conexión a la nube para funciones avanzadas. Cada una de estas soluciones se adapta a diferentes necesidades: desde el control total y la privacidad de la IA local hasta la conveniencia y la integración de plataformas como Copilot, haciendo de la IA generativa una tecnología accesible y versátil para diversos escenarios de uso

[](https://vinlam.com/posts/local-llm-options/)