using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IProductRepository
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        bool Verify(int idProduct);
        bool Delete(int idProduct);

        // Writes purchase_price / sale_price for one product. Separate from Update because it is
        // gated by its own permission (productos.editar_precios) - Update only ever touches the
        // non-price fields. Returns false if no row matched.
        bool SetPrices(int idProduct, decimal purchasePrice, decimal salePrice);

        // Returns raw rows now that CultureInfoHelper/DateHelper live in Domain and formatting
        // moved to ReportPresenter - this used to build a pre-formatted-string DataTable here.
        List<ProductReportRow> Report(string categoryId);
    }
}
