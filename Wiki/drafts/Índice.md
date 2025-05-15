### **Índice de Contenidos**

1.  **Resumen / Abstract**
    -   1.1. Resumen (en español)
    -   1.2. Abstract (lo mismo en inglés)

2.  **Introducción**
    -   2.1. Contexto y Motivación del Proyecto
    -   2.2. Objetivos del Proyecto
    -   2.3. Metodología y Enfoque de Trabajo
    -   2.4. Estructura del Documento

3.  **Estado del Arte y Fundamentos Teóricos**
    -   3.1. Conceptos de IA Generativa
        -   3.1.1. Historia y Evolución de la IA Generativa
        -   3.1.2. Principios de Modelos Generativos (Transformers, etc.)
    -   3.2. Aplicaciones Conversacionales
    -   3.3. Revisión de Soluciones Existentes
        -   3.3.1. Herramientas para ejecutar LLMs localmente: Hollama, Enchanted, LM Studio, Jan, GPT4All, msty, LibreChat, Open Web-UI, llama.cpp, etc.
        -   3.3.2. Comparativa de enfoques, ventajas y limitaciones

4.  **Diseño y Arquitectura del Sistema**
    -   4.1. Requisitos funcionales y no funcionales
    -   4.2. Arquitectura general del sistema (cliente PWA/desktop, integración con Ollama, etc.)
    -   4.3. Diseño de la gestión de modelos y actualización automatizada
    -   4.4. Gestión de chats y almacenamiento local (base de datos)
    -   4.5. Funcionalidades adicionales de la interfaz
        -   4.5.1. Paleta de accesos directos a chat o asistentes (resumir, traducir, completar, formatear borradores, etc.)
        -   4.5.2. Funcionalidad de "notas inteligentes": edición y enriquecimiento de documentos Markdown mediante órdenes al modelo
    -   4.6. Opciones para la integración de documentos
        -   4.6.1. Enfoque simple (envío directo de texto)
        -   4.6.2. Propuesta de vectorización y búsqueda vectorial (RAG) para enviar solo los chunks relevantes

5.  **Implementación**
    -   5.1. Entorno y herramientas de desarrollo
    -   5.2. Desarrollo del cliente (PWA/WebAssembly o aplicación de escritorio)
    -   5.3. Integración y control de Ollama desde la aplicación
    -   5.4. Desarrollo de módulos para la gestión y descarga de modelos
    -   5.5. Pruebas unitarias, de integración y validación funcional

6.  **Benchmarking y Evaluación del Rendimiento**
    -   6.1. Diseño de escenarios de pruebas y selección de hardware (ej. equipos con diferentes GPU)
    -   6.2. Resultados de benchmarking: análisis comparativo en distintos entornos
    -   6.3. Discusión sobre escalabilidad y eficiencia de la solución

7.  **Impactos Sociales, Ambientales y Éticos** (5 pag.)
    -   7.1. Impacto social
    -   7.2. Impacto ambiental
    -   7.3. Responsabilidad Ética y Profesional
        -   7.3.1. Consideraciones éticas en el desarrollo y uso de LLMs
        -   7.3.2. Transparencia y responsabilidad en proyectos open source
    -   7.4. Protección de Datos y Privacidad

8.  **Conclusiones y Trabajos Futuros**
    -   9.1. Resumen de logros y aportes del proyecto
    -   9.2. Limitaciones encontradas y aprendizajes
    -   9.3. Propuestas de mejoras y líneas de investigación futura
        -   9.3.1. Sistemas RAG y vectorización
        -   9.3.2. Integración con el API del S.O.
        -   9.3.3. Extensión a otros entornos (android, MAC)

9.  **Referencias Bibliográficas**

10. **Anexos**
    -   10.1. Código fuente y documentación técnica complementaria
    -   10.2. Resultados detallados de benchmarking
    -   10.3. Diagramas, esquemas y documentación adicional