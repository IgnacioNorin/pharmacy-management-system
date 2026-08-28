using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceProcess;
using PharmacySystem.Helpers;

namespace PharmacySystem
{
    public partial class PrintSale : Form
    {
        int _IdSale;
        private readonly IStoreService _storeService;
        private readonly ISaleService _saleService;

        public PrintSale(int idsale = 0)
        {
            InitializeComponent();
            _IdSale = idsale;
            _storeService = new StoreService(new StoreRepository(CompositionRoot.ConnectionFactory));
            _saleService = new SaleService(new SaleRepository(CompositionRoot.ConnectionFactory), new LocalSequenceIssuer());
        }

        #region Plain Text Ticket Generation
        private string GenerateFormattedPharmacyTicket()
        {
            Store store = _storeService.ListStore();
            Sale sale = _saleService.ListSale().Where(v => v.idSale == _IdSale).FirstOrDefault();
            List<SaleDetail> saleDetails = _saleService.ListSaleDetail().Where(dv => dv.idSale == _IdSale).ToList();

            string ticketText = PharmacyTicketBuilder.Build(store, sale, saleDetails);
            Console.WriteLine(ticketText);
            return ticketText;
        }
        #endregion

        #region Printer Detection and Management
        private bool HasThermalPrinter()
        {
            try
            {
                // Search for printers that contain thermal keywords
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    string name = printer.ToLower();
                    if (name.Contains("thermal") || name.Contains("pos") || name.Contains("receipt") ||
                        name.Contains("térmica") || name.Contains("ticket") || name.Contains("tm-") ||
                        name.Contains("epson") && (name.Contains("tm") || name.Contains("receipt")) ||
                        name.Contains("star") && name.Contains("tsp") ||
                        name.Contains("bixolon") || name.Contains("citizen"))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error detecting thermal printer: {ex.Message}");
                return false;
            }
        }

        private bool IsPrintSpoolerRunning()
        {
            try
            {
                ServiceController sc = new ServiceController("Spooler");
                return sc.Status == ServiceControllerStatus.Running;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking print spooler: {ex.Message}");
                return false;
            }
        }
        #endregion

        #region Print Methods
        private void PrintPlainText()
        {
            try
            {
                string ticketText = GenerateFormattedPharmacyTicket();

                if (ticketText.StartsWith("Error:"))
                {
                    MessageBox.Show(ticketText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create a PrintDocument for direct printing
                System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();
                printDoc.PrintPage += (sender, e) =>
                {
                    Font font = new Font("Courier New", 8); // Monospaced font
                    float yPosition = 0;
                    float leftMargin = e.MarginBounds.Left;
                    float topMargin = e.MarginBounds.Top;
                    float lineHeight = font.GetHeight(e.Graphics);

                    string[] lines = ticketText.Split('\n');

                    foreach (string line in lines)
                    {
                        if (yPosition + lineHeight > e.MarginBounds.Height)
                        {
                            e.HasMorePages = true;
                            return;
                        }

                        e.Graphics.DrawString(line, font, Brushes.Black, leftMargin, topMargin + yPosition);
                        yPosition += lineHeight;
                    }
                };

                // Show print dialog
                System.Windows.Forms.PrintDialog printDialog = new System.Windows.Forms.PrintDialog();
                printDialog.Document = printDoc;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error de Impresión",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintHTMLTicket()
        {
            try
            {
                BrowserPrintSale.ShowPrintDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}", "Error de Impresión",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Form Events
        private void PrintSale_Load(object sender, EventArgs e)
        {
            try
            {
                Store store = _storeService.ListStore();
                Sale sale = _saleService.ListSale().Where(v => v.idSale == _IdSale).FirstOrDefault();

                if (sale == null)
                {
                    MessageBox.Show("No se encontró la venta especificada.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                List<SaleDetail> saleDetails = _saleService.ListSaleDetail().Where(dv => dv.idSale == _IdSale).ToList();

                if (saleDetails == null || !saleDetails.Any())
                {
                    MessageBox.Show("No hay detalles para esta venta.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Load HTML ticket template
                string ticketText = Properties.Resources.Ticket.ToString();
                ticketText = ticketText.Replace("¡nombreempresa!", store.companyName.ToUpper());
                ticketText = ticketText.Replace("¡documentoempresa!", store.document);
                ticketText = ticketText.Replace("¡correoempresa!", store.email);
                ticketText = ticketText.Replace("!telefonoempresa¡", store.phone);

                ticketText = ticketText.Replace("¡tipodocumento!", sale.typeDocument);
                ticketText = ticketText.Replace("¡numerodocumento!", sale.numberDocument);
                ticketText = ticketText.Replace("¡fechaventa!", sale.registrationDate.ToString());

                StringBuilder tableRows = new StringBuilder();
                foreach (SaleDetail detail in saleDetails)
                {
                    tableRows.AppendLine("<tr>");
                    tableRows.AppendLine("<td width=\"20\">" + detail.amount + "</td>");
                    tableRows.AppendLine("<td width=\"180\">" + detail.oProduct.name + "</td>");
                    tableRows.AppendLine("<td style=\"font-size:14px\">" + CultureInfoHelper.FormatAsCurrency(detail.salePrice) + "</td>");
                    tableRows.AppendLine("<td style=\"font-size:14px\">" + CultureInfoHelper.FormatAsCurrency(detail.subtotal) + "</td>");
                    tableRows.AppendLine("</tr>");
                }
                ticketText = ticketText.Replace("¡detalleventa!", tableRows.ToString());

                ticketText = ticketText.Replace("¡totalpagar!", CultureInfoHelper.FormatAsCurrency(sale.totalPay));
                ticketText = ticketText.Replace("¡pagocon!", CultureInfoHelper.FormatAsCurrency(sale.payWith));
                ticketText = ticketText.Replace("¡cambio!", CultureInfoHelper.FormatAsCurrency(sale.change));

                BrowserPrintSale.DocumentText = ticketText;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar la venta: {ex.Message}", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (!IsPrintSpoolerRunning())
            {
                MessageBox.Show("El servicio de impresión está desactivado.\nActívalo desde 'services.msc' (Print Spooler) para poder imprimir.",
                               "Servicio de impresión detenido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Smart printer detection and selection
            if (HasThermalPrinter())
            {
                // Ask user which type of printing they prefer
                var result = MessageBox.Show(
                    "Se detectaron impresoras térmicas disponibles.\n\n" +
                    "¿Cómo deseas imprimir el ticket?\n\n" +
                    "• SÍ = Impresora térmica (rápido, texto simple)\n" +
                    "• NO = Impresora normal (formato completo)\n" +
                    "• CANCELAR = No imprimir",
                    "Seleccionar tipo de impresión",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2); // Default to "No" (normal printer)

                switch (result)
                {
                    case DialogResult.Yes:
                        PrintPlainText(); // Thermal printer
                        break;
                    case DialogResult.No:
                        PrintHTMLTicket(); // Normal printer
                        break;
                    case DialogResult.Cancel:
                        // Do nothing
                        break;
                }
            }
            else
            {
                // Only normal printers available
                PrintHTMLTicket();
            }
        }
        #endregion

        #region Keyboard Shortcuts (Optional)
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Alt + I = Quick thermal print
            if (keyData == (Keys.Alt | Keys.I))
            {
                if (IsPrintSpoolerRunning() && HasThermalPrinter())
                {
                    PrintPlainText();
                    return true;
                }
            }

            // Ctrl + P = Normal print
            if (keyData == (Keys.Control | Keys.P))
            {
                if (IsPrintSpoolerRunning())
                {
                    PrintHTMLTicket();
                    return true;
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion
    }
}