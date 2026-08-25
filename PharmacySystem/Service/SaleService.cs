using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmReport.cs, frmSale.cs, PrintSale.cs).
    // Delegates to PharmacySystem.Business for everything except ReportSale(), which stays here
    // unchanged for the same reason as Product/PurchaseService's Report methods: it formats
    // currency/dates with CultureInfoHelper/DateHelper, which Data cannot reference without a
    // circular dependency.
    public class SaleService
    {
        private static SaleService instance = null;
        private readonly Business.ISaleService _inner;

        public SaleService()
        {
            _inner = new Business.SaleService(new SaleRepository(CompositionRoot.ConnectionFactory));
        }

        public static SaleService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SaleService();
                }
                return instance;
            }
        }

        public List<Sale> ListSale() => _inner.ListSale();

        public List<SaleDetail> ListSaleDetail() => _inner.ListSaleDetail();

        public bool ControlStock(int idproduct, int amount, bool subtract) =>
            _inner.ControlStock(idproduct, amount, subtract);

        public int RegisterSale(Sale obj) => _inner.Register(obj);

        public decimal SumTotalPay(string startDate, string endDate) => _inner.SumTotalPay(startDate, endDate);

        public decimal SumAmountReceived(string startDate, string endDate) => _inner.SumAmountReceived(startDate, endDate);

        public decimal SumChangeAmount(string startDate, string endDate) => _inner.SumChangeAmount(startDate, endDate);

        public DataTable ReportSale(string startDate , string endDate)
        {
            DataTable dt = new DataTable();
            DataTable dtFinal = new DataTable();

            dtFinal.Columns.Add("Fecha Venta", typeof(string));
            dtFinal.Columns.Add("Tipo Documento", typeof(string));
            dtFinal.Columns.Add("Numero Documento", typeof(string));
            dtFinal.Columns.Add("CI Vendedor", typeof(string));
            dtFinal.Columns.Add("Nombre Vendedor", typeof(string));
            dtFinal.Columns.Add("CI Cliente", typeof(string));
            dtFinal.Columns.Add("Nombre Cliente", typeof(string));
            dtFinal.Columns.Add("Total Pagar", typeof(string));
            dtFinal.Columns.Add("Pago Con", typeof(string));
            dtFinal.Columns.Add("Cambio", typeof(string));


            using (SqlConnection oConnection = CompositionRoot.ConnectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("SELECT s.date_registered,s.document_type,s.document_number AS document_tribute_number,p.document_number AS document_number_person,p.name,s.document_client,s.name_client,");
                    sb.AppendLine("s.total_amount,s.amount_received,s.change_amount FROM sale s");
                    sb.AppendLine("INNER JOIN person p ON p.id = s.user_id");
                    sb.AppendLine("WHERE CAST(s.date_registered AS DATE) BETWEEN @startDate AND @endDate");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@startDate", startDate);
                    cmd.Parameters.AddWithValue("@endDate", endDate);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd)) {
                        //fill the datatable with sql query data
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            string dateRegister = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_registered"]));
                            string typeDocument = row["document_type"].ToString();
                            string numberDocument = row["document_tribute_number"].ToString();
                            string idDocument = row["document_number_person"].ToString();
                            string nameVendor = row["name"].ToString();
                            string idDocumentClient = row["document_client"].ToString();
                            string nameClient = row["name_client"].ToString();
                            string totalPay = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["total_amount"]));
                            string amountReceived = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["amount_received"]));
                            string changeAmount = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["change_amount"]));

                            dtFinal.Rows.Add(dateRegister, typeDocument,
                                            numberDocument, idDocument,
                                            nameVendor, idDocumentClient,
                                            nameClient, totalPay,
                                            amountReceived, changeAmount);

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
