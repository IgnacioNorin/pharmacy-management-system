using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmManagement.cs's Product region. Both Register and Update return silently
    // on failure (`if (!result) return;` inside each branch), same shape as SupplierPresenter -
    // the trailing `if (result) CleanProduct(); else MessageBox.Show(...)` is dead code in the
    // original for the same reason it was in frmSupplier.cs, and stays dead here too.
    public class ProductManagementPresenter
    {
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
        }

        public void OnSave()
        {
            if (!Can("productos.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar productos.");
                return;
            }

            var errors = _view.Validate();
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

            if (product.idProduct == 0)
            {
                int id = _productService.Register(product);
                if (id == 0)
                {
                    return;
                }

                _view.AddRow(new ManagementProductRow
                {
                    Id = id,
                    Code = product.code,
                    Name = product.name,
                    Description = product.description,
                    CategoryText = _view.SelectedCategoryText,
                    TaxAffected = product.taxAffected,
                    Stock = "0"
                });
            }
            else
            {
                if (!_productService.Update(product))
                {
                    return;
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
                    ExpirationDateText = null
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
                ExpirationDateText = shortDate == epoch ? "" : shortDate
            };
        }
    }
}
