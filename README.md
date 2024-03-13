# Ejercicio

La idea es crear una API o una app Winform (a elección) para mantener una tabla de clientes (clásicoABM).

La estructura de los datos es:

● Id (numerico)
● Nombres (alfanumérico)
● Apellidos (alfanumérico)
● Fecha de Nacimiento (fecha)
● CUIT (alfanumérico)
● Domicilio (alfanumérico)
● Celular (alfanumérico)
● email (alfanumérico)

Para ello se pide crear una tabla en la base de datos y documentar los comandos para su creación yadjuntarlos en un archivo. Se puede utilizar cualquier motor de base de datos relacional (SQL).Los puntos a desarrollar son los siguientes:

Modalidad API:

  1. Crear la tabla y cargar algunos datos de prueba
  2. Crear la API y resolver la conexión a la DB
  3. Implementar los siguientes métodos:
     a. GetAll: Obtener todos los registros de la tabla.
     b. Get(Id): Obtener un registro por Id
     c. Search: Búsqueda por nombre
     d. Insert: Crear registros nuevos
     e. Update: Actualizar Registros
     f. Delete: Para eliminar registros (o marcarlos como eliminados si se prefiere)
Los endpoints se ejecutarán desde un cliente tipo Postman o similar.

Modalidad App Winform
  1. Crear la tabla y cargar algunos datos de prueba
  2. Crear los formularios que creas necesarios para la implementación del ABM y resolver laconexión a la DB
  3. Implementar los siguientes funcionalidades:
     a. Obtener todos los registros de la tabla.
     b. Búsqueda por nombre
     c. Crear registros nuevos
     d. Actualizar Registros
     e. Exportar datos en un archivo de texto.
Extras:Cualquier tipo de mejora es bienvenida y suma en la evaluación, como por ejemplo:
  ● Validar la unicidad de campo Id.
  ● Validar los datos ingresados.
  ● Obligatoriedad de ciertos datos ingresados (Nombre, Apellido, etc).
  ● Registrar Logs de errores o eventos.
  ● Elaborar documentación detallando el funcionamiento de la AP
