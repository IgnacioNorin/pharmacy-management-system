using System;

namespace PharmacySystem.Model
{
    // One entry of the security_event audit trail, for the "Bitácora" screen.
    public class SecurityEventRow
    {
        public DateTime At { get; set; }
        public string ActorName { get; set; } = string.Empty;   // person.name, or "" when actor_id is null / gone
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
    }
}
