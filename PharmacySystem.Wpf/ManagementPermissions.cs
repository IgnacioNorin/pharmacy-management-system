namespace PharmacySystem.Ui
{
    // Per-tab permissions the WinForms shell resolves from MainForm.Session and hands to
    // ManagementWindow, since the WPF project can't reach the session itself. A tab whose flag is
    // false is removed rather than left disabled, matching the original frmManagement behavior.
    public sealed class ManagementPermissions
    {
        public bool Categories { get; set; }
        public bool Products { get; set; }
        public bool ProductPrices { get; set; }
        public bool Store { get; set; }
    }
}
