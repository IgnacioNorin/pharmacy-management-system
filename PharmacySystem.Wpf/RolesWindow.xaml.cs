using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of frmRoles. Implements the same IRolesView; RolesPresenter is unchanged. The
    // permission tree's cascade ("checking pulls in ancestors, unchecking clears descendants")
    // is applied here on the CheckBox check events, guarded against re-entrancy.
    public partial class RolesWindow : Window, IRolesView
    {
        private readonly RolesPresenter _presenter;
        private List<PermNodeVm> _permRoots = new List<PermNodeVm>();
        private bool _suppressCascade;

        public RolesWindow(Func<IRolesView, RolesPresenter> presenterFactory)
        {
            InitializeComponent();
            _presenter = presenterFactory(this);
            Loaded += (s, e) => _presenter.OnLoad();
        }

        #region IRolesView

        public int? SelectedRoleId => (lstRoles.SelectedItem as RoleRow)?.Id;
        public string RoleNameInput => txtRoleName.Text;

        public IReadOnlyCollection<int> CheckedPermissionIds =>
            _permRoots.SelectMany(r => r.DescendantsAndSelf()).Where(n => n.IsChecked).Select(n => n.Id).ToList();

        public bool ConfirmDeleteRole() =>
            MessageBox.Show(this, "¿Eliminar el rol seleccionado?", "Mensaje",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public void LoadRoles(IEnumerable<RoleRow> roles)
        {
            lstRoles.SelectionChanged -= lstRoles_SelectionChanged;
            lstRoles.ItemsSource = roles.ToList();
            lstRoles.SelectionChanged += lstRoles_SelectionChanged;
        }

        public void ShowRolePermissions(IEnumerable<PermissionNode> permissionTree)
        {
            _suppressCascade = true;
            _permRoots = permissionTree.Select(n => ToVm(n, null)).ToList();
            tvPermissions.ItemsSource = _permRoots;
            _suppressCascade = false;
        }

        private static PermNodeVm ToVm(PermissionNode node, PermNodeVm? parent)
        {
            var vm = new PermNodeVm { Id = node.Id, Description = node.Description, IsChecked = node.Checked, Parent = parent };
            foreach (PermissionNode child in node.Children)
                vm.Children.Add(ToVm(child, vm));
            return vm;
        }

        public void SetPermissionsEditable(bool editable)
        {
            tvPermissions.IsEnabled = editable;
            btnSavePermissions.IsEnabled = editable;
        }

        public void SetRoleActionsEnabled(bool canRename, bool canDelete)
        {
            btnRenameRole.IsEnabled = canRename;
            btnDeleteRole.IsEnabled = canDelete;
        }

        public void ClearRoleNameInput() => txtRoleName.Clear();

        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);

        #endregion

        private void Permission_CheckChanged(object sender, RoutedEventArgs e)
        {
            if (_suppressCascade) return;
            if (!((e.OriginalSource as CheckBox)?.DataContext is PermNodeVm node)) return;

            _suppressCascade = true;
            if (node.IsChecked)
            {
                for (PermNodeVm? ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
                    ancestor.IsChecked = true;
            }
            else
            {
                foreach (PermNodeVm descendant in node.Children.SelectMany(c => c.DescendantsAndSelf()))
                    descendant.IsChecked = false;
            }
            _suppressCascade = false;
        }

        private void lstRoles_SelectionChanged(object sender, SelectionChangedEventArgs e) => _presenter.OnRoleSelected();
        private void btnSavePermissions_Click(object sender, RoutedEventArgs e) => _presenter.OnSavePermissions();
        private void btnNewRole_Click(object sender, RoutedEventArgs e) => _presenter.OnCreateRole();
        private void btnRenameRole_Click(object sender, RoutedEventArgs e) => _presenter.OnRenameRole();
        private void btnDeleteRole_Click(object sender, RoutedEventArgs e) => _presenter.OnDeleteRole();
    }
}
