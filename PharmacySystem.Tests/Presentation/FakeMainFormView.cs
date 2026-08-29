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
        public SidebarPermissions AppliedSidebarPermissions { get; private set; }
        public List<ProductAlert> ShownAlerts { get; private set; }

        public void SetUserName(string name, string role)
        {
            UserName = name;
            UserRole = role;
        }
        public void ApplySidebarPermissions(SidebarPermissions permissions) => AppliedSidebarPermissions = permissions;
        public void ShowAlerts(IReadOnlyList<ProductAlert> alerts) => ShownAlerts = alerts.ToList();
    }
}
