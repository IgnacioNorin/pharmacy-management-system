using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin: the duplicate-code check lives in sp_create_product/sp_update_product, and the
    // physical-delete-vs-soft-delete decision lives inside sp_delete_product (fixed earlier this
    // session). Nothing here needs to duplicate that.
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        public int Register(Product obj) => _repository.Register(obj);

        public bool Update(Product obj) => _repository.Update(obj);

        public List<Product> List() => _repository.List();

        public List<ProductLot> GetLots(int idProduct) => _repository.GetLots(idProduct);

        public PagedResult<Product> ListPaged(int pageNumber, int pageSize, string search) =>
            _repository.ListPaged(pageNumber, pageSize, search);

        public List<Product> ListSellable() => _repository.ListSellable();

        public Product? GetSellableByCode(string code) => _repository.GetSellableByCode(code);

        public Product? GetSellableById(int idProduct) => _repository.GetSellableById(idProduct);

        public Product? GetByCode(string code) => _repository.GetByCode(code);

        public Product? GetById(int idProduct) => _repository.GetById(idProduct);

        public bool Verify(int idProduct) => _repository.Verify(idProduct);

        public bool Delete(int idProduct) => _repository.Delete(idProduct);

        public List<ProductReportRow> Report(string categoryId) => _repository.Report(categoryId);

        public bool SetSalePrice(int idProduct, decimal salePrice, string reason, int? userId) =>
            _repository.SetSalePrice(idProduct, salePrice, reason, userId);

        public bool Unrelease(int idProduct, string reason, int? userId) =>
            _repository.Unrelease(idProduct, reason, userId);

        public List<ProductPriceHistoryEntry> GetPriceHistory(int idProduct) =>
            _repository.GetPriceHistory(idProduct);
    }
}
