namespace PharmacySystem.Model
{
    // One row of the permission catalogue. Code is the identifier used everywhere in code
    // (e.g. "productos.eliminar"); Section groups codes; ParentCode is the code of the
    // permission this one sits under in the roles admin tree (null for a section root).
    public class Permission
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ParentCode { get; set; }
    }
}
