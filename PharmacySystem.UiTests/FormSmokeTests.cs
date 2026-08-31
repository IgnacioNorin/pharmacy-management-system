using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.UiTests
{
    // Every Presenter has thorough unit test coverage (PharmacySystem.Tests), but nothing verified
    // that the Forms implementing their View interfaces actually construct without blowing up:
    // a typo in CompositionRoot wiring, a missing designer resource, or an interface member left
    // unimplemented would only surface by actually running the app. These tests only construct and
    // dispose each Form - they never call OnLoad()/Show(), so they need a well-formed connection
    // string (CompositionRoot builds repositories eagerly) but never actually query the database.
    public class FormSmokeTests
    {
        // frmSupplier was ported to WPF (PharmacySystem.Wpf.SupplierWindow). SupplierPresenter
        // keeps its full unit-test coverage in PharmacySystem.Tests.

        // ModalConfignotification and ModalCashCount were ported to WPF
        // (PharmacySystem.Wpf.NotificationConfigWindow / CashCountWindow). Their presenters keep
        // full coverage in PharmacySystem.Tests.

        // ModalSupplier and ModalProduct were ported to WPF (PharmacySystem.Wpf.SupplierPickerWindow
        // / ProductPickerWindow). Their presenters keep coverage in PharmacySystem.Tests.

        // frmClient was ported to WPF (PharmacySystem.Wpf.ClientWindow) - see the WPF migration.
        // The presenter (ClientPresenter) keeps its full unit-test coverage in PharmacySystem.Tests.

        // frmUser was ported to WPF (PharmacySystem.Wpf.UserWindow). UserPresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests.

        // ModalPerson (client picker) was ported to WPF (PharmacySystem.Wpf.ClientPickerWindow).
        // ClientPickerPresenter keeps its coverage in PharmacySystem.Tests.

        // frmReport was ported to WPF (PharmacySystem.Wpf.ReportWindow). ReportPresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests.

        // frmRoles was ported to WPF (PharmacySystem.Wpf.RolesWindow). RolesPresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests.

        // frmCreditNote was ported to WPF (PharmacySystem.Wpf.CreditNoteWindow) - see the WPF
        // migration. CreditNotePresenter keeps its full unit-test coverage in PharmacySystem.Tests.

        // frmManagement was ported to WPF (PharmacySystem.Wpf.ManagementWindow). Its four
        // presenters (Category / Product / ProductPrice / Store management) keep their full
        // unit-test coverage in PharmacySystem.Tests.

        // MainForm was ported to WPF (PharmacySystem.Wpf.MainWindow). MainFormPresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests.

        // Login was ported to WPF (PharmacySystem.Wpf.LoginWindow). LoginPresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests.

        // frmPurchase and frmSale were ported to WPF (PharmacySystem.Wpf.PurchaseWindow /
        // SaleWindow). PurchasePresenter / SalePresenter keep their full unit-test coverage in
        // PharmacySystem.Tests.

        // ModalAlerts (notification center) was ported to WPF (PharmacySystem.Wpf.AlertsWindow).
        // It has no Presenter; NotificationConfigPresenter (its "Configurar umbrales" button)
        // keeps its full unit-test coverage in PharmacySystem.Tests.

        // frmHome was ported to WPF (PharmacySystem.Wpf.HomeView). HomePresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests; HomeAccess.Resolve is covered by
        // HomeAccessTests.

        [Fact]
        public void PrintSale_ConstructsWithoutException()
        {
            StaThread.Run(() =>
            {
                using (var form = new PrintSale())
                {
                    Assert.NotNull(form);
                }
            });
        }
    }
}
