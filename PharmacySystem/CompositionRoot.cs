using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Wpf;

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
        private static readonly ILoginAttemptRepository _loginAttemptRepository = new LoginAttemptRepository(ConnectionFactory);
        private static readonly IAuthenticationService _authService = new AuthenticationService(new PersonRepository(ConnectionFactory), _loginAttemptRepository);
        private static readonly IPasswordChangeService _passwordChangeService = new PasswordChangeService(new PersonRepository(ConnectionFactory), _loginAttemptRepository);
        private static readonly IClientService _clientService = new ClientService(new ClientRepository(ConnectionFactory));
        private static readonly IProductService _productService = new ProductService(new ProductRepository(ConnectionFactory));
        private static readonly ICategoryService _categoryService = new CategoryService(new CategoryRepository(ConnectionFactory));
        private static readonly IStoreService _storeService = new StoreService(new StoreRepository(ConnectionFactory));
        private static readonly INotificationConfigService _notificationConfigService = new NotificationConfigService(new NotificationConfigRepository(ConnectionFactory), new ProductAlertHistoryRepository(ConnectionFactory));
        private static readonly IPurchaseService _purchaseService = new PurchaseService(new PurchaseRepository(ConnectionFactory));
        // LocalSequenceIssuer: receipts stay internal (numbered by the local sequence, no DTE).
        // Replace with a provider-backed IFiscalDocumentIssuer to emit electronic documents.
        private static readonly ISaleService _saleService = new SaleService(new SaleRepository(ConnectionFactory), new LocalSequenceIssuer());
        private static readonly IPermissionService _permissionService = new PermissionService(new PermissionRepository(ConnectionFactory));
        private static readonly ICashCountService _cashCountService = new CashCountService(new CashCountRepository(ConnectionFactory));
        private static readonly ISecurityAudit _securityAudit = new SecurityAudit(new SecurityEventRepository(ConnectionFactory));

        #region Supplier

        public static SupplierPresenter CreateSupplierPresenter(ISupplierView view) =>
            new SupplierPresenter(view, _supplierService, MainForm.Session, _securityAudit);

        public static SupplierPickerPresenter CreateSupplierPickerPresenter(ISupplierPickerView view) =>
            new SupplierPickerPresenter(view, _supplierService);

        // Bundle of the sub-picker factories the WPF sale/purchase windows need (they cannot see
        // this class). Handed to them by the shell.
        public static PickerFactories CreatePickerFactories() =>
            new PickerFactories(CreateSupplierPickerPresenter, CreateProductPickerPresenter, CreateClientPickerPresenter);

        #endregion

        #region Person (client / user / login)

        public static ClientPresenter CreateClientPresenter(IClientView view) =>
            new ClientPresenter(view, _clientService, MainForm.Session, _securityAudit);

        public static UserPresenter CreateUserPresenter(IUserView view) =>
            new UserPresenter(view, _personService, MainForm.Session, _permissionService, _passwordChangeService, _authService, _securityAudit);

        public static ClientPickerPresenter CreateClientPickerPresenter(IClientPickerView view) =>
            new ClientPickerPresenter(view, _clientService);

        public static LoginPresenter CreateLoginPresenter(ILoginView view) =>
            new LoginPresenter(view, _authService);

        public static ChangePasswordPresenter CreateChangePasswordPresenter(IChangePasswordView view, int personId) =>
            new ChangePasswordPresenter(view, _passwordChangeService, personId);

        #endregion

        #region Product / Category

        public static ProductPickerPresenter CreateProductPickerPresenter(IProductPickerView view, string origin) =>
            new ProductPickerPresenter(view, _productService, origin);

        public static CategoryManagementPresenter CreateCategoryManagementPresenter(ICategoryManagementView view) =>
            new CategoryManagementPresenter(view, _categoryService, MainForm.Session, _securityAudit);

        public static ProductManagementPresenter CreateProductManagementPresenter(IProductManagementView view) =>
            new ProductManagementPresenter(view, _productService, _categoryService, MainForm.Session, _securityAudit);

        public static ProductPricePresenter CreateProductPricePresenter(IProductPriceView view) =>
            new ProductPricePresenter(view, _productService, MainForm.Session);

        // Bundle handed to the WPF Gestión window: the four tab presenters plus the lots lookup
        // (ModalProductLots' old ad-hoc service, now resolved from the shared ProductService).
        public static ManagementPresenterFactories CreateManagementFactories() =>
            new ManagementPresenterFactories(
                CreateCategoryManagementPresenter,
                CreateProductManagementPresenter,
                CreateProductPricePresenter,
                CreateStoreManagementPresenter,
                id => _productService.GetLots(id));

        #endregion

        #region Store / Notifications

        public static StoreManagementPresenter CreateStoreManagementPresenter(IStoreManagementView view) =>
            new StoreManagementPresenter(view, _storeService, MainForm.Session, _securityAudit);

        public static NotificationConfigPresenter CreateNotificationConfigPresenter(INotificationConfigView view) =>
            new NotificationConfigPresenter(view, _notificationConfigService, MainForm.Session, _securityAudit);

        public static MainFormPresenter CreateMainFormPresenter(IMainFormView view) =>
            new MainFormPresenter(view, _notificationConfigService, _personService, _permissionService);

        // The notification center (PharmacySystem.Wpf.AlertsWindow) isn't a Presenter/View screen -
        // it just needs the service directly to acknowledge or mute an alert.
        public static INotificationConfigService NotificationConfigService => _notificationConfigService;

        #endregion

        #region Purchase / Sale / Reports

        public static PurchasePresenter CreatePurchasePresenter(IPurchaseView view, int idPerson) =>
            new PurchasePresenter(view, _purchaseService, _productService, _storeService, MainForm.Session, idPerson);

        public static SalePresenter CreateSalePresenter(ISaleView view, int idPerson) =>
            new SalePresenter(view, _saleService, _productService, _storeService, MainForm.Session, idPerson);

        public static CreditNotePresenter CreateCreditNotePresenter(ICreditNoteView view) =>
            new CreditNotePresenter(view, _saleService, MainForm.Session, MainForm.oPerson?.idPerson ?? 0);

        public static ReportPresenter CreateReportPresenter(IReportView view) =>
            new ReportPresenter(view, _supplierService, _categoryService, _saleService, _purchaseService, _productService, _notificationConfigService, _clientService, MainForm.Session);

        #endregion

        #region Home

        public static HomePresenter CreateHomePresenter(IHomeView view) =>
            new HomePresenter(view, _saleService, _notificationConfigService);

        #endregion

        #region Cash count

        public static CashCountPresenter CreateCashCountPresenter(ICashCountView view) =>
            new CashCountPresenter(view, _cashCountService, MainForm.Session);

        #endregion

        #region Security log

        public static SecurityLogPresenter CreateSecurityLogPresenter(ISecurityLogView view) =>
            new SecurityLogPresenter(view, _securityAudit, MainForm.Session);

        #endregion

        #region Permissions / session

        // Resolves the logged-in user's permission set from their role. Built once, right after
        // login, and handed to the screens that need to gate actions or hide UI.
        public static CurrentUser CreateCurrentUser(Person person) =>
            new CurrentUser(person, _permissionService.GetPermissionsForRole(person.oPersonType?.idPersonType ?? 0));

        public static RolesPresenter CreateRolesPresenter(IRolesView view) =>
            new RolesPresenter(view, _permissionService, MainForm.Session, _securityAudit);

        public static IPermissionService PermissionService => _permissionService;

        #endregion
    }
}
