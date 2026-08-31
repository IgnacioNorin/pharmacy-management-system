using System;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmClient.cs. Preserves two real quirks from the original rather than fixing
    // them in passing:
    //  - OnSave shows "No se pudo guardar los cambios" on failure for BOTH register and update
    //    (unlike SupplierPresenter, where a failed Update returns silently).
    //  - OnDelete does nothing at all (no message) when nothing is selected.
    //
    // The grid is server-paged: ListClientsPaged returns one page plus the total count, the
    // search box is a WHERE clause, and a successful save or delete reloads the current page.
    public class ClientPresenter
    {
        private readonly IClientView _view;
        private readonly IClientService _service;
        private readonly CurrentUser _currentUser;
        private readonly ISecurityAudit _audit;

        private const int PageSize = PagedResult<Client>.DefaultPageSize;

        private int _page = 1;
        private int _totalPages = 1;
        private string _search = string.Empty;

        public ClientPresenter(IClientView view, IClientService service, CurrentUser currentUser, ISecurityAudit audit)
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

            PagedResult<Client> result = _service.ListClientsPaged(page, PageSize, _search);

            if (result.TotalCount > 0 && page > result.TotalPages)
            {
                result = _service.ListClientsPaged(result.TotalPages, PageSize, _search);
            }

            _totalPages = result.TotalPages;
            _page = result.TotalPages == 0 ? 1 : Math.Min(Math.Max(result.PageNumber, 1), result.TotalPages);

            _view.LoadClients(result.Items.Select(ClientRow.From));
            _view.SetPageInfo(_page, _totalPages, result.TotalCount);
        }

        public void OnSave()
        {
            if (!Can("clientes.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para crear o editar clientes.");
                return;
            }

            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (_view.IsCompany && (string.IsNullOrWhiteSpace(_view.BusinessName) || string.IsNullOrWhiteSpace(_view.Activity)))
            {
                _view.ShowValidationErrors(new[] { "Para una empresa, la razón social y el giro son obligatorios." });
                return;
            }

            Client client = new Client
            {
                idClient = _view.PersonId,
                document = _view.Document?.Trim(),
                name = _view.Name?.Trim(),
                address = _view.Address?.Trim(),
                phone = _view.Phone?.Trim(),
                businessName = _view.BusinessName?.Trim(),
                activity = _view.Activity?.Trim(),
                commune = _view.Commune?.Trim(),
                email = _view.Email?.Trim(),
                isCompany = _view.IsCompany
            };

            bool isNew = client.idClient == 0;
            int newId = isNew ? _service.Register(client) : 0;
            bool result = isNew ? newId != 0 : _service.Update(client);

            if (result)
            {
                _audit.Record(ActorId, isNew ? "client.create" : "client.update", "client",
                    isNew ? newId : client.idClient, $"'{client.name}' (doc {client.document})");
                _view.ClearForm();
                LoadPage(_page);
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

            if (!Can("clientes.gestionar"))
            {
                _view.ShowMessage("No tiene permiso para eliminar clientes.");
                return;
            }

            if (!_view.ConfirmDelete())
            {
                return;
            }

            if (_service.Delete(_view.PersonId))
            {
                _audit.Record(ActorId, "client.delete", "client", _view.PersonId, $"'{_view.Name}' (doc {_view.Document})");
                _view.ClearForm();
                LoadPage(_page);
            }
            else
            {
                _view.ShowMessage("No se pudo eliminar el registro\nRevise los datos");
            }
        }
    }
}
