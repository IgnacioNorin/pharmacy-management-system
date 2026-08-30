using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.UiTests
{
    // frmHome.ResolveAccess is the single decision the landing screen uses to hide the tiles and
    // quick-access buttons a role cannot reach. These are plain logic checks - no Form, no STA -
    // so the "an Empleado sees nothing about purchases or restocking" rule stays pinned down.
    public class HomeAccessTests
    {
        private static CurrentUser User(params string[] permissions) =>
            new CurrentUser(new Person { idPerson = 1, name = "Test", oPersonType = new TypePerson { idPersonType = 3 } }, permissions);

        // The permission set Database/PharmacyDB.sql seeds for role 3 (Empleado).
        private static CurrentUser Empleado() =>
            User("ventas.acceso", "clientes.acceso", "clientes.gestionar",
                 "alertas.acceso", "alertas.reconocer", "alertas.silenciar");

        [Fact]
        public void Empleado_SeesNoPurchaseOrProductEntryPoints()
        {
            var access = frmHome.ResolveAccess(Empleado());

            Assert.False(access.NewPurchase);
            Assert.False(access.ManageProducts);
            Assert.False(access.ProductDrillDown);
        }

        [Fact]
        public void Empleado_StillSeesTheSaleShortcutAndAlertContent()
        {
            var access = frmHome.ResolveAccess(Empleado());

            Assert.True(access.NewSale);
            Assert.True(access.SalesTile);
            Assert.True(access.AlertTiles);
            Assert.True(access.AttentionList);
            Assert.True(access.QuickActionsPanel);
        }

        [Fact]
        public void RoleWithoutSalesOrAlerts_HidesEveryTileAndTheQuickActionsPanel()
        {
            var access = frmHome.ResolveAccess(User("clientes.acceso"));

            Assert.False(access.SalesTile);
            Assert.False(access.AlertTiles);
            Assert.False(access.AttentionList);
            Assert.False(access.NewSale);
            Assert.False(access.NewPurchase);
            Assert.False(access.ManageProducts);
            Assert.False(access.QuickActionsPanel);
        }

        [Fact]
        public void FullPermissionUser_SeesEveryEntryPoint()
        {
            var access = frmHome.ResolveAccess(
                User("ventas.acceso", "compras.acceso", "productos.acceso", "alertas.acceso"));

            Assert.True(access.NewSale);
            Assert.True(access.NewPurchase);
            Assert.True(access.ManageProducts);
            Assert.True(access.ProductDrillDown);
            Assert.True(access.QuickActionsPanel);
        }

        [Fact]
        public void NullSession_ResolvesToNothingVisible()
        {
            var access = frmHome.ResolveAccess(null);

            Assert.False(access.NewSale || access.NewPurchase || access.ManageProducts);
            Assert.False(access.SalesTile || access.AlertTiles || access.AttentionList);
            Assert.False(access.ProductDrillDown || access.QuickActionsPanel);
        }
    }
}
