using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface IUserView
    {
        int SelectedIndex { get; }
        int RowCount { get; }
        int UserId { get; }
        string Document { get; }
        string Name { get; }
        string Password { get; }
        string ConfirmPassword { get; }
        int RoleId { get; }
        string RoleText { get; }

        List<string> Validate();
        bool ConfirmDelete();

        void LoadRoleOptions(IEnumerable<ComboBoxItem> options);
        void LoadUsers(IEnumerable<UserRow> users);
        void AddRow(UserRow row);
        void ReplaceRow(int index, UserRow row);
        void RemoveRow(int index);
        void ClearForm();
        void ShowMessage(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);
        void ShowPasswordMismatch();
    }
}
