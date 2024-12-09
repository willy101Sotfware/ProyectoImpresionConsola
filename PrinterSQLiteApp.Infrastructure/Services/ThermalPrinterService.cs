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
            Console.WriteLine("ImprimirRecibo llamado para la transacción ID: " + transaction.TransactionId);

            // Inspecciona los datos de la transacción
            Console.WriteLine($"Datos de transacción: {JsonSerializer.Serialize(transaction, new JsonSerializerOptions { WriteIndented = true })}");

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
            Console.WriteLine("PrintPage llamado");

            if (_transaction == null)
            {
                Console.WriteLine("Error: No hay datos de transacción para imprimir.");
                return;
            }

            Graphics graphics = e.Graphics;

            // Definir fuentes
            Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
            Font fontBold = new Font("Arial", 10, FontStyle.Bold);
            Font fontRegular = new Font("Arial", 10, FontStyle.Regular);
            Font fontSmall = new Font("Arial", 8, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            // Definir márgenes y posiciones
            int xLeft = 10; // Margen izquierdo
            int y = 20;     // Margen superior inicial

            // Imagen centrada (si existe)
            if (_logo != null)
            {
                int logoWidth = 150;
                int logoX = (e.PageBounds.Width - logoWidth) / 2;
                graphics.DrawImage(_logo, logoX, y, logoWidth, 50);
                y += 70; // Espacio después del logo
            }

            // Título
            graphics.DrawString("Recibo de Transacción", fontTitle, brush, xLeft, y);
            y += 30;

            // Línea separadora
            graphics.DrawString("------------------------------------------", fontRegular, brush, xLeft, y);
            y += 20;

            // Encabezado de la transacción
            graphics.DrawString($"ID Transacción: {_transaction.TransactionId}", fontBold, brush, xLeft, y);
            y += 20;
            graphics.DrawString($"Fecha Creación: {_transaction.DateCreated:dd/MM/yyyy HH:mm tt}", fontRegular, brush, xLeft, y);
            y += 20;

            // Detalles de la transacción
            DrawReceiptLine(graphics, fontBold, "Documento:", _transaction.Document ?? "N/A", xLeft, ref y);
            DrawReceiptLine(graphics, fontBold, "Referencia:", _transaction.Reference ?? "N/A", xLeft, ref y);
            DrawReceiptLine(graphics, fontBold, "Producto:", _transaction.Product ?? "N/A", xLeft, ref y);

            // Montos
            DrawReceiptLine(graphics, fontBold, "Monto Total:", $"{_transaction.TotalAmount:C}", xLeft, ref y);
            DrawReceiptLine(graphics, fontBold, "Monto Real:", $"{_transaction.RealAmount:C}", xLeft, ref y);
            DrawReceiptLine(graphics, fontBold, "Monto Ingreso:", $"{_transaction.IncomeAmount:C}", xLeft, ref y);
            DrawReceiptLine(graphics, fontBold, "Monto Devolución:", $"{_transaction.ReturnAmount:C}", xLeft, ref y);

            // Estado y descripción
            DrawReceiptLine(graphics, fontBold, "Estado:", _transaction.StateTransaction ?? "N/A", xLeft, ref y);
            DrawReceiptLine(graphics, fontBold, "Descripción:", _transaction.Description ?? "N/A", xLeft, ref y);

            // Línea separadora final
            graphics.DrawString("------------------------------------------", fontRegular, brush, xLeft, y);
            y += 20;

            // Información adicional
            graphics.DrawString("Dirección: Calle Principal 123", fontSmall, brush, xLeft, y);
            y += 20;
            graphics.DrawString("Línea de Atención: 123-456-7890", fontSmall, brush, xLeft, y);
            y += 20;

            // Pie de página
            graphics.DrawString("Gracias por su transacción", fontSmall, brush, xLeft, y);
            y += 20;

            // Marca
            Font fontBrand = new Font("Arial", 16, FontStyle.Bold);
            graphics.DrawString("E-city Software", fontBrand, brush, xLeft, y);
        }

        // Método auxiliar para dibujar líneas de recibo con clave-valor
        private void DrawReceiptLine(Graphics graphics, Font font, string key, string value, int xLeft, ref int y)
        {
            graphics.DrawString($"{key} {value}", font, Brushes.Black, xLeft, y);
            y += 20; // Incrementar posición vertical
        }
    }
}