using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface IStoreManagementView
    {
        string Document { get; }
        string CompanyName { get; }
        string Email { get; }
        string Phone { get; }
        string Address { get; }
        string SelectedCurrency { get; }
        string TaxRate { get; }
        string DefaultDocumentType { get; }

        List<string> Validate();

        void LoadStoreFields(string document, string companyName, string email, string phone, string address);
        void SetTaxRate(string value);
        void LoadDocumentTypeOptions(IReadOnlyList<string> options, string selected);
        void LoadCurrencyOptions(IReadOnlyList<ComboBoxItem> options, int selectedIndex);
        void SetCurrencyEditable(bool enabled);
        void ShowInfo(string message);
        void ShowError(string message);
        void ShowValidationErrors(IReadOnlyList<string> errors);
    }
}
