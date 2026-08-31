using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Business
{
    internal class FakeSecurityEventRepository : ISecurityEventRepository
    {
        public Exception Throws { get; set; }

        public readonly List<(int? ActorId, string Action, string Entity, int? EntityId, string Summary, string Station)> Recorded =
            new List<(int?, string, string, int?, string, string)>();

        public List<SecurityEventRow> Listed { get; set; } = new List<SecurityEventRow>();
        public (DateTime From, DateTime To, int Max)? ListArgs { get; private set; }

        public void Record(int? actorId, string action, string entity, int? entityId, string summary, string station)
        {
            Recorded.Add((actorId, action, entity, entityId, summary, station));
            if (Throws != null) throw Throws;
        }

        public List<SecurityEventRow> List(DateTime from, DateTime to, int max)
        {
            ListArgs = (from, to, max);
            return Listed;
        }
    }
}
