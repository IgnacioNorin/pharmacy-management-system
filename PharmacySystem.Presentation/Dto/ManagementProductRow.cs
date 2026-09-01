namespace PharmacySystem.Presentation
{
    // Distinct from ProductPickerRow (ModalProduct): this grid also needs a Stock/expiration
    // display column shaped like the original frmManagement.cs grid, not the picker's.
    public class ManagementProductRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryText { get; set; } = string.Empty;
        public string Stock { get; set; } = string.Empty;
        public string ExpirationDateText { get; set; } = string.Empty;
        public bool TaxAffected { get; set; } = true;
    }
}
