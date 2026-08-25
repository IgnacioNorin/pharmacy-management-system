using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePersonService : IPersonService
    {
        public bool RegisterResult { get; set; } = true;
        public bool UpdateResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public List<Person> ListResult { get; set; } = new List<Person>();

        public Person RegisteredWith { get; private set; }
        public Person UpdatedWith { get; private set; }
        public int? DeletedId { get; private set; }

        public bool Register(Person person)
        {
            RegisteredWith = person;
            return RegisterResult;
        }

        public bool Update(Person person)
        {
            UpdatedWith = person;
            return UpdateResult;
        }

        public List<Person> List() => ListResult;
        public Person GetByDocument(string document) => null;
        public bool UpdatePassword(int idPerson, string hashedPassword) => true;

        public bool Delete(int idPerson)
        {
            DeletedId = idPerson;
            return DeleteResult;
        }
    }
}
