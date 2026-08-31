using System;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // The WPF sale/purchase windows open sub-pickers (supplier, product, client). They cannot
    // reach CompositionRoot (it lives in the WinForms exe), so the shell hands them the presenter
    // factories for those pickers, bundled here. When the shell itself becomes WPF this collapses
    // into whatever composition root it uses.
    public sealed class PickerFactories
    {
        public Func<ISupplierPickerView, SupplierPickerPresenter> Supplier { get; }
        public Func<IProductPickerView, string, ProductPickerPresenter> Product { get; }
        public Func<IClientPickerView, ClientPickerPresenter> Client { get; }

        public PickerFactories(
            Func<ISupplierPickerView, SupplierPickerPresenter> supplier,
            Func<IProductPickerView, string, ProductPickerPresenter> product,
            Func<IClientPickerView, ClientPickerPresenter> client = null)
        {
            Supplier = supplier;
            Product = product;
            Client = client;
        }
    }
}
