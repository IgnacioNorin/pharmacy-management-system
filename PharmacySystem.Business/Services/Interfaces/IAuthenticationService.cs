using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IAuthenticationService
    {
        AuthResult Authenticate(string document, string password);

        // Manual unlock from the Usuarios screen: records a success row for the document, which
        // resets its running failure count so the user can try again immediately.
        void Unlock(string document, int actorId);
    }
}
