using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IProductService
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        bool Verify(int idProduct);
        bool Delete(int idProduct);
        List<ProductReportRow> Report(string categoryId);

        // Writes the two price fields for one product. Gated by productos.editar_precios in the
        // presenter; Update never touches prices.
        bool SetPrices(int idProduct, decimal purchasePrice, decimal salePrice);
    }
}
