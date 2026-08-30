using System.Collections.Generic;
using PharmacySystem.Business;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSecurityAudit : ISecurityAudit
    {
        public readonly List<(int ActorId, string Action, string Entity, int? EntityId, string Summary)> Recorded =
            new List<(int, string, string, int?, string)>();

        public void Record(int actorId, string action, string entity, int? entityId, string summary) =>
            Recorded.Add((actorId, action, entity, entityId, summary));
    }
}
