using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // What the grid displays for a client. Built from a PharmacySystem.Model.Client row.
    public class ClientRow
    {
        public int Id { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string BusinessName { get; set; }
        public string Activity { get; set; }
        public string Commune { get; set; }
        public string Email { get; set; }
        public bool IsCompany { get; set; }

        public static ClientRow From(Client client) => new ClientRow
        {
            Id = client.idClient,
            Document = client.document,
            Name = client.name,
            Address = client.address,
            Phone = client.phone,
            BusinessName = client.businessName,
            Activity = client.activity,
            Commune = client.commune,
            Email = client.email,
            IsCompany = client.isCompany
        };
    }
}
