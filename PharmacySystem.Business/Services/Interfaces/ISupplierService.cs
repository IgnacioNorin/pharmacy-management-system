using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface ISupplierService
    {
        int Register(Supplier obj);
        bool Update(Supplier obj);
        List<Supplier> List();
        bool Delete(int idSupplier);
    }
}
