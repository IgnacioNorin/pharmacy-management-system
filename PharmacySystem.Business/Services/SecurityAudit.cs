using System;
using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;

namespace PharmacySystem.Business
{
    public class SecurityAudit : ISecurityAudit
    {
        private readonly ISecurityEventRepository _repository;

        public SecurityAudit(ISecurityEventRepository repository)
        {
            _repository = repository;
        }

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
