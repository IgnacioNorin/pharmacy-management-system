using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from ModalProduct.cs's fillProductList(). The original's "frmSale" branch filtered
    // on `p != null && p.expirationDate != null && p.stock > 0`; expirationDate is a non-nullable
    // DateTime, so comparing it to null is a tautology (always true) and p is never null inside a
    // foreach over List<Product>. Both clauses are dropped here as pure no-ops - not a behavior
    // change, since neither could ever evaluate false. Only p.stock > 0 ever filtered anything.
    public class ProductPickerPresenter
    {
        private readonly IProductPickerView _view;
        private readonly IProductService _service;
        private readonly string _origin;

        public ProductPickerPresenter(IProductPickerView view, IProductService service, string origin)
        {
            _view = view;
            _service = service;
            _origin = origin;
        }

        public void OnLoad()
        {
            var products = _service.List().AsEnumerable();

            if (_origin == "frmSale")
            {
                products = products.Where(p => p.stock > 0);
            }
            else if (_origin != "frmPurchase")
            {
                products = Enumerable.Empty<Product>();
            }

            _view.LoadProducts(products.Select(ProductPickerRow.From));
        }
    }
}
