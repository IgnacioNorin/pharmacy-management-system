using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    public interface IProductPickerView
    {
        void LoadProducts(IEnumerable<ProductPickerRow> products);
    }
}
