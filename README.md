# PrinterSQLiteApp

## Estructura del Proyecto

- **PrinterSQLiteApp.Domain**: Contiene las entidades y interfaces.
- **PrinterSQLiteApp.Infrastructure**: Implementaciones de repositorios y servicios.
- **PrinterSQLiteApp.Console**: Aplicación de consola principal.

## Componentes Principales

- **Transaction.cs**: Entidad que representa una transacción.
- **Config.cs**: Clase para manejar la configuración.
- **ITransactionRepository.cs**: Interfaz para el repositorio de transacciones.
- **TransactionRepository.cs**: Implementación del repositorio usando SQLite.
- **ThermalPrinterService.cs**: Servicio para imprimir recibos.
- **Program.cs**: Punto de entrada con la lógica principal.

## Funcionalidades

- Lectura de configuración desde `config.json`.
- Consulta de transacciones por `IdApi` (5 dígitos).
- Visualización de resultados en formato JSON.
- Impresión de recibos en impresora térmica.

## Dependencias Instaladas

- **System.Data.SQLite** (1.0.118)
- **System.Drawing.Common** (7.0.0)

## Instrucciones para Ejecutar la Aplicación

1. Asegúrate de que el archivo `config.json` esté en la carpeta de salida con la ruta correcta a la base de datos SQLite.
2. La base de datos debe estar en la ruta especificada (`C:\SQL_lite\Local.db`).
3. Ejecuta la aplicación y sigue las instrucciones en pantalla para ingresar el `IdApi`.
4. Cuando encuentre transacciones, te mostrará los detalles y te dará la opción de imprimir el recibo.

## Contacto

Para más información, puedes contactar a:

- **Email**: wruiz@e-city.co
