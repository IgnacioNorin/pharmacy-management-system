using System;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmSupplier.cs's btnSave_Click/btnDelete_Click/frmSupplier_Load. Behavior is
    // preserved, including one asymmetry worth flagging rather than silently fixing:
    // a failed Register shows a message, but a failed Update does not (mirrors the original
    // `if (!result) return;` with no MessageBox). See SupplierPresenterTests for both cases.
    //
    // The grid is server-paged: ListPaged returns one page plus the total count, the search
    // box is a WHERE clause, and a successful save or delete reloads the current page.
    public class SupplierPresenter
    {
        private readonly ISupplierView _view;
        private readonly ISupplierService _service;
        private readonly CurrentUser _currentUser;
        private readonly ISecurityAudit _audit;

        private const int PageSize = PagedResult<Supplier>.DefaultPageSize;

        private int _page = 1;
        private int _totalPages = 1;
        private string _search = string.Empty;

        public SupplierPresenter(ISupplierView view, ISupplierService service, CurrentUser currentUser, ISecurityAudit audit)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _audit = audit;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;
        private int ActorId => _currentUser?.PersonId ?? 0;

        public void OnLoad() => LoadPage(1);

        public void OnSearch()
        {
            _search = (_view.SearchText ?? string.Empty).Trim();
            LoadPage(1);
        }

        public void OnFirstPage() => LoadPage(1);

        public void OnPreviousPage() => LoadPage(_page - 1);

        public void OnNextPage() => LoadPage(_page + 1);

        public void OnLastPage() => LoadPage(_totalPages);

        private void LoadPage(int requestedPage)
        {
            int page = requestedPage < 1 ? 1 : requestedPage;

            PagedResult<Supplier> result = _service.ListPaged(page, PageSize, _search);

            if (result.TotalCount > 0 && page > result.TotalPages)
            {
                result = _service.ListPaged(result.TotalPages, PageSize, _search);
            }

            _totalPages = result.TotalPages;
            _page = result.TotalPages == 0 ? 1 : Math.Min(Math.Max(result.PageNumber, 1), result.TotalPages);

            _view.LoadSuppliers(result.Items.Select(SupplierRow.From));
            _view.SetPageInfo(_page, _totalPages, result.TotalCount);
        }

        public void OnSave()
        {
            if (!Can("proveedores.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar proveedores.");
                return;
            }

            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (_view.SelectedIndex < 0 || _view.SelectedIndex > _view.RowCount)
            {
                return;
            }

            Supplier supplier = new Supplier
            {
                idSupplier = _view.SupplierId,
                document = (_view.Document ?? string.Empty).Trim(),
                companyName = (_view.CompanyName ?? string.Empty).Trim(),
                email = (_view.Email ?? string.Empty).Trim(),
                phone = (_view.Phone ?? string.Empty).Trim()
            };

            if (supplier.idSupplier == 0)
            {
                int newId = _service.Register(supplier);
                if (newId == 0)
                {
                    _view.ShowMessage("Ya existe un proveedor con ese documento");
                    return;
                }
                _audit.Record(ActorId, "supplier.create", "supplier", newId, $"'{supplier.companyName}' (doc {supplier.document})");
            }
            else
            {
                if (!_service.Update(supplier))
                {
                    return;
                }
                _audit.Record(ActorId, "supplier.update", "supplier", supplier.idSupplier, $"'{supplier.companyName}' (doc {supplier.document})");
            }

            _view.ClearForm();
            LoadPage(_page);
        }

        public void OnDelete()
        {
            if (_view.SelectedIndex <= 0)
            {
                _view.ShowMessage("No se pudo eliminar, seleccione un proveedor");
                return;
            }

            if (!Can("proveedores.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para eliminar proveedores.");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (!_service.Delete(_view.SupplierId))
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
                return;
            }

            _audit.Record(ActorId, "supplier.delete", "supplier", _view.SupplierId, $"'{_view.CompanyName}' (doc {_view.Document})");
            _view.ClearForm();
            LoadPage(_page);
        }
    }
}
