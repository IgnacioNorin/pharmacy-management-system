using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // Persists whatever is in person.password verbatim - the decision of whether to hash it
    // first belongs to Business.PersonService, not here.
    public interface IPersonRepository
    {
        // The new person's id, or 0 if the insert failed (duplicate document or error).
        int Register(Person person);
        bool Update(Person person);
        List<Person> List();
        // Active clients only, without password or person-type. For the client picker / screen / report filter.
        List<Person> ListClients();
        PagedResult<Person> ListClientsPaged(int pageNumber, int pageSize, string search);
        Person GetByDocument(string document);
        bool UpdatePassword(int idPerson, string hashedPassword);
        bool Delete(int idPerson);
    }
}
