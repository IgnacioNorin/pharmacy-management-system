using System;
using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeSecurityAudit : ISecurityAudit
    {
        public readonly List<(int ActorId, string Action, string Entity, int? EntityId, string Summary)> Recorded =
            new List<(int, string, string, int?, string)>();

        // Rows handed back by List; tests set this directly.
        public List<SecurityEventRow> Events { get; set; } = new List<SecurityEventRow>();
        public DateTime? ListedFrom { get; private set; }
        public DateTime? ListedTo { get; private set; }

        public void Record(int actorId, string action, string entity, int? entityId, string summary) =>
            Recorded.Add((actorId, action, entity, entityId, summary));

        public List<SecurityEventRow> List(DateTime from, DateTime to)
        {
            ListedFrom = from;
            ListedTo = to;
            return Events;
        }
    }
}
