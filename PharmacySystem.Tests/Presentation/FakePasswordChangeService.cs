using System;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakePasswordChangeService : IPasswordChangeService
    {
        public PasswordChangeResult ChangeOwnResult { get; set; } = PasswordChangeResult.Ok;
        public string AdminResetPassword { get; set; } = "TEMP-ab23";
        public Exception ChangeOwnThrows { get; set; }

        public (int PersonId, string Current, string New)? ChangeOwnCall { get; private set; }
        public (int TargetId, int ActorId)? AdminResetCall { get; private set; }

        public PasswordChangeResult ChangeOwnPassword(int personId, string currentPlain, string newPlain)
        {
            ChangeOwnCall = (personId, currentPlain, newPlain);
            if (ChangeOwnThrows != null) throw ChangeOwnThrows;
            return ChangeOwnResult;
        }

        public string AdminReset(int targetId, int actorId)
        {
            AdminResetCall = (targetId, actorId);
            return AdminResetPassword;
        }
    }
}
