using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Presentation;

namespace PharmacySystem
{
    // Manual composition instead of a DI container: with a handful of services this stays
    // readable, and a container would add a dependency plus runtime "magic" to resolve a
    // graph this small. Revisit only if the graph grows enough that this file gets unwieldy.
    internal static class CompositionRoot
    {
        internal static readonly ISqlConnectionFactory ConnectionFactory = SqlConnectionFactory.FromConfiguration();

        // Services are stateless wrappers around a repository, which in turn only holds the
        // shared ConnectionFactory above - safe to build each one once and hand the same instance
        // to every screen that needs it, instead of re-building the same repository+service pair
        // in every Create*Presenter method below.
        private static readonly ISupplierService _supplierService = new SupplierService(new SupplierRepository(ConnectionFactory));
        private static readonly IPersonService _personService = new PersonService(new PersonRepository(ConnectionFactory));
        private static readonly IProductService _productService = new ProductService(new ProductRepository(ConnectionFactory));
        private static readonly ICategoryService _categoryService = new CategoryService(new CategoryRepository(ConnectionFactory));
        private static readonly IStoreService _storeService = new StoreService(new StoreRepository(ConnectionFactory));
        private static readonly INotificationConfigService _notificationConfigService = new NotificationConfigService(new NotificationConfigRepository(ConnectionFactory), new ProductAlertHistoryRepository(ConnectionFactory));
        private static readonly IPurchaseService _purchaseService = new PurchaseService(new PurchaseRepository(ConnectionFactory));
        private static readonly ISaleService _saleService = new SaleService(new SaleRepository(ConnectionFactory));

        #region Supplier

        public static SupplierPresenter CreateSupplierPresenter(ISupplierView view) =>
            new SupplierPresenter(view, _supplierService);

        public static SupplierPickerPresenter CreateSupplierPickerPresenter(ISupplierPickerView view) =>
            new SupplierPickerPresenter(view, _supplierService);

        #endregion

        #region Person (client / user / login)

        public static ClientPresenter CreateClientPresenter(IClientView view) =>
            new ClientPresenter(view, _personService);

        public static UserPresenter CreateUserPresenter(IUserView view) =>
            new UserPresenter(view, _personService);

        public static ClientPickerPresenter CreateClientPickerPresenter(IClientPickerView view) =>
            new ClientPickerPresenter(view, _personService);

        public static LoginPresenter CreateLoginPresenter(ILoginView view) =>
            new LoginPresenter(view, _personService);

        #endregion

        #region Product / Category

        public static ProductPickerPresenter CreateProductPickerPresenter(IProductPickerView view, string origin) =>
            new ProductPickerPresenter(view, _productService, origin);

        public static CategoryManagementPresenter CreateCategoryManagementPresenter(ICategoryManagementView view) =>
            new CategoryManagementPresenter(view, _categoryService);

        public static ProductManagementPresenter CreateProductManagementPresenter(IProductManagementView view) =>
            new ProductManagementPresenter(view, _productService, _categoryService);

        #endregion

        #region Store / Notifications

        public static StoreManagementPresenter CreateStoreManagementPresenter(IStoreManagementView view) =>
            new StoreManagementPresenter(view, _storeService);

        public static NotificationConfigPresenter CreateNotificationConfigPresenter(INotificationConfigView view) =>
            new NotificationConfigPresenter(view, _notificationConfigService);

        public static MainFormPresenter CreateMainFormPresenter(IMainFormView view) =>
            new MainFormPresenter(view, _storeService, _notificationConfigService);

        // ModalAlerts isn't a Presenter/View screen (see its own comment) - it just needs the
        // service directly to acknowledge an alert.
        public static INotificationConfigService NotificationConfigService => _notificationConfigService;

        #endregion

        #region Purchase / Sale / Reports

        public static PurchasePresenter CreatePurchasePresenter(IPurchaseView view, int idPerson) =>
            new PurchasePresenter(view, _purchaseService, _productService, idPerson);

        public static SalePresenter CreateSalePresenter(ISaleView view, int idPerson) =>
            new SalePresenter(view, _saleService, _productService, idPerson);

        public static ReportPresenter CreateReportPresenter(IReportView view) =>
            new ReportPresenter(view, _supplierService, _categoryService, _saleService, _purchaseService, _productService, _notificationConfigService);

        #endregion
    }
}
