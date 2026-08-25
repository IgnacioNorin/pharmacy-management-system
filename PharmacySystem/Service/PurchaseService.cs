using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmPurchase.cs, frmReport.cs). Delegates
    // to PharmacySystem.Business for everything except ReportPurchase(), which stays here
    // unchanged for the same reason as ProductService.Report(): it formats currency/dates with
    // CultureInfoHelper/DateHelper, which Data cannot reference without a circular dependency.
    public class PurchaseService
    {
        private static PurchaseService _instance = null;
        private readonly Business.IPurchaseService _inner;

        public PurchaseService()
        {
            _inner = new Business.PurchaseService(new PurchaseRepository(CompositionRoot.ConnectionFactory));
        }

        public static PurchaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PurchaseService();
                }

                return _instance;
            }
        }

        public bool RegisterPurchase(Purchase purchase) => _inner.Register(purchase);

        public decimal GetTotalAmount(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalAmount(idSupplier, startDate, endDate);

        public decimal GetTotalPurchasePrice(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalPurchasePrice(idSupplier, startDate, endDate);

        public int GetTotalQuantity(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalQuantity(idSupplier, startDate, endDate);

        public decimal GetTotalSalesPrice(string idSupplier, string startDate, string endDate) =>
            _inner.GetTotalSalesPrice(idSupplier, startDate, endDate);

        public decimal GetSubTotal(string idSupplier, string startDate, string endDate) =>
            _inner.GetSubTotal(idSupplier, startDate, endDate);

        public DataTable ReportPurchase(string idSupplier, string startDate, string endDate)
        {
            DataTable dt = new DataTable();
            DataTable dtFinal = new DataTable();
            dtFinal.Columns.Add("Fecha Compra", typeof(string));
            dtFinal.Columns.Add("RUC", typeof(string));
            dtFinal.Columns.Add("Razon Social", typeof(string));
            dtFinal.Columns.Add("Tipo Documento", typeof(string));
            dtFinal.Columns.Add("Numero Documento", typeof(string));
            dtFinal.Columns.Add("Monto Total", typeof(string));
            dtFinal.Columns.Add("Nombre,", typeof(string));
            dtFinal.Columns.Add("Cantidad", typeof(string));
            dtFinal.Columns.Add("Precio Compra", typeof(string));
            dtFinal.Columns.Add("Precio Venta", typeof(string));
            using (SqlConnection oConnection = CompositionRoot.ConnectionFactory.Create())
            {
                try
                {
                     StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT pu.date_registered,su.document_number AS document_number_supplier,su.company_name,");
                    sb.AppendLine("pu.document_type,pu.document_number AS document_number_employee,pu.total_amount,");
                    sb.AppendLine("pr.name,pd.stock,pd.purchase_price,pd.sale_price");
                    sb.AppendLine("FROM purchase pu");
                    sb.AppendLine("INNER JOIN supplier su ON su.id = pu.supplier_id");
                    sb.AppendLine("INNER JOIN purchase_detail pd ON pd.purchase_id = pu.id");
                    sb.AppendLine("INNER JOIN product pr on pr.id = pd.product_id");
                    sb.AppendLine("WHERE CAST(pu.date_registered AS DATE) BETWEEN @startDate and @endDate");
                    sb.AppendLine("and pu.supplier_id =  CASE @supplier_id WHEN '0' THEN pu.supplier_id ");
                    sb.AppendLine("WHEN 0 THEN pu.supplier_id ELSE @supplier_id END");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.Parameters.AddWithValue("@supplier_id", idSupplier);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            string datePurchase = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_registered"]));
                            string idSupplierTemp = row["document_number_supplier"].ToString();
                            string companyName = row["company_name"].ToString();
                            string typeDocument = row["document_type"].ToString();
                            string numberDocument = row["document_number_employee"].ToString();
                            string totalAmount = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["total_amount"]));
                            string nameProduct = row["name"].ToString();
                            string quantity = row["stock"].ToString();
                            string pricePurchase = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["purchase_price"]));
                            string priceSale = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["sale_price"]));

                            dtFinal.Rows.Add( datePurchase, idSupplierTemp,
                                            companyName, typeDocument,
                                            numberDocument, totalAmount,
                                            nameProduct, quantity,
                                            pricePurchase, priceSale);


                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    dt = new DataTable();
                    dtFinal = new DataTable();
                }
            }
            return dtFinal;

        }
    }
}
