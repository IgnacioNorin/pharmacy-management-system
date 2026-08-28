using ClosedXML.Excel;
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

        DataTable dtSale = new DataTable();
        DataTable dtPurchase = new DataTable();
        DataTable dtProduct = new DataTable();
        DataTable dtAlertHistory = new DataTable();

        // Pre-formatted "Total:" row for the reports that have one. The grid is bound without it
        // (so sorting leaves it alone); the export appends it to a copy of the grid table.
        string[] saleTotalsRow;
        string[] purchaseTotalsRow;

        #region IReportView

        public DateTime SaleStartDate => txtstartdate.Value;
        public DateTime SaleEndDate => txtenddate.Value;
        public DateTime PurchaseStartDate => txtstartdatepurchase.Value;
        public DateTime PurchaseEndDate => txtenddatepurchase.Value;
        public DateTime AlertHistoryStartDate => txtstartdatealerthistory.Value;
        public DateTime AlertHistoryEndDate => txtenddatealerthistory.Value;
        public string SelectedSupplierId => ((ComboBoxItem)cbosupplier.SelectedItem).Value.ToString();
        public string SelectedCategoryId => ((ComboBoxItem)cbocategory.SelectedItem).Value.ToString();

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
            dtSale = BuildTable(definition, result);
            dgdatasale.DataSource = dtSale;
            saleTotalsRow = BuildTotalsRow(definition, result);
            lblSaleTotals.Text = TotalsCaption(definition, result);
        }

        public void SetPurchaseReport(ReportDefinition<PurchaseReportRow> definition, ReportResult<PurchaseReportRow> result)
        {
            dtPurchase = BuildTable(definition, result);
            dgdatapurchase.DataSource = dtPurchase;
            purchaseTotalsRow = BuildTotalsRow(definition, result);
            lblPurchaseTotals.Text = TotalsCaption(definition, result);
        }

        public void SetProductReport(ReportDefinition<ProductReportRow> definition, ReportResult<ProductReportRow> result)
        {
            dtProduct = BuildTable(definition, result);
            dgdataproduct.DataSource = dtProduct;
        }

        public void SetAlertHistoryReport(ReportDefinition<ProductAlertHistoryEntry> definition, ReportResult<ProductAlertHistoryEntry> result)
        {
            dtAlertHistory = BuildTable(definition, result);
            dgdataalerthistory.DataSource = dtAlertHistory;
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

        // The pre-formatted "Total:" cells for the export, or null when the report has no totals.
        private static string[] BuildTotalsRow<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result)
        {
            if (!result.HasTotals)
            {
                return null;
            }

            string[] cells = new string[definition.Columns.Count];
            cells[0] = "Total:";
            for (int i = 1; i < definition.Columns.Count; i++)
            {
                ReportColumn<TRow> column = definition.Columns[i];
                bool numeric = column.Type == ReportValueType.Currency || column.Type == ReportValueType.Integer;
                cells[i] = numeric ? FormatCell(column.Value(result.Totals), column.Type) : "";
            }
            return cells;
        }

        // Grid table plus a blank spacer and the totals row, for exporting. The totals stay out
        // of the bound grid so sorting cannot move them.
        private static DataTable WithTotals(DataTable gridTable, string[] totalsRow)
        {
            if (totalsRow == null)
            {
                return gridTable;
            }

            DataTable dt = gridTable.Copy();
            dt.Rows.Add(dt.NewRow());
            dt.Rows.Add(totalsRow);
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

        // Falls back to allowed when there is no session (form-construction smoke test).
        private static bool CanSee(string permission) => MainForm.Session?.Can(permission) ?? true;

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

        private void btnExportSale_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.ventas.exportar")) return;

            if (dgdatasale.Rows.Count > 0)
            {
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("Reporte_Venta_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                savefile.Filter = "Excel Files|*.xlsx";
                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string report = "Informe";
                        XLWorkbook wb = new XLWorkbook();
                        var sheet = wb.Worksheets.Add(WithTotals(dtSale, saleTotalsRow), report);
                        sheet.ColumnsUsed().AdjustToContents();
                        wb.SaveAs(savefile.FileName);
                        MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        MessageBox.Show("Error al generar reporte", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("No existen datos para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnConsultSale_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultSale);
        }

        private void btnConsultPurchase_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultPurchase);
        }

        private void btnExportPurchases_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.compras.exportar")) return;

            if (dgdatapurchase.Rows.Count > 0)
            {
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("Reporte_Compra_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                savefile.Filter = "Excel Files|*.xlsx";
                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        XLWorkbook wb = new XLWorkbook();
                        var sheet = wb.Worksheets.Add(WithTotals(dtPurchase, purchaseTotalsRow), "Informe");
                        sheet.ColumnsUsed().AdjustToContents();
                        wb.SaveAs(savefile.FileName);
                        MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        MessageBox.Show("Error al generar reporte", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("No existen datos para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnConsultProduct_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultProduct);
        }

        private void btnExportProduct_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.productos.exportar")) return;

            if (dgdataproduct.Rows.Count > 0)
            {
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("Reporte_Producto_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                savefile.Filter = "Excel Files|*.xlsx";
                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        XLWorkbook wb = new XLWorkbook();
                        var sheet = wb.Worksheets.Add(dtProduct, "Informe");
                        sheet.ColumnsUsed().AdjustToContents();
                        wb.SaveAs(savefile.FileName);
                        MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Error al generar reporte", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("No existen datos para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnConsultAlertHistory_Click(object sender, EventArgs e)
        {
            RunConsult(_presenter.OnConsultAlertHistory);
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

        private void btnExportAlertHistory_Click(object sender, EventArgs e)
        {
            if (!CanSee("reportes.alertas.exportar")) return;

            if (dgdataalerthistory.Rows.Count > 0)
            {
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("Historial_Alertas_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                savefile.Filter = "Excel Files|*.xlsx";
                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        XLWorkbook wb = new XLWorkbook();
                        var sheet = wb.Worksheets.Add(dtAlertHistory, "Informe");
                        sheet.ColumnsUsed().AdjustToContents();
                        wb.SaveAs(savefile.FileName);
                        MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Error al generar reporte", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
            else
            {
                MessageBox.Show("No existen datos para exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
