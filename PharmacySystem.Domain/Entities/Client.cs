namespace PharmacySystem.Model
{
    // A retail customer. Split out of `person` (which now holds only application users): a client
    // has no password and no role. The fiscal profile (businessName / activity / commune / email
    // / isCompany) is filled when the client is the recipient of a Factura.
    public class Client
    {
        public int idClient { get; set; }
        public string document { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        public string businessName { get; set; }
        public string activity { get; set; }
        public string commune { get; set; }
        public string email { get; set; }
        public bool isCompany { get; set; }
        public bool Estado { get; set; }
    }
}
