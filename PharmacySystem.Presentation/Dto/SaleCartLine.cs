namespace PharmacySystem.Presentation
{
    public class SaleCartLine
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal SubTotal { get; set; }
        public bool TaxAffected { get; set; } = true;
    }
}
