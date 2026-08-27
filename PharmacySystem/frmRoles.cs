using PharmacySystem.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace PharmacySystem
{
    // Roles admin screen. Hand-authored Designer.cs (no visual designer), same as ModalAlerts.
    public partial class frmRoles : Form, IRolesView
    {
        private readonly RolesPresenter _presenter;

        public frmRoles()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateRolesPresenter(this);
        }

        private void frmRoles_Load(object sender, EventArgs e) => _presenter.OnLoad();

        #region IRolesView

        public int? SelectedRoleId => (lstRoles.SelectedItem as RoleRow)?.Id;

        public string RoleNameInput => txtRoleName.Text;

        public IReadOnlyCollection<int> CheckedPermissionIds =>
            clbPermissions.CheckedItems.Cast<PermissionCheckItem>().Select(p => p.Id).ToList();

        public bool ConfirmDeleteRole() =>
            MessageBox.Show("¿Eliminar el rol seleccionado?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void LoadRoles(IEnumerable<RoleRow> roles)
        {
            lstRoles.SelectedIndexChanged -= lstRoles_SelectedIndexChanged;
            lstRoles.Items.Clear();
            foreach (RoleRow role in roles)
            {
                lstRoles.Items.Add(role);
            }
            lstRoles.SelectedIndexChanged += lstRoles_SelectedIndexChanged;
        }

        public void ShowRolePermissions(IEnumerable<PermissionCheckItem> permissions)
        {
            clbPermissions.Items.Clear();
            foreach (PermissionCheckItem permission in permissions)
            {
                clbPermissions.Items.Add(permission, permission.Checked);
            }
        }

        public void SetPermissionsEditable(bool editable)
        {
            clbPermissions.Enabled = editable;
            btnSavePermissions.Enabled = editable;
        }

        public void SetRoleActionsEnabled(bool canRename, bool canDelete)
        {
            btnRenameRole.Enabled = canRename;
            btnDeleteRole.Enabled = canDelete;
        }

        public void ClearRoleNameInput() => txtRoleName.Clear();

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);

        #endregion

        private void lstRoles_SelectedIndexChanged(object sender, EventArgs e) => _presenter.OnRoleSelected();
        private void btnSavePermissions_Click(object sender, EventArgs e) => _presenter.OnSavePermissions();
        private void btnNewRole_Click(object sender, EventArgs e) => _presenter.OnCreateRole();
        private void btnRenameRole_Click(object sender, EventArgs e) => _presenter.OnRenameRole();
        private void btnDeleteRole_Click(object sender, EventArgs e) => _presenter.OnDeleteRole();
    }
}
