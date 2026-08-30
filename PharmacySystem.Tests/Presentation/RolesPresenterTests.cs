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
                    new Permission { Id = 1, Code = "ventas.acceso", Section = "ventas", Description = "Vender", ParentCode = null },
                    new Permission { Id = 2, Code = "productos.acceso", Section = "productos", Description = "Ver productos", ParentCode = null },
                    new Permission { Id = 3, Code = "productos.eliminar", Section = "productos", Description = "Eliminar productos", ParentCode = "productos.acceso" }
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
            return (new RolesPresenter(view, service, TestUser.With("roles.gestionar"), new FakeSecurityAudit()), view, service);
        }

        private static (RolesPresenter Presenter, FakeRolesView View, FakePermissionService Service, FakeSecurityAudit Audit) CreateAudited()
        {
            var view = new FakeRolesView();
            var service = ServiceWithRoles();
            var audit = new FakeSecurityAudit();
            return (new RolesPresenter(view, service, TestUser.With("roles.gestionar"), audit), view, service, audit);
        }

        [Fact]
        public void OnSavePermissions_AuditsTheChangeWithADiffSummary()
        {
            var (presenter, view, service, audit) = CreateAudited();
            presenter.OnLoad();
            view.SelectedRoleId = 100;                                  // "Cajero senior", currently has {1}
            view.CheckedPermissionIds = new List<int> { 2, 3 };         // -> productos.acceso (+ancestor), productos.eliminar

            presenter.OnSavePermissions();

            var evt = Assert.Single(audit.Recorded);
            Assert.Equal("role.permissions", evt.Action);
            Assert.Equal(100, evt.EntityId);
            Assert.Contains("+productos.acceso", evt.Summary);
            Assert.Contains("+productos.eliminar", evt.Summary);
            Assert.Contains("-ventas.acceso", evt.Summary);
            Assert.Contains("Cajero senior", evt.Summary);
        }

        [Fact]
        public void OnCreateRole_OnRenameRole_OnDeleteRole_AreAudited()
        {
            var (presenter, view, service, audit) = CreateAudited();
            presenter.OnLoad();

            view.RoleNameInput = "Reponedor";
            presenter.OnCreateRole();

            view.SelectedRoleId = 100;
            view.RoleNameInput = "Cajero";
            presenter.OnRenameRole();

            view.SelectedRoleId = 100;
            presenter.OnDeleteRole();

            Assert.Equal(new[] { "role.create", "role.rename", "role.delete" }, audit.Recorded.Select(e => e.Action));
            Assert.Contains("Reponedor", audit.Recorded[0].Summary);
            Assert.Contains("'Cajero senior' -> 'Cajero'", audit.Recorded[1].Summary);
        }

        // A catalogue that actually contains roles.gestionar (under usuarios.acceso), plus the two
        // built-in admin roles, for the "don't lock yourself out" guard tests.
        private const int UsuariosAccesoId = 9;
        private const int RolesGestionarId = 10;

        private static (RolesPresenter Presenter, FakeRolesView View, FakePermissionService Service) CreateWithRoleAdmin(
            params int[] rolesThatGrantRoleAdmin)
        {
            var view = new FakeRolesView();
            var service = new FakePermissionService
            {
                Catalogue = new List<Permission>
                {
                    new Permission { Id = 1, Code = "ventas.acceso", Section = "ventas", Description = "Vender", ParentCode = null },
                    new Permission { Id = UsuariosAccesoId, Code = "usuarios.acceso", Section = "usuarios", Description = "Usuarios", ParentCode = null },
                    new Permission { Id = RolesGestionarId, Code = "roles.gestionar", Section = "usuarios", Description = "Administrar roles", ParentCode = "usuarios.acceso" }
                },
                Roles = new List<TypePerson>
                {
                    new TypePerson { idPersonType = 1, description = "Administrador General", IsSystem = true },
                    new TypePerson { idPersonType = 2, description = "Administrador", IsSystem = true }
                }
            };
            service.RolesGrantingByCode["roles.gestionar"] = rolesThatGrantRoleAdmin.ToList();
            return (new RolesPresenter(view, service, TestUser.With("roles.gestionar"), new FakeSecurityAudit()), view, service);
        }

        [Fact]
        public void OnSavePermissions_StrippingRolesGestionarFromItsOnlyHolder_IsBlocked()
        {
            var (presenter, view, service) = CreateWithRoleAdmin(1);
            presenter.OnLoad();
            view.SelectedRoleId = 1;
            view.CheckedPermissionIds = new List<int> { UsuariosAccesoId }; // roles.gestionar unchecked

            presenter.OnSavePermissions();

            Assert.Null(service.SavedRolePermissions);
            Assert.Contains("unico rol", view.ShownMessages.Single());
        }

        [Fact]
        public void OnSavePermissions_StrippingRolesGestionarWhenAnotherRoleAlsoHasIt_IsAllowed()
        {
            var (presenter, view, service) = CreateWithRoleAdmin(1, 2);
            presenter.OnLoad();
            view.SelectedRoleId = 1;
            view.CheckedPermissionIds = new List<int> { UsuariosAccesoId }; // roles.gestionar unchecked

            presenter.OnSavePermissions();

            Assert.Equal(new[] { UsuariosAccesoId }, service.SavedRolePermissions.Value.Ids);
            Assert.Contains("guardados", view.ShownMessages.Single());
        }

        [Fact]
        public void OnSavePermissions_KeepingRolesGestionarChecked_SavesEvenIfItIsTheOnlyHolder()
        {
            var (presenter, view, service) = CreateWithRoleAdmin(1);
            presenter.OnLoad();
            view.SelectedRoleId = 1;
            view.CheckedPermissionIds = new List<int> { RolesGestionarId };

            presenter.OnSavePermissions();

            // usuarios.acceso (9) is pulled in as the ancestor of roles.gestionar (10).
            Assert.Equal(new[] { UsuariosAccesoId, RolesGestionarId }, service.SavedRolePermissions.Value.Ids);
            Assert.Contains("guardados", view.ShownMessages.Single());
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
        public void OnRoleSelected_BuildsATreeWithSectionRootsAndNestedChildren()
        {
            var (presenter, view, _) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;

            presenter.OnRoleSelected();

            // Two section roots (ventas.acceso, productos.acceso); productos.eliminar sits under
            // productos.acceso, not at the top level.
            Assert.Equal(new[] { 1, 2 }, view.ShownPermissionRoots.Select(n => n.Id).ToArray());
            PermissionNode productos = view.ShownPermissionRoots.Single(n => n.Id == 2);
            Assert.Equal(new[] { 3 }, productos.Children.Select(c => c.Id).ToArray());
        }

        [Fact]
        public void OnSavePermissions_ChildWithoutItsParent_StillSavesTheParent()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;
            view.CheckedPermissionIds = new List<int> { 3 }; // productos.eliminar only

            presenter.OnSavePermissions();

            // productos.acceso (2) is pulled in as the ancestor of productos.eliminar (3).
            Assert.Equal(new[] { 2, 3 }, service.SavedRolePermissions.Value.Ids);
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
        public void OnCreateRole_Succeeds_ReloadsRolesClearsInputAndPermissionPanel()
        {
            var (presenter, view, service) = Create();
            presenter.OnLoad();
            view.SelectedRoleId = 100;
            presenter.OnRoleSelected();
            service.CreateRoleResult = 101;
            view.RoleNameInput = "Deposito";

            presenter.OnCreateRole();

            Assert.Equal(1, view.ClearRoleNameInputCount);
            Assert.NotNull(view.LoadedRoles);
            Assert.Empty(view.ShownPermissions);
            Assert.False(view.PermissionsEditable);
        }

        [Fact]
        public void MutatingActions_WithoutRolesGestionar_AreRejected()
        {
            var view = new FakeRolesView { SelectedRoleId = 100, RoleNameInput = "X" };
            var service = ServiceWithRoles();
            var presenter = new RolesPresenter(view, service, TestUser.With(), new FakeSecurityAudit());

            presenter.OnSavePermissions();
            presenter.OnCreateRole();
            presenter.OnRenameRole();
            presenter.OnDeleteRole();

            Assert.All(view.ShownMessages, m => Assert.Contains("No tiene permiso", m));
            Assert.Equal(4, view.ShownMessages.Count);
            Assert.Null(service.SavedRolePermissions);
            Assert.Null(service.CreatedRoleName);
            Assert.Null(service.RenamedRole);
            Assert.Null(service.DeletedRoleId);
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
