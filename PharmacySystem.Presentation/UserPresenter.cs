using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmUser.cs. Preserves the original's asymmetry: a failed Register shows
    // "Ya existe un usuario..." and returns; a failed Update returns completely silently (same
    // shape as SupplierPresenter, unlike ClientPresenter which always shows a message).
    public class UserPresenter
    {
        private readonly IUserView _view;
        private readonly IPersonService _service;

        public UserPresenter(IUserView view, IPersonService service)
        {
            _view = view;
            _service = service;
        }

        public void OnLoad()
        {
            var users = _service.List()
                .Where(p => p.oPersonType.idPersonType != 3)
                .Select(p => new UserRow
                {
                    Id = p.idPerson,
                    Document = p.document,
                    Name = p.name,
                    RoleText = p.oPersonType.description,
                    Password = p.password
                });
            _view.LoadUsers(users);
        }

        public void OnSave()
        {
            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (_view.Password != _view.ConfirmPassword)
            {
                _view.ShowPasswordMismatch();
                return;
            }

            if (_view.SelectedIndex < 0 || _view.SelectedIndex > _view.RowCount)
            {
                return;
            }

            Person person = new Person
            {
                idPerson = _view.UserId,
                document = _view.Document?.Trim(),
                name = _view.Name?.Trim(),
                address = "",
                phone = "",
                password = _view.Password,
                oPersonType = new TypePerson { idPersonType = _view.RoleId }
            };

            if (person.idPerson == 0)
            {
                if (!_service.Register(person))
                {
                    _view.ShowMessage("Ya existe un usuario con esa Cedula de Identidad");
                    return;
                }

                _view.AddRow(new UserRow
                {
                    Id = person.idPerson,
                    Document = person.document,
                    Name = person.name,
                    RoleText = _view.RoleText,
                    Password = person.password
                });
                _view.ClearForm();
            }
            else
            {
                if (!_service.Update(person))
                {
                    return;
                }

                _view.ReplaceRow(_view.SelectedIndex - 1, new UserRow
                {
                    Id = person.idPerson,
                    Document = person.document,
                    Name = person.name,
                    RoleText = _view.RoleText,
                    Password = person.password
                });
                _view.ClearForm();
            }
        }

        public void OnDelete()
        {
            if (_view.SelectedIndex <= 0)
            {
                _view.ShowMessage("No se pudo eliminar, seleccione un usuario");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (!_service.Delete(_view.UserId))
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
                return;
            }

            _view.RemoveRow(_view.SelectedIndex - 1);
            _view.ClearForm();
        }
    }
}
