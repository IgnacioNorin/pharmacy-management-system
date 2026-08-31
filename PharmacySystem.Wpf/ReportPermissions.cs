namespace PharmacySystem.Wpf
{
    // Per-report-type permissions the WinForms shell resolves from MainForm.Session and hands to
    // ReportWindow, since the WPF project can't reach the session itself. A tab is dropped when
    // its "acceso" flag is false; its "Exportar" button is enabled only with both flags.
    public sealed class ReportPermissions
    {
        public bool Sales { get; set; }
        public bool SalesExport { get; set; }
        public bool Purchases { get; set; }
        public bool PurchasesExport { get; set; }
        public bool Products { get; set; }
        public bool ProductsExport { get; set; }
        public bool AlertHistory { get; set; }
        public bool AlertHistoryExport { get; set; }
    }
}
