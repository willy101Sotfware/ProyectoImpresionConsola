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

            Graphics graphics = e.Graphics;

            // Definir fuentes
            Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
            Font fontBold = new Font("Arial", 10, FontStyle.Bold);
            Font fontRegular = new Font("Arial", 10, FontStyle.Regular);
            Font fontSmall = new Font("Arial", 8, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            // Definir márgenes y posiciones
            int xLeft = 50;
            int xRight = 350;
            int y = 50;
            int pageWidth = e.PageBounds.Width;

            // Imagen centrada (si existe)
            if (_logo != null)
            {
                int logoWidth = 150;
                int logoX = (pageWidth - logoWidth) / 2;
                graphics.DrawImage(_logo, new Rectangle(logoX, y, logoWidth, 50));
                y += 70;
            }

            // Línea separadora
            graphics.DrawString("==========================================", fontRegular, brush, xLeft, y);
            y += 20;

            // Encabezado de la transacción
            graphics.DrawString($"ID Transacción: {_transaction.TransactionId}", fontBold, brush, xLeft, y);
            graphics.DrawString($"Fecha Creación: {_transaction.DateCreated:dd/MM/yyyy HH:mm tt}", fontBold, brush, xRight, y);
            y += 25;

            // Hora actual
            graphics.DrawString($"Hora de Impresión: {DateTime.Now:dd/MM/yyyy HH:mm tt}", fontSmall, brush, xLeft, y);
            y += 25;

            // Línea separadora
            graphics.DrawString("==========================================", fontRegular, brush, xLeft, y);
            y += 20;

            // Detalles de la transacción
            DrawReceiptLine(graphics, fontBold, "Documento:", _transaction.Document, xLeft, xRight, ref y);
            DrawReceiptLine(graphics, fontBold, "Referencia:", _transaction.Reference, xLeft, xRight, ref y);
            DrawReceiptLine(graphics, fontBold, "Producto:", _transaction.Product, xLeft, xRight, ref y);

            // Línea separadora
            graphics.DrawString("==========================================", fontRegular, brush, xLeft, y);
            y += 20;

            // Montos
            DrawReceiptLine(graphics, fontBold, "Monto Total:", $"{_transaction.TotalAmount:C}", xLeft, xRight, ref y);
            DrawReceiptLine(graphics, fontBold, "Monto Real:", $"{_transaction.RealAmount:C}", xLeft, xRight, ref y);
            DrawReceiptLine(graphics, fontBold, "Monto Ingreso:", $"{_transaction.IncomeAmount:C}", xLeft, xRight, ref y);
            DrawReceiptLine(graphics, fontBold, "Monto Devolución:", $"{_transaction.ReturnAmount:C}", xLeft, xRight, ref y);

            // Línea separadora
            graphics.DrawString("==========================================", fontRegular, brush, xLeft, y);
            y += 20;

            // Estado y descripción
            DrawReceiptLine(graphics, fontBold, "Estado:", _transaction.StateTransaction, xLeft, xRight, ref y);
            DrawReceiptLine(graphics, fontBold, "Descripción:", _transaction.Description, xLeft, xRight, ref y);

            // Línea separadora
            graphics.DrawString("==========================================", fontRegular, brush, xLeft, y);
            y += 20;

            // Información adicional (hardcodeada por ahora)
            graphics.DrawString("Dirección: Calle Principal 123", fontSmall, brush, xLeft, y);
            y += 20;
            graphics.DrawString("Línea de Atención: 123-456-7890", fontSmall, brush, xLeft, y);
            y += 20;

            // Pie de página
            graphics.DrawString("Gracias por su transacción", fontSmall, brush, xLeft, y);
            y += 20;

            // E-city Software centrado y más grande
            Font fontBrand = new Font("Arial", 16, FontStyle.Bold);
            SizeF brandSize = graphics.MeasureString("E-city Software", fontBrand);
            int brandX = (int)((pageWidth - brandSize.Width) / 2);
            graphics.DrawString("E-city Software", fontBrand, brush, brandX, y);
            y += 40;

            // Indicar que no hay más páginas
            e.HasMorePages = false;

            // Cortar papel (simular corte)
            graphics.DrawLine(new Pen(Color.Black, 2), xLeft, y, xRight, y);
        }

        // Método auxiliar para dibujar líneas de recibo con clave-valor
        private void DrawReceiptLine(Graphics graphics, Font font, string key, string value, int xLeft, int xRight, ref int y)
        {
            graphics.DrawString(key, font, Brushes.Black, xLeft, y);
            graphics.DrawString(value, font, Brushes.Black, xRight, y);
            y += 25; // Incrementar la posición vertical
        }
    }
}