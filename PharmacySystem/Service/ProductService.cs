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
    // Thin adapter kept for screens not migrated yet (frmManagement.cs, frmPurchase.cs,
    // frmReport.cs, frmSale.cs, ModalProduct.cs). Delegates to PharmacySystem.Business for every
    // method except Report(), which stays here unchanged: it formats currency/dates with
    // CultureInfoHelper/DateHelper (WinForms-side helpers), so moving it into Data would create
    // a circular reference. It belongs with frmReport's own migration later in this plan.
    public class ProductService
    {
        private static ProductService instance = null;
        private readonly Business.IProductService _inner;

        public ProductService()
        {
            _inner = new Business.ProductService(new ProductRepository(CompositionRoot.ConnectionFactory));
        }

        public static ProductService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ProductService();
                }

                return instance;
            }
        }

        public int RegisterProduct(Product obj) => _inner.Register(obj);

        public bool UpdateProduct(Product obj) => _inner.Update(obj);

        public List<Product> ListProduct() => _inner.List();

        public bool VerifyProduct(int idProduct) => _inner.Verify(idProduct);

        public bool DeleteProduct(int id) => _inner.Delete(id);

        public DataTable Report(string idcategory)
        {
            DataTable dt = new DataTable();
            DataTable dtFinal = new DataTable();

            dtFinal.Columns.Add("Fecha Registro", typeof(string));
            dtFinal.Columns.Add("Codigo", typeof(string));
            dtFinal.Columns.Add("Nombre", typeof(string));
            dtFinal.Columns.Add("Descripcion", typeof(string));
            dtFinal.Columns.Add("Categoria", typeof(string));
            dtFinal.Columns.Add("Stock", typeof(string));
            dtFinal.Columns.Add("Precio Compra", typeof(string));
            dtFinal.Columns.Add("Precio Venta", typeof(string));
            dtFinal.Columns.Add("Fecha Vencimiento", typeof(string));
            dtFinal.Columns.Add("Estado", typeof(string));



            using (SqlConnection oConnection = CompositionRoot.ConnectionFactory.Create())
            {
                try
                {

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT p.date_created, p.code,p.name AS product_name,p.description AS description_product,c.description");
                    sb.AppendLine(" AS description_category ,p.stock,p.purchase_price,p.sale_price, p.date_expired,s.name AS status_name ");
                    sb.AppendLine("FROM product p INNER JOIN category c on c.id = p.category_id");
                    sb.AppendLine("INNER JOIN state_product s on s.id = p.status");
                    sb.AppendLine("WHERE c.id = case @category_id when '0' then c.id when 0 then c.id else @category_id end");
                    sb.AppendLine("and p.date_expired IS NOT NULL");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@category_id", idcategory);
                    cmd.CommandType = CommandType.Text;

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                        foreach (DataRow row in dt.Rows)
                        {
                            string createdDate = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_created"]));
                            string codeProduct = row["code"].ToString();
                            string nameProduct = row["product_name"].ToString();
                            string descriptionProduct = row["description_product"].ToString();
                            string categoryDescription = row["description_category"].ToString();
                            string stockProduct = row["stock"].ToString();
                            string pricePurchase = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["purchase_price"]));
                            string priceSales = CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(row["sale_price"]));
                            string expirationDate = DateHelper.FormatDatePresentation(Convert.ToDateTime(row["date_expired"]));
                            string state = row["status_name"].ToString();


                            dtFinal.Rows.Add( createdDate, codeProduct,
                                            nameProduct, descriptionProduct,
                                            categoryDescription, stockProduct,
                                            pricePurchase, priceSales,
                                            expirationDate,state);

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
