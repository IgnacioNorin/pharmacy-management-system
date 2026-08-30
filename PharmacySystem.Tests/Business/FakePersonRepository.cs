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

        public Person GetByDocumentResult { get; set; }
        public Person GetByIdResult { get; set; }
        public List<Person> ListResult { get; set; } = new List<Person>();

        public (int Id, string Hash) UpdatePasswordCall { get; private set; }
        public (int Id, string Hash, bool MustChange)? SetPasswordAndFlagCall { get; private set; }

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
        public Person GetByDocument(string document) => GetByDocumentResult;
        public Person GetById(int idPerson) => GetByIdResult;

        public bool UpdatePassword(int idPerson, string hashedPassword)
        {
            UpdatePasswordCall = (idPerson, hashedPassword);
            return true;
        }

        public bool SetPasswordAndFlag(int idPerson, string hashedPassword, bool mustChangePassword)
        {
            SetPasswordAndFlagCall = (idPerson, hashedPassword, mustChangePassword);
            return true;
        }

        public (int Id, bool Active)? SetActiveCall { get; private set; }
        public bool SetActive(int idPerson, bool active)
        {
            SetActiveCall = (idPerson, active);
            return true;
        }

        public bool Delete(int idPerson) => true;
    }
}
