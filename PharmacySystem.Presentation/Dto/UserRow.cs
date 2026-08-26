namespace PharmacySystem.Presentation
{
    // What the grid displays for a user (person_type_id != 3: Administrador/Empleado).
    // RoleText and Password are shown as stored/selected verbatim - RoleText comes from either
    // person_type.description (on load) or the role combo's selected text (on save), and
    // Password is whatever is in the person.password column, which is already hashed.
    public class UserRow
    {
        public int Id { get; set; }
        public string Document { get; set; }
        public string Name { get; set; }
        public string RoleText { get; set; }
        public string Password { get; set; }
    }
}
