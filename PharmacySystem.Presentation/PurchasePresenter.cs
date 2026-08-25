using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmPurchase.cs. Preserves two real quirks from the original:
    // - btnAdd_Click silently does nothing (no message) when the product is already in the cart -
    //   `if (!product_exists) { ... }` had no else branch.
    // - The total registered with the purchase is the same total shown on screen: originally that
    //   meant re-parsing the formatted lbltotalamount.Text; here it's the same sum computed once
    //   from the cart lines, which is equivalent because FormatAsCurrency/ConverterStringToDecimal
    //   round-trip exactly (see CultureInfoHelperTests).
    public class PurchasePresenter
    {
        private readonly IPurchaseView _view;
        private readonly IPurchaseService _purchaseService;
        private readonly IProductService _productService;
        private readonly int _idPerson;

        public PurchasePresenter(IPurchaseView view, IPurchaseService purchaseService, IProductService productService, int idPerson)
        {
            _view = view;
            _purchaseService = purchaseService;
            _productService = productService;
            _idPerson = idPerson;
        }

        public void OnProductCodeEntered(string code)
        {
            Product product = _productService.List().FirstOrDefault(p => p.code == code);
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
            decimal priceSale;
            try
            {
                pricePurchase = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PricePurchaseText);
            }
            catch
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Precio Compra\nEjemplo Formato ##.##");
                return;
            }

            try
            {
                priceSale = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PriceSaleText);
            }
            catch
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##");
                return;
            }

            bool productExists = _view.CartLines.Any(l => l.ProductId == _view.SelectedProductId);
            if (productExists)
            {
                return;
            }

            decimal subTotal = _view.Amount * pricePurchase;

            _view.AddCartLine(new PurchaseCartLine
            {
                ProductId = _view.SelectedProductId,
                Code = _view.SelectedProductCode,
                Name = _view.SelectedProductName,
                Quantity = _view.Amount,
                ExpirationDate = _view.ExpirationDate,
                PurchasePrice = pricePurchase,
                SalePrice = priceSale,
                SubTotal = subTotal
            });

            _view.ClearProductEntry();
            RecalculateTotal();
        }

        public void OnRemoveProduct(int index)
        {
            _view.RemoveCartLineAt(index);
            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            decimal total = _view.CartLines.Sum(l => l.SubTotal);
            _view.SetTotalText(CultureInfoHelper.FormatAsCurrency(total));
        }

        public void OnFinishPurchase()
        {
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

            if (_view.CartLines.Count < 1)
            {
                _view.ShowMessage("Debe ingresar un producto como minimo\npara registrar una compra");
                return;
            }

            decimal totalAmount = _view.CartLines.Sum(l => l.SubTotal);

            Purchase purchase = new Purchase
            {
                oPerson = new Person { idPerson = _idPerson },
                oSupplier = new Supplier { idSupplier = _view.SelectedSupplierId },
                totalAmount = totalAmount,
                documentType = _view.DocumentType,
                documentNumber = _view.DocumentNumber.Trim(),
                oPurchaseDetail = _view.CartLines.Select(l => new PurchaseDetail
                {
                    oProduct = new Product { idProduct = l.ProductId },
                    quantity = (int)l.Quantity,
                    expirationDate = l.ExpirationDate,
                    purchasePrice = l.PurchasePrice,
                    salePrice = l.SalePrice,
                    total = l.SubTotal
                }).ToList()
            };

            if (_purchaseService.Register(purchase))
            {
                _view.ClearPurchase();
                _view.ShowMessage("La compra fue registrada");
            }
            else
            {
                _view.ShowMessage("No se pudo registrar la compra");
            }
        }
    }
}
