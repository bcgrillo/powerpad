Genérame ahora el contenido inicial para este apartado: 4.3 Definición de Componentes de Desarrollo

Los componentes serán:
- Entidades (debemos explicar qué son, cómo estan definidas - schemas json -, cómo almacenamos sus instancias - a las que llamaremos documentos - en colecciones dentro de BBDD no SQL. Ejemplo de entidades, introducción al concepto dominio como conjunto de entidades intimamente relacionadas. Adicionalmente un subapartado podría hablar de entidad débil, dar un ejemplo, e indicar no tienen colecciones propias sino que siempre van almacenadas dentro de un documento principal)
Además de un esquema, una entidad tendrá siempre un nombre, una versión y un estado. Los estados pueden ser distintos para distintas entidades. Los estados son importantes ya que pueden afecta a cómo funcionan las validaciones o las reglas de negocio.

- Reglas de negocio: Distinguiremos: validaciones básicas, que van definidas en el propio esquema de la entidad (ejemplo, un empleado siempre tiene Nombre, Apellidos, DNI válido...), que las validaciones complejas, que son aquellas que dependen un estado (como por ejemplo, un empleado dado de baja debe tener una fecha de baja, mientras que uno en activo, no puede tenerla). También puede haber reglas de negocio más complejas que requieren cálculo o consultas incluso en otras entidades de la solución. En este último caso, la diferencia principal entre un flujo y una regla de negocio - en este sistema -, es que las reglas de negocio se validan antes de realizar una operación, pudiendo impedir que la operación se realize, como una transacción. Su ejecución es sincrona.

- Flujos de trabajo: Los flujos de trabajo incluyen aquellos procesos o lógica de negocio que se realizan de forma intependiente unas a otras. Un flujo de trabajo tiene un disparador, que puede ser un evento del sistema (la creación de un documento, una tarea programada...) o una ejecución a petición de un usuario. Los flujos de trabajo pueden disparar otros flujos, o realizar operaciones que contemplen reglas de negocio que impidan la ejecución del flujo. Sin embargo, si una operación dispara un flujo de trabajo, esa operación ya se ha realizado, por lo que no puede condicionarla (esa es la diferencia principal con una regla de negocio compleja).
Los flujos de trabajo tienen estados y trazabilidad dentro del sistema, y pueden ser consultados por los usuarios. Su ejecución se realiza de forma asíncrona.

- Interfaces de usuario: Existen interfaces de usuario de varios tipos, estos son los principales contemplados dentro del marco:
* Exploradores: Son aquellas interfaces que permiten recuperar listados de documentos, filtrarlos, ordenarlos. También dan acceso a operaciones de gestión de los documentos, como la creación, modificación o eliminación de los mismos. Los Exploradores permiten el uso de consultas, que son plantillas que permiten mostrar los documentos de una forma determinada.
* Formularios: Son interfaces que permiten la visualización, creación o modificación de un documento.
* Diálogos: Son interfaces que permiten interactuar con el usuario durante la ejecución de un flujo de trabajo. Pueden ser de varios tipos, como confirmación, selección de opciones, introducción de datos.

- Consultas: Las consultas son plantillas que permiten recuperar documentos de una colección. Las consultas pueden ser de varios tipos, como listados, agrupaciones, resúmenes, etc. Las consultas pueden ser utilizadas en los exploradores, en los flujos de trabajo, o en las interfaces de usuario. Las consultas están formadas por un conjunto de filtros, ordenaciones y agrupaciones. Además, pueden utilizar parámetros, que son valores que se pasan a la consulta de forma dinámica (por ejemplo, el usuario que ejecuta la consulta, o la fecha actual).

- Informes: Existen también distintos tipos que acaban desde formatos para la impresión (como una factura), listados de documentos, hasta cuadros de mando, que permiten visualizar la información de una forma determinada. Lo que tienen en común es que se basan en consultas, y que pueden ser exportados a distintos formatos. Los informes pueden recibir parámetros que son enviados a las consultas que los generan.


----


### 4.3 Definición de Componentes de Desarrollo

El **GenCode Framework** organiza sus funcionalidades en un conjunto de componentes clave, diseñados para garantizar una arquitectura modular, escalable y fácilmente mantenible. A continuación, se detallan estos componentes, destacando sus principales características y su rol dentro del proceso de desarrollo.

* * * * *

### 4.3.1 Entidades

Las **entidades** representan el núcleo estructural del sistema, actuando como modelos que definen los datos del dominio. Estas son definidas a través de esquemas JSON y almacenadas en bases de datos NoSQL bajo el concepto de **documentos**.

**Definición y estructura:**

-   El esquema de cada entidad especifica los atributos principales (nombre, tipo de dato, restricciones) y validaciones básicas.
-   Las instancias de las entidades se representan como documentos y se agrupan en colecciones dentro de la base de datos NoSQL, permitiendo flexibilidad y escalabilidad en la gestión de datos.

**Dominio del sistema:**

-   Un **dominio** se entiende como un conjunto de entidades estrechamente relacionadas. Por ejemplo, el dominio de "Recursos Humanos" puede incluir entidades como "Empleado", "Departamento" y "Contrato", cada una de ellas con relaciones que modelan el comportamiento del área.

**Entidades débiles:**

-   Algunas entidades no son independientes y dependen de una entidad principal. A estas se les denomina **entidades débiles**.
    -   Ejemplo: El "Detalle de línea" de una factura no se almacena en su propia colección, sino dentro del documento principal de la factura.

**Atributos adicionales:**

-   Cada entidad incluye:
    -   **Nombre**: Identificador único dentro del sistema.
    -   **Versión**: Indicador del control evolutivo del esquema.
    -   **Estado**: Determina el comportamiento y las reglas aplicables a la entidad.

**Importancia del estado:**

-   Los estados afectan la validación y las reglas de negocio. Por ejemplo, una entidad "Pedido" puede tener estados como "Pendiente", "Enviado" o "Cancelado", y cada uno desencadenará validaciones o flujos específicos.

* * * * *

### 4.3.2 Reglas de negocio

Las **reglas de negocio** son responsables de garantizar que las operaciones y datos procesados cumplan con los requisitos de la aplicación, estableciendo restricciones que aseguran la integridad del sistema.

**Clasificación de las reglas:**

1.  **Validaciones básicas:** Definidas directamente en el esquema JSON de la entidad.

    -   Ejemplo: Un "Empleado" siempre debe tener un "Nombre", "Apellidos" y un "DNI" válido.
2.  **Validaciones complejas:** Dependientes del estado de la entidad.

    -   Ejemplo: Si un "Empleado" está en estado "Dado de baja", debe tener una fecha de baja registrada; si está "Activo", esta fecha no puede estar presente.
3.  **Reglas avanzadas:** Involucran cálculos o consultas a otras entidades.

    -   Ejemplo: Una regla que verifique la disponibilidad de stock antes de confirmar un pedido.

**Sincronización y transacciones:**

-   Las reglas de negocio son ejecutadas de manera sincrónica y pueden bloquear operaciones si no se cumplen las condiciones establecidas, garantizando la coherencia del sistema mediante el uso de transacciones.

* * * * *

### 4.3.3 Flujos de trabajo

Los **flujos de trabajo** son procesos automatizados que ejecutan lógica de negocio en respuesta a eventos del sistema o interacciones del usuario.

**Disparadores:**

-   Los flujos de trabajo pueden ser activados por:
    -   Eventos del sistema, como la creación de un documento o una tarea programada.
    -   Peticiones explícitas del usuario.

**Interacción con reglas de negocio:**

-   Los flujos de trabajo pueden invocar reglas de negocio, pero su ejecución es asíncrona, lo que significa que las reglas no bloquean la operación inicial que desencadenó el flujo.

**Estados y trazabilidad:**

-   Cada flujo de trabajo tiene un estado (por ejemplo, "En proceso", "Completado") y un registro histórico accesible para los usuarios.

* * * * *

### 4.3.4 Interfaces de usuario

El sistema ofrece distintas interfaces adaptadas a los diversos roles y necesidades de los usuarios, facilitando la interacción con las entidades y flujos del sistema.

1.  **Exploradores:**

    -   Permiten buscar, filtrar y gestionar documentos mediante consultas específicas.
    -   Incluyen funcionalidades como la creación, edición y eliminación de documentos.
2.  **Formularios:**

    -   Se utilizan para la visualización y modificación de documentos.
    -   Permiten la entrada y validación de datos por parte del usuario.
3.  **Diálogos:**

    -   Interacciones breves dentro de un flujo de trabajo, como la confirmación de acciones, selección de opciones o ingreso de datos adicionales.

* * * * *

### 4.3.5 Consultas

Las **consultas** son plantillas configurables que recuperan y procesan documentos almacenados en las colecciones del sistema.

**Tipos de consultas:**

-   **Listados básicos:** Devuelven un conjunto de documentos basado en filtros simples.
-   **Consultas de agrupación:** Resumen datos agrupándolos por criterios específicos.
-   **Resúmenes y reportes:** Extraen información detallada o consolidada.

**Parámetros dinámicos:**

-   Las consultas pueden aceptar parámetros dinámicos (como la fecha actual o el usuario que ejecuta la consulta), generando resultados personalizados.

* * * * *

### 4.3.6 Informes

Los **informes** presentan la información procesada por las consultas de manera estructurada, con el objetivo de ofrecer al usuario una visión clara y accesible de los datos.

**Tipos de informes:**

-   **Informes imprimibles:** Por ejemplo, facturas o recibos.
-   **Listados de documentos:** Para la revisión y análisis de datos en conjunto.
-   **Cuadros de mando:** Visualizan métricas clave y resultados analíticos.

**Exportación:**

-   Los informes pueden exportarse a formatos como PDF o Excel, según las necesidades del usuario.

* * * * *

### Consideraciones finales

Este enfoque modular proporciona flexibilidad y escalabilidad, permitiendo que cada componente pueda evolucionar de forma independiente o integrada, facilitando así la incorporación de nuevas funcionalidades sin comprometer la coherencia del sistema.