using ClosedXML.Excel;
using PharmacySystem.Helpers;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmReport : Form
    {
        public frmReport()
        {
            InitializeComponent();
        }
        DataTable dtSale = new DataTable();
        DataTable dtPurchase = new DataTable();
        DataTable dtProduct = new DataTable();

        private void frmReport_Load(object sender, EventArgs e)
        {
            cbosupplier.Items.Add(new ComboBoxItem() { Value = "0", Text = "Todos" });
            foreach (Supplier pr in SupplierService.Instance.ListSupplier())
            {
                cbosupplier.Items.Add(new ComboBoxItem() { Value = pr.idSupplier, Text = pr.companyName });
            }
            cbosupplier.DisplayMember = "Text";
            cbosupplier.ValueMember = "Value";
            cbosupplier.SelectedIndex = 0;


            cbocategory.Items.Add(new ComboBoxItem() { Value = "0", Text = "Todos" });
            foreach (Categories c in CategoryService.Instance.ListCategory())
            {
                cbocategory.Items.Add(new ComboBoxItem() { Value = c.IdCategory, Text = c.description });
            }
            cbocategory.DisplayMember = "Text";
            cbocategory.ValueMember = "Value";
            cbocategory.SelectedIndex = 0;


            ChangeMaxDate(txtstartdate,txtenddate,txtstartdatepurchase,txtenddatepurchase);
        }

        private void btnExportSale_Click(object sender, EventArgs e)
        {
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
            decimal sumTotalPay;
            decimal sumAmountReceived;
            decimal sumChangeAmount;
            var startDate = DateHelper.FormatDateBackend(txtstartdate.Value);
            var endDate = DateHelper.FormatDateBackend(txtenddate.Value);

            sumTotalPay = SaleService.Instance.SumTotalPay(startDate, endDate);
            sumAmountReceived = SaleService.Instance.SumAmountReceived(startDate, endDate);
            sumChangeAmount = SaleService.Instance.SumChangeAmount(startDate, endDate);
            dtSale = SaleService.Instance.ReportSale(startDate, endDate);


            if (dtSale != null)
            {
                dtSale.Rows.Add(null, null);
                dtSale.Rows.Add(null, null, null, null, null, null, "Total:", 
                                CultureInfoHelper.FormatAsEcuadorCurrency(sumTotalPay),
                                CultureInfoHelper.FormatAsEcuadorCurrency(sumAmountReceived),
                                CultureInfoHelper.FormatAsEcuadorCurrency(sumChangeAmount));
                dgdatasale.DataSource = dtSale;
            }
        }

        private void btnConsultPurchase_Click(object sender, EventArgs e)
        {
            decimal sumTotalAmount;
            decimal sumQuantityProduct;
            decimal sumPurchasePrice;
            decimal sumSalePrice;

            var startDate = DateHelper.FormatDateBackend(txtstartdatepurchase.Value);
            var endDate = DateHelper.FormatDateBackend(txtenddatepurchase.Value);
            var cboSupplier = ((ComboBoxItem)cbosupplier.SelectedItem).Value.ToString();


            dtPurchase = PurchaseService.Instance.ReportPurchase(cboSupplier, startDate, endDate);
            sumTotalAmount = PurchaseService.Instance.GetTotalAmount(cboSupplier, startDate, endDate);
            sumQuantityProduct = PurchaseService.Instance.GetTotalQuantity( cboSupplier , startDate, endDate);
            sumPurchasePrice = PurchaseService.Instance.GetTotalPurchasePrice(cboSupplier, startDate, endDate);
            sumSalePrice = PurchaseService.Instance.GetTotalSalesPrice(cboSupplier, startDate, endDate);

            if (dtPurchase != null)
            {
                dtPurchase.Rows.Add(null, null);
                dtPurchase.Rows.Add(null, null, null, null, "Total:", 
                                CultureInfoHelper.FormatAsEcuadorCurrency(sumTotalAmount), null,
                                sumQuantityProduct.ToString(), 
                                CultureInfoHelper.FormatAsEcuadorCurrency(sumPurchasePrice),
                                CultureInfoHelper.FormatAsEcuadorCurrency(sumSalePrice));


                dgdatapurchase.DataSource = dtPurchase;

            }
        }

        private void btnExportPurchases_Click(object sender, EventArgs e)
        {
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
            dtProduct = ProductService.Instance.Report(((ComboBoxItem)cbocategory.SelectedItem).Value.ToString());
            if (dtProduct != null)
            {
                dgdataproduct.DataSource = dtProduct;
            }

        }

        private void btnExportProduct_Click(object sender, EventArgs e)
        {
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

        private void ChangeMaxDate(params DateTimePicker[] camps)
        {
            foreach (var camp in camps) { 
            
                camp.MaxDate = DateTime.Now;
            }

        }
    }
}
