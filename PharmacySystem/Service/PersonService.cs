using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmClient.cs, frmUser.cs, Login.cs,
    // ModalPerson.cs). Delegates to PharmacySystem.Business, which now owns the
    // hash-if-not-already-hashed decision; delete this class once nothing calls .Instance.
    public class PersonService
    {
        private static PersonService _instance = null;
        private readonly Business.IPersonService _inner;

        public PersonService()
        {
            _inner = new Business.PersonService(new PersonRepository(CompositionRoot.ConnectionFactory));
        }

        public static PersonService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PersonService();
                }

                return _instance;
            }
        }

        public bool RegisterPerson(Person person) => _inner.Register(person);

        public bool UpdatePerson(Person person) => _inner.Update(person);

        public List<Person> ListPerson() => _inner.List();

        public Person GetPersonByDocument(string document) => _inner.GetByDocument(document);

        public bool UpdatePassword(int idPerson, string hashedPassword) => _inner.UpdatePassword(idPerson, hashedPassword);

        public bool DeletePerson(int idPerson) => _inner.Delete(idPerson);
    }
}
