using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacySystem.Business;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Read-only "Bitácora" screen: shows the security_event audit trail for the selected date
    // range. Gated by bitacora.acceso. The query runs off the caller's thread so the window
    // stays responsive; the view inputs are read before that, on the caller's thread.
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

        public async Task OnConsultAsync()
        {
            if (!(_currentUser?.Can("bitacora.acceso") ?? false))
            {
                _view.ShowError("No tiene permiso para ver la bitácora.");
                return;
            }

            DateTime from = _view.StartDate;
            DateTime to = _view.EndDate;

            try
            {
                IReadOnlyList<SecurityEventRow> events = await Task.Run(() => _audit.List(from, to));
                _view.ShowEvents(events);
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowError(ex.Message);
            }
        }
    }
}
