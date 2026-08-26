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
        public void OnLoad_OnlyIncludesClientRole()
        {
            var view = new FakeView();
            var service = new FakePersonService
            {
                ListResult = new List<Person>
                {
                    new Person { idPerson = 1, name = "Client", oPersonType = new TypePerson { idPersonType = 4 } },
                    new Person { idPerson = 2, name = "Employee", oPersonType = new TypePerson { idPersonType = 3 } }
                }
            };

            new ClientPickerPresenter(view, service).OnLoad();

            Assert.Single(view.Loaded);
            Assert.Equal("Client", view.Loaded[0].Name);
        }
    }
}
