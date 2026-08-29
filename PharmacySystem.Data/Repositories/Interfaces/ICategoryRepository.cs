using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface ICategoryRepository
    {
        int Register(Categories obj);
        bool Update(Categories obj);
        List<Categories> List();
        // List() plus any inactive category still on an active product - for the product form combo.
        List<Categories> ListForProductForm();
        bool Delete(int idCategory);
    }
}
