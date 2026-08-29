using System.Collections.Generic;

namespace PharmacySystem.Presentation
{
    // One node of the roles admin permission tree (frmRoles). A section root carries its own Id
    // (the '<section>.acceso' permission) plus Children; a leaf has an empty Children list.
    // Checked reflects whether the selected role currently has that permission.
    public class PermissionNode
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool Checked { get; set; }
        public List<PermissionNode> Children { get; } = new List<PermissionNode>();
    }
}
