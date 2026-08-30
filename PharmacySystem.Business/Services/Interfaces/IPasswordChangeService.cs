using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPasswordChangeService
    {
        // Self-service: the user proves the current password and picks a new one. On success the
        // must_change_password flag is cleared.
        PasswordChangeResult ChangeOwnPassword(int personId, string currentPlain, string newPlain);

        // Admin reset: generates a temporary password for another user, turns on the
        // must-change flag so they replace it on next login, and writes the reset to the audit
        // trail. Returns the generated temporary password to hand over (throws on a data error).
        string AdminReset(int targetId, int actorId);
    }
}
