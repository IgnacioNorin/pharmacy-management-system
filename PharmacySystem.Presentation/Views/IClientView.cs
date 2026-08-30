using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // Passive View, same shape as ISupplierView. Clients are PharmacySystem.Model.Person rows
    // with person_type_id = PersonType.Cliente and no password - that mapping is the presenter's
    // job, not the view's, so this interface only exposes the raw text fields.
    public interface IClientView
    {
        int SelectedIndex { get; }
        int PersonId { get; }
        string Document { get; }
        string Name { get; }
        string Address { get; }
        string Phone { get; }
        // Fiscal profile, used when this client is the recipient of a Factura.
        string BusinessName { get; }
        string Activity { get; }
        string Commune { get; }
        string Email { get; }
        bool IsCompany { get; }

        // Free-text term for the server-side search (matches name / document / business name / email).
        string SearchText { get; }

        List<string> Validate();
        bool ConfirmDelete();

        // Replaces the whole grid with one page of rows.
        void LoadClients(IEnumerable<ClientRow> clients);
        // Updates the pager: current page, total pages and total row count.
        void SetPageInfo(int currentPage, int totalPages, int totalCount);
        void ClearForm();
        void ShowMessage(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);
    }
}
