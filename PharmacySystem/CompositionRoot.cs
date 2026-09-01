using Microsoft.Extensions.DependencyInjection;
using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Ui;

namespace PharmacySystem
{
    // Composition seam. The service / repository graph is registered in a
    // Microsoft.Extensions.DependencyInjection container (BuildServiceProvider below) and
    // resolved once here; presenters still get built by the Create* factory methods because
    // each one needs a runtime IView argument the container cannot supply.
    internal static class CompositionRoot
    {
        private static readonly ServiceProvider _services = BuildServiceProvider();

        internal static readonly ISqlConnectionFactory ConnectionFactory = _services.GetRequiredService<ISqlConnectionFactory>();

        // Resolved singletons, kept as fields so every Create* method reads like before.
        private static readonly ISupplierService _supplierService = _services.GetRequiredService<ISupplierService>();
        private static readonly IPersonService _personService = _services.GetRequiredService<IPersonService>();
        private static readonly IAuthenticationService _authService = _services.GetRequiredService<IAuthenticationService>();
        private static readonly IPasswordChangeService _passwordChangeService = _services.GetRequiredService<IPasswordChangeService>();
        private static readonly IClientService _clientService = _services.GetRequiredService<IClientService>();
        private static readonly IProductService _productService = _services.GetRequiredService<IProductService>();
        private static readonly ICategoryService _categoryService = _services.GetRequiredService<ICategoryService>();
        private static readonly IStoreService _storeService = _services.GetRequiredService<IStoreService>();
        private static readonly INotificationConfigService _notificationConfigService = _services.GetRequiredService<INotificationConfigService>();
        private static readonly IPurchaseService _purchaseService = _services.GetRequiredService<IPurchaseService>();
        private static readonly ISaleService _saleService = _services.GetRequiredService<ISaleService>();
        private static readonly IPermissionService _permissionService = _services.GetRequiredService<IPermissionService>();
        private static readonly ICashCountService _cashCountService = _services.GetRequiredService<ICashCountService>();
        private static readonly ISecurityAudit _securityAudit = _services.GetRequiredService<ISecurityAudit>();

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddSingleton<ISqlConnectionFactory>(_ => SqlConnectionFactory.FromConfiguration());

            services.AddSingleton<ISupplierRepository, SupplierRepository>();
            services.AddSingleton<IPersonRepository, PersonRepository>();
            services.AddSingleton<ILoginAttemptRepository, LoginAttemptRepository>();
            services.AddSingleton<IClientRepository, ClientRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<ICategoryRepository, CategoryRepository>();
            services.AddSingleton<IStoreRepository, StoreRepository>();
            services.AddSingleton<INotificationConfigRepository, NotificationConfigRepository>();
            services.AddSingleton<IProductAlertHistoryRepository, ProductAlertHistoryRepository>();
            services.AddSingleton<IPurchaseRepository, PurchaseRepository>();
            services.AddSingleton<ISaleRepository, SaleRepository>();
            services.AddSingleton<IPermissionRepository, PermissionRepository>();
            services.AddSingleton<ICashCountRepository, CashCountRepository>();
            services.AddSingleton<ISecurityEventRepository, SecurityEventRepository>();

            services.AddSingleton<ISupplierService, SupplierService>();
            services.AddSingleton<IPersonService, PersonService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<IPasswordChangeService, PasswordChangeService>();
            services.AddSingleton<IClientService, ClientService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<ICategoryService, CategoryService>();
            services.AddSingleton<IStoreService, StoreService>();
            services.AddSingleton<INotificationConfigService, NotificationConfigService>();
            services.AddSingleton<IPurchaseService, PurchaseService>();
            services.AddSingleton<IPermissionService, PermissionService>();
            services.AddSingleton<ICashCountService, CashCountService>();
            services.AddSingleton<ISecurityAudit, SecurityAudit>();

            // LocalSequenceIssuer: receipts stay internal (numbered by the local sequence, no
            // DTE). Swap for a provider-backed IFiscalDocumentIssuer to emit electronic documents.
            services.AddSingleton<IFiscalDocumentIssuer, LocalSequenceIssuer>();
            services.AddSingleton<ISaleService, SaleService>();

            return services.BuildServiceProvider();
        }

        // Every Create*Presenter runs after the shell has signed a user in, so AppSession.Current
        // is set. Fail loudly rather than hand a presenter a null session.
        private static PharmacySystem.Presentation.CurrentUser RequireSession() =>
            AppSession.Current ?? throw new System.InvalidOperationException("No hay una sesión activa.");

        #region Supplier

        public static SupplierPresenter CreateSupplierPresenter(ISupplierView view) =>
            new SupplierPresenter(view, _supplierService, RequireSession(), _securityAudit);

        public static SupplierPickerPresenter CreateSupplierPickerPresenter(ISupplierPickerView view) =>
            new SupplierPickerPresenter(view, _supplierService);

        // Bundle of the sub-picker factories the WPF sale/purchase windows need (they cannot see
        // this class). Handed to them by the shell.
        public static PickerFactories CreatePickerFactories() =>
            new PickerFactories(CreateSupplierPickerPresenter, CreateProductPickerPresenter, CreateClientPickerPresenter);

        #endregion

        #region Person (client / user / login)

        public static ClientPresenter CreateClientPresenter(IClientView view) =>
            new ClientPresenter(view, _clientService, RequireSession(), _securityAudit);

        public static UserPresenter CreateUserPresenter(IUserView view) =>
            new UserPresenter(view, _personService, RequireSession(), _permissionService, _passwordChangeService, _authService, _securityAudit);

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
            new CategoryManagementPresenter(view, _categoryService, RequireSession(), _securityAudit);

        public static ProductManagementPresenter CreateProductManagementPresenter(IProductManagementView view) =>
            new ProductManagementPresenter(view, _productService, _categoryService, RequireSession(), _securityAudit);

        public static ProductPricePresenter CreateProductPricePresenter(IProductPriceView view) =>
            new ProductPricePresenter(view, _productService, RequireSession());

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
            new StoreManagementPresenter(view, _storeService, RequireSession(), _securityAudit);

        public static NotificationConfigPresenter CreateNotificationConfigPresenter(INotificationConfigView view) =>
            new NotificationConfigPresenter(view, _notificationConfigService, RequireSession(), _securityAudit);

        public static MainFormPresenter CreateMainFormPresenter(IMainFormView view) =>
            new MainFormPresenter(view, _notificationConfigService, _personService, _permissionService);

        // The notification center (PharmacySystem.Ui.AlertsWindow) isn't a Presenter/View screen -
        // it just needs the service directly to acknowledge or mute an alert.
        public static INotificationConfigService NotificationConfigService => _notificationConfigService;

        #endregion

        #region Purchase / Sale / Reports

        public static PurchasePresenter CreatePurchasePresenter(IPurchaseView view, int idPerson) =>
            new PurchasePresenter(view, _purchaseService, _productService, _storeService, RequireSession(), idPerson);

        public static SalePresenter CreateSalePresenter(ISaleView view, int idPerson) =>
            new SalePresenter(view, _saleService, _productService, _storeService, RequireSession(), idPerson);

        public static CreditNotePresenter CreateCreditNotePresenter(ICreditNoteView view) =>
            new CreditNotePresenter(view, _saleService, RequireSession(), AppSession.Person?.idPerson ?? 0);

        public static ReportPresenter CreateReportPresenter(IReportView view) =>
            new ReportPresenter(view, _supplierService, _categoryService, _saleService, _purchaseService, _productService, _notificationConfigService, _clientService, RequireSession());

        #endregion

        #region Home

        public static HomePresenter CreateHomePresenter(IHomeView view) =>
            new HomePresenter(view, _saleService, _notificationConfigService);

        #endregion

        #region Cash count

        public static CashCountPresenter CreateCashCountPresenter(ICashCountView view) =>
            new CashCountPresenter(view, _cashCountService, RequireSession());

        #endregion

        #region Security log

        public static SecurityLogPresenter CreateSecurityLogPresenter(ISecurityLogView view) =>
            new SecurityLogPresenter(view, _securityAudit, RequireSession());

        #endregion

        #region Permissions / session

        // Resolves the logged-in user's permission set from their role. Built once, right after
        // login, and handed to the screens that need to gate actions or hide UI.
        public static CurrentUser CreateCurrentUser(Person person) =>
            new CurrentUser(person, _permissionService.GetPermissionsForRole(person.oPersonType?.idPersonType ?? 0));

        public static RolesPresenter CreateRolesPresenter(IRolesView view) =>
            new RolesPresenter(view, _permissionService, RequireSession(), _securityAudit);

        public static IPermissionService PermissionService => _permissionService;

        #endregion

        #region Shell

        // Everything the WPF shell's sidebar needs to open a screen, in one bundle so MainWindow
        // (in PharmacySystem.Ui) never references this class.
        public static ShellServices CreateShellServices() => new ShellServices
        {
            MainPresenter = CreateMainFormPresenter,
            HomePresenter = CreateHomePresenter,
            ClientPresenter = CreateClientPresenter,
            SupplierPresenter = CreateSupplierPresenter,
            UserPresenter = CreateUserPresenter,
            RolesPresenter = CreateRolesPresenter,
            ReportPresenter = CreateReportPresenter,
            CashCountPresenter = CreateCashCountPresenter,
            SecurityLogPresenter = CreateSecurityLogPresenter,
            NotificationConfigPresenter = CreateNotificationConfigPresenter,
            ChangePasswordPresenter = (view, personId) => CreateChangePasswordPresenter(view, personId),
            PurchasePresenter = (view, idPerson) => CreatePurchasePresenter(view, idPerson),
            SalePresenter = (view, idPerson) => CreateSalePresenter(view, idPerson),
            CreditNotePresenter = CreateCreditNotePresenter,
            ManagementFactories = CreateManagementFactories(),
            Pickers = CreatePickerFactories(),
            NotificationConfigService = _notificationConfigService,
            TicketData = idSale =>
            {
                Sale? sale = _saleService.GetById(idSale);
                if (sale != null)
                {
                    sale.payments = _saleService.GetPaymentsBySaleId(idSale);
                }
                return new PrintTicketData
                {
                    Store = _storeService.ListStore(),
                    Sale = sale,
                    Details = sale == null ? null : _saleService.GetDetailsBySaleId(idSale),
                    HtmlTemplate = Properties.Resources.Ticket
                };
            }
        };

        #endregion
    }
}
