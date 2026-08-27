using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmSupplier.cs's btnSave_Click/btnDelete_Click/frmSupplier_Load. Behavior is
    // preserved exactly, including one asymmetry worth flagging rather than silently fixing:
    // a failed Register shows a message, but a failed Update does not (mirrors the original
    // `if (!result) return;` with no MessageBox). See SupplierPresenterTests for both cases
    // pinned down explicitly.
    public class SupplierPresenter
    {
        private readonly ISupplierView _view;
        private readonly ISupplierService _service;
        private readonly CurrentUser _currentUser;

        public SupplierPresenter(ISupplierView view, ISupplierService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            _view.LoadSuppliers(_service.List().ConvertAll(SupplierRow.From));
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
                document = _view.Document?.Trim(),
                companyName = _view.CompanyName?.Trim(),
                email = _view.Email?.Trim(),
                phone = _view.Phone?.Trim()
            };

            if (supplier.idSupplier == 0)
            {
                RegisterNew(supplier);
            }
            else
            {
                UpdateExisting(supplier);
            }
        }

        private void RegisterNew(Supplier supplier)
        {
            int id = _service.Register(supplier);
            if (id == 0)
            {
                _view.ShowMessage("Ya existe un proveedor con esa CI/RUC");
                return;
            }

            supplier.idSupplier = id;
            _view.AddRow(SupplierRow.From(supplier));
            _view.ClearForm();
        }

        private void UpdateExisting(Supplier supplier)
        {
            if (!_service.Update(supplier))
            {
                return;
            }

            _view.ReplaceRow(_view.SelectedIndex - 1, SupplierRow.From(supplier));
            _view.ClearForm();
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

            _view.RemoveRow(_view.SelectedIndex - 1);
            _view.ClearForm();
        }
    }
}
