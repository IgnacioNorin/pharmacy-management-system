using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmManagement.cs's Category region. Both Register and Update failures return
    // silently in the original (`if (!result) ... else MessageBox...` after both branches already
    // handled success) - wait, unlike Supplier/User, frmManagement's btnSaveCategory_Click does
    // NOT return early on failure inside each branch; it falls through to a shared
    // `if (result) CleanCategory(); else MessageBox.Show(...)`, so a failed Register OR Update
    // both show "No se pudo guardar los cambios". OnDelete does nothing at all (no message) when
    // there's no selection, matching ClientPresenter's silence rather than SupplierPresenter's.
    public class CategoryManagementPresenter
    {
        private readonly ICategoryManagementView _view;
        private readonly ICategoryService _service;
        private readonly CurrentUser _currentUser;

        public CategoryManagementPresenter(ICategoryManagementView view, ICategoryService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            _view.LoadCategories(_service.List().Select(CategoryRow.From));
        }

        public void OnSave()
        {
            if (!Can("categorias.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar categorias.");
                return;
            }

            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            Categories category = new Categories
            {
                IdCategory = _view.CategoryId,
                description = _view.Description?.Trim()
            };

            bool result;
            if (category.IdCategory == 0)
            {
                int id = _service.Register(category);
                result = id != 0;
                if (result)
                {
                    _view.AddRow(new CategoryRow { Id = id, Description = category.description });
                }
            }
            else
            {
                result = _service.Update(category);
                if (result)
                {
                    _view.ReplaceRow(_view.SelectedIndex - 1, new CategoryRow { Id = category.IdCategory, Description = category.description });
                }
            }

            if (result)
            {
                RefreshProductCategoryOptions();
                _view.ClearForm();
            }
            else
            {
                _view.ShowMessage("No se pudo guardar los cambios\nRevise los datos");
            }
        }

        public void OnDelete()
        {
            if (_view.SelectedIndex <= 0)
            {
                return;
            }

            if (!Can("categorias.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para eliminar categorias.");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (!_service.Delete(_view.CategoryId))
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
                return;
            }

            RefreshProductCategoryOptions();
            _view.RemoveRow(_view.SelectedIndex - 1);
            _view.ClearForm();
        }

        private void RefreshProductCategoryOptions()
        {
            var options = _service.List().Select(c => new ComboBoxItem { Value = c.IdCategory, Text = c.description });
            _view.RefreshProductCategoryOptions(options);
        }
    }
}
