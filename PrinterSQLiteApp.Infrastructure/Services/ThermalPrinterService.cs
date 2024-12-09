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
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            ImprimirRecibo();
        }

        // Método que maneja el evento PrintPage para imprimir la imagen y los detalles del recibo
        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            if (_transaction == null)
            {
                Console.WriteLine("No hay transacción para imprimir.");
                e.HasMorePages = false;
                return;
            }

            // Configurar el margen
            float margenIzquierdo = e.MarginBounds.Left;
            float margenSuperior = e.MarginBounds.Top;

            // Dibujar la imagen (logo) si está cargada
            if (_logo != null)
            {
                e.Graphics.DrawImage(_logo, margenIzquierdo, margenSuperior, 120, 120); // Ajusta el tamaño del logo si es necesario
            }

            // Ajustar la posición del texto después de la imagen
            margenSuperior += 130; // Deja espacio suficiente después de la imagen

            // Ejemplo de contenido del recibo
            using (Font fontTitle = new Font("Arial", 14, FontStyle.Bold))
            using (Font fontText = new Font("Arial", 10))
            using (Brush brush = Brushes.Black)
            {
                e.Graphics.DrawString("Recibo de Transacción", fontTitle, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 30; // Mueve el cursor para la siguiente línea

                // Información de la transacción
                e.Graphics.DrawString($"ID Transacción: {_transaction.TransactionId}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"ID API: {_transaction.IdApi}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"Documento: {_transaction.Document}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"Referencia: {_transaction.Reference}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"Producto: {_transaction.Product}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"Monto Total: ${_transaction.TotalAmount:N2}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"Estado: {_transaction.StateTransaction}", fontText, brush, margenIzquierdo, margenSuperior);
                margenSuperior += 20;

                e.Graphics.DrawString($"Fecha: {_transaction.DateCreated:dd/MM/yyyy HH:mm:ss}", fontText, brush, margenIzquierdo, margenSuperior);
            }

            e.HasMorePages = false;
        }

        public void ImprimirRecibo()
        {
            try
            {
                _printDocument.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al imprimir: {ex.Message}");
            }
        }
    }
}
