using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from MainForm.cs's notificationDate()/notificationStock(). Both checks used to pull
    // every active product and filter by threshold in this loop; the threshold is now applied in
    // the repository's SQL (see NotificationConfigRepository), so this just checks whether
    // anything came back - the observable behavior (message shown whenever at least one product
    // qualifies) is unchanged from before.
    public class MainFormPresenter
    {
        private readonly IMainFormView _view;
        private readonly IStoreService _storeService;
        private readonly INotificationConfigService _notificationService;

        public MainFormPresenter(IMainFormView view, IStoreService storeService, INotificationConfigService notificationService)
        {
            _view = view;
            _storeService = storeService;
            _notificationService = notificationService;
        }

        public void OnLoad(Person person)
        {
            Store store = _storeService.ListStore();
            CultureInfoHelper.SetCurrency(store?.currencyCulture);

            _view.SetUserName(person.name);
            _view.SetAdministrativeMenusVisible(person.oPersonType.idPersonType != 2);
        }

        public void CheckExpirationWarnings()
        {
            int days = _notificationService.ConfigDay();
            List<Product> expiringProducts = _notificationService.ListExpirationDate(days);

            bool visible = expiringProducts.Count > 0;
            string message = visible ? "Hay productos con Fechas Vencidas Revise" : "";

            _view.ShowExpirationWarning(visible, message);
        }

        public void CheckStockWarnings()
        {
            int criticalStockThreshold = _notificationService.ConfigStock();
            List<Product> lowStockProducts = _notificationService.ListStock(criticalStockThreshold);

            bool visible = lowStockProducts.Count > 0;
            string message = visible ? "Revise si hay productos con Stock Crítico" : "";

            _view.ShowStockWarning(visible, message);
        }
    }
}
