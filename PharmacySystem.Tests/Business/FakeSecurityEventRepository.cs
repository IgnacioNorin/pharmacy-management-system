using System;
using System.Collections.Generic;
using PharmacySystem.Data;

namespace PharmacySystem.Tests.Business
{
    internal class FakeSecurityEventRepository : ISecurityEventRepository
    {
        public Exception Throws { get; set; }

        public readonly List<(int? ActorId, string Action, string Entity, int? EntityId, string Summary, string Station)> Recorded =
            new List<(int?, string, string, int?, string, string)>();

        public void Record(int? actorId, string action, string entity, int? entityId, string summary, string station)
        {
            Recorded.Add((actorId, action, entity, entityId, summary, station));
            if (Throws != null) throw Throws;
        }
    }
}
