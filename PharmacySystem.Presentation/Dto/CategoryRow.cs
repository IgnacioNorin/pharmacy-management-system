using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public class CategoryRow
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;

        public static CategoryRow From(Categories category) => new CategoryRow
        {
            Id = category.IdCategory,
            Description = category.description
        };
    }
}
