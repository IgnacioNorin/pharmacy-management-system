using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IProductService
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        PagedResult<Product> ListPaged(int pageNumber, int pageSize, string search);
        List<Product> ListSellable();
        Product GetSellableByCode(string code);
        Product GetSellableById(int idProduct);
        bool Verify(int idProduct);
        bool Delete(int idProduct);
        List<ProductReportRow> Report(string categoryId);

        // Sets the sale price, releases the product for sale and records the change in the price
        // history. Gated by productos.editar_precios in the presenter.
        bool SetSalePrice(int idProduct, decimal salePrice, string reason, int? userId);
        bool Unrelease(int idProduct, string reason, int? userId);
        List<ProductPriceHistoryEntry> GetPriceHistory(int idProduct);
    }
}
