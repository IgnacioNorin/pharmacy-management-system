using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmPurchase.cs. Preserves a real quirk from the original: btnAdd_Click silently
    // does nothing (no message) when the product is already in the cart - `if (!product_exists)
    // { ... }` had no else branch.
    //
    // The cart itself is owned here, not read back from the grid: the View used to be the source
    // of truth for cart state, reconstructing it on every access by re-parsing the formatted
    // currency text out of each grid cell. That round-trip is safe (FormatAsCurrency and
    // CultureInfoConverterStringToDecimal are exact inverses - see CultureInfoHelperTests) but
    // fragile: any future formatting tweak could silently corrupt cart totals. Holding the cart as
    // a plain list here makes the Presenter the single source of truth and the View a pure render
    // target, closer to the intent of Passive View.
    //
    // A successful purchase raises InventoryChangeNotifier.StockChanged so MainForm can recheck
    // its stock/expiration alerts immediately instead of waiting for the next timer tick - Fase 2
    // of the alerts rework.
    public class PurchasePresenter
    {
        private readonly IPurchaseView _view;
        private readonly IPurchaseService _purchaseService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly CurrentUser _session;
        private readonly int _idPerson;
        private readonly List<PurchaseCartLine> _cart = new List<PurchaseCartLine>();

        // The store VAT rate rarely changes during a session; read it once on first use.
        private decimal? _taxRate;

        public PurchasePresenter(IPurchaseView view, IPurchaseService purchaseService, IProductService productService, IStoreService storeService, CurrentUser session, int idPerson)
        {
            _view = view;
            _purchaseService = purchaseService;
            _productService = productService;
            _storeService = storeService;
            _session = session;
            _idPerson = idPerson;
        }

        private decimal TaxRate()
        {
            if (_taxRate == null)
            {
                _taxRate = _storeService.ListStore()?.defaultTaxRate ?? 19m;
            }
            return _taxRate.Value;
        }

        public void OnProductCodeEntered(string code)
        {
            Product product = _productService.GetByCode(code);
            if (product != null)
            {
                _view.SetSelectedProduct(product.idProduct, product.code, product.name);
            }
        }

        public void OnAddProduct()
        {
            var errors = _view.ValidateProductEntry();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (_view.SelectedProductId == 0)
            {
                _view.ShowMessage("Debe seleccionar un producto primero");
                return;
            }

            decimal pricePurchase;
            try
            {
                pricePurchase = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PricePurchaseText);
            }
            catch
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Precio Compra\nEjemplo Formato ##.##");
                return;
            }

            bool productExists = _cart.Any(l => l.ProductId == _view.SelectedProductId);
            if (productExists)
            {
                // Was a silent no-op: the user could not tell why re-adding did nothing (DEF-30).
                _view.ShowMessage("El producto ya está en la compra.\nQuítelo y agréguelo de nuevo si quiere cambiar la cantidad.");
                return;
            }

            decimal subTotal = _view.Amount * pricePurchase;

            Product product = _productService.GetById(_view.SelectedProductId);

            var line = new PurchaseCartLine
            {
                ProductId = _view.SelectedProductId,
                Code = _view.SelectedProductCode,
                Name = _view.SelectedProductName,
                Quantity = _view.Amount,
                ExpirationDate = _view.ExpirationDate,
                PurchasePrice = pricePurchase,
                SubTotal = subTotal,
                TaxAffected = product?.taxAffected ?? true
            };

            _cart.Add(line);
            _view.AddCartLine(line);
            _view.ClearProductEntry();
            RecalculateTotal();
        }

        public void OnRemoveProduct(int index)
        {
            _cart.RemoveAt(index);
            _view.RemoveCartLineAt(index);
            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            decimal total = _cart.Sum(l => l.SubTotal);
            _view.SetTotalText(CultureInfoHelper.FormatAsCurrency(total));

            TaxCalculator.Breakdown vat = ComputeVat();
            _view.SetVatBreakdown(vat.Net, vat.Tax, vat.Exempt);
        }

        // Line prices are entered VAT-included, so the taxable base is backed out of the gross
        // (see TaxCalculator). Exempt lines (product.tax_affected = 0) go straight to Exempt.
        private TaxCalculator.Breakdown ComputeVat() =>
            TaxCalculator.Compute(_cart.Select(l => (l.SubTotal, l.TaxAffected)), TaxRate());

        public void OnFinishPurchase()
        {
            // Defense in depth: the action re-checks the permission, like every other sensitive
            // operation, instead of relying only on the navigation gate (DEF-22).
            if (!(_session?.Can("compras.acceso") ?? false))
            {
                _view.ShowMessage("No tiene permiso para registrar compras.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_view.DocumentNumber))
            {
                _view.ShowMessage("Debe ingresar el numero de documento\npara registrar una compra");
                _view.FocusDocumentNumber();
                return;
            }

            if (_view.SelectedSupplierId == 0)
            {
                _view.ShowMessage("Debe seleccionar un proveedor\npara registrar una compra");
                return;
            }

            if (_cart.Count < 1)
            {
                _view.ShowMessage("Debe ingresar un producto como minimo\npara registrar una compra");
                return;
            }

            TaxCalculator.Breakdown vat = ComputeVat();

            Purchase purchase = new Purchase
            {
                oPerson = new Person { idPerson = _idPerson },
                oSupplier = new Supplier { idSupplier = _view.SelectedSupplierId },
                totalAmount = vat.Total,
                netAmount = vat.Net,
                taxAmount = vat.Tax,
                exemptAmount = vat.Exempt,
                taxRate = TaxRate(),
                documentType = _view.DocumentType,
                documentNumber = _view.DocumentNumber.Trim(),
                oPurchaseDetail = _cart.Select(l => new PurchaseDetail
                {
                    oProduct = new Product { idProduct = l.ProductId },
                    quantity = (int)l.Quantity,
                    expirationDate = l.ExpirationDate,
                    purchasePrice = l.PurchasePrice,
                    total = l.SubTotal
                }).ToList()
            };

            try
            {
                if (_purchaseService.Register(purchase))
                {
                    _cart.Clear();
                    _view.ClearPurchase();
                    _view.ShowMessage("La compra fue registrada");
                    InventoryChangeNotifier.NotifyStockChanged();
                }
                else
                {
                    _view.ShowMessage("No se pudo registrar la compra");
                }
            }
            catch (DuplicateInvoiceException ex)
            {
                _view.ShowMessage(ex.Message);
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }
    }
}
