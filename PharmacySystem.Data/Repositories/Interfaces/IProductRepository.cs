using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public interface IProductRepository
    {
        int Register(Product obj);
        bool Update(Product obj);
        List<Product> List();
        bool Verify(int idProduct);
        bool Delete(int idProduct);

        // Returns raw rows now that CultureInfoHelper/DateHelper live in Domain and formatting
        // moved to ReportPresenter - this used to build a pre-formatted-string DataTable here.
        List<ProductReportRow> Report(string categoryId);
    }
}
