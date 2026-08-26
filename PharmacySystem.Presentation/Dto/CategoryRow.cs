using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public class CategoryRow
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public static CategoryRow From(Categories category) => new CategoryRow
        {
            Id = category.IdCategory,
            Description = category.description
        };
    }
}
