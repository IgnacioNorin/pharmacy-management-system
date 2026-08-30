using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Infrastructure;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class SecurityAuditTests
    {
        private readonly FakeSecurityEventRepository _repo = new FakeSecurityEventRepository();
        private SecurityAudit Service() => new SecurityAudit(_repo);

        [Fact]
        public void Record_PassesTheEventThroughWithTheStation()
        {
            Service().Record(42, "role.delete", "person_type", 7, "rol 'X'");

            var row = _repo.Recorded.Single();
            Assert.Equal(42, row.ActorId);
            Assert.Equal("role.delete", row.Action);
            Assert.Equal(7, row.EntityId);
            Assert.False(string.IsNullOrEmpty(row.Station));
        }

        [Fact]
        public void Record_NonPositiveActor_IsStoredAsNull()
        {
            Service().Record(0, "store.update", "store", 1, "x");

            Assert.Null(_repo.Recorded.Single().ActorId);
        }

        [Fact]
        public void Record_DataUnavailable_IsSwallowed()
        {
            _repo.Throws = new DataUnavailableException();

            Service().Record(1, "user.create", "person", 5, "x"); // must not throw
        }
    }
}
