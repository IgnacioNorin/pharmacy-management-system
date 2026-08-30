using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPersonService
    {
        // The new person's id, or 0 if the insert failed (duplicate document or error).
        int Register(Person person);
        bool Update(Person person);
        List<Person> List();
        Person GetByDocument(string document);
        bool UpdatePassword(int idPerson, string hashedPassword);
        bool Delete(int idPerson);
    }
}
