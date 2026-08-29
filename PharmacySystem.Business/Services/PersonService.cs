using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // The real rule this split surfaces: a caller can pass a plain-text password and this
    // layer decides whether it needs hashing before anything reaches the database - the
    // repository never sees an un-hashed decision, only the final string to persist.
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _repository;

        public PersonService(IPersonRepository repository)
        {
            _repository = repository;
        }

        public int Register(Person person)
        {
            person.password = HashIfNeeded(person.password);
            return _repository.Register(person);
        }

        public bool Update(Person person)
        {
            // A blank password on an edit means "keep the current one": pass null straight
            // through so sp_update_person leaves person.password untouched, instead of storing
            // the hash of an empty string and locking the user out.
            person.password = string.IsNullOrWhiteSpace(person.password) ? null : HashIfNeeded(person.password);
            return _repository.Update(person);
        }

        public List<Person> List() => _repository.List();

        public List<Person> ListClients() => _repository.ListClients();

        public Person GetByDocument(string document) => _repository.GetByDocument(document);

        public bool UpdatePassword(int idPerson, string hashedPassword) => _repository.UpdatePassword(idPerson, hashedPassword);

        public bool Delete(int idPerson) => _repository.Delete(idPerson);

        private static string HashIfNeeded(string password) =>
            PasswordHasher.IsHashed(password) ? password : PasswordHasher.Hash(password);
    }
}
