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

        public bool Register(Person person)
        {
            person.password = HashIfNeeded(person.password);
            return _repository.Register(person);
        }

        public bool Update(Person person)
        {
            person.password = HashIfNeeded(person.password);
            return _repository.Update(person);
        }

        public List<Person> List() => _repository.List();

        public Person GetByDocument(string document) => _repository.GetByDocument(document);

        public bool UpdatePassword(int idPerson, string hashedPassword) => _repository.UpdatePassword(idPerson, hashedPassword);

        public bool Delete(int idPerson) => _repository.Delete(idPerson);

        private static string HashIfNeeded(string password) =>
            PasswordHasher.IsHashed(password) ? password : PasswordHasher.Hash(password);
    }
}
