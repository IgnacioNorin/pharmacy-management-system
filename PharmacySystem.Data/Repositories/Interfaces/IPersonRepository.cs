using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // Persists whatever is in person.password verbatim - the decision of whether to hash it
    // first belongs to Business.PersonService, not here.
    public interface IPersonRepository
    {
        bool Register(Person person);
        bool Update(Person person);
        List<Person> List();
        Person GetByDocument(string document);
        bool UpdatePassword(int idPerson, string hashedPassword);
        bool Delete(int idPerson);
    }
}
