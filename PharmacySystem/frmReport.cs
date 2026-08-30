using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmReport : Form, IReportView
    {
        private readonly ReportPresenter _presenter;

        public frmReport()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateReportPresenter(this);
        }

        // Last consulted report per tab, kept typed so an export walks the same definition the
        // grid does (the exporters add the totals row; the grid stays without it).
        private ReportDefinition<SaleReportRow> _saleDefinition;
        private ReportResult<SaleReportRow> _saleResult;
        private ReportDefinition<PurchaseReportRow> _purchaseDefinition;
        private ReportResult<PurchaseReportRow> _purchaseResult;
        private ReportDefinition<ProductReportRow> _productDefinition;
        private ReportResult<ProductReportRow> _productResult;
        private ReportDefinition<ProductAlertHistoryEntry> _alertHistoryDefinition;
        private ReportResult<ProductAlertHistoryEntry> _alertHistoryResult;

        private static readonly IReportExporter[] Exporters =
        {
            new XlsxReportExporter(),
            new CsvReportExporter(),
            new PdfReportExporter()
        };

        #region IReportView

        public DateTime SaleStartDate => txtstartdate.Value;
        public DateTime SaleEndDate => txtenddate.Value;
        public DateTime PurchaseStartDate => txtstartdatepurchase.Value;
        public DateTime PurchaseEndDate => txtenddatepurchase.Value;
        public DateTime AlertHistoryStartDate => txtstartdatealerthistory.Value;
        public DateTime AlertHistoryEndDate => txtenddatealerthistory.Value;
        public string SelectedSupplierId => ((ComboBoxItem)cbosupplier.SelectedItem).Value.ToString();
        public string SelectedCategoryId => ((ComboBoxItem)cbocategory.SelectedItem).Value.ToString();
        public string SelectedSaleClientId => (cbosaleclient.SelectedItem as ComboBoxItem)?.Value.ToString() ?? "0";

        public void LoadSaleClientOptions(IReadOnlyList<ComboBoxItem> options)
        {
            foreach (ComboBoxItem item in options)
            {
                cbosaleclient.Items.Add(item);
            }
            cbosaleclient.DisplayMember = "Text";
            cbosaleclient.ValueMember = "Value";
            cbosaleclient.SelectedIndex = 0;
        }

        public void LoadSupplierOptions(IReadOnlyList<ComboBoxItem> options)
        {
            foreach (ComboBoxItem item in options)
            {
                cbosupplier.Items.Add(item);
            }
            cbosupplier.DisplayMember = "Text";
            cbosupplier.ValueMember = "Value";
            cbosupplier.SelectedIndex = 0;
        }

        public void LoadCategoryOptions(IReadOnlyList<ComboBoxItem> options)
        {
            foreach (ComboBoxItem item in options)
            {
                cbocategory.Items.Add(item);
            }
            cbocategory.DisplayMember = "Text";
            cbocategory.ValueMember = "Value";
            cbocategory.SelectedIndex = 0;
        }

        public void SetSaleReport(ReportDefinition<SaleReportRow> definition, ReportResult<SaleReportRow> result)
        {
            _saleDefinition = definition;
            _saleResult = result;
            dgdatasale.DataSource = BuildTable(definition, result);
            lblSaleTotals.Text = TotalsCaption(definition, result);
        }

        public void SetPurchaseReport(ReportDefinition<PurchaseReportRow> definition, ReportResult<PurchaseReportRow> result)
        {
            _purchaseDefinition = definition;
            _purchaseResult = result;
            dgdatapurchase.DataSource = BuildTable(definition, result);
            lblPurchaseTotals.Text = TotalsCaption(definition, result);
        }

        public void SetProductReport(ReportDefinition<ProductReportRow> definition, ReportResult<ProductReportRow> result)
        {
            _productDefinition = definition;
            _productResult = result;
            dgdataproduct.DataSource = BuildTable(definition, result);
            lblProductTotals.Text = TotalsCaption(definition, result);
        }

        public void SetAlertHistoryReport(ReportDefinition<ProductAlertHistoryEntry> definition, ReportResult<ProductAlertHistoryEntry> result)
        {
            _alertHistoryDefinition = definition;
            _alertHistoryResult = result;
            dgdataalerthistory.DataSource = BuildTable(definition, result);
        }

        #endregion

        // Builds the string DataTable the grid binds to: data rows only, no totals row, so
        // sorting a column never drags the totals around. Formatting lives here, in the view:
        // the presenter only supplies raw values and their ReportValueType.
        private static DataTable BuildTable<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result)
        {
            DataTable dt = new DataTable();
            foreach (ReportColumn<TRow> column in definition.Columns)
            {
                dt.Columns.Add(column.Header, typeof(string));
            }

            foreach (TRow row in result.Rows)
            {
                dt.Rows.Add(definition.Columns.Select(c => (object)FormatCell(c.Value(row), c.Type)).ToArray());
            }

            return dt;
        }

        // One-line totals strip shown under the grid: every currency / integer column with its
        // total, formatted the same way as the cells above it.
        private static string TotalsCaption<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result)
        {
            if (!result.HasTotals)
            {
                return "";
            }

            IEnumerable<string> parts = definition.Columns
                .Where(c => c.Type == ReportValueType.Currency || c.Type == ReportValueType.Integer)
                .Select(c => c.Header + " " + FormatCell(c.Value(result.Totals), c.Type));
            return "Totales:      " + string.Join("       ", parts);
        }

        private static string FormatCell(object value, ReportValueType type)
        {
            if (value == null) return "";
            if (value is string s) return s;
            if (value is DateTime d) return DateHelper.FormatDatePresentation(d);

            switch (type)
            {
                case ReportValueType.Currency: return CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(value));
                case ReportValueType.Integer: return Convert.ToInt64(value).ToString();
                default: return value.ToString();
            }
        }

        // No session -> deny (DEF-23). The form-construction smoke test never reaches this path.
        private static bool CanSee(string permission) => MainForm.Session?.Can(permission) ?? false;

        private void frmReport_Load(object sender, EventArgs e)
        {
            _presenter.OnLoad();

            ChangeMaxDate(txtstartdate, txtenddate, txtstartdatepurchase, txtenddatepurchase, txtstartdatealerthistory, txtenddatealerthistory);

            // Start the "from" pickers at the first day of the current month so a freshly opened
            // report is not empty by default; the "to" pickers stay at today.
            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            txtstartdate.Value = monthStart;
            txtstartdatepurchase.Value = monthStart;
            txtstartdatealerthistory.Value = monthStart;

            // Each report type has its own view permission: drop the tab the role cannot see, and
            // enable its Excel export only with the matching "<tipo>.exportar" permission on top.
            bool canSales = CanSee("reportes.ventas");
            bool canPurchases = CanSee("reportes.compras");
            bool canProducts = CanSee("reportes.productos");
            bool canAlertHistory = CanSee("reportes.alertas");

            if (!canSales) tabManagement.TabPages.Remove(tabProduct);
            if (!canPurchases) tabManagement.TabPages.Remove(tabCategory);
            if (!canProducts) tabManagement.TabPages.Remove(tabStore);
            if (!canAlertHistory) tabManagement.TabPages.Remove(tabAlertHistory);

            btnExportSale.Enabled = canSales && CanSee("reportes.ventas.exportar");
            btnExportPurchases.Enabled = canPurchases && CanSee("reportes.compras.exportar");
            btnExportProduct.Enabled = canProducts && CanSee("reportes.productos.exportar");
            btnExportAlertHistory.Enabled = canAlertHistory && CanSee("reportes.alertas.exportar");
        }

        private void btnConsultSale_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultSale);
        }

        private void btnConsultPurchase_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultPurchase);
        }

        private void btnConsultProduct_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultProduct);
        }

        private void btnConsultAlertHistory_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultAlertHistory);
        }

        private void btnExportSale_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.ventas.exportar")) return;
            ExportReport("Ventas", RowCount(_saleResult),
                (exporter, stream) => exporter.Export(_saleDefinition, _saleResult, "Ventas", stream));
        }

        private void btnExportPurchases_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.compras.exportar")) return;
            ExportReport("Compras", RowCount(_purchaseResult),
                (exporter, stream) => exporter.Export(_purchaseDefinition, _purchaseResult, "Compras", stream));
        }

        private void btnExportProduct_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.productos.exportar")) return;
            ExportReport("Productos", RowCount(_productResult),
                (exporter, stream) => exporter.Export(_productDefinition, _productResult, "Productos", stream));
        }

        private void btnExportAlertHistory_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.alertas.exportar")) return;
            ExportReport("Historial_Alertas", RowCount(_alertHistoryResult),
                (exporter, stream) => exporter.Export(_alertHistoryDefinition, _alertHistoryResult, "Historial de alertas", stream));
        }

        private static int RowCount<TRow>(ReportResult<TRow> result) => result?.Rows.Count ?? 0;

        // Shared by the four export buttons: the format is chosen from the SaveFileDialog filter
        // (extension -> exporter), the write goes through the matching IReportExporter, and a
        // locked target file is reported specifically instead of as a generic failure.
        private void ExportReport(string baseName, int rowCount, Action<IReportExporter, System.IO.Stream> write)
        {
            if (rowCount == 0)
            {
                MessageBox.Show("No existen datos para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.FileName = baseName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                dialog.Filter = string.Join("|", Exporters.Select(x => x.FilterLabel + "|*." + x.Extension));
                dialog.FilterIndex = 1;
                if (dialog.ShowDialog() != DialogResult.OK) return;

                string extension = System.IO.Path.GetExtension(dialog.FileName).TrimStart('.').ToLowerInvariant();
                IReportExporter exporter =
                    Exporters.FirstOrDefault(x => x.Extension == extension) ?? Exporters[dialog.FilterIndex - 1];

                Cursor previous = Cursor.Current;
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    using (var stream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                    {
                        write(exporter, stream);
                    }
                    MessageBox.Show("Reporte generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (System.IO.IOException ex)
                {
                    Logger.LogError(ex);
                    MessageBox.Show("No se pudo escribir el archivo. Verifica que no esté abierto en otra aplicación.",
                        "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    MessageBox.Show("Error al generar el reporte: " + ex.Message, "Mensaje",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                finally
                {
                    Cursor.Current = previous;
                }
            }
        }

        // A failed consult must surface as a logged error and a message, never a silent no-op or
        // a frozen window.
        private void RunConsult(Action consult)
        {
            Cursor previous = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                consult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show("No se pudo generar el reporte: " + ex.Message, "Mensaje",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor.Current = previous;
            }
        }

        private void ChangeMaxDate(params DateTimePicker[] camps)
        {
            foreach (var camp in camps) {

                camp.MaxDate = DateTime.Now;
            }

        }
    }
}
