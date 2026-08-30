using System;
using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeAuthenticationService : IAuthenticationService
    {
        public AuthResult Result { get; set; } = AuthResult.Invalid();
        public Exception AuthenticateThrows { get; set; }
        public ISet<string> LockedDocuments { get; set; } = new HashSet<string>();

        public (string Document, string Password)? AuthenticatedWith { get; private set; }
        public (string Document, int ActorId)? UnlockedWith { get; private set; }

        public AuthResult Authenticate(string document, string password)
        {
            AuthenticatedWith = (document, password);
            if (AuthenticateThrows != null) throw AuthenticateThrows;
            return Result;
        }

        public void Unlock(string document, int actorId) => UnlockedWith = (document, actorId);

        public (string Document, bool Suspended, int ActorId)? SuspensionRecorded { get; private set; }
        public void RecordSuspension(string document, bool suspended, int actorId) =>
            SuspensionRecorded = (document, suspended, actorId);

        public ISet<string> GetLockedDocuments() => LockedDocuments;
    }
}
