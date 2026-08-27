using System.Collections.Generic;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class UserPresenterTests
    {
        private static PharmacySystem.Presentation.UserPresenter CreatePresenter(FakeUserView view, FakePersonService service)
            => new PharmacySystem.Presentation.UserPresenter(view, service, TestUser.With("usuarios.gestionar"));

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotRegister()
        {
            var view = ValidView();
            new PharmacySystem.Presentation.UserPresenter(view, new FakePersonService(), TestUser.With()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.AddedRows);
        }

        [Fact]
        public void OnDelete_WithoutManagePermission_ShowsDeniedAndDoesNotRemove()
        {
            var view = new FakeUserView { SelectedIndex = 3, UserId = 9 };
            new PharmacySystem.Presentation.UserPresenter(view, new FakePersonService(), TestUser.With()).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.RemovedIndexes);
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
        public void OnLoad_ExcludesClientRole()
        {
            var view = new FakeUserView();
            var service = new FakePersonService
            {
                ListResult = new List<Person>
                {
                    new Person { idPerson = 1, name = "Admin", oPersonType = new TypePerson { idPersonType = 2, description = "Administrador" } },
                    new Person { idPerson = 2, name = "Client", oPersonType = new TypePerson { idPersonType = 4, description = "Cliente" } }
                }
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Single(view.LoadedUsers);
            Assert.Equal("Admin", view.LoadedUsers[0].Name);
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
            var service = new FakePersonService { RegisterResult = true };

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
            var service = new FakePersonService { RegisterResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "Ya existe un usuario con esa Cedula de Identidad" }, view.ShownMessages);
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
    }
}
