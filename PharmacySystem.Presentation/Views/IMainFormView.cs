using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface IMainFormView
    {
        void SetUserName(string name, string role);
        void ApplySidebarPermissions(SidebarPermissions permissions);
        void ShowAlerts(IReadOnlyList<ProductAlert> alerts);
    }
}
