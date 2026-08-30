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

        // The currency loaded from the store profile, and whether the combo is editable. Once the
        // store has sales/purchases the currency is locked, so on save we send back the loaded
        // value instead of reading the (disabled) combo - which could otherwise round-trip to a
        // different string and make the business layer reject the save as "changing currency".
        private string _loadedCurrency;
        private bool _currencyEditable;

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
            _view.LoadDocumentTypeOptions(CountryPresets.ForCode(store.countryCode).SaleDocumentTypes, store.defaultDocumentType);

            _loadedCurrency = store.currencyCulture;

            var options = CultureInfoHelper.SupportedCurrencies;
            int currencyIndex = options.ToList()
                .FindIndex(c => string.Equals((string)c.Value, store.currencyCulture, StringComparison.OrdinalIgnoreCase));
            _view.LoadCurrencyOptions(options, currencyIndex >= 0 ? currencyIndex : 0);

            var presetOptions = CountryPresets.All
                .Select(p => new ComboBoxItem { Value = p.Code, Text = p.DisplayName })
                .ToList();
            int presetIndex = presetOptions.FindIndex(o =>
                string.Equals((string)o.Value, store.countryCode ?? "", StringComparison.OrdinalIgnoreCase));
            _view.LoadCountryPresetOptions(presetOptions, presetIndex >= 0 ? presetIndex : 0);

            _currencyEditable = !_service.HasOperationalData();
            _view.SetCurrencyEditable(_currencyEditable);
        }

        // A concrete preset (not "Genérico") pre-fills the VAT rate and currency; the admin can
        // still change them before saving. Picking "Genérico" leaves the fields as they are.
        public void OnCountryPresetChanged()
        {
            CountryPreset preset = CountryPresets.ForCode(_view.SelectedCountryCode);

            // The sale document types are preset-driven, so refresh them even for "Genérico"
            // (keep the current choice if the new preset still offers it).
            var docTypes = preset.SaleDocumentTypes;
            string currentDocType = _view.DefaultDocumentType;
            _view.LoadDocumentTypeOptions(docTypes, docTypes.Contains(currentDocType) ? currentDocType : docTypes[0]);

            if (preset.IsGeneric)
            {
                return;
            }
            _view.SetTaxRate(preset.DefaultTaxRate.ToString("0.##", CultureInfo.InvariantCulture));
            _view.SelectCurrency(preset.CurrencyCulture);
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

            // When the currency is locked, keep the stored value: reading the disabled combo can
            // return a different string and get the save rejected as a currency change.
            string selectedCurrency = _currencyEditable ? _view.SelectedCurrency : _loadedCurrency;

            bool isSuccess = _service.UpdateStore(new Store
            {
                document = _view.Document,
                companyName = _view.CompanyName,
                email = _view.Email,
                phone = _view.Phone,
                address = _view.Address,
                currencyCulture = selectedCurrency,
                countryCode = string.IsNullOrWhiteSpace(_view.SelectedCountryCode) ? null : _view.SelectedCountryCode.Trim(),
                defaultTaxRate = taxRate,
                defaultDocumentType = _view.DefaultDocumentType
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
