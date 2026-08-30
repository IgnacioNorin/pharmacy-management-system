namespace PharmacySystem.Data
{
    // Append-only audit trail of sensitive administrative actions.
    public interface ISecurityEventRepository
    {
        void Record(int? actorId, string action, string entity, int? entityId, string summary, string station);
    }
}
