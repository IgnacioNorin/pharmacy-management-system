using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class SupplierPickerPresenterTests
    {
        private class FakeView : ISupplierPickerView
        {
            public List<SupplierRow> Loaded { get; private set; }
            public void LoadSuppliers(IEnumerable<SupplierRow> suppliers) => Loaded = suppliers.ToList();
        }

        [Fact]
        public void OnLoad_PopulatesViewFromService()
        {
            var view = new FakeView();
            var service = new FakeSupplierService
            {
                ListResult = new List<Supplier>
                {
                    new Supplier { idSupplier = 1, document = "123", companyName = "Acme", email = "a@a.com", phone = "111" }
                }
            };

            new SupplierPickerPresenter(view, service).OnLoad();

            Assert.Single(view.Loaded);
            Assert.Equal("Acme", view.Loaded[0].CompanyName);
        }
    }
}
