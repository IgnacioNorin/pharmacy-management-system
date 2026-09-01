using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // What the grid displays - separate from the Supplier entity so the view never needs to know
    // about domain shape, only about these five columns.
    public class SupplierRow
    {
        public int Id { get; set; }
        public string Document { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public static SupplierRow From(Supplier supplier) => new SupplierRow
        {
            Id = supplier.idSupplier,
            Document = supplier.document,
            CompanyName = supplier.companyName,
            Email = supplier.email,
            Phone = supplier.phone
        };
    }
}
