using System;
using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // The four presenters behind ManagementWindow's tabs plus the lots lookup, bundled so the
    // WinForms shell can wire them from CompositionRoot without the WPF project referencing it.
    public sealed class ManagementPresenterFactories
    {
        public Func<ICategoryManagementView, CategoryManagementPresenter> Category { get; }
        public Func<IProductManagementView, ProductManagementPresenter> Product { get; }
        public Func<IProductPriceView, ProductPricePresenter> ProductPrice { get; }
        public Func<IStoreManagementView, StoreManagementPresenter> Store { get; }
        public Func<int, IReadOnlyList<ProductLot>> LotsProvider { get; }

        public ManagementPresenterFactories(
            Func<ICategoryManagementView, CategoryManagementPresenter> category,
            Func<IProductManagementView, ProductManagementPresenter> product,
            Func<IProductPriceView, ProductPricePresenter> productPrice,
            Func<IStoreManagementView, StoreManagementPresenter> store,
            Func<int, IReadOnlyList<ProductLot>> lotsProvider)
        {
            Category = category;
            Product = product;
            ProductPrice = productPrice;
            Store = store;
            LotsProvider = lotsProvider;
        }
    }
}
