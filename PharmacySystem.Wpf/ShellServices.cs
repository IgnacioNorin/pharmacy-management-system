using System;
using PharmacySystem.Business;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Everything MainWindow's sidebar needs to open a screen, bundled so the WPF shell does not
    // reference the exe's CompositionRoot. Built once by CompositionRoot.CreateShellServices()
    // and handed to the shell at startup. Presenter factories only ever produce Presentation
    // types from Presentation IView types, so they cross the assembly boundary cleanly.
    public sealed class ShellServices
    {
        public Func<IMainFormView, MainFormPresenter> MainPresenter { get; set; }
        public Func<IHomeView, HomePresenter> HomePresenter { get; set; }

        public Func<IClientView, ClientPresenter> ClientPresenter { get; set; }
        public Func<ISupplierView, SupplierPresenter> SupplierPresenter { get; set; }
        public Func<IUserView, UserPresenter> UserPresenter { get; set; }
        public Func<IRolesView, RolesPresenter> RolesPresenter { get; set; }
        public Func<IReportView, ReportPresenter> ReportPresenter { get; set; }
        public Func<ICashCountView, CashCountPresenter> CashCountPresenter { get; set; }
        public Func<ISecurityLogView, SecurityLogPresenter> SecurityLogPresenter { get; set; }
        public Func<INotificationConfigView, NotificationConfigPresenter> NotificationConfigPresenter { get; set; }
        public Func<IChangePasswordView, int, ChangePasswordPresenter> ChangePasswordPresenter { get; set; }

        // These take the acting person's id (resolved from the live session by the shell).
        public Func<IPurchaseView, int, PurchasePresenter> PurchasePresenter { get; set; }
        public Func<ISaleView, int, SalePresenter> SalePresenter { get; set; }
        public Func<ICreditNoteView, CreditNotePresenter> CreditNotePresenter { get; set; }

        public ManagementPresenterFactories ManagementFactories { get; set; }
        public PickerFactories Pickers { get; set; }
        public INotificationConfigService NotificationConfigService { get; set; }

        // Resolves one sale's ticket data (store, sale, details, HTML template) for
        // PrintSaleWindow. The exe owns the sale services and the template resource.
        public Func<int, PrintTicketData> TicketData { get; set; }
    }
}
