namespace PharmacySystem.Presentation
{
    // What the landing screen (HomeView) may show for a given session. The presenter still gates
    // every real action; this only decides which tiles, quick-access buttons and the product
    // drill-down the current role can see. A null session resolves to nothing visible (DEF-23).
    public struct HomeAccess
    {
        public bool SalesTile;
        public bool AlertTiles;
        public bool AttentionList;
        public bool NewSale;
        public bool NewPurchase;
        public bool ManageProducts;
        public bool ProductDrillDown;

        public bool QuickActionsPanel => NewSale || NewPurchase || ManageProducts;

        public static HomeAccess Resolve(CurrentUser session)
        {
            bool Can(string permission) => session?.Can(permission) ?? false;

            bool canProducts = Can("productos.acceso");
            bool canAlerts = Can("alertas.acceso");

            return new HomeAccess
            {
                SalesTile = Can("ventas.acceso"),
                AlertTiles = canAlerts,
                AttentionList = canAlerts,
                NewSale = Can("ventas.acceso"),
                NewPurchase = Can("compras.acceso"),
                ManageProducts = canProducts,
                ProductDrillDown = canProducts
            };
        }
    }
}
