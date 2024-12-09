using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using PrinterSQLiteApp.Domain.Configuration;
using PrinterSQLiteApp.Domain.Entities;
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

        while (true)
        {
            try
            {
                Console.Write("Ingrese el IdApi (número de 5 dígitos): ");
                string input = Console.ReadLine() ?? string.Empty;

                if (!string.IsNullOrEmpty(input) && input.Length == 5 && int.TryParse(input, out int idApi))
                {
                    var transactions = transactionRepository.GetTransactionsByIdApi(idApi);
                    if (transactions != null && transactions.Any())
                    {
                        // Sort transactions by DateCreated to ensure order
                        var sortedTransactions = transactions.OrderBy(t => t.DateCreated).ToList();

                        // Mostrar detalles de las transacciones
                        Console.WriteLine("Detalles de las Transacciones:");
                        Console.WriteLine("-----------------------------");
                        foreach (var transaction in sortedTransactions)
                        {
                            Console.WriteLine($"ID Transacción: {transaction.TransactionId}");
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
                        }

                        // Preguntar si desea imprimir
                        Console.Write("¿Desea imprimir estas transacciones? (s/n): ");
                        string respuesta = Console.ReadLine()?.ToLower() ?? string.Empty;

                        if (respuesta == "s")
                        {
                            foreach (var transaction in sortedTransactions)
                            {
                                printerService.ImprimirRecibo(transaction);
                                Console.WriteLine($"Transacción {transaction.IdApi} impresa correctamente.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Impresión cancelada.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("No se encontraron transacciones para el IdApi proporcionado.");
                    }
                }
                else
                {
                    Console.WriteLine("Por favor, ingrese un número válido de 5 dígitos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}