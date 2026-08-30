namespace PharmacySystem.Business
{
    // Records a sensitive administrative action. Best-effort: a failure here never propagates,
    // so it cannot undo or block the operation that already succeeded.
    public interface ISecurityAudit
    {
        void Record(int actorId, string action, string entity, int? entityId, string summary);
    }
}
