using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface ICategoryService
    {
        int Register(Categories obj);
        bool Update(Categories obj);
        List<Categories> List();
        bool Delete(int idCategory);
    }
}
