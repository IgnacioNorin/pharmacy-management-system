namespace PharmacySystem.Presentation
{
    // A role as shown in the roles admin list. IsSystem = one of the four built-ins, which the
    // screen lets you re-permission but not rename or delete.
    public class RoleRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsSystem { get; set; }

        // Shown in the roles list.
        public override string ToString() => IsSystem ? Name + "  (sistema)" : Name;
    }
}
