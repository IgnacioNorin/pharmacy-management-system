namespace PharmacySystem.Model
{
    // A retail customer. Split out of `person` (which now holds only application users): a client
    // has no password and no role. The fiscal profile (businessName / activity / commune / email
    // / isCompany) is filled when the client is the recipient of a Factura.
    public class Client
    {
        public int idClient { get; set; }
        public string document { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;
        public string activity { get; set; } = string.Empty;
        public string commune { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public bool isCompany { get; set; }
        public bool Estado { get; set; }
    }
}
