using PharmacySystem.Business;
using PharmacySystem.Infrastructure;

namespace PharmacySystem.Presentation
{
    // Read-only "Bitácora" screen: shows the security_event audit trail for the selected date
    // range. Gated by bitacora.acceso.
    public class SecurityLogPresenter
    {
        private readonly ISecurityLogView _view;
        private readonly ISecurityAudit _audit;
        private readonly CurrentUser _currentUser;

        public SecurityLogPresenter(ISecurityLogView view, ISecurityAudit audit, CurrentUser currentUser)
        {
            _view = view;
            _audit = audit;
            _currentUser = currentUser;
        }

        public void OnConsult()
        {
            if (!(_currentUser?.Can("bitacora.acceso") ?? false))
            {
                _view.ShowError("No tiene permiso para ver la bitácora.");
                return;
            }

            try
            {
                _view.ShowEvents(_audit.List(_view.StartDate, _view.EndDate));
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowError(ex.Message);
            }
        }
    }
}
