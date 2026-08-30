using System;
using System.Collections.Generic;
using System.Linq;
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
        public List<Person> ListClientsResult { get; set; }
        public Person GetByDocumentResult { get; set; }
        public Exception GetByDocumentThrows { get; set; }

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

        // Default: derive active clients from ListResult, so existing tests that only set ListResult
        // keep working; ListClientsResult overrides when a test needs to.
        public List<Person> ListClients() => ListClientsResult ??
            ListResult.FindAll(p => (p.oPersonType?.idPersonType ?? 0) == (int)PharmacySystem.Model.PersonType.Cliente && p.Estado);

        public (int Page, int PageSize, string Search)? LastClientsPagedCall { get; private set; }

        // Pages over whatever ListClients() would return, applying the same text match the real
        // query does, so a test only needs to populate ListResult or ListClientsResult.
        public PagedResult<Person> ListClientsPaged(int pageNumber, int pageSize, string search)
        {
            LastClientsPagedCall = (pageNumber, pageSize, search);

            List<Person> all = ListClients();
            string term = (search ?? string.Empty).Trim();
            List<Person> matches = string.IsNullOrEmpty(term)
                ? all
                : all.Where(p =>
                    (p.name ?? "").Contains(term) ||
                    (p.document ?? "").Contains(term) ||
                    (p.businessName ?? "").Contains(term) ||
                    (p.email ?? "").Contains(term)).ToList();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = PagedResult<Person>.DefaultPageSize;

            return new PagedResult<Person>
            {
                Items = matches.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = matches.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public Person GetByDocument(string document)
        {
            RequestedDocument = document;
            if (GetByDocumentThrows != null) throw GetByDocumentThrows;
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
