using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakePersonRepository : IPersonRepository
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public Person RegisteredWith { get; private set; }
        public Person UpdatedWith { get; private set; }

        public int Register(Person person)
        {
            RegisteredWith = person;
            return RegisterResult;
        }

        public bool Update(Person person)
        {
            UpdatedWith = person;
            return UpdateResult;
        }

        public List<Person> List() => new List<Person>();
        public Person GetByDocument(string document) => null;
        public bool UpdatePassword(int idPerson, string hashedPassword) => true;
        public bool Delete(int idPerson) => true;
    }
}
