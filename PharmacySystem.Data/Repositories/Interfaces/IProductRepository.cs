using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IProductRepository
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        // The lots of one product (quantity > 0), earliest expiry first.
        List<ProductLot> GetLots(int idProduct);
        // One page of active products, filtered by a free-text term over code / name / description.
        PagedResult<Product> ListPaged(int pageNumber, int pageSize, string search);
        // Only products released for sale (product.is_released = 1). Backs the sale screen.
        List<Product> ListSellable();
        // One sellable product by code / id, for the sale screen's scan and add-to-cart steps.
        Product GetSellableByCode(string code);
        Product GetSellableById(int idProduct);
        // One active product by code / id, released or not, for the purchase screen's scan and
        // add-to-cart steps (buying stock does not require the product to be on sale).
        Product GetByCode(string code);
        Product GetById(int idProduct);
        bool Verify(int idProduct);
        bool Delete(int idProduct);

        // Sets the sale price of one product, releases it for sale (is_released = 1) and records
        // the change in product_price_history with the current cost, the user and the reason.
        // Gated by productos.editar_precios in the presenter. Returns false if no row matched.
        bool SetSalePrice(int idProduct, decimal salePrice, string reason, int? userId);

        // Withdraws a product from sale (is_released = 0) and records it in the history. Returns
        // false if the product was not released.
        bool Unrelease(int idProduct, string reason, int? userId);

        // The price timeline of one product, newest first.
        List<ProductPriceHistoryEntry> GetPriceHistory(int idProduct);

        // Returns raw rows now that CultureInfoHelper/DateHelper live in Domain and formatting
        // moved to ReportPresenter - this used to build a pre-formatted-string DataTable here.
        List<ProductReportRow> Report(string categoryId);
    }
}
