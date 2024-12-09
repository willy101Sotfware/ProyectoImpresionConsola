using System.Text.Json;
using PrinterSQLiteApp.Domain.Configuration;
using PrinterSQLiteApp.Domain.Interfaces;
using PrinterSQLiteApp.Infrastructure.Repositories;
using PrinterSQLiteApp.Infrastructure.Services;

class Program
{
    static void Main(string[] args)
    {
        string jsonConfig;

        try
        {
            string configPath = Path.Combine(AppContext.BaseDirectory, "config.json");
            jsonConfig = File.ReadAllText(configPath);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Error: No se pudo encontrar el archivo 'config.json'. Asegúrate de que esté en la carpeta correcta.");
            return;
        }

        var config = JsonSerializer.Deserialize<Config>(jsonConfig);

        if (config == null || string.IsNullOrEmpty(config.RutaDb))
        {
            Console.WriteLine("Error: La configuración no es válida o falta la propiedad 'RutaDb' en el archivo config.json.");
            return;
        }

        string connectionString = $"Data Source={config.RutaDb}";
        ITransactionRepository transactionRepository = new TransactionRepository(connectionString);
        var printerService = new ThermalPrinterService();

        try
        {
            Console.Write("Ingrese el IdApi (número de 5 dígitos): ");
            string input = Console.ReadLine() ?? string.Empty;

            if (!string.IsNullOrEmpty(input) && input.Length == 5 && int.TryParse(input, out int idApi))
            {
                var transactions = transactionRepository.GetTransactionsByIdApi(idApi);
                if (transactions != null && transactions.Any())
                {
                    var sortedTransactions = transactions.OrderBy(t => t.DateCreated).ToList();

                    Console.WriteLine("Detalles de las Transacciones:");
                    Console.WriteLine("-----------------------------");
                    foreach (var transaction in sortedTransactions)
                    {
                        Console.WriteLine($"ID API: {transaction.IdApi}");
                        Console.WriteLine($"Documento: {transaction.Document}");
                        Console.WriteLine($"Referencia: {transaction.Reference}");
                        Console.WriteLine($"Producto: {transaction.Product}");
                        Console.WriteLine($"Monto Total: {transaction.TotalAmount:C}");
                        Console.WriteLine($"Monto Real: {transaction.RealAmount:C}");
                        Console.WriteLine($"Monto Ingreso: {transaction.IncomeAmount:C}");
                        Console.WriteLine($"Monto Devolución: {transaction.ReturnAmount:C}");
                        Console.WriteLine($"Estado: {transaction.StateTransaction}");
                        Console.WriteLine($"Fecha Creación: {transaction.DateCreated:g}");
                        Console.WriteLine($"Fecha Actualización: {transaction.DateUpdated:g}");
                        Console.WriteLine($"Descripción: {transaction.Description}");
                        Console.WriteLine("-----------------------------");

                        // Imprimir automáticamente
                        try
                        {
                            printerService.ImprimirRecibo(transaction);
                            Console.WriteLine($"Transacción {transaction.IdApi} impresa correctamente.");
                            Thread.Sleep(1000); // Esperar 1 segundo entre impresiones
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error al imprimir: {ex.Message}");
                        }
                    }

                    // Esperar 3 segundos y cerrar
                    Thread.Sleep(3000);
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("No se encontraron transacciones para el IdApi proporcionado.");
                    Thread.Sleep(2000);
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("Por favor, ingrese un número válido de 5 dígitos.");
                Thread.Sleep(2000);
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Thread.Sleep(2000);
            Environment.Exit(0);
        }
    }
}