using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public class ProductPickerRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CategoryDescription { get; set; }
        public int Stock { get; set; }
        public decimal SalePrice { get; set; }

        public static ProductPickerRow From(Product product) => new ProductPickerRow
        {
            Id = product.idProduct,
            Code = product.code,
            Name = product.name,
            Description = product.description,
            CategoryDescription = product.oCategory.description,
            Stock = product.stock,
            SalePrice = product.salePrice
        };
    }
}
