using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeClientRepository : IClientRepository
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public List<Client> ListClientsResult { get; set; } = new List<Client>();
        public PagedResult<Client> ListClientsPagedResult { get; set; }

        public Client RegisteredWith { get; private set; }
        public Client UpdatedWith { get; private set; }
        public int? DeletedId { get; private set; }
        public (int Page, int PageSize, string Search)? LastPagedCall { get; private set; }

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

        public List<Client> ListClients() => ListClientsResult;

        public PagedResult<Client> ListClientsPaged(int pageNumber, int pageSize, string search)
        {
            LastPagedCall = (pageNumber, pageSize, search);
            return ListClientsPagedResult ?? PagedResult<Client>.Empty(pageSize);
        }

        public bool Delete(int idClient)
        {
            DeletedId = idClient;
            return DeleteResult;
        }
    }
}
