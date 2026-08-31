using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PharmacySystem.Business;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class SecurityLogPresenterTests
    {
        private readonly FakeSecurityLogView _view = new FakeSecurityLogView();
        private readonly FakeSecurityAudit _audit = new FakeSecurityAudit();

        private SecurityLogPresenter Presenter(CurrentUser user) =>
            new SecurityLogPresenter(_view, _audit, user);

        [Fact]
        public async Task OnConsult_WithoutPermission_ShowsErrorAndDoesNotQuery()
        {
            await Presenter(TestUser.With()).OnConsultAsync();

            Assert.Null(_view.ShownEvents);
            Assert.False(string.IsNullOrEmpty(_view.ShownError));
            Assert.Null(_audit.ListedFrom);
        }

        [Fact]
        public async Task OnConsult_WithPermission_ShowsEventsForTheSelectedRange()
        {
            _view.StartDate = new DateTime(2026, 8, 1);
            _view.EndDate = new DateTime(2026, 8, 30);
            _audit.Events = new List<SecurityEventRow>
            {
                new SecurityEventRow { Action = "user.create", ActorName = "Ana" }
            };

            await Presenter(TestUser.With("bitacora.acceso")).OnConsultAsync();

            Assert.Same(_audit.Events, _view.ShownEvents);
            Assert.Equal(new DateTime(2026, 8, 1), _audit.ListedFrom);
            Assert.Equal(new DateTime(2026, 8, 30), _audit.ListedTo);
            Assert.Null(_view.ShownError);
        }

        [Fact]
        public async Task OnConsult_DataUnavailable_ShowsError()
        {
            var audit = new ThrowingAudit();

            await new SecurityLogPresenter(_view, audit, TestUser.With("bitacora.acceso")).OnConsultAsync();

            Assert.Equal(DataUnavailableException.DefaultMessage, _view.ShownError);
            Assert.Null(_view.ShownEvents);
        }

        private class ThrowingAudit : ISecurityAudit
        {
            public void Record(int actorId, string action, string entity, int? entityId, string summary) { }

            public List<SecurityEventRow> List(DateTime from, DateTime to) =>
                throw new DataUnavailableException();
        }
    }
}
