using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IProductService
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        bool Verify(int idProduct);
        bool Delete(int idProduct);
    }
}
