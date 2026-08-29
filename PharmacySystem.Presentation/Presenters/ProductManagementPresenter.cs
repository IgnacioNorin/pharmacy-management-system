using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmManagement.cs's Product region. Both Register and Update return silently
    // on failure (`if (!result) return;` inside each branch), same shape as SupplierPresenter -
    // the trailing `if (result) CleanProduct(); else MessageBox.Show(...)` is dead code in the
    // original for the same reason it was in frmSupplier.cs, and stays dead here too.
    //
    // Prices are a separate capability: the core Register/Update never touch purchase_price /
    // sale_price. When the role has "productos.editar_precios" the two price fields are enabled
    // and, if filled in, written through _productService.SetPrices after the core save. This is
    // the only way to change a price without registering a purchase.
    public class ProductManagementPresenter
    {
        private const string PriceEditPermission = "productos.editar_precios";

        private readonly IProductManagementView _view;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly CurrentUser _currentUser;

        public ProductManagementPresenter(IProductManagementView view, IProductService productService, ICategoryService categoryService, CurrentUser currentUser)
        {
            _view = view;
            _productService = productService;
            _categoryService = categoryService;
            _currentUser = currentUser;
        }

        public void OnLoad()
        {
            var categoryOptions = _categoryService.List().Select(c => new ComboBoxItem { Value = c.IdCategory, Text = c.description });
            _view.LoadCategoryOptions(categoryOptions);

            var products = _productService.List().Select(ToRow);
            _view.LoadProducts(products);

            _view.SetPriceEditingEnabled(Can(PriceEditPermission));
        }

        public void OnSave()
        {
            if (!Can("productos.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar productos.");
                return;
            }

            var errors = new List<string>(_view.Validate());

            // Prices are only considered when the role may edit them and at least one field was
            // filled in. Both must be present and valid, or neither - SetPrices writes the pair.
            bool applyPrices = false;
            decimal purchasePrice = 0m;
            decimal salePrice = 0m;
            if (Can(PriceEditPermission))
            {
                string purchaseInput = (_view.PurchasePriceText ?? "").Trim();
                string saleInput = (_view.SalePriceText ?? "").Trim();

                if (purchaseInput.Length > 0 || saleInput.Length > 0)
                {
                    int errorsBefore = errors.Count;
                    if (!TryParsePrice(purchaseInput, out purchasePrice))
                    {
                        errors.Add("Precio de compra : ingrese un monto valido no negativo.");
                    }
                    if (!TryParsePrice(saleInput, out salePrice))
                    {
                        errors.Add("Precio de venta : ingrese un monto valido no negativo.");
                    }
                    applyPrices = errors.Count == errorsBefore;
                }
            }

            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            Product product = new Product
            {
                idProduct = _view.ProductId,
                code = _view.Code?.Trim(),
                name = _view.Name?.Trim(),
                description = _view.Description?.Trim(),
                taxAffected = _view.TaxAffected,
                oCategory = new Categories { IdCategory = _view.SelectedCategoryId }
            };

            string purchaseText = applyPrices ? CultureInfoHelper.CultureInfoConverterDecimal(purchasePrice) : null;
            string saleText = applyPrices ? CultureInfoHelper.CultureInfoConverterDecimal(salePrice) : null;

            if (product.idProduct == 0)
            {
                int id = _productService.Register(product);
                if (id == 0)
                {
                    return;
                }

                if (applyPrices && !_productService.SetPrices(id, purchasePrice, salePrice))
                {
                    _view.ShowMessage("El producto se guardo, pero no se pudieron guardar los precios.");
                    purchaseText = saleText = null;
                }

                _view.AddRow(new ManagementProductRow
                {
                    Id = id,
                    Code = product.code,
                    Name = product.name,
                    Description = product.description,
                    CategoryText = _view.SelectedCategoryText,
                    TaxAffected = product.taxAffected,
                    Stock = "0",
                    PurchasePriceText = purchaseText,
                    SalePriceText = saleText
                });
            }
            else
            {
                if (!_productService.Update(product))
                {
                    return;
                }

                if (applyPrices && !_productService.SetPrices(product.idProduct, purchasePrice, salePrice))
                {
                    _view.ShowMessage("El producto se guardo, pero no se pudieron guardar los precios.");
                    purchaseText = saleText = null;
                }

                _view.ReplaceRow(_view.SelectedIndex - 1, new ManagementProductRow
                {
                    Id = product.idProduct,
                    Code = product.code,
                    Name = product.name,
                    Description = product.description,
                    CategoryText = _view.SelectedCategoryText,
                    TaxAffected = product.taxAffected,
                    // Stock/expiration aren't touched on update, same as the original, which
                    // never rewrote those two grid cells in the else branch.
                    Stock = null,
                    ExpirationDateText = null,
                    // Prices are rewritten only when they actually changed (null = leave the cell).
                    PurchasePriceText = purchaseText,
                    SalePriceText = saleText
                });
            }

            _view.ClearForm();
        }

        public void OnDelete()
        {
            if (_view.SelectedIndex <= 0)
            {
                return;
            }

            if (!Can("productos.eliminar"))
            {
                _view.ShowMessage("No tiene permiso para eliminar productos.");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (!_productService.Delete(_view.ProductId))
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
                return;
            }

            _view.RemoveRow(_view.SelectedIndex - 1);
            _view.ClearForm();
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        // Accepts a non-negative amount in the "##.##" form the UI hints at (and the currency
        // string the grid shows back). Empty or malformed input fails.
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

        private static ManagementProductRow ToRow(Product p)
        {
            const string epoch = "01/01/0001";
            string shortDate = p.expirationDate.ToShortDateString();

            return new ManagementProductRow
            {
                Id = p.idProduct,
                Code = p.code,
                Name = p.name,
                Description = p.description,
                CategoryText = p.oCategory.description,
                TaxAffected = p.taxAffected,
                Stock = p.stock.ToString(),
                ExpirationDateText = shortDate == epoch ? "" : shortDate,
                PurchasePriceText = CultureInfoHelper.CultureInfoConverterDecimal(p.purchasePrice),
                SalePriceText = CultureInfoHelper.CultureInfoConverterDecimal(p.salePrice)
            };
        }
    }
}
