using ClosedXML.Excel;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Data;
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

        public void SetSaleReport(DataTable table)
        {
            dtSale = table;
            dgdatasale.DataSource = dtSale;
        }

        public void SetPurchaseReport(DataTable table)
        {
            dtPurchase = table;
            dgdatapurchase.DataSource = dtPurchase;
        }

        public void SetProductReport(DataTable table)
        {
            dtProduct = table;
            dgdataproduct.DataSource = dtProduct;
        }

        public void SetAlertHistoryReport(DataTable table)
        {
            dtAlertHistory = table;
            dgdataalerthistory.DataSource = dtAlertHistory;
        }

        #endregion

        // Falls back to allowed when there is no session (form-construction smoke test).
        private static bool CanSee(string permission) => MainForm.Session?.Can(permission) ?? true;

        private void frmReport_Load(object sender, EventArgs e)
        {
            _presenter.OnLoad();

            ChangeMaxDate(txtstartdate, txtenddate, txtstartdatepurchase, txtenddatepurchase, txtstartdatealerthistory, txtenddatealerthistory);

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
                        var sheet = wb.Worksheets.Add(dtSale, report);
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

        private void btnConsultSale_Click(object sender, EventArgs e)
        {
            _presenter.OnConsultSale();
        }

        private void btnConsultPurchase_Click(object sender, EventArgs e)
        {
            _presenter.OnConsultPurchase();
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
                        var sheet = wb.Worksheets.Add(dtPurchase, "Informe");
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

        private void btnConsultProduct_Click(object sender, EventArgs e)
        {
            _presenter.OnConsultProduct();
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
            _presenter.OnConsultAlertHistory();
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
