using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface ICategoryRepository
    {
        int Register(Categories obj);
        bool Update(Categories obj);
        List<Categories> List();
        bool Delete(int idCategory);
    }
}
