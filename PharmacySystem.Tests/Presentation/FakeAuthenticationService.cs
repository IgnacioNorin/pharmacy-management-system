using System;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeAuthenticationService : IAuthenticationService
    {
        public AuthResult Result { get; set; } = AuthResult.Invalid();
        public Exception AuthenticateThrows { get; set; }

        public (string Document, string Password)? AuthenticatedWith { get; private set; }
        public (string Document, int ActorId)? UnlockedWith { get; private set; }

        public AuthResult Authenticate(string document, string password)
        {
            AuthenticatedWith = (document, password);
            if (AuthenticateThrows != null) throw AuthenticateThrows;
            return Result;
        }

        public void Unlock(string document, int actorId) => UnlockedWith = (document, actorId);
    }
}
