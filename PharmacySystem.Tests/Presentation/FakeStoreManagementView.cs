using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeStoreManagementView : IStoreManagementView
    {
        public string Document { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string TaxRate { get; set; } = "19";
        public string DefaultDocumentType { get; set; } = "Boleta";
        public List<string> ValidationErrors { get; set; } = new List<string>();

        List<string> IStoreManagementView.Validate() => ValidationErrors;

        public string SetTaxRateValue { get; private set; }
        public void SetTaxRate(string value) => SetTaxRateValue = value;

        public IReadOnlyList<string> LoadedDocumentTypeOptions { get; private set; }
        public string LoadedDocumentTypeSelected { get; private set; }
        public void LoadDocumentTypeOptions(IReadOnlyList<string> options, string selected)
        {
            LoadedDocumentTypeOptions = options;
            LoadedDocumentTypeSelected = selected;
        }

        public string LoadedDocument { get; private set; }
        public string LoadedCompanyName { get; private set; }
        public string LoadedEmail { get; private set; }
        public string LoadedPhone { get; private set; }
        public string LoadedAddress { get; private set; }
        public List<string> InfoMessages { get; } = new List<string>();
        public List<string> ErrorMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }

        public void LoadStoreFields(string document, string companyName, string email, string phone, string address)
        {
            LoadedDocument = document;
            LoadedCompanyName = companyName;
            LoadedEmail = email;
            LoadedPhone = phone;
            LoadedAddress = address;
        }

        public void ShowInfo(string message) => InfoMessages.Add(message);
        public void ShowError(string message) => ErrorMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
    }
}
