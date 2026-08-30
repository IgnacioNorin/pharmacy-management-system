namespace PharmacySystem.Presentation
{
    // Which sidebar destinations the signed-in user may open, resolved from their permissions by
    // MainFormPresenter. The View turns each flag into a button's Visible. Inicio has no flag -
    // anyone who can sign in sees it.
    public class SidebarPermissions
    {
        public bool Sales { get; set; }
        public bool Purchases { get; set; }
        public bool Clients { get; set; }
        public bool Suppliers { get; set; }
        public bool Management { get; set; }
        public bool Users { get; set; }
        public bool Roles { get; set; }
        public bool Reports { get; set; }
        public bool CashCount { get; set; }
        public bool Alerts { get; set; }
    }
}
