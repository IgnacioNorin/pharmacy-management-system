namespace PharmacySystem.Model
{
    // One row of the permission catalogue. Code is the identifier used everywhere in code
    // (e.g. "productos.eliminar"); Section groups codes for the roles admin screen.
    public class Permission
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Section { get; set; }
        public string Description { get; set; }
    }
}
