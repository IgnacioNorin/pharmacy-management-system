namespace PharmacySystem.Presentation
{
    // What the grid displays for a user (person_type_id != 4: Administrador General/Administrador/
    // Empleado). RoleText comes from either person_type.description (on load) or the role combo's
    // selected text (on save). The password column is deliberately not carried here - it never
    // needs to reach the view, and round-tripping the stored hash through the grid only put it a
    // click away from the password box.
    public class UserRow
    {
        public int Id { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public string RoleText { get; set; }
    }
}
