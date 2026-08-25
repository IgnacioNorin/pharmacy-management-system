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

        public static SupplierPresenter CreateSupplierPresenter(ISupplierView view)
        {
            ISupplierRepository repository = new SupplierRepository(ConnectionFactory);
            ISupplierService service = new SupplierService(repository);
            return new SupplierPresenter(view, service);
        }

        public static NotificationConfigPresenter CreateNotificationConfigPresenter(INotificationConfigView view)
        {
            INotificationConfigRepository repository = new NotificationConfigRepository(ConnectionFactory);
            INotificationConfigService service = new NotificationConfigService(repository);
            return new NotificationConfigPresenter(view, service);
        }

        public static SupplierPickerPresenter CreateSupplierPickerPresenter(ISupplierPickerView view)
        {
            ISupplierRepository repository = new SupplierRepository(ConnectionFactory);
            ISupplierService service = new SupplierService(repository);
            return new SupplierPickerPresenter(view, service);
        }

        public static ClientPresenter CreateClientPresenter(IClientView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new ClientPresenter(view, service);
        }

        public static UserPresenter CreateUserPresenter(IUserView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new UserPresenter(view, service);
        }

        public static ClientPickerPresenter CreateClientPickerPresenter(IClientPickerView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new ClientPickerPresenter(view, service);
        }

        public static ProductPickerPresenter CreateProductPickerPresenter(IProductPickerView view, string origin)
        {
            IProductRepository repository = new ProductRepository(ConnectionFactory);
            IProductService service = new ProductService(repository);
            return new ProductPickerPresenter(view, service, origin);
        }

        public static CategoryManagementPresenter CreateCategoryManagementPresenter(ICategoryManagementView view)
        {
            ICategoryRepository repository = new CategoryRepository(ConnectionFactory);
            ICategoryService service = new CategoryService(repository);
            return new CategoryManagementPresenter(view, service);
        }

        public static ProductManagementPresenter CreateProductManagementPresenter(IProductManagementView view)
        {
            IProductService productService = new ProductService(new ProductRepository(ConnectionFactory));
            ICategoryService categoryService = new CategoryService(new CategoryRepository(ConnectionFactory));
            return new ProductManagementPresenter(view, productService, categoryService);
        }

        public static StoreManagementPresenter CreateStoreManagementPresenter(IStoreManagementView view)
        {
            IStoreRepository repository = new StoreRepository(ConnectionFactory);
            IStoreService service = new StoreService(repository);
            return new StoreManagementPresenter(view, service);
        }

        public static LoginPresenter CreateLoginPresenter(ILoginView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new LoginPresenter(view, service);
        }

        public static MainFormPresenter CreateMainFormPresenter(IMainFormView view)
        {
            IStoreService storeService = new StoreService(new StoreRepository(ConnectionFactory));
            INotificationConfigService notificationService = new NotificationConfigService(new NotificationConfigRepository(ConnectionFactory));
            return new MainFormPresenter(view, storeService, notificationService);
        }

        public static ReportPresenter CreateReportPresenter(IReportView view)
        {
            ISupplierService supplierService = new SupplierService(new SupplierRepository(ConnectionFactory));
            ICategoryService categoryService = new CategoryService(new CategoryRepository(ConnectionFactory));
            ISaleService saleService = new SaleService(new SaleRepository(ConnectionFactory));
            IPurchaseService purchaseService = new PurchaseService(new PurchaseRepository(ConnectionFactory));
            IProductService productService = new ProductService(new ProductRepository(ConnectionFactory));
            return new ReportPresenter(view, supplierService, categoryService, saleService, purchaseService, productService);
        }
    }
}
