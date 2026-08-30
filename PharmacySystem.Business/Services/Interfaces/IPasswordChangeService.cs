using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public interface IPasswordChangeService
    {
        // Self-service: the user proves the current password and picks a new one. On success the
        // must_change_password flag is cleared.
        PasswordChangeResult ChangeOwnPassword(int personId, string currentPlain, string newPlain);

        // Admin sets a temporary password for another user: the flag is turned on so that user
        // must replace it on next login, and the reset is written to the audit trail.
        PasswordChangeResult AdminReset(int targetId, string tempPlain, int actorId);
    }
}
