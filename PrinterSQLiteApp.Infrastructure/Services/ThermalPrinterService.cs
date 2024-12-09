using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text.Json;

public class Config
{
    public string RutaDb { get; set; }
    public string RutaImg { get; set; }
}

public class ThermalPrinterService
{
    private readonly string _rutaImg;
    private readonly PrintDocument _printDocument;
    private Image _logo;

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
            _logo = Image.FromFile(_rutaImg);
        }
        else
        {
            Console.WriteLine("No se encontró la imagen en la ruta especificada.");
        }
    }

    private Config LoadConfig(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al leer el archivo de configuración: {ex.Message}");
            return null;
        }
    }

    // Método que maneja el evento PrintPage para imprimir la imagen y los detalles del recibo
    private void PrintPage(object sender, PrintPageEventArgs e)
    {
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
        Font fontTitle = new Font("Arial", 14, FontStyle.Bold);
        Font fontText = new Font("Arial", 10);
        Brush brush = Brushes.Black;

        e.Graphics.DrawString("Recibo de Pago", fontTitle, brush, margenIzquierdo, margenSuperior);
        margenSuperior += 30; // Mueve el cursor para la siguiente línea

        // Información de ejemplo para el recibo
        e.Graphics.DrawString("Fecha: " + DateTime.Now.ToShortDateString(), fontText, brush, margenIzquierdo, margenSuperior);
        margenSuperior += 20;

        e.Graphics.DrawString("Cliente: Juan Pérez", fontText, brush, margenIzquierdo, margenSuperior);
        margenSuperior += 20;

        e.Graphics.DrawString("Monto pagado: $100.00", fontText, brush, margenIzquierdo, margenSuperior);
        margenSuperior += 20;

        // Puedes seguir añadiendo más información o detalles de la transacción aquí
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

public class Program
{
    public static void Main(string[] args)
    {
        var servicioImpresion = new ThermalPrinterService();
        servicioImpresion.ImprimirRecibo();
    }
}
