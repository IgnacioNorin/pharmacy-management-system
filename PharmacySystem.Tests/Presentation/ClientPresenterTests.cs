using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ClientPresenterTests
    {
        private static ClientPresenter CreatePresenter(FakeClientView view, FakePersonService service)
            => new ClientPresenter(view, service);

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
            var service = new FakePersonService { RegisterResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(4, service.RegisteredWith.oPersonType.idPersonType); // Cliente
            Assert.Equal("", service.RegisteredWith.password);
            Assert.True(view.ClearFormCalled);
        }

        // Locks in a real quirk: Register() only returns bool, never the new row's id, so the
        // grid row added for a brand-new client keeps Id = 0 - same as the original frmClient.cs,
        // which reused the still-"0" txtid.Text for the new row.
        [Fact]
        public void OnSave_NewClient_Succeeds_AddedRowHasIdZero()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakePersonService { RegisterResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Single(view.AddedRows);
            Assert.Equal(0, view.AddedRows[0].Id);
        }

        // Unlike SupplierPresenter (which returns silently on a failed Update), ClientPresenter's
        // OnSave always falls through to a shared "did it work" check - the original
        // btnSave_Click never returns early inside either branch.
        [Fact]
        public void OnSave_RegisterFails_ShowsMessage()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakePersonService { RegisterResult = false };

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
