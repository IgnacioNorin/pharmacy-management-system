using System.Globalization;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmManagement.cs's Store region. The system is CLP-only: there is no currency
    // or country-preset setting here anymore. The editable fields are the store profile, the VAT
    // rate (Chile 19 by default) and the default sale document type.
    public class StoreManagementPresenter
    {
        private readonly IStoreManagementView _view;
        private readonly IStoreService _service;
        private readonly CurrentUser _currentUser;
        private readonly ISecurityAudit _audit;

        public StoreManagementPresenter(IStoreManagementView view, IStoreService service, CurrentUser currentUser, ISecurityAudit audit)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _audit = audit;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            Store store = _service.ListStore();
            _view.LoadStoreFields(store.document, store.companyName, store.email, store.phone, store.address);
            _view.SetTaxRate(store.defaultTaxRate.ToString("0.##", CultureInfo.InvariantCulture));
            _view.LoadDocumentTypeOptions(DocumentTypes.Selectable, store.defaultDocumentType);
        }

        public void OnSave()
        {
            if (!Can("tienda.editar"))
            {
                _view.ShowError("No tiene permiso para modificar los datos de la tienda.");
                return;
            }

            var errors = _view.Validate();
            if (errors.Count > 0)
            {
                _view.ShowValidationErrors(errors);
                return;
            }

            if (!decimal.TryParse((_view.TaxRate ?? "").Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal taxRate)
                || taxRate < 0m || taxRate > 100m)
            {
                _view.ShowError("La tasa de IVA debe ser un número entre 0 y 100.");
                return;
            }

            bool isSuccess = _service.UpdateStore(new Store
            {
                document = _view.Document,
                companyName = _view.CompanyName,
                email = _view.Email,
                phone = _view.Phone,
                address = _view.Address,
                defaultTaxRate = taxRate,
                defaultDocumentType = _view.DefaultDocumentType
            });

            if (isSuccess)
            {
                _audit.Record(_currentUser?.PersonId ?? 0, "store.update", "store", 1,
                    $"razón social '{_view.CompanyName}', doc {_view.Document}, IVA {taxRate:0.##}%, doc por defecto {_view.DefaultDocumentType}");
                _view.ShowInfo("Se actualizaron los datos ingresados exitosamente");
            }
            else
            {
                _view.ShowError("No se pudo guardar los datos ingresados\nRevise los datos");
            }
        }
    }
}
