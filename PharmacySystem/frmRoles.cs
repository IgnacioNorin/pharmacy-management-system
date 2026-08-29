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
        private bool _suppressAfterCheck;

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
            AllNodes(tvPermissions.Nodes).Where(n => n.Checked).Select(n => (int)n.Tag).ToList();

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

        public void ShowRolePermissions(IEnumerable<PermissionNode> permissionTree)
        {
            _suppressAfterCheck = true;
            tvPermissions.BeginUpdate();
            tvPermissions.Nodes.Clear();
            foreach (PermissionNode node in permissionTree)
            {
                tvPermissions.Nodes.Add(ToTreeNode(node));
            }
            tvPermissions.ExpandAll();
            tvPermissions.EndUpdate();
            _suppressAfterCheck = false;
        }

        private static TreeNode ToTreeNode(PermissionNode node)
        {
            TreeNode tn = new TreeNode(node.Description) { Tag = node.Id, Checked = node.Checked };
            foreach (PermissionNode child in node.Children)
            {
                tn.Nodes.Add(ToTreeNode(child));
            }
            return tn;
        }

        private static IEnumerable<TreeNode> AllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                yield return node;
                foreach (TreeNode child in AllNodes(node.Nodes))
                {
                    yield return child;
                }
            }
        }

        // Checking a node pulls in its ancestors; unchecking a node clears its descendants -
        // "no child without its parent". _suppressAfterCheck guards the re-entrancy from setting
        // Checked inside this handler (and from the initial ShowRolePermissions load).
        private void tvPermissions_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_suppressAfterCheck)
            {
                return;
            }

            _suppressAfterCheck = true;
            if (e.Node.Checked)
            {
                for (TreeNode ancestor = e.Node.Parent; ancestor != null; ancestor = ancestor.Parent)
                {
                    ancestor.Checked = true;
                }
            }
            else
            {
                foreach (TreeNode descendant in AllNodes(e.Node.Nodes))
                {
                    descendant.Checked = false;
                }
            }
            _suppressAfterCheck = false;
        }

        public void SetPermissionsEditable(bool editable)
        {
            tvPermissions.Enabled = editable;
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
