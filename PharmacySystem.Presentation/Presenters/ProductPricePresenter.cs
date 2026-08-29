using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Backs the Prices tab of frmManagement. This is the only place a sale price is set: doing
    // so also releases the product for sale (product.is_released = 1). The purchase flow moves
    // stock and cost only; a product bought but not released here cannot be sold.
    //
    // Every change goes through product_price_history with the cost at that moment, the user and
    // a free-text reason (SetSalePrice / Unrelease in the repository).
    public class ProductPricePresenter
    {
        private const string Permission = "productos.editar_precios";

        private readonly IProductPriceView _view;
        private readonly IProductService _productService;
        private readonly CurrentUser _currentUser;

        public ProductPricePresenter(IProductPriceView view, IProductService productService, CurrentUser currentUser)
        {
            _view = view;
            _productService = productService;
            _currentUser = currentUser;
        }

        public void OnLoad()
        {
            List<Product> all = _productService.List();

            // "To release": in stock but not yet priced/released. Something with no stock does not
            // appear until a purchase brings it in.
            _view.LoadReleasable(all.Where(p => !p.isReleased && p.stock > 0).Select(ToRow));
            _view.LoadCommercialized(all.Where(p => p.isReleased).Select(ToRow));
        }

        public void OnSelectProduct(int productId)
        {
            if (productId <= 0)
            {
                _view.LoadHistory(Enumerable.Empty<ProductPriceHistoryRow>());
                return;
            }

            _view.LoadHistory(_productService.GetPriceHistory(productId).Select(ToHistoryRow));
        }

        // Sets the sale price and releases the product for sale.
        public void OnApplyPrice()
        {
            if (!Can())
            {
                _view.ShowMessage("No tiene permiso para modificar precios.");
                return;
            }

            int id = _view.SelectedProductId;
            if (id <= 0)
            {
                _view.ShowMessage("Seleccione un producto.");
                return;
            }

            if (!TryParsePrice(_view.NewPriceText, out decimal price) || price <= 0m)
            {
                _view.ShowValidationErrors(new[] { "Precio de venta : ingrese un monto valido mayor que cero." });
                return;
            }

            if (!_productService.SetSalePrice(id, price, (_view.Reason ?? "").Trim(), _currentUser?.PersonId))
            {
                _view.ShowMessage("No se pudo guardar el precio.");
                return;
            }

            _view.ClearEntry();
            OnLoad();
            OnSelectProduct(id);
        }

        // Withdraws a product from sale without deleting it.
        public void OnUnrelease()
        {
            if (!Can())
            {
                _view.ShowMessage("No tiene permiso para modificar precios.");
                return;
            }

            int id = _view.SelectedProductId;
            if (id <= 0)
            {
                _view.ShowMessage("Seleccione un producto en comercializacion.");
                return;
            }

            if (!_productService.Unrelease(id, (_view.Reason ?? "").Trim(), _currentUser?.PersonId))
            {
                _view.ShowMessage("No se pudo retirar el producto de comercializacion.");
                return;
            }

            _view.ClearEntry();
            OnLoad();
            OnSelectProduct(id);
        }

        private bool Can() => _currentUser?.Can(Permission) ?? false;

        private static ProductPriceRow ToRow(Product p)
        {
            // Prefer the moving weighted average; fall back to the last purchase price for a
            // product that has never been costed.
            decimal cost = p.averageCost > 0m ? p.averageCost : p.purchasePrice;

            decimal? salePrice = p.isReleased ? p.salePrice : (decimal?)null;
            decimal? margin = null;
            if (salePrice.HasValue && salePrice.Value > 0m)
            {
                margin = Math.Round((salePrice.Value - cost) / salePrice.Value * 100m, 1);
            }

            return new ProductPriceRow
            {
                Id = p.idProduct,
                Code = p.code,
                Name = p.name,
                Stock = p.stock,
                Cost = cost,
                SalePrice = salePrice,
                MarginPercent = margin,
                TaxAffected = p.taxAffected
            };
        }

        private static ProductPriceHistoryRow ToHistoryRow(ProductPriceHistoryEntry e) => new ProductPriceHistoryRow
        {
            DateText = e.ChangedAt.ToString("dd-MM-yyyy HH:mm"),
            EventText = EventLabel(e.EventType),
            SalePriceText = CultureInfoHelper.FormatAsCurrency(e.SalePrice),
            CostText = e.Cost.HasValue ? CultureInfoHelper.FormatAsCurrency(e.Cost.Value) : "",
            UserName = e.UserName ?? "",
            Reason = e.Reason ?? ""
        };

        private static string EventLabel(string eventType)
        {
            switch (eventType)
            {
                case "liberacion": return "Liberación";
                case "retiro": return "Retiro";
                default: return "Cambio de precio";
            }
        }

        private static bool TryParsePrice(string text, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            try
            {
                value = CultureInfoHelper.CultureInfoConverterStringToDecimal(text);
            }
            catch
            {
                return false;
            }

            return value >= 0m;
        }
    }
}
