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

        public bool Verify(int idProduct) => _repository.Verify(idProduct);

        public bool Delete(int idProduct) => _repository.Delete(idProduct);
    }
}
