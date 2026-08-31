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
        [Fact]
        public void FrmSupplier_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmSupplier())
                {
                    Assert.IsAssignableFrom<ISupplierView>(form);
                }
            });
        }

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

        [Fact]
        public void FrmReport_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmReport())
                {
                    Assert.IsAssignableFrom<IReportView>(form);
                }
            });
        }

        // frmRoles was ported to WPF (PharmacySystem.Wpf.RolesWindow). RolesPresenter keeps its
        // full unit-test coverage in PharmacySystem.Tests.

        // frmCreditNote was ported to WPF (PharmacySystem.Wpf.CreditNoteWindow) - see the WPF
        // migration. CreditNotePresenter keeps its full unit-test coverage in PharmacySystem.Tests.

        [Fact]
        public void FrmManagement_ConstructsAndImplementsAllThreeTabViews()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmManagement())
                {
                    Assert.IsAssignableFrom<ICategoryManagementView>(form);
                    Assert.IsAssignableFrom<IProductManagementView>(form);
                    Assert.IsAssignableFrom<IProductPriceView>(form);
                    Assert.IsAssignableFrom<IStoreManagementView>(form);
                }
            });
        }

        [Fact]
        public void MainForm_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new MainForm())
                {
                    Assert.IsAssignableFrom<IMainFormView>(form);
                }
            });
        }

        [Fact]
        public void Login_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new Login())
                {
                    Assert.IsAssignableFrom<ILoginView>(form);
                }
            });
        }

        // frmPurchase and frmSale were ported to WPF (PharmacySystem.Wpf.PurchaseWindow /
        // SaleWindow). PurchasePresenter / SalePresenter keep their full unit-test coverage in
        // PharmacySystem.Tests.

        // ModalAlerts is a plain data-display dialog (Fase 3 of the alerts rework), not an MVP
        // screen - no View interface to assert against, just confirms the hand-authored
        // Designer.cs/.resx pair (no visual designer was used to generate them) actually wires up.
        [Fact]
        public void ModalAlerts_ConstructsWithoutException()
        {
            StaThread.Run(() =>
            {
                using (var form = new ModalAlerts())
                {
                    Assert.NotNull(form);
                }
            });
        }

        [Fact]
        public void FrmHome_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmHome(() => { }, () => { }, () => { }, code => { }))
                {
                    Assert.IsAssignableFrom<PharmacySystem.Presentation.IHomeView>(form);
                }
            });
        }

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
