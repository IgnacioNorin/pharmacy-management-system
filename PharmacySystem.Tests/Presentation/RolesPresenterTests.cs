using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class RolesPresenterTests
    {
        private static FakePermissionService ServiceWithRoles()
        {
            var service = new FakePermissionService
            {
                Catalogue = new List<Permission>
                {
                    new Permission { Id = 1, Code = "ventas.acceso", Section = "ventas", Description = "Vender" },
                    new Permission { Id = 2, Code = "productos.eliminar", Section = "productos", Description = "Eliminar productos" }
                },
                Roles = new List<TypePerson>
                {
                    new TypePerson { idPersonType = 2, description = "Administrador", IsSystem = true },
                    new TypePerson { idPersonType = 100, description = "Cajero senior", IsSystem = false }
                }
            };
            service.PermissionIdsByRole[100] = new List<int> { 1 };
            return service;
        }

        private static (RolesPresenter Presenter, FakeRolesView View, FakePermissionService Service) Create()
        {
            var view = new FakeRolesView();
            var service = ServiceWithRoles();
            return (new RolesPresenter(view, service), view, service);
        }

        [Fact]
        public void OnLoad_LoadsRolesAndClearsPermissionPanel()
        {
            var (presenter, view, _) = Create();

            presenter.OnLoad();

            Assert.Equal(new[] { "Administrador", "Cajero senior" }, view.LoadedRoles.Select(r => r.Name));
            Assert.Empty(view.ShownPermissions);
            Assert.False(view.PermissionsEditable);
        }

        [Fact]
        public void OnRoleSelected_CustomRole_ShowsPermissionsCheckedFromTheRoleAndEnablesActions()
        {
            var (presenter, view, _) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;

            presenter.OnRoleSelected();

            Assert.True(view.ShownPermissions.Single(p => p.Id == 1).Checked);
            Assert.False(view.ShownPermissions.Single(p => p.Id == 2).Checked);
            Assert.True(view.PermissionsEditable);
            Assert.Equal((true, true), view.RoleActionsEnabled);
        }

        [Fact]
        public void OnRoleSelected_SystemRole_DisablesRenameAndDelete()
        {
            var (presenter, view, _) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 2;

            presenter.OnRoleSelected();

            Assert.True(view.PermissionsEditable);
            Assert.Equal((false, false), view.RoleActionsEnabled);
        }

        [Fact]
        public void OnSavePermissions_SendsCheckedIdsToService()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;
            view.CheckedPermissionIds = new List<int> { 1, 2 };

            presenter.OnSavePermissions();

            Assert.Equal(100, service.SavedRolePermissions.Value.RoleId);
            Assert.Equal(new[] { 1, 2 }, service.SavedRolePermissions.Value.Ids);
            Assert.Contains("guardados", view.ShownMessages.Single());
        }

        [Fact]
        public void OnCreateRole_BlankName_ShowsMessageAndDoesNotCallService()
        {
            var (presenter, view, service) = Create();
            view.RoleNameInput = "   ";

            presenter.OnCreateRole();

            Assert.Null(service.CreatedRoleName);
            Assert.Contains("nombre", view.ShownMessages.Single());
        }

        [Fact]
        public void OnCreateRole_DuplicateName_ShowsMessage()
        {
            var (presenter, view, service) = Create();
            service.CreateRoleResult = 0;
            view.RoleNameInput = "Administrador";

            presenter.OnCreateRole();

            Assert.Equal("Administrador", service.CreatedRoleName);
            Assert.Contains("Ya existe", view.ShownMessages.Single());
        }

        [Fact]
        public void OnCreateRole_Succeeds_ReloadsRolesAndClearsInput()
        {
            var (presenter, view, service) = Create();
            service.CreateRoleResult = 101;
            view.RoleNameInput = "Deposito";

            presenter.OnCreateRole();

            Assert.Equal(1, view.ClearRoleNameInputCount);
            Assert.NotNull(view.LoadedRoles);
        }

        [Fact]
        public void OnRenameRole_SystemRole_IsRejectedWithoutCallingService()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 2;
            view.RoleNameInput = "Otro nombre";

            presenter.OnRenameRole();

            Assert.Null(service.RenamedRole);
            Assert.Contains("sistema", view.ShownMessages.Single());
        }

        [Fact]
        public void OnRenameRole_CustomRole_CallsService()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;
            view.RoleNameInput = "Cajero jefe";

            presenter.OnRenameRole();

            Assert.Equal((100, "Cajero jefe"), service.RenamedRole);
        }

        [Fact]
        public void OnDeleteRole_SystemRole_IsRejected()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 2;

            presenter.OnDeleteRole();

            Assert.Null(service.DeletedRoleId);
            Assert.Contains("sistema", view.ShownMessages.Single());
        }

        [Fact]
        public void OnDeleteRole_CustomRoleWithUsers_ShowsAssignedMessage()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;
            service.DeleteRoleResult = false;

            presenter.OnDeleteRole();

            Assert.Equal(100, service.DeletedRoleId);
            Assert.Contains("usuarios asignados", view.ShownMessages.Single());
        }

        [Fact]
        public void OnDeleteRole_NotConfirmed_DoesNothing()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;
            view.ConfirmDeleteRoleResult = false;

            presenter.OnDeleteRole();

            Assert.Null(service.DeletedRoleId);
        }
    }
}
