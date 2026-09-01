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
        public required Func<IMainFormView, MainFormPresenter> MainPresenter { get; set; }
        public required Func<IHomeView, HomePresenter> HomePresenter { get; set; }

        public required Func<IClientView, ClientPresenter> ClientPresenter { get; set; }
        public required Func<ISupplierView, SupplierPresenter> SupplierPresenter { get; set; }
        public required Func<IUserView, UserPresenter> UserPresenter { get; set; }
        public required Func<IRolesView, RolesPresenter> RolesPresenter { get; set; }
        public required Func<IReportView, ReportPresenter> ReportPresenter { get; set; }
        public required Func<ICashCountView, CashCountPresenter> CashCountPresenter { get; set; }
        public required Func<ISecurityLogView, SecurityLogPresenter> SecurityLogPresenter { get; set; }
        public required Func<INotificationConfigView, NotificationConfigPresenter> NotificationConfigPresenter { get; set; }
        public required Func<IChangePasswordView, int, ChangePasswordPresenter> ChangePasswordPresenter { get; set; }

        // These take the acting person's id (resolved from the live session by the shell).
        public required Func<IPurchaseView, int, PurchasePresenter> PurchasePresenter { get; set; }
        public required Func<ISaleView, int, SalePresenter> SalePresenter { get; set; }
        public required Func<ICreditNoteView, CreditNotePresenter> CreditNotePresenter { get; set; }

        public required ManagementPresenterFactories ManagementFactories { get; set; }
        public required PickerFactories Pickers { get; set; }
        public required INotificationConfigService NotificationConfigService { get; set; }

        // Resolves one sale's ticket data (store, sale, details, HTML template) for
        // PrintSaleWindow. The exe owns the sale services and the template resource.
        public required Func<int, PrintTicketData> TicketData { get; set; }
    }
}
