using System;
using System.Globalization;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmManagement.cs's Store region. The original compared `store == null` to
    // decide between "Se guardaron los datos ingresados" and "Se actualizaron los datos
    // ingresados exitosamente" - but ListStore() (both before and after this migration) never
    // returns null, only an empty Store() at worst. That comparison was always false, so the
    // "guardaron" branch was dead: every successful save always showed "actualizaron", which is
    // the only message this presenter shows on success.
    public class StoreManagementPresenter
    {
        private readonly IStoreManagementView _view;
        private readonly IStoreService _service;
        private readonly CurrentUser _currentUser;

        public StoreManagementPresenter(IStoreManagementView view, IStoreService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            Store store = _service.ListStore();
            _view.LoadStoreFields(store.document, store.companyName, store.email, store.phone, store.address);
            _view.SetTaxRate(store.defaultTaxRate.ToString("0.##", CultureInfo.InvariantCulture));

            var options = CultureInfoHelper.SupportedCurrencies;
            int currencyIndex = options.ToList()
                .FindIndex(c => string.Equals((string)c.Value, store.currencyCulture, StringComparison.OrdinalIgnoreCase));
            _view.LoadCurrencyOptions(options, currencyIndex >= 0 ? currencyIndex : 0);

            _view.SetCurrencyEditable(!_service.HasOperationalData());
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

            string selectedCurrency = _view.SelectedCurrency;

            bool isSuccess = _service.UpdateStore(new Store
            {
                document = _view.Document,
                companyName = _view.CompanyName,
                email = _view.Email,
                phone = _view.Phone,
                address = _view.Address,
                currencyCulture = selectedCurrency,
                defaultTaxRate = taxRate
            });

            if (isSuccess)
            {
                CultureInfoHelper.SetCurrency(selectedCurrency);
                _view.ShowInfo("Se actualizaron los datos ingresados exitosamente");
            }
            else
            {
                _view.ShowError("No se pudo guardar los datos ingresados\nRevise los datos");
            }
        }
    }
}
