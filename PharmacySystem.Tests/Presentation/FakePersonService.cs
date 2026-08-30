using System;
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
        public Exception GetByDocumentThrows { get; set; }

        public Person RegisteredWith { get; private set; }
        public Person UpdatedWith { get; private set; }
        public int? DeletedId { get; private set; }
        public int? UpdatedPasswordForId { get; private set; }
        public string UpdatedPasswordHash { get; private set; }
        public string RequestedDocument { get; private set; }

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

        public List<Person> List() => ListResult;

        public Person GetByDocument(string document)
        {
            RequestedDocument = document;
            if (GetByDocumentThrows != null) throw GetByDocumentThrows;
            return GetByDocumentResult;
        }

        public Person GetByIdResult { get; set; }
        public Exception GetByIdThrows { get; set; }
        public int? RequestedId { get; private set; }
        public Person GetById(int idPerson)
        {
            RequestedId = idPerson;
            if (GetByIdThrows != null) throw GetByIdThrows;
            return GetByIdResult;
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

        public (int Id, bool Active)? SetActiveCall { get; private set; }
        public bool SetActive(int idPerson, bool active)
        {
            SetActiveCall = (idPerson, active);
            return true;
        }
    }
}
