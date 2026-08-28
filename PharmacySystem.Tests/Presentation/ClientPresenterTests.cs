using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ClientPresenterTests
    {
        private static ClientPresenter CreatePresenter(FakeClientView view, FakePersonService service)
            => new ClientPresenter(view, service, TestUser.With("clientes.gestionar"));

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotRegister()
        {
            var view = new FakeClientView { PersonId = 0, Document = "1", Name = "N", Address = "A", Phone = "9" };
            new ClientPresenter(view, new FakePersonService(), TestUser.With()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.AddedRows);
        }

        [Fact]
        public void OnDelete_WithoutManagePermission_ShowsDeniedAndDoesNotRemove()
        {
            var view = new FakeClientView { SelectedIndex = 2, PersonId = 4 };
            new ClientPresenter(view, new FakePersonService(), TestUser.With()).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnLoad_OnlyIncludesClientRole()
        {
            var view = new FakeClientView();
            var service = new FakePersonService
            {
                ListResult = new List<Person>
                {
                    new Person { idPerson = 1, name = "Client", oPersonType = new TypePerson { idPersonType = 4 } },
                    new Person { idPerson = 2, name = "Employee", oPersonType = new TypePerson { idPersonType = 3 } }
                }
            };

            CreatePresenter(view, service).OnLoad();

            Assert.Single(view.LoadedClients);
            Assert.Equal("Client", view.LoadedClients[0].Name);
        }

        [Fact]
        public void OnSave_ValidationErrors_ShowsThemAndNeverCallsService()
        {
            var view = new FakeClientView { ValidationErrors = new List<string> { "error" } };
            var service = new FakePersonService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new List<string> { "error" }, view.ShownValidationErrors);
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnSave_NewClient_SetsClientRoleAndEmptyPassword()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakePersonService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(4, service.RegisteredWith.oPersonType.idPersonType); // Cliente
            Assert.Equal("", service.RegisteredWith.password);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_NewClient_CarriesTheFiscalProfile()
        {
            var view = new FakeClientView
            {
                PersonId = 0, Document = "76.1-2", Name = "Contacto", Address = "Calle 1", Phone = "111",
                BusinessName = "Ejemplo SpA", Activity = "Comercio", Commune = "Centro",
                Email = "a@b.cl", IsCompany = true
            };
            var service = new FakePersonService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal("Ejemplo SpA", service.RegisteredWith.businessName);
            Assert.Equal("Comercio", service.RegisteredWith.activity);
            Assert.Equal("Centro", service.RegisteredWith.commune);
            Assert.Equal("a@b.cl", service.RegisteredWith.email);
            Assert.True(service.RegisteredWith.isCompany);
        }

        [Fact]
        public void OnSave_IsCompanyWithoutBusinessNameOrActivity_ShowsErrorAndNeverCallsService()
        {
            var view = new FakeClientView
            {
                PersonId = 0, Document = "1", Name = "N", Address = "A", Phone = "9",
                IsCompany = true, BusinessName = "  ", Activity = ""
            };
            var service = new FakePersonService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.NotNull(view.ShownValidationErrors);
            Assert.Contains(view.ShownValidationErrors, e => e.Contains("empresa"));
            Assert.Null(service.RegisteredWith);
        }

        // Register() now returns the new id, so the row added to the grid carries it and can be
        // re-selected / edited without registering a duplicate.
        [Fact]
        public void OnSave_NewClient_Succeeds_AddedRowGetsTheNewId()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakePersonService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.AddedRows);
            Assert.Equal(55, view.AddedRows[0].Id);
        }

        // Unlike SupplierPresenter (which returns silently on a failed Update), ClientPresenter's
        // OnSave always falls through to a shared "did it work" check - the original
        // btnSave_Click never returns early inside either branch.
        [Fact]
        public void OnSave_RegisterFails_ShowsMessage()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakePersonService { RegisterResult = 0 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los cambios\nRevise los datos" }, view.ShownMessages);
            Assert.False(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_UpdateFails_AlsoShowsMessage()
        {
            var view = new FakeClientView { PersonId = 7, SelectedIndex = 1, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakePersonService { UpdateResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los cambios\nRevise los datos" }, view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingClient_UpdateSucceeds_ReplacesRow()
        {
            var view = new FakeClientView { PersonId = 7, SelectedIndex = 2, Name = "Updated" };
            var service = new FakePersonService { UpdateResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.ReplacedRows);
            Assert.Equal(1, view.ReplacedRows[0].Index);
            Assert.True(view.ClearFormCalled);
        }

        // OnDelete shows nothing at all when there's no selection - unlike SupplierPresenter's
        // explicit "seleccione un proveedor" message. Preserves frmClient.cs's original silence.
        [Fact]
        public void OnDelete_NoSelection_DoesNothingSilently()
        {
            var view = new FakeClientView { SelectedIndex = 0 };
            var service = new FakePersonService();

            CreatePresenter(view, service).OnDelete();

            Assert.Empty(view.ShownMessages);
            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_UserCancels_NeverCallsService()
        {
            var view = new FakeClientView { SelectedIndex = 1, ConfirmDeleteResult = false };
            var service = new FakePersonService();

            CreatePresenter(view, service).OnDelete();

            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_ServiceFails_ShowsMessage()
        {
            var view = new FakeClientView { SelectedIndex = 1, PersonId = 9 };
            var service = new FakePersonService { DeleteResult = false };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar el registro\nRevise los datos" }, view.ShownMessages);
            Assert.Empty(view.RemovedIndexes);
        }

        [Fact]
        public void OnDelete_Succeeds_RemovesRowAndClears()
        {
            var view = new FakeClientView { SelectedIndex = 3, PersonId = 9 };
            var service = new FakePersonService { DeleteResult = true };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(9, service.DeletedId);
            Assert.Equal(new[] { 2 }, view.RemovedIndexes);
            Assert.True(view.ClearFormCalled);
        }
    }
}
