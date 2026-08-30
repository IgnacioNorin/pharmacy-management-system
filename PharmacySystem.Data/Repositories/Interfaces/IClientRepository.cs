using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // Retail customers. Their own table since the person/user split: no password, no role.
    public interface IClientRepository
    {
        // The new client's id, or 0 if the insert failed (duplicate document or error).
        int Register(Client client);
        bool Update(Client client);
        // Active clients only. For the client picker / screen / report filter.
        List<Client> ListClients();
        PagedResult<Client> ListClientsPaged(int pageNumber, int pageSize, string search);
        bool Delete(int idClient);
    }
}
