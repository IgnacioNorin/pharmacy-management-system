using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeUserView : IUserView
    {
        public int SelectedIndex { get; set; }
        public int RowCount { get; set; }
        public int UserId { get; set; }
        public string Document { get; set; }
        string IUserView.Name => Name;
        public string Name { get; set; }
        public string Password { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
        public int RoleId { get; set; } = 1;
        public string RoleText { get; set; } = "Administrador";
        public List<string> ValidationErrors { get; set; } = new List<string>();
        public bool ConfirmDeleteResult { get; set; } = true;

        List<string> IUserView.Validate() => ValidationErrors;
        public bool ConfirmDelete() => ConfirmDeleteResult;

        public List<ComboBoxItem> LoadedRoleOptions { get; private set; }
        public List<UserRow> LoadedUsers { get; private set; }
        public List<UserRow> AddedRows { get; } = new List<UserRow>();
        public List<(int Index, UserRow Row)> ReplacedRows { get; } = new List<(int, UserRow)>();
        public List<int> RemovedIndexes { get; } = new List<int>();
        public bool ClearFormCalled { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }
        public int PasswordMismatchCount { get; private set; }
        public string ShownTemporaryPassword { get; private set; }

        public void LoadRoleOptions(IEnumerable<ComboBoxItem> options) => LoadedRoleOptions = options.ToList();
        public void LoadUsers(IEnumerable<UserRow> users) => LoadedUsers = users.ToList();
        public void AddRow(UserRow row) => AddedRows.Add(row);
        public void ReplaceRow(int index, UserRow row) => ReplacedRows.Add((index, row));
        public void RemoveRow(int index) => RemovedIndexes.Add(index);
        public void ClearForm() => ClearFormCalled = true;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
        public void ShowPasswordMismatch() => PasswordMismatchCount++;
        public void ShowTemporaryPassword(string tempPassword) => ShownTemporaryPassword = tempPassword;
    }
}
