using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // Report() deliberately has no home here yet. In the original ProductService it formats
    // currency/dates using CultureInfoHelper/DateHelper, which live in the WinForms project -
    // Data cannot reference those without a circular dependency, and moving that formatting
    // helper down prematurely would misplace a presentation concern. It stays on the adapter
    // (PharmacySystem.Logical.ProductService) untouched until frmReport itself is migrated,
    // which is where a real answer (repository returns entities, presenter formats) belongs.
    public interface IProductRepository
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        bool Verify(int idProduct);
        bool Delete(int idProduct);
    }
}
