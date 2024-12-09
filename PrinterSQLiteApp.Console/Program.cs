using System;
using System.IO;
using System.Text.Json;
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
                        foreach (var transaction in transactions)
                        {
                            printerService.ImprimirRecibo();
                            Console.WriteLine($"Transacción {transaction.Id} impresa correctamente.");
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
