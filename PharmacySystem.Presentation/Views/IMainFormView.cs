using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface IMainFormView
    {
        void SetUserName(string name, string role);
        void SetAdministrativeMenusVisible(bool visible);
        void ShowAlerts(IReadOnlyList<ProductAlert> alerts);
    }
}
