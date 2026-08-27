namespace PharmacySystem.Presentation
{
    // One permission in the roles admin checklist for the selected role.
    public class PermissionCheckItem
    {
        public int Id { get; set; }
        public string Section { get; set; }
        public string Description { get; set; }
        public bool Checked { get; set; }

        // Shown in the permissions checklist.
        public override string ToString() => "[" + Section + "] " + Description;
    }
}
