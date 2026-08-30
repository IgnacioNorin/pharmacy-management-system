using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface ISupplierRepository
    {
        // Returns the new id, or 0 if a supplier with the same document already exists
        // (sp_create_supplier's own duplicate check) or the insert failed.
        int Register(Supplier obj);

        bool Update(Supplier obj);

        List<Supplier> List();
        PagedResult<Supplier> ListPaged(int pageNumber, int pageSize, string search);

        bool Delete(int idSupplier);
    }
}
