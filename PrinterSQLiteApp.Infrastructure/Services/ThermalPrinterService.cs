using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text.Json;
using PrinterSQLiteApp.Domain.Entities;

namespace PrinterSQLiteApp.Infrastructure.Services
{
    public class PrinterConfig
    {
        public string? RutaDb { get; set; }
        public string? RutaImg { get; set; }
    }

    public class ThermalPrinterService
    {
        private readonly string? _rutaImg;
        private readonly PrintDocument _printDocument;
        private Image? _logo;
        private Transaction? _transaction;

        public ThermalPrinterService()
        {
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintPage;

            // Leer la ruta de la imagen desde el archivo config.json
            var config = LoadConfig("config.json");

            _rutaImg = config?.RutaImg;

            // Cargar la imagen del logo
            if (!string.IsNullOrEmpty(_rutaImg) && File.Exists(_rutaImg))
            {
                try
                {
                    _logo = Image.FromFile(_rutaImg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al cargar la imagen: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("No se encontró la imagen en la ruta especificada.");
            }
        }

        private PrinterConfig? LoadConfig(string path)
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<PrinterConfig>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al leer el archivo de configuración: {ex.Message}");
                return null;
            }
        }

        public void PrintTransaction(Transaction transaction)
        {
            ImprimirRecibo(transaction);
        }

        public void ImprimirRecibo(Transaction transaction)
        {
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

            try
            {
                _printDocument.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al imprimir el recibo: {ex.Message}");
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_transaction == null) return;

            var graphics = e.Graphics;
            var font = new Font("Arial", 10);
            var brush = Brushes.Black;
            int y = 50;
            int x = 50;

            // Logo
            if (_logo != null)
            {
                // Ajustar tamaño del logo si es necesario
                graphics.DrawImage(_logo, new Rectangle(x, y, 150, 50));
                y += 70;
            }

            // Encabezado del recibo
            graphics.DrawString("COMPROBANTE DE TRANSACCIÓN", new Font("Arial", 12, FontStyle.Bold), brush, x, y);
            y += 30;

            // Detalles de la transacción
            graphics.DrawString($"ID Transacción: {_transaction.TransactionId}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"ID API: {_transaction.IdApi}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Documento: {_transaction.Document}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Referencia: {_transaction.Reference}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Producto: {_transaction.Product}", font, brush, x, y);
            y += 20;

            // Montos
            graphics.DrawString($"Monto Total: {_transaction.TotalAmount:C}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Monto Real: {_transaction.RealAmount:C}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Monto Ingreso: {_transaction.IncomeAmount:C}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Monto Devolución: {_transaction.ReturnAmount:C}", font, brush, x, y);
            y += 20;

            // Estado y fechas
            graphics.DrawString($"Estado: {_transaction.StateTransaction}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Fecha Creación: {_transaction.DateCreated:g}", font, brush, x, y);
            y += 20;
            graphics.DrawString($"Fecha Actualización: {_transaction.DateUpdated:g}", font, brush, x, y);
            y += 20;

            // Descripción
            graphics.DrawString($"Descripción: {_transaction.Description}", font, brush, x, y);

            // Indicar que no hay más páginas
            e.HasMorePages = false;
        }
    }
}
