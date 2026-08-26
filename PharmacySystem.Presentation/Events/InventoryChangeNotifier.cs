using System;

namespace PharmacySystem.Presentation
{
    // Lightweight in-process signal: SalePresenter/PurchasePresenter raise this after a successful
    // register, so MainForm can recheck stock/expiration alerts immediately instead of waiting for
    // the next background timer tick. Deliberately not a full event bus/mediator - this app has a
    // single relevant subscriber (MainForm) and no DI container, so a static event is the simplest
    // thing that works. Revisit only if a second subscriber shows up.
    public static class InventoryChangeNotifier
    {
        public static event Action StockChanged;

        public static void NotifyStockChanged() => StockChanged?.Invoke();
    }
}
