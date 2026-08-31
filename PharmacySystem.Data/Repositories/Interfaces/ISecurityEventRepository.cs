using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // Append-only audit trail of sensitive administrative actions.
    public interface ISecurityEventRepository
    {
        void Record(int? actorId, string action, string entity, int? entityId, string summary, string station);

        // Events in [from, endOfDay(to)], newest first, capped at 'max' rows.
        List<SecurityEventRow> List(DateTime from, DateTime to, int max);
    }
}
