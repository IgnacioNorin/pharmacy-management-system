using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Records a sensitive administrative action. Recording is best-effort: a failure there never
    // propagates, so it cannot undo or block the operation that already succeeded.
    public interface ISecurityAudit
    {
        void Record(int actorId, string action, string entity, int? entityId, string summary);

        // For the "Bitácora" screen: events in [from, to] (inclusive of the whole 'to' day),
        // newest first, capped.
        List<SecurityEventRow> List(DateTime from, DateTime to);
    }
}
