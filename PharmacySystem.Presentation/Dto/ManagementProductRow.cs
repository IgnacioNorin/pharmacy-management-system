namespace PharmacySystem.Presentation
{
    // Distinct from ProductPickerRow (ModalProduct): this grid also needs a Stock/expiration
    // display column shaped like the original frmManagement.cs grid, not the picker's.
    public class ManagementProductRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryText { get; set; }
        public string Stock { get; set; }
        public string ExpirationDateText { get; set; }
    }
}
