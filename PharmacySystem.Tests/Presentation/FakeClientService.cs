using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeClientService : IClientService
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public List<Client> ClientsResult { get; set; } = new List<Client>();

        public Client RegisteredWith { get; private set; }
        public Client UpdatedWith { get; private set; }
        public int? DeletedId { get; private set; }
        public (int Page, int PageSize, string Search)? LastClientsPagedCall { get; private set; }

        public int Register(Client client)
        {
            RegisteredWith = client;
            return RegisterResult;
        }

        public bool Update(Client client)
        {
            UpdatedWith = client;
            return UpdateResult;
        }

        public List<Client> ListClients() => ClientsResult;

        // Pages over ClientsResult with the same text match the real query does, so a test only
        // needs to populate ClientsResult.
        public PagedResult<Client> ListClientsPaged(int pageNumber, int pageSize, string search)
        {
            LastClientsPagedCall = (pageNumber, pageSize, search);

            string term = (search ?? string.Empty).Trim();
            List<Client> matches = string.IsNullOrEmpty(term)
                ? ClientsResult
                : ClientsResult.Where(c =>
                    (c.name ?? "").Contains(term) ||
                    (c.document ?? "").Contains(term) ||
                    (c.businessName ?? "").Contains(term) ||
                    (c.email ?? "").Contains(term)).ToList();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = PagedResult<Client>.DefaultPageSize;

            return new PagedResult<Client>
            {
                Items = matches.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = matches.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public bool Delete(int idClient)
        {
            DeletedId = idClient;
            return DeleteResult;
        }
    }
}
