using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // What the grid displays for a client (person_type_id = 3). Separate from PersonService's
    // full Supplier-shaped list, and from SupplierRow, so this view never needs to know person
    // has a password/person type at all.
    public class ClientRow
    {
        public int Id { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public static ClientRow From(Person person) => new ClientRow
        {
            Id = person.idPerson,
            Document = person.document,
            Name = person.name,
            Address = person.address,
            Phone = person.phone
        };
    }
}
