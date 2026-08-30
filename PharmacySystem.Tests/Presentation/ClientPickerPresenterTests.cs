using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ClientPickerPresenterTests
    {
        private class FakeView : IClientPickerView
        {
            public List<ClientRow> Loaded { get; private set; }
            public void LoadClients(IEnumerable<ClientRow> clients) => Loaded = clients.ToList();
        }

        [Fact]
        public void OnLoad_MapsTheServiceClientsToRows()
        {
            var view = new FakeView();
            var service = new FakeClientService
            {
                ClientsResult = new List<Client>
                {
                    new Client { idClient = 1, name = "Ana", document = "111" },
                    new Client { idClient = 2, name = "Bruno", document = "222" }
                }
            };

            new ClientPickerPresenter(view, service).OnLoad();

            Assert.Equal(2, view.Loaded.Count);
            Assert.Equal("Ana", view.Loaded[0].Name);
            Assert.Equal("222", view.Loaded[1].Document);
        }

        [Fact]
        public void OnLoad_NoClients_LoadsAnEmptyList()
        {
            var view = new FakeView();

            new ClientPickerPresenter(view, new FakeClientService()).OnLoad();

            Assert.Empty(view.Loaded);
        }
    }
}
