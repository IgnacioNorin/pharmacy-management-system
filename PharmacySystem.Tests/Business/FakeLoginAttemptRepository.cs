using System.Collections.Generic;
using PharmacySystem.Data;

namespace PharmacySystem.Tests.Business
{
    internal class FakeLoginAttemptRepository : ILoginAttemptRepository
    {
        public int FailureCount { get; set; }
        public int? MinutesLeft { get; set; }

        public readonly List<(string Document, bool Success, string Reason, int? ActorId, string Station)> Recorded =
            new List<(string, bool, string, int?, string)>();

        public void Record(string document, bool success, string reason, int? actorId, string station) =>
            Recorded.Add((document, success, reason, actorId, station));

        public int CountFailuresSinceLastReset(string document, int windowMinutes) => FailureCount;

        public int? MinutesUntilUnlock(string document, int windowMinutes) => MinutesLeft;
    }
}
