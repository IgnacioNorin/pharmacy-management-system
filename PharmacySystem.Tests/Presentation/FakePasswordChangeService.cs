using System;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePasswordChangeService : IPasswordChangeService
    {
        public PasswordChangeResult ChangeOwnResult { get; set; } = PasswordChangeResult.Ok;
        public PasswordChangeResult AdminResetResult { get; set; } = PasswordChangeResult.Ok;
        public Exception ChangeOwnThrows { get; set; }

        public (int PersonId, string Current, string New)? ChangeOwnCall { get; private set; }
        public (int TargetId, string Temp, int ActorId)? AdminResetCall { get; private set; }

        public PasswordChangeResult ChangeOwnPassword(int personId, string currentPlain, string newPlain)
        {
            ChangeOwnCall = (personId, currentPlain, newPlain);
            if (ChangeOwnThrows != null) throw ChangeOwnThrows;
            return ChangeOwnResult;
        }

        public PasswordChangeResult AdminReset(int targetId, string tempPlain, int actorId)
        {
            AdminResetCall = (targetId, tempPlain, actorId);
            return AdminResetResult;
        }
    }
}
