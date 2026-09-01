using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public class ProductPickerRow
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryDescription { get; set; } = string.Empty;
        public int Stock { get; set; }
        public decimal SalePrice { get; set; }

        public static ProductPickerRow From(Product product) => new ProductPickerRow
        {
            Id = product.idProduct,
            Code = product.code,
            Name = product.name,
            Description = product.description,
            CategoryDescription = product.oCategory?.description ?? string.Empty,
            Stock = product.stock,
            SalePrice = product.salePrice
        };
    }
}
