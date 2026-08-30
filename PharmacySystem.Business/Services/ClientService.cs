using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin, like SupplierService: clients have no password to hash and no rules beyond the
    // document-uniqueness the repository/index already enforce. The seam is where a real rule
    // (credit limit, loyalty tier) would land later.
    public class ClientService : IClientService
    {
        private readonly IClientRepository _repository;

        public ClientService(IClientRepository repository)
        {
            _repository = repository;
        }

        public int Register(Client client) => _repository.Register(client);

        public bool Update(Client client) => _repository.Update(client);

        public List<Client> ListClients() => _repository.ListClients();

        public PagedResult<Client> ListClientsPaged(int pageNumber, int pageSize, string search) =>
            _repository.ListClientsPaged(pageNumber, pageSize, search);

        public bool Delete(int idClient) => _repository.Delete(idClient);
    }
}
