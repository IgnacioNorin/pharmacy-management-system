using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of PrintSale. Renders the HTML ticket in a WebBrowser and prints it: the default
    // path goes through the browser's own print dialog (pick "Microsoft Print to PDF" to preview
    // the layout), and a thermal printer, if one is detected, offers the plain-text path instead.
    // PharmacyTicketBuilder / HtmlTicketBuilder are unchanged; the sale rows and the HTML
    // template are handed in by the exe.
    public partial class PrintSaleWindow : Window
    {
        private readonly int _saleId;
        private readonly Func<int, PrintTicketData> _dataProvider;
        private PrintTicketData _data;

        public PrintSaleWindow(int saleId, Func<int, PrintTicketData> dataProvider)
        {
            InitializeComponent();
            _saleId = saleId;
            _dataProvider = dataProvider;
            Loaded += PrintSaleWindow_Loaded;
        }

        private void PrintSaleWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _data = _dataProvider(_saleId);

                if (_data?.Sale == null)
                {
                    MessageBox.Show(this, "No se encontró la venta especificada.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                if (_data.Details == null || _data.Details.Count == 0)
                {
                    MessageBox.Show(this, "No hay detalles para esta venta.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // The HTML fill (RECEPTOR block + NETO/IVA/EXENTO breakdown, HTML-encoded) lives
                // in HtmlTicketBuilder so this normal-printer receipt cannot drift from the
                // thermal one (PharmacyTicketBuilder). DEF-11 / DEF-12.
                string html = HtmlTicketBuilder.Build(_data.HtmlTemplate, _data.Store, _data.Sale, _data.Details);

                // NavigateToString feeds the WebBrowser UTF-8 bytes, but the template carries no
                // charset, so Trident guesses and mangles accented text ("Público" -> "PÃºblico").
                // Declare UTF-8 explicitly.
                html = WithUtf8Charset(html);
                webBrowser.NavigateToString(html);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "Error al cargar la venta: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        // Inserts a UTF-8 <meta> right after the opening <html> tag (the ticket template has no
        // <head>), or prepends one if there is no <html> tag at all.
        private static string WithUtf8Charset(string html)
        {
            const string meta = "<head><meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"></head>";
            if (html.IndexOf("charset", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return html;
            }

            int htmlTag = html.IndexOf("<html", StringComparison.OrdinalIgnoreCase);
            int close = htmlTag >= 0 ? html.IndexOf('>', htmlTag) : -1;
            return close >= 0 ? html.Insert(close + 1, meta) : meta + html;
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (!IsPrintSpoolerRunning())
            {
                MessageBox.Show(this,
                    "El servicio de impresión está desactivado.\nActívalo desde 'services.msc' (Print Spooler) para poder imprimir.",
                    "Servicio de impresión detenido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (HasThermalPrinter())
            {
                MessageBoxResult result = MessageBox.Show(this,
                    "Se detectaron impresoras térmicas disponibles.\n\n" +
                    "¿Cómo deseas imprimir el ticket?\n\n" +
                    "• SÍ = Impresora térmica (rápido, texto simple)\n" +
                    "• NO = Impresora normal (formato completo)\n" +
                    "• CANCELAR = No imprimir",
                    "Seleccionar tipo de impresión",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.No);

                switch (result)
                {
                    case MessageBoxResult.Yes: PrintPlainText(); break;
                    case MessageBoxResult.No: PrintHtmlTicket(); break;
                }
            }
            else
            {
                PrintHtmlTicket();
            }
        }

        // Prints the HTML shown in the WebBrowser via mshtml's own print command, which shows the
        // standard Windows print dialog (where "Microsoft Print to PDF" can be picked).
        private void PrintHtmlTicket()
        {
            try
            {
                dynamic document = webBrowser.Document;
                if (document == null)
                {
                    MessageBox.Show(this, "El ticket todavía no terminó de cargar.", "Impresión",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                document.execCommand("Print", true, null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "Error al imprimir: " + ex.Message, "Error de Impresión",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Thermal / plain-text path: monospaced text straight to a PrintDocument.
        private void PrintPlainText()
        {
            try
            {
                string ticketText = PharmacyTicketBuilder.Build(_data.Store, _data.Sale, _data.Details);
                if (ticketText.StartsWith("Error:"))
                {
                    MessageBox.Show(this, ticketText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var printDoc = new PrintDocument();
                printDoc.PrintPage += (s, e) =>
                {
                    using (var font = new Font("Courier New", 8f))
                    {
                        float y = 0;
                        float left = e.MarginBounds.Left;
                        float top = e.MarginBounds.Top;
                        float lineHeight = font.GetHeight(e.Graphics);

                        foreach (string line in ticketText.Split('\n'))
                        {
                            if (y + lineHeight > e.MarginBounds.Height)
                            {
                                e.HasMorePages = true;
                                return;
                            }
                            e.Graphics.DrawString(line, font, Brushes.Black, left, top + y);
                            y += lineHeight;
                        }
                    }
                };

                var dialog = new System.Windows.Controls.PrintDialog();
                if (dialog.ShowDialog() == true)
                {
                    if (dialog.PrintQueue != null)
                    {
                        printDoc.PrinterSettings.PrinterName = dialog.PrintQueue.FullName;
                    }
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "Error al imprimir: " + ex.Message, "Error de Impresión",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool IsPrintSpoolerRunning()
        {
            try
            {
                using (var sc = new ServiceController("Spooler"))
                {
                    return sc.Status == ServiceControllerStatus.Running;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }

        private static bool HasThermalPrinter()
        {
            try
            {
                foreach (string printer in PrinterSettings.InstalledPrinters)
                {
                    string name = printer.ToLowerInvariant();
                    if (name.Contains("thermal") || name.Contains("pos") || name.Contains("receipt") ||
                        name.Contains("térmica") || name.Contains("ticket") || name.Contains("tm-") ||
                        (name.Contains("epson") && (name.Contains("tm") || name.Contains("receipt"))) ||
                        (name.Contains("star") && name.Contains("tsp")) ||
                        name.Contains("bixolon") || name.Contains("citizen"))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }
        }
    }
}
