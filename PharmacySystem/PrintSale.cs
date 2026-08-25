using PharmacySystem.Logical;
using PharmacySystem.Model;
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
        StringBuilder linea = new StringBuilder();
        int maxCant = 50;
        int _IdSale;

        public PrintSale(int idsale = 0)
        {
            InitializeComponent();
            _IdSale = idsale;
        }

        #region Plain Text Formatting Helpers
        private void AddCharacter(string c)
        {
            string text = "";
            for (int i = 0; i < maxCant; i++)
            {
                text += c;
            }
            linea.AppendLine(text);
        }

        private void AddCenteredText(string text)
        {
            if (text.Length > maxCant)
            {
                // If text is too long, truncate it
                linea.AppendLine(text.Substring(0, maxCant));
            }
            else
            {
                decimal spacesToAdd = Math.Truncate(Convert.ToDecimal((maxCant - text.Length) / 2));
                string spaces = "";
                for (int i = 0; i < spacesToAdd; i++)
                {
                    spaces += " ";
                }
                linea.AppendLine(spaces + text);
            }
        }

        private void AddTwoColumns(string leftText, string rightText)
        {
            int totalTextLength = leftText.Length + rightText.Length;
            if (totalTextLength > maxCant)
            {
                // If it doesn't fit, truncate the first text to make space
                int availableSpace = maxCant - rightText.Length - 1; // -1 for at least one space
                if (availableSpace > 0)
                {
                    leftText = leftText.Substring(0, Math.Min(leftText.Length, availableSpace));
                    linea.AppendLine(leftText + " " + rightText);
                }
                else
                {
                    // If it still doesn't fit, put each text on its own line
                    linea.AppendLine(leftText.Length > maxCant ? leftText.Substring(0, maxCant) : leftText);
                    linea.AppendLine(rightText.Length > maxCant ? rightText.Substring(0, maxCant) : rightText);
                }
            }
            else
            {
                int spacesCount = maxCant - totalTextLength;
                string spaces = "";
                for (int i = 0; i < spacesCount; i++)
                {
                    spaces += " ";
                }
                linea.AppendLine(leftText + spaces + rightText);
            }
        }
        #endregion

        #region Plain Text Ticket Generation
        private string GenerateFormattedPharmacyTicket()
        {
            Store store = StoreService.Instance.ListStore();
            Sale sale = SaleService.Instance.ListSale().Where(v => v.idSale == _IdSale).FirstOrDefault();
            List<SaleDetail> saleDetails = SaleService.Instance.ListSaleDetail().Where(dv => dv.idSale == _IdSale).ToList();

            if (sale == null || saleDetails == null || !saleDetails.Any())
            {
                return "Error: Sale not found or no details available.";
            }

            // Reset the StringBuilder
            linea.Clear();

            string date = sale.registrationDate.ToString("dd/MM/yyyy");
            string time = sale.registrationDate.ToString("HH:mm:ss");

            // Company header
            AddCenteredText(store.companyName.ToUpper());
            AddCenteredText($"RUC: {store.document}");
            AddCenteredText(store.address.ToUpper());
            if (!string.IsNullOrEmpty(store.phone))
                AddCenteredText($"Tel: {store.phone}");
            if (!string.IsNullOrEmpty(store.email))
                AddCenteredText(store.email);

            AddCharacter("-");

            // Sale information
            AddTwoColumns("Tipo Doc:", sale.typeDocument);
            AddTwoColumns("Número:", sale.numberDocument);
            AddTwoColumns("Fecha:", date);
            AddTwoColumns("Hora:", time);
            AddCenteredText("Cliente: Público General");

            AddCharacter("-");

            // Product headers
            string header = string.Format("{0,-4} {1,-20} {2,-8} {3,8}", "Cant", "Producto", "Precio", "Subtotal");
            if (header.Length > maxCant)
            {
                linea.AppendLine("Cant Producto           P.Unit  Subtot");
            }
            else
            {
                linea.AppendLine(header);
            }
            AddCharacter("-");

            // Product details
            foreach (SaleDetail detail in saleDetails)
            {
                string productName = detail.oProduct.name.Length > 20 ?
                    detail.oProduct.name.Substring(0, 17) + "..." : detail.oProduct.name;

                string priceStr = CultureInfoHelper.FormatAsCurrency(detail.salePrice);
                string subtotalStr = CultureInfoHelper.FormatAsCurrency(detail.subtotal);

                string productLine = string.Format("{0,-4} {1,-20} {2,-8} {3,8}",
                    detail.amount.ToString(),
                    productName,
                    priceStr,
                    subtotalStr);

                if (productLine.Length > maxCant)
                {
                    // Alternative format for very long lines
                    linea.AppendLine($"{detail.amount} {productName}");
                    AddTwoColumns($"  {priceStr} x {detail.amount}", subtotalStr);
                }
                else
                {
                    linea.AppendLine(productLine);
                }
            }

            AddCharacter("-");

            // Totals
            AddTwoColumns("TOTAL A PAGAR:", CultureInfoHelper.FormatAsCurrency(sale.totalPay));
            AddTwoColumns("PAGO CON:", CultureInfoHelper.FormatAsCurrency(sale.payWith));
            AddTwoColumns("CAMBIO:", CultureInfoHelper.FormatAsCurrency(sale.change));

            AddCharacter("-");
            AddCenteredText("¡Gracias por su compra!");
            AddCenteredText("¡Vuelva pronto!");

            // Final spaces for paper cutting
            linea.AppendLine("\n\n\n");

            Console.WriteLine(linea.ToString());
            return linea.ToString();
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
                Store store = StoreService.Instance.ListStore();
                Sale sale = SaleService.Instance.ListSale().Where(v => v.idSale == _IdSale).FirstOrDefault();

                if (sale == null)
                {
                    MessageBox.Show("No se encontró la venta especificada.", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                List<SaleDetail> saleDetails = SaleService.Instance.ListSaleDetail().Where(dv => dv.idSale == _IdSale).ToList();

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