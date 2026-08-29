using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface ICategoryService
    {
        int Register(Categories obj);
        bool Update(Categories obj);
        List<Categories> List();
        List<Categories> ListForProductForm();
        bool Delete(int idCategory);
    }
}
