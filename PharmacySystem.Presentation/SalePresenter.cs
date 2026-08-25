using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmSale.cs. Preserves several real quirks from the original rather than fixing
    // them, since this migration must not change observable behavior:
    // - CalculateChange() always returns true (the "Error al convertir..." message it can trigger
    //   is dead code); moneyToPay there is parsed with a plain Convert.ToDecimal, not the
    //   culture-aware converter used everywhere else, unlike totalPay.
    // - OnFinishSale's first ControlStock check reads the *product-entry* fields (SelectedProductId
    //   / Amount), not the cart - which are usually back to "0"/1 after CleanProduct() ran on the
    //   last add. If that check fails, the whole sale silently does nothing (no message), exactly
    //   like the original's `if (result) { ... }` with no else.
    public class SalePresenter
    {
        private readonly ISaleView _view;
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly int _idPerson;

        public SalePresenter(ISaleView view, ISaleService saleService, IProductService productService, int idPerson)
        {
            _view = view;
            _saleService = saleService;
            _productService = productService;
            _idPerson = idPerson;
        }

        public void OnProductCodeEntered(string code)
        {
            Product product = _productService.List().FirstOrDefault(p => p.code == code);
            if (product != null)
            {
                _view.SetSelectedProduct(product.idProduct, product.code, product.name, product.stock, CultureInfoHelper.FormatAsCurrency(product.salePrice));
            }
        }

        public void OnAddProduct()
        {
            if (_view.SelectedProductId == 0)
            {
                _view.ShowMessage("Debe seleccionar un producto primero");
                return;
            }

            if (_view.Stock < _view.Amount)
            {
                _view.ShowMessage("No hay suficiente stock del producto");
                return;
            }

            decimal priceSale;
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
                _view.ShowMessage("El producto ya fue agregado\nElimínelo e ingrese el nuevo si quiere cambiar la cantidad.");
                return;
            }

            decimal subTotal = _view.Amount * priceSale;

            _view.AddCartLine(new SaleCartLine
            {
                ProductId = _view.SelectedProductId,
                Name = _view.SelectedProductName,
                Quantity = _view.Amount,
                SalePrice = priceSale,
                SubTotal = subTotal
            });

            RecalculateTotal();
            _view.ClearProductEntry();
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

        public void OnCalculateChangeRequested()
        {
            if (!CalculateChange())
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##");
            }
        }

        private bool CalculateChange()
        {
            decimal moneyToPay = Convert.ToDecimal(_view.PayWithText);
            decimal totalPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.TotalPayText);

            if (moneyToPay < totalPay)
            {
                _view.SetChangeText(CultureInfoHelper.FormatAsCurrency(0));
            }
            else
            {
                _view.SetChangeText(CultureInfoHelper.FormatAsCurrency(moneyToPay - totalPay));
            }

            return true;
        }

        public void OnFinishSale()
        {
            if (_view.DocumentClient.Trim() == "" || _view.NameClient.Trim() == "")
            {
                _view.ShowMessage("Debe ingresar todos los datos del cliente");
                return;
            }

            if (_view.CartLines.Count < 1)
            {
                _view.ShowMessage("Debe ingresar un producto como minimo\npara registrar una venta");
                return;
            }

            if (_view.PayWithText.Trim() == "0")
            {
                _view.ShowMessage("Debe ingresar con cuanto paga el cliente");
                return;
            }

            if (!CalculateChange())
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##");
                return;
            }

            decimal moneyToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PayWithText);
            decimal totalToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.TotalPayText);
            decimal changeMoney = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.ChangeText);

            if (totalToPay > moneyToPay)
            {
                _view.ShowMessage("Falta dinero para pagar");
                return;
            }

            bool result = _saleService.ControlStock(_view.SelectedProductId, (int)_view.Amount, true);
            if (!result)
            {
                return;
            }

            var details = new List<SaleDetail>();

            foreach (SaleCartLine line in _view.CartLines)
            {
                bool existsProduct = _productService.Verify(line.ProductId);

                if (existsProduct)
                {
                    details.Add(new SaleDetail
                    {
                        oProduct = new Product { idProduct = line.ProductId },
                        amount = (int)line.Quantity,
                        salePrice = line.SalePrice,
                        subtotal = line.SubTotal
                    });

                    bool subtractStock = _saleService.ControlStock(line.ProductId, (int)line.Quantity, true);
                    if (!subtractStock)
                    {
                        details.Clear();
                        _view.ShowMessage("No se pudo registrar la venta\n Problema con Stock");
                        return;
                    }
                }
                else
                {
                    _view.ShowMessage("No se pudo registrar la venta\n Problema con producto");
                    return;
                }
            }

            Sale sale = new Sale
            {
                typeDocument = _view.DocumentType,
                oPerson = new Person { idPerson = _idPerson },
                documentClient = _view.DocumentClient.Trim(),
                nameClient = _view.NameClient.Trim(),
                totalPay = totalToPay,
                payWith = moneyToPay,
                change = changeMoney,
                oSaleDetail = details
            };

            int idSale = _saleService.Register(sale);

            if (idSale != 0)
            {
                _view.ClearSale();
                _view.SaleRegistered(idSale);
            }
            else
            {
                _view.ShowMessage("No se pudo registrar la venta");
            }
        }
    }
}
