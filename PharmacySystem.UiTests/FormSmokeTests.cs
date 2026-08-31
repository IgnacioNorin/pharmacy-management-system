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

        [Fact]
        public void ModalConfignotification_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new ModalConfignotification())
                {
                    Assert.IsAssignableFrom<INotificationConfigView>(form);
                }
            });
        }

        [Fact]
        public void ModalSupplier_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new ModalSupplier())
                {
                    Assert.IsAssignableFrom<ISupplierPickerView>(form);
                }
            });
        }

        // frmClient was ported to WPF (PharmacySystem.Wpf.ClientWindow) - see the WPF migration.
        // The presenter (ClientPresenter) keeps its full unit-test coverage in PharmacySystem.Tests.

        [Fact]
        public void FrmUser_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmUser())
                {
                    Assert.IsAssignableFrom<IUserView>(form);
                }
            });
        }

        [Fact]
        public void ModalPerson_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new ModalPerson())
                {
                    Assert.IsAssignableFrom<IClientPickerView>(form);
                }
            });
        }

        [Fact]
        public void ModalProduct_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new ModalProduct("frmSale"))
                {
                    Assert.IsAssignableFrom<IProductPickerView>(form);
                }
            });
        }

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

        [Fact]
        public void FrmRoles_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmRoles())
                {
                    Assert.IsAssignableFrom<IRolesView>(form);
                }
            });
        }

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

        [Fact]
        public void FrmPurchase_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmPurchase())
                {
                    Assert.IsAssignableFrom<IPurchaseView>(form);
                }
            });
        }

        [Fact]
        public void FrmSale_ConstructsAndImplementsView()
        {
            StaThread.Run(() =>
            {
                using (var form = new frmSale())
                {
                    Assert.IsAssignableFrom<ISaleView>(form);
                }
            });
        }

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
