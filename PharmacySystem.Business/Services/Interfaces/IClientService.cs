using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IClientService
    {
        int Register(Client client);
        bool Update(Client client);
        List<Client> ListClients();
        PagedResult<Client> ListClientsPaged(int pageNumber, int pageSize, string search);
        bool Delete(int idClient);
    }
}
