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

            if (_transaction == null)
            {
                Console.WriteLine("Error: No hay datos de transacción para imprimir.");
                return;
            }

            Graphics graphics = e.Graphics;

            // Fuentes
            Font fontTitle = new Font("Arial", 12, FontStyle.Bold);
            Font fontBold = new Font("Arial", 10, FontStyle.Bold);
            Font fontRegular = new Font("Arial", 10, FontStyle.Regular);
            Font fontSmall = new Font("Arial", 8, FontStyle.Regular);
            SolidBrush brush = new SolidBrush(Color.Black);

            // Márgenes y posiciones iniciales
            int xLeft = 10;
            int pageWidth = e.PageBounds.Width;
            int y = 20;
            int lineSpacing = 20;

            // Logo (centrado)
            if (_logo != null)
            {
                int logoWidth = 100;
                int logoX = pageWidth / 2 - (logoWidth / 2);
                graphics.DrawImage(_logo, logoX, y, logoWidth, 50);
                y += 60;
            }

            // Título
            graphics.DrawString("Recibo de Transacción", fontTitle, brush, pageWidth / 2 - 70, y);
            y += 30;

            // Separador
            DrawSeparator(graphics, fontRegular, xLeft, pageWidth, ref y, lineSpacing);

            // Sección de encabezado
            DrawReceiptLine(graphics, fontBold, "Recaudo", _transaction.Product ?? "N/A", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Fecha", _transaction.DateCreated.ToString("yyyy-MM-dd"), xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Hora", _transaction.DateCreated.ToString("HH:mm:ss"), xLeft, ref y, lineSpacing);

            DrawSeparator(graphics, fontRegular, xLeft, pageWidth, ref y, lineSpacing);

            // Sección de datos personales y transacción
            DrawReceiptLine(graphics, fontBold, "Nro. Documento", _transaction.Document ?? "N/A", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Nro. Transacción", _transaction.TransactionId.ToString(), xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Estado", _transaction.StateTransaction ?? "N/A", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Referencia", _transaction.Reference ?? "N/A", xLeft, ref y, lineSpacing);

            DrawSeparator(graphics, fontRegular, xLeft, pageWidth, ref y, lineSpacing);

            // Sección de montos
            DrawReceiptLine(graphics, fontBold, "Pago sin redondear", $"{_transaction.RealAmount:C}", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Pago redondeado", $"{_transaction.TotalAmount:C}", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Valor Ingresado", $"{_transaction.IncomeAmount:C}", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Valor Devuelto", $"{_transaction.ReturnAmount:C}", xLeft, ref y, lineSpacing);

            DrawSeparator(graphics, fontRegular, xLeft, pageWidth, ref y, lineSpacing);

            // Dirección y contacto
            DrawReceiptLine(graphics, fontBold, "Dirección", "Calle 20 Sur n° 23 a - 16", xLeft, ref y, lineSpacing);
            DrawReceiptLine(graphics, fontBold, "Línea de Atención", "(+57) 3204240394", xLeft, ref y, lineSpacing);

            DrawSeparator(graphics, fontRegular, xLeft, pageWidth, ref y, lineSpacing);

            // Descripción (ajustar texto largo)
            string description = _transaction.Description ?? "N/A";
            DrawMultilineText(graphics, fontRegular, description, xLeft, ref y, pageWidth - xLeft * 2);

            y += 20;

            // Nota
            graphics.DrawString("Recuerde siempre esperar la tirilla de soporte de su pago.", fontSmall, brush, xLeft, y);
            y += 20;
            graphics.DrawString("Es el único documento que lo respalda.", fontSmall, brush, xLeft, y);
            y += lineSpacing;

            // Pie de página
            Font fontBrand = new Font("Arial", 14, FontStyle.Bold);
            graphics.DrawString("E-city Software", fontBrand, brush, pageWidth / 2 - 70, y);
        }

        // Método auxiliar para dibujar líneas de recibo con clave-valor
        private void DrawReceiptLine(Graphics graphics, Font font, string key, string value, int xLeft, ref int y, int lineSpacing)
        {
            graphics.DrawString($"{key}:", font, Brushes.Black, xLeft, y);
            graphics.DrawString(value, font, Brushes.Black, xLeft + 150, y); // Ajustar posición del valor
            y += lineSpacing;
        }


        private void DrawMultilineText(Graphics graphics, Font font, string text, int xLeft, ref int y, int maxWidth)
        {
            // Divide el texto en líneas que se ajustan al ancho máximo
            string[] words = text.Split(' ');
            string currentLine = "";
            foreach (var word in words)
            {
                string testLine = currentLine + word + " ";
                SizeF size = graphics.MeasureString(testLine, font);
                if (size.Width > maxWidth)
                {
                    graphics.DrawString(currentLine, font, Brushes.Black, xLeft, y);
                    y += 15; // Espaciado entre líneas
                    currentLine = word + " ";
                }
                else
                {
                    currentLine = testLine;
                }
            }
            if (!string.IsNullOrEmpty(currentLine))
            {
                graphics.DrawString(currentLine, font, Brushes.Black, xLeft, y);
                y += 15;
            }
        }

        private void DrawSeparator(Graphics graphics, Font font, int xLeft, int pageWidth, ref int y, int lineSpacing)
        {
            string separator = new string('=', (pageWidth - xLeft * 2) / 7); // Ajusta el tamaño del separador
            graphics.DrawString(separator, font, Brushes.Black, xLeft, y);
            y += lineSpacing;
        }

    }
}