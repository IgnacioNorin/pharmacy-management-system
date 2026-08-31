using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ClientPresenterTests
    {
        private static ClientPresenter CreatePresenter(FakeClientView view, FakeClientService service)
            => new ClientPresenter(view, service, TestUser.With("clientes.gestionar"), new FakeSecurityAudit());

        [Fact]
        public void OnSave_And_OnDelete_AreAudited()
        {
            var audit = new FakeSecurityAudit();
            var createView = new FakeClientView { PersonId = 0, Document = "12.3", Name = "Clínica Andes", Address = "A", Phone = "9" };
            new ClientPresenter(createView, new FakeClientService { RegisterResult = 5 }, TestUser.With("clientes.gestionar"), audit).OnSave();

            var deleteView = new FakeClientView { SelectedIndex = 2, PersonId = 5, Name = "Clínica Andes", Document = "12.3" };
            new ClientPresenter(deleteView, new FakeClientService { DeleteResult = true }, TestUser.With("clientes.gestionar"), audit).OnDelete();

            Assert.Equal(new[] { "client.create", "client.delete" }, audit.Recorded.Select(e => e.Action));
            Assert.Equal(5, audit.Recorded[0].EntityId);
            Assert.Contains("Clínica Andes", audit.Recorded[0].Summary);
        }

        [Fact]
        public void OnSave_WithoutManagePermission_ShowsDeniedAndDoesNotRegister()
        {
            var view = new FakeClientView { PersonId = 0, Document = "1", Name = "N", Address = "A", Phone = "9" };
            new ClientPresenter(view, new FakeClientService(), TestUser.With(), new FakeSecurityAudit()).OnSave();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Equal(0, view.LoadClientsCallCount);
        }

        [Fact]
        public void OnDelete_WithoutManagePermission_ShowsDeniedAndDoesNotRemove()
        {
            var view = new FakeClientView { SelectedIndex = 2, PersonId = 4 };
            new ClientPresenter(view, new FakeClientService(), TestUser.With(), new FakeSecurityAudit()).OnDelete();

            Assert.Contains(view.ShownMessages, m => m.Contains("No tiene permiso"));
            Assert.Equal(0, view.LoadClientsCallCount);
        }

        [Fact]
        public void OnLoad_LoadsThePageFromTheService()
        {
            var view = new FakeClientView();
            var service = new FakeClientService
            {
                ClientsResult = new List<Client>
                {
                    new Client { idClient = 1, name = "Client" }
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
            var service = new FakeClientService();

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new List<string> { "error" }, view.ShownValidationErrors);
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnSave_NewClient_RegistersWithTheEnteredData()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakeClientService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(0, service.RegisteredWith.idClient);
            Assert.Equal("123", service.RegisteredWith.document);
            Assert.Equal("Test", service.RegisteredWith.name);
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
            var service = new FakeClientService { RegisterResult = 55 };

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
            var service = new FakeClientService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.NotNull(view.ShownValidationErrors);
            Assert.Contains(view.ShownValidationErrors, e => e.Contains("empresa"));
            Assert.Null(service.RegisteredWith);
        }

        [Fact]
        public void OnSave_NewClient_Succeeds_ReloadsPageAndClears()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakeClientService { RegisterResult = 55 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(1, view.LoadClientsCallCount);
            Assert.True(view.ClearFormCalled);
        }

        // Unlike SupplierPresenter (which returns silently on a failed Update), ClientPresenter's
        // OnSave always falls through to a shared "did it work" check - the original
        // btnSave_Click never returns early inside either branch.
        [Fact]
        public void OnSave_RegisterFails_ShowsMessage()
        {
            var view = new FakeClientView { PersonId = 0, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakeClientService { RegisterResult = 0 };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los cambios\nRevise los datos" }, view.ShownMessages);
            Assert.False(view.ClearFormCalled);
        }

        [Fact]
        public void OnSave_UpdateFails_AlsoShowsMessage()
        {
            var view = new FakeClientView { PersonId = 7, SelectedIndex = 1, Document = "123", Name = "Test", Address = "Addr", Phone = "111" };
            var service = new FakeClientService { UpdateResult = false };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(new[] { "No se pudo guardar los cambios\nRevise los datos" }, view.ShownMessages);
        }

        [Fact]
        public void OnSave_ExistingClient_UpdateSucceeds_ReloadsPageAndClears()
        {
            var view = new FakeClientView { PersonId = 7, SelectedIndex = 2, Name = "Updated" };
            var service = new FakeClientService { UpdateResult = true };

            CreatePresenter(view, service).OnSave();

            Assert.Equal(7, service.UpdatedWith.idClient);
            Assert.Equal(1, view.LoadClientsCallCount);
            Assert.True(view.ClearFormCalled);
        }

        // OnDelete shows nothing at all when there's no selection - unlike SupplierPresenter's
        // explicit "seleccione un proveedor" message. Preserves frmClient.cs's original silence.
        [Fact]
        public void OnDelete_NoSelection_DoesNothingSilently()
        {
            var view = new FakeClientView { SelectedIndex = 0 };
            var service = new FakeClientService();

            CreatePresenter(view, service).OnDelete();

            Assert.Empty(view.ShownMessages);
            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_UserCancels_NeverCallsService()
        {
            var view = new FakeClientView { SelectedIndex = 1, ConfirmDeleteResult = false };
            var service = new FakeClientService();

            CreatePresenter(view, service).OnDelete();

            Assert.Null(service.DeletedId);
        }

        [Fact]
        public void OnDelete_ServiceFails_ShowsMessage()
        {
            var view = new FakeClientView { SelectedIndex = 1, PersonId = 9 };
            var service = new FakeClientService { DeleteResult = false };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(new[] { "No se pudo eliminar el registro\nRevise los datos" }, view.ShownMessages);
            Assert.Equal(0, view.LoadClientsCallCount);
        }

        [Fact]
        public void OnDelete_Succeeds_ReloadsPageAndClears()
        {
            var view = new FakeClientView { SelectedIndex = 3, PersonId = 9 };
            var service = new FakeClientService { DeleteResult = true };

            CreatePresenter(view, service).OnDelete();

            Assert.Equal(9, service.DeletedId);
            Assert.Equal(1, view.LoadClientsCallCount);
            Assert.True(view.ClearFormCalled);
        }

        [Fact]
        public void OnSearch_QueriesWithTheTermAndResetsToPageOne()
        {
            var view = new FakeClientView();
            var service = new FakeClientService
            {
                ClientsResult = new List<Client>
                {
                    new Client { idClient = 1, name = "Ana" },
                    new Client { idClient = 2, name = "Bruno" }
                }
            };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            view.SearchText = "Bruno";
            presenter.OnSearch();

            Assert.Equal("Bruno", service.LastClientsPagedCall?.Search);
            Assert.Equal(1, service.LastClientsPagedCall?.Page);
            Assert.Single(view.LoadedClients);
            Assert.Equal("Bruno", view.LoadedClients[0].Name);
        }

        [Fact]
        public void OnNextPage_AdvancesOnePage()
        {
            var view = new FakeClientView();
            var many = new List<Client>();
            for (int i = 1; i <= 60; i++) many.Add(new Client { idClient = i, name = "C" + i.ToString("D2") });
            var service = new FakeClientService { ClientsResult = many };
            var presenter = CreatePresenter(view, service);
            presenter.OnLoad();

            presenter.OnNextPage();

            Assert.Equal(2, view.LastPageInfo?.CurrentPage);
            Assert.Equal(2, service.LastClientsPagedCall?.Page);
        }
    }
}
