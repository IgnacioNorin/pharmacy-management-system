using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeMainFormView : IMainFormView
    {
        public string UserName { get; private set; }
        public string UserRole { get; private set; }
        public bool? AdministrativeMenusVisible { get; private set; }
        public List<ProductAlert> ShownAlerts { get; private set; }

        public void SetUserName(string name, string role)
        {
            UserName = name;
            UserRole = role;
        }
        public void SetAdministrativeMenusVisible(bool visible) => AdministrativeMenusVisible = visible;
        public void ShowAlerts(IReadOnlyList<ProductAlert> alerts) => ShownAlerts = alerts.ToList();
    }
}
