using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class UserPresenterTests
    {
        private static PharmacySystem.Presentation.UserPresenter CreatePresenter(FakeUserView view, FakePersonService service)
            => CreatePresenter(view, service, new FakePermissionService());

        private static PharmacySystem.Presentation.UserPresenter CreatePresenter(FakeUserView view, FakePersonService service, FakePermissionService permissionService)
            => new PharmacySystem.Presentation.UserPresenter(view, service, TestUser.With("usuarios.gestionar"), permissionService);

        private static PharmacySystem.Presentation.UserPresenter CreatePresenter(FakeUserView view, FakePersonService service, CurrentUser user)
            => new PharmacySystem.Presentation.UserPresenter(view, service, user, new FakePermissionService());

        private static Person AdminGeneral(int id, bool active = true) =>
            new Person { idPerson = id, name = "AG" + id, Estado = active,
                         oPersonType = new TypePerson { idPersonType = 1, description = "Administrador General" } };

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotRegister()
        {
            var view = ValidView();
            new PharmacySystem.Presentation.UserPresenter(view, new FakePersonService(), TestUser.With(), new FakePermissionService()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.AddedRows);
        }

        [Fact]
        public void OnDelete_WithoutManagePermission_ShowsDeniedAndDoesNotRemove()
        {
            var view = new FakeUserView { SelectedIndex = 3, UserId = 9 };
            new PharmacySystem.Presentation.UserPresenter(view, new FakePersonService(), TestUser.With(), new FakePermissionService()).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnLoad_LoadsRoleOptionsFromPersonTypes()
        {
            var view = new FakeUserView();
            var permissions = new FakePermissionService
            {
                Roles = new List<TypePerson>
                {
                    new TypePerson { idPersonType = 2, description = "Administrador" },
                    new TypePerson { idPersonType = 100, description = "Cajero senior" }
                }
            };

            CreatePresenter(view, new FakePersonService(), permissions).OnLoad();

            Assert.Equal(new[] { "Administrador", "Cajero senior" }, view.LoadedRoleOptions.Select(o => o.Text));
        }

        private static FakeUserView ValidView() => new FakeUserView
        {
            UserId = 0,
            Document = "123",
            Name = "Test",
            Password = "Passw0rd!",
            ConfirmPassword = "Passw0rd!",
            RowCount = 0
        };

        [Fact]
        public void OnLoad_LoadsEveryUserFromTheService()
        {
            var view = new FakeUserView();
            var service = new FakePersonService
            {
                ListResult = new List<Person>
                {
                    new Person { idPerson = 1, name = "Admin", oPersonType = new TypePerson { idPersonType = 2, description = "Administrador" } },
                    new Person { idPerson = 2, name = "Empleado", oPersonType = new TypePerson { idPersonType = 3, description = "Empleado" } }
                }
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Equal(2, view.LoadedUsers.Count);
            Assert.Equal(new[] { "Admin", "Empleado" }, view.LoadedUsers.Select(u => u.Name));
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = ValidView();
            view.ValidationErrors = new List<string> { "error" };
            var service = new FakePersonService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new List<string> { "error" }, view.ShownValidationErrors);
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnSave_PasswordsDoNotMatch_ShowsMismatchBeforeCallingService()
        {
            var view = ValidView();
            view.ConfirmPassword = "different";
            var service = new FakePersonService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(1, view.PasswordMismatchCount);
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnSave_NewUser_Succeeds_AddsRowWithComboRoleText()
        {
            var view = ValidView();
            view.RoleId = 2;
            view.RoleText = "Empleado";
            var service = new FakePersonService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(2, service.RegisteredWith.oPersonType.idPersonType);
            Assert.Single(view.AddedRows);
            Assert.Equal("Empleado", view.AddedRows[0].RoleText);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_RegisterFails_ShowsDuplicateMessage()
        {
            var view = ValidView();
            var service = new FakePersonService { RegisterResult = 0 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "Ya existe un usuario con ese documento" }, view.ShownMessages);
            Assert.Empty(view.AddedRows);
            Assert.False(view.ClearFormCalled);
        }

        // Unlike ClientPresenter, a failed Update here returns completely silently - same shape
        // as SupplierPresenter. Locks in the original frmUser.cs's `if (!result) return;`.
        [Fact]
        public void OnSave_UpdateFails_DoesNothingSilently()
        {
            var view = ValidView();
            view.UserId = 7;
            view.SelectedIndex = 1;
            view.RowCount = 5;
            var service = new FakePersonService { UpdateResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Empty(view.ReplacedRows);
            Assert.False(view.ClearFormCalled);
            Assert.Empty(view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingUser_UpdateSucceeds_ReplacesRow()
        {
            var view = ValidView();
            view.UserId = 7;
            view.SelectedIndex = 2;
            view.RowCount = 5;
            var service = new FakePersonService { UpdateResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.ReplacedRows);
            Assert.Equal(1, view.ReplacedRows[0].Index);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnDelete_NoSelection_ShowsMessage()
        {
            var view = new FakeUserView { SelectedIndex = 0 };
            var service = new FakePersonService();

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar, seleccione un usuario" }, view.ShownMessages);
            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_Succeeds_RemovesRowAndClears()
        {
            var view = new FakeUserView { SelectedIndex = 3, UserId = 9 };
            var service = new FakePersonService { DeleteResult = true };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(9, service.DeletedId);
            Assert.Equal(new[] { 2 }, view.RemovedIndexes);
            Assert.True(view.ClearFormCalled);
        }

        // --- Administrador General protection ---

        [Fact]
        public void OnDelete_TargetIsAdminGeneral_NonAdminGeneralOperator_IsRejected()
        {
            var view = new FakeUserView { SelectedIndex = 3, UserId = 9 };
            var service = new FakePersonService
            {
                ListResult = new List<Person> { AdminGeneral(9), AdminGeneral(10) }, // two, so it is not "the last one"
                DeleteResult = true
            };
            var presenter = CreatePresenter(view, service, TestUser.WithRole(2, "usuarios.gestionar"));
            presenter.OnLoad();

            presenter.OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("Administrador General"));
            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_LastActiveAdminGeneral_IsRejectedEvenForAnAdminGeneral()
        {
            var view = new FakeUserView { SelectedIndex = 3, UserId = 9 };
            var service = new FakePersonService
            {
                ListResult = new List<Person> { AdminGeneral(9) },
                DeleteResult = true
            };
            var presenter = CreatePresenter(view, service, TestUser.WithRole(1, "usuarios.gestionar"));
            presenter.OnLoad();

            presenter.OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("último Administrador General"));
            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_AdminGeneral_WithAnotherActiveOne_IsAllowedForAnAdminGeneral()
        {
            var view = new FakeUserView { SelectedIndex = 3, UserId = 9 };
            var service = new FakePersonService
            {
                ListResult = new List<Person> { AdminGeneral(9), AdminGeneral(10) },
                DeleteResult = true
            };
            var presenter = CreatePresenter(view, service, TestUser.WithRole(1, "usuarios.gestionar"));
            presenter.OnLoad();

            presenter.OnDelete();

            Assert.Equal(9, service.DeletedId);
        }

        [Fact]
        public void OnSave_NonAdminGeneralOperator_AssigningAdminGeneralRole_IsRejected()
        {
            var view = ValidView();
            view.RoleId = 1;
            var service = new FakePersonService { RegisterResult = 55 };
            var presenter = CreatePresenter(view, service, TestUser.WithRole(2, "usuarios.gestionar"));

            presenter.OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("Administrador General"));
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnSave_NonAdminGeneralOperator_EditingAnAdminGeneral_IsRejected()
        {
            var view = ValidView();
            view.UserId = 7;
            view.SelectedIndex = 1;
            view.RowCount = 5;
            view.RoleId = 2;
            var service = new FakePersonService
            {
                ListResult = new List<Person> { AdminGeneral(7), AdminGeneral(8) },
                UpdateResult = true
            };
            var presenter = CreatePresenter(view, service, TestUser.WithRole(2, "usuarios.gestionar"));
            presenter.OnLoad();

            presenter.OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("Administrador General"));
            Assert.Null(service.UpdatedWith);
        }

        [Fact]
        public void OnSave_DemotingTheLastActiveAdminGeneral_IsRejected()
        {
            var view = ValidView();
            view.UserId = 7;
            view.SelectedIndex = 1;
            view.RowCount = 5;
            view.RoleId = 2; // moving it off Administrador General
            var service = new FakePersonService
            {
                ListResult = new List<Person> { AdminGeneral(7) },
                UpdateResult = true
            };
            var presenter = CreatePresenter(view, service, TestUser.WithRole(1, "usuarios.gestionar"));
            presenter.OnLoad();

            presenter.OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("último Administrador General"));
            Assert.Null(service.UpdatedWith);
        }

        [Fact]
        public void OnLoad_NonAdminGeneralOperator_RoleOptionsExcludeAdminGeneral()
        {
            var view = new FakeUserView();
            var permissions = new FakePermissionService
            {
                Roles = new List<TypePerson>
                {
                    new TypePerson { idPersonType = 1, description = "Administrador General" },
                    new TypePerson { idPersonType = 2, description = "Administrador" },
                    new TypePerson { idPersonType = 100, description = "Cajero senior" }
                }
            };
            var presenter = new PharmacySystem.Presentation.UserPresenter(
                view, new FakePersonService(), TestUser.WithRole(2, "usuarios.gestionar"), permissions);

            presenter.OnLoad();

            Assert.Equal(new[] { "Administrador", "Cajero senior" }, view.LoadedRoleOptions.Select(o => o.Text));
        }

        [Fact]
        public void OnLoad_AdminGeneralOperator_RoleOptionsIncludeAdminGeneral()
        {
            var view = new FakeUserView();
            var permissions = new FakePermissionService
            {
                Roles = new List<TypePerson>
                {
                    new TypePerson { idPersonType = 1, description = "Administrador General" },
                    new TypePerson { idPersonType = 2, description = "Administrador" }
                }
            };
            var presenter = new PharmacySystem.Presentation.UserPresenter(
                view, new FakePersonService(), TestUser.WithRole(1, "usuarios.gestionar"), permissions);

            presenter.OnLoad();

            Assert.Contains(view.LoadedRoleOptions, o => o.Text == "Administrador General");
        }
    }
}
