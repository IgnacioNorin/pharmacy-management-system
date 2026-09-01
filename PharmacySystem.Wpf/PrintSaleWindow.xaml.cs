using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.ServiceProcess;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using PharmacySystem.Helpers;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of PrintSale. Renders the HTML ticket in WebView2 (Edge/Chromium) and prints it:
    // the default path opens the browser print dialog (pick "Microsoft Print to PDF" to preview
    // the layout), and a thermal printer, if one is detected, offers the plain-text path instead.
    // PharmacyTicketBuilder / HtmlTicketBuilder are unchanged; the sale rows and the HTML template
    // are handed in by the exe.
    public partial class PrintSaleWindow : Window
    {
        private readonly int _saleId;
        private readonly Func<int, PrintTicketData> _dataProvider;
        // Set in the Loaded handler, which closes the window if the data does not resolve, so
        // every other method runs only when it is populated.
        private PrintTicketData _data = null!;
        private string _ticketHtml = string.Empty;
        private bool _webViewReady;

        public PrintSaleWindow(int saleId, Func<int, PrintTicketData> dataProvider)
        {
            InitializeComponent();
            _saleId = saleId;
            _dataProvider = dataProvider;

            // Pin the user-data folder to a writable per-user location. The WebView2 default sits
            // next to the executable, which is read-only under "Program Files" in a real install
            // and makes CoreWebView2 initialization fail hard. Setting it here (before the control
            // loads) also steers the control's own implicit initialization.
            webView.CreationProperties = new CoreWebView2CreationProperties
            {
                UserDataFolder = ResolveUserDataFolder()
            };
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

            Loaded += PrintSaleWindow_Loaded;
        }

        private static string ResolveUserDataFolder()
        {
            string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string folder = Path.Combine(root, "PharmacySystem", "WebView2");
            try { Directory.CreateDirectory(folder); }
            catch (Exception ex) { Logger.LogError(ex); }
            return folder;
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
                _ticketHtml = HtmlTicketBuilder.Build(_data.HtmlTemplate, _data.Store, _data.Sale, _data.Details);

                // Kicks off (or joins) CoreWebView2 initialization; the result is handled in
                // WebView_CoreWebView2InitializationCompleted for both this call and the control's
                // own implicit initialization.
                _ = webView.EnsureCoreWebView2Async(null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "Error al cargar la venta: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void WebView_CoreWebView2InitializationCompleted(
            object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                Logger.LogError(e.InitializationException ??
                    new InvalidOperationException("CoreWebView2 initialization failed."));
                MessageBox.Show(this,
                    "No se pudo inicializar el visor del ticket. Instale el runtime de " +
                    "Microsoft Edge WebView2 y vuelva a intentar.",
                    "Impresión", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            try
            {
                var settings = webView.CoreWebView2.Settings;
                settings.AreDevToolsEnabled = false;
                settings.AreDefaultContextMenusEnabled = false;
                settings.IsStatusBarEnabled = false;
                settings.IsZoomControlEnabled = false;

                _webViewReady = true;
                // WebView2 (Chromium) always reads NavigateToString content as UTF-8, so the old
                // Trident charset workaround is gone.
                webView.NavigateToString(_ticketHtml);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "Error al preparar el visor del ticket: " + ex.Message, "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
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

        // Opens WebView2's own print dialog (Chromium), which offers a preview and every installed
        // printer, including "Microsoft Print to PDF".
        private void PrintHtmlTicket()
        {
            try
            {
                if (!_webViewReady || webView.CoreWebView2 == null)
                {
                    MessageBox.Show(this, "El ticket todavía no terminó de cargar.", "Impresión",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                webView.CoreWebView2.ShowPrintUI(CoreWebView2PrintDialogKind.Browser);
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
                string ticketText = PharmacyTicketBuilder.Build(_data.Store, _data.Sale!, _data.Details!);
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
                        float lineHeight = font.GetHeight(e.Graphics!);

                        foreach (string line in ticketText.Split('\n'))
                        {
                            if (y + lineHeight > e.MarginBounds.Height)
                            {
                                e.HasMorePages = true;
                                return;
                            }
                            e.Graphics!.DrawString(line, font, Brushes.Black, left, top + y);
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
