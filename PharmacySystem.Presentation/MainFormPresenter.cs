using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from MainForm.cs's notificationDate()/notificationStock(). Both loops preserve a
    // real quirk from the original: `if (expiredDates.Count() >= 1)` is checked right after the
    // item that made the count >= 1, so it is always true and the trailing `else` branch (which
    // would clear the message) is dead code. Left as-is rather than simplified to "first match
    // wins", since it changes nothing observable but keeps the diff a faithful port.
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
            var expiredDates = new List<DateTime>();
            string message = "";
            bool visible = false;

            foreach (var item in _notificationService.ListExpirationDate().ToList())
            {
                DateTime expirationDate = item.expirationDate.AddDays(-days);

                if (DateTime.Today >= expirationDate)
                {
                    expiredDates.Add(expirationDate);
                    if (expiredDates.Count() >= 1)
                    {
                        message = "Hay productos con Fechas Vencidas Revise";
                        expiredDates.Clear();
                        visible = true;
                    }
                    else
                    {
                        message = "";
                        expiredDates.Clear();
                    }
                }
            }

            _view.ShowExpirationWarning(visible, message);
        }

        public void CheckStockWarnings()
        {
            var criticalStock = new List<int>();
            int criticalStockThreshold = _notificationService.ConfigStock();
            string message = "";
            bool visible = false;

            foreach (var item in _notificationService.ListStock().ToList())
            {
                int stock = item.stock;
                if (stock <= criticalStockThreshold)
                {
                    criticalStock.Add(stock);
                    if (criticalStock.Count() >= 1)
                    {
                        message = "Revise si hay productos con Stock Crítico";
                        criticalStock.Clear();
                        visible = true;
                    }
                    else
                    {
                        message = "";
                        criticalStock.Clear();
                    }
                }
            }

            _view.ShowStockWarning(visible, message);
        }
    }
}
