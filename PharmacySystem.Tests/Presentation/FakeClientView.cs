using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeClientView : IClientView
    {
        public int SelectedIndex { get; set; }
        public int PersonId { get; set; }
        public string Document { get; set; }
        string IClientView.Name => Name;
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string Activity { get; set; }
        public string Commune { get; set; }
        public string Email { get; set; }
        public bool IsCompany { get; set; }
        public string SearchText { get; set; } = "";
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool ConfirmDeleteResult { get; set; } = true;

        List<string> IClientView.Validate() => ValidationErrors;
        public bool ConfirmDelete() => ConfirmDeleteResult;

        public List<ClientRow> LoadedClients { get; private set; }
        public int LoadClientsCallCount { get; private set; }
        public (int CurrentPage, int TotalPages, int TotalCount)? LastPageInfo { get; private set; }
        public bool ClearFormCalled { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }

        public void LoadClients(IEnumerable<ClientRow> clients)
        {
            LoadedClients = clients.ToList();
            LoadClientsCallCount++;
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount) =>
            LastPageInfo = (currentPage, totalPages, totalCount);

        public void ClearForm() => ClearFormCalled = true;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
    }
}
