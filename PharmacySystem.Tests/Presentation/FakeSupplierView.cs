using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSupplierView : ISupplierView
    {
        // Inputs the presenter reads - set these up before calling OnSave/OnDelete/OnLoad.
        public int SelectedIndex { get; set; }
        public int RowCount { get; set; }
        public int SupplierId { get; set; }
        public string Document { get; set; }
        string ISupplierView.CompanyName => CompanyName;
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool ConfirmDeleteResult { get; set; } = true;

        List<string> ISupplierView.Validate() => ValidationErrors;
        public bool ConfirmDelete() => ConfirmDeleteResult;

        // Outputs the presenter drives - assert on these after the call.
        public List<SupplierRow> LoadedSuppliers { get; private set; }
        public List<SupplierRow> AddedRows { get; } = new List<SupplierRow>();
        public List<(int Index, SupplierRow Row)> ReplacedRows { get; } = new List<(int, SupplierRow)>();
        public List<int> RemovedIndexes { get; } = new List<int>();
        public bool ClearFormCalled { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }

        public void LoadSuppliers(IEnumerable<SupplierRow> suppliers) => LoadedSuppliers = suppliers.ToList();
        public void AddRow(SupplierRow row) => AddedRows.Add(row);
        public void ReplaceRow(int index, SupplierRow row) => ReplacedRows.Add((index, row));
        public void RemoveRow(int index) => RemovedIndexes.Add(index);
        public void ClearForm() => ClearFormCalled = true;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
    }
}
