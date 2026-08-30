using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmManagement.cs's Product region. Both Register and Update return silently
    // on failure (`if (!result) return;` inside each branch), same shape as SupplierPresenter.
    //
    // The grid is server-paged: ListPaged returns one PageSize slice plus the total count, and
    // the free-text search is a WHERE clause, not an in-memory row filter. After any successful
    // create / edit / delete the current page is reloaded so the count and the row membership
    // stay correct.
    //
    // This screen never touches prices or the release state. The purchase flow moves stock and
    // cost; the Prices screen (ProductPricePresenter) sets the sale price and releases the
    // product for sale.
    public class ProductManagementPresenter
    {
        private readonly IProductManagementView _view;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly CurrentUser _currentUser;

        private const int PageSize = PagedResult<Product>.DefaultPageSize;

        private int _page = 1;
        private int _totalPages = 1;
        private string _search = string.Empty;

        public ProductManagementPresenter(IProductManagementView view, IProductService productService, ICategoryService categoryService, CurrentUser currentUser)
        {
            _view = view;
            _productService = productService;
            _categoryService = categoryService;
            _currentUser = currentUser;
        }

        public void OnLoad()
        {
            // Includes the current category of every active product even if it was soft-deleted,
            // so editing such a product does not silently reassign it (DEF-10).
            var categoryOptions = _categoryService.ListForProductForm().Select(c => new ComboBoxItem { Value = c.IdCategory, Text = c.description });
            _view.LoadCategoryOptions(categoryOptions);

            LoadPage(1);
        }

        public void OnSearch()
        {
            _search = _view.SearchText?.Trim() ?? string.Empty;
            LoadPage(1);
        }

        public void OnFirstPage() => LoadPage(1);

        public void OnPreviousPage() => LoadPage(_page - 1);

        public void OnNextPage() => LoadPage(_page + 1);

        public void OnLastPage() => LoadPage(_totalPages);

        private void LoadPage(int requestedPage)
        {
            int page = requestedPage < 1 ? 1 : requestedPage;

            PagedResult<Product> result = _productService.ListPaged(page, PageSize, _search);

            // A shrinking list (e.g. the last row of the last page was just deleted) can leave the
            // requested page past the end - fall back to the new last page once.
            if (result.TotalCount > 0 && page > result.TotalPages)
            {
                result = _productService.ListPaged(result.TotalPages, PageSize, _search);
            }

            _totalPages = result.TotalPages;
            _page = result.TotalPages == 0 ? 1 : Math.Min(Math.Max(result.PageNumber, 1), result.TotalPages);

            _view.LoadProducts(result.Items.Select(ToRow));
            _view.SetPageInfo(_page, _totalPages, result.TotalCount);
        }

        public void OnSave()
        {
            if (!Can("productos.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar productos.");
                return;
            }

            var errors = new List<string>(_view.Validate());
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
                if (_productService.Register(product) == 0)
                {
                    return;
                }
            }
            else
            {
                if (!_productService.Update(product))
                {
                    return;
                }
            }

            _view.ClearForm();
            LoadPage(_page);
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

            _view.ClearForm();
            LoadPage(_page);
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
