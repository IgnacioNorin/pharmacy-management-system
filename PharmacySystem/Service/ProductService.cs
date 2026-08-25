using PharmacySystem.Data;
using PharmacySystem.Model;
using System.Collections.Generic;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmManagement.cs, frmPurchase.cs,
    // frmSale.cs, ModalProduct.cs). Delegates to PharmacySystem.Business.
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
    }
}
