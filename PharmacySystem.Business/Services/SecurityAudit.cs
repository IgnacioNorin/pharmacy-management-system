using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public class SecurityAudit : ISecurityAudit
    {
        // Cap on what the Bitácora screen pulls in one query.
        private const int MaxRows = 1000;

        private readonly ISecurityEventRepository _repository;

        public SecurityAudit(ISecurityEventRepository repository)
        {
            _repository = repository;
        }

        public List<SecurityEventRow> List(DateTime from, DateTime to) => _repository.List(from, to, MaxRows);

        public void Record(int actorId, string action, string entity, int? entityId, string summary)
        {
            try
            {
                _repository.Record(
                    actorId <= 0 ? (int?)null : actorId,
                    action, entity, entityId, summary, Environment.MachineName);
            }
            catch (DataUnavailableException ex)
            {
                // The audited operation already succeeded; a transient outage on the audit
                // write must not surface to the user. The repository logged the details.
                Logger.LogError(ex);
            }
        }
    }
}
