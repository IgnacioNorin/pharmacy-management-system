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
        Person GetByDocument(string document);
        Person GetById(int idPerson);
        bool UpdatePassword(int idPerson, string hashedPassword);
        // Sets the password and the must_change_password flag in one statement. Used by the
        // password-change and admin-reset paths; UpdatePassword stays for the login self-heal,
        // which must not touch the flag.
        bool SetPasswordAndFlag(int idPerson, string hashedPassword, bool mustChangePassword);
        // Flips person.status. Used by the Suspender / Reactivar action, which is also how an
        // accidental soft-delete is undone. Business enforces the last-Administrador-General rule.
        bool SetActive(int idPerson, bool active);
        bool Delete(int idPerson);
    }
}
