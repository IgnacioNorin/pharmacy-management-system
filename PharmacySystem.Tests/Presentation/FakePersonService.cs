using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePersonService : IPersonService
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public bool UpdatePasswordResult { get; set; } = true;
        public List<Person> ListResult { get; set; } = new List<Person>();
        public Person GetByDocumentResult { get; set; }

        public Person RegisteredWith { get; private set; }
        public Person UpdatedWith { get; private set; }
        public int? DeletedId { get; private set; }
        public int? UpdatedPasswordForId { get; private set; }
        public string UpdatedPasswordHash { get; private set; }

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

        public string RequestedDocument { get; private set; }

        public List<Person> List() => ListResult;
        public Person GetByDocument(string document)
        {
            RequestedDocument = document;
            return GetByDocumentResult;
        }

        public bool UpdatePassword(int idPerson, string hashedPassword)
        {
            UpdatedPasswordForId = idPerson;
            UpdatedPasswordHash = hashedPassword;
            return UpdatePasswordResult;
        }

        public bool Delete(int idPerson)
        {
            DeletedId = idPerson;
            return DeleteResult;
        }
    }
}
