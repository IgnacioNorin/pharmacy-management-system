using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class AuthenticationServiceTests
    {
        private readonly FakePersonRepository _people = new FakePersonRepository();
        private readonly FakeLoginAttemptRepository _attempts = new FakeLoginAttemptRepository();

        private AuthenticationService Service() => new AuthenticationService(_people, _attempts);

        private static Person ActiveUser(string hashed = null, bool mustChange = false) => new Person
        {
            idPerson = 1,
            document = "123",
            password = hashed ?? PasswordHasher.Hash("correct"),
            Estado = true,
            mustChangePassword = mustChange,
            oPersonType = new TypePerson { idPersonType = 1 }
        };

        [Fact]
        public void Authenticate_CorrectPassword_ReturnsOk_AndRecordsASuccess()
        {
            _people.GetByDocumentResult = ActiveUser();

            AuthResult result = Service().Authenticate("123", "correct");

            Assert.Equal(AuthStatus.Ok, result.Status);
            Assert.Same(_people.GetByDocumentResult, result.Person);
            Assert.Contains(_attempts.Recorded, r => r.Success && r.Document == "123");
        }

        [Fact]
        public void Authenticate_WrongPassword_ReturnsInvalid_AndRecordsAFailure()
        {
            _people.GetByDocumentResult = ActiveUser();

            AuthResult result = Service().Authenticate("123", "nope");

            Assert.Equal(AuthStatus.InvalidCredentials, result.Status);
            Assert.Contains(_attempts.Recorded, r => !r.Success && r.Document == "123");
        }

        [Fact]
        public void Authenticate_DisabledAccount_ReturnsInvalid_EvenWithTheRightPassword()
        {
            Person person = ActiveUser();
            person.Estado = false;
            _people.GetByDocumentResult = person;

            AuthResult result = Service().Authenticate("123", "correct");

            Assert.Equal(AuthStatus.InvalidCredentials, result.Status);
        }

        [Fact]
        public void Authenticate_UnknownDocument_ReturnsInvalid()
        {
            _people.GetByDocumentResult = null;

            AuthResult result = Service().Authenticate("999", "whatever");

            Assert.Equal(AuthStatus.InvalidCredentials, result.Status);
        }

        [Fact]
        public void Authenticate_EmptyDocument_ReturnsInvalid_WithoutTouchingTheRepo()
        {
            AuthResult result = Service().Authenticate("   ", "x");

            Assert.Equal(AuthStatus.InvalidCredentials, result.Status);
        }

        [Fact]
        public void Authenticate_AtOrAboveTheFailureThreshold_ReturnsLockedOut_WithTheReportedMinutes()
        {
            _attempts.FailureCount = AuthenticationService.MaxFailures;
            _attempts.MinutesLeft = 5;
            _people.GetByDocumentResult = ActiveUser();

            AuthResult result = Service().Authenticate("123", "correct");

            Assert.Equal(AuthStatus.LockedOut, result.Status);
            Assert.Equal(5, result.RetryAfterMinutes);
        }

        [Fact]
        public void Authenticate_LockedButMinutesLeftIsNotPositive_ReportsAtLeastOne()
        {
            _attempts.FailureCount = AuthenticationService.MaxFailures;
            _attempts.MinutesLeft = 0;

            Assert.Equal(1, Service().Authenticate("123", "x").RetryAfterMinutes);
        }

        [Fact]
        public void Authenticate_LockedWithNoReportedMinutes_FallsBackToTheFullWindow()
        {
            _attempts.FailureCount = AuthenticationService.MaxFailures;
            _attempts.MinutesLeft = null;

            AuthResult result = Service().Authenticate("123", "x");

            Assert.Equal(AuthStatus.LockedOut, result.Status);
            Assert.Equal(AuthenticationService.LockWindowMinutes, result.RetryAfterMinutes);
        }

        [Fact]
        public void Authenticate_ValidButMustChangePassword_ReturnsThatStatus()
        {
            _people.GetByDocumentResult = ActiveUser(mustChange: true);

            AuthResult result = Service().Authenticate("123", "correct");

            Assert.Equal(AuthStatus.MustChangePassword, result.Status);
            Assert.Same(_people.GetByDocumentResult, result.Person);
        }

        [Fact]
        public void Authenticate_LegacyPlainTextPassword_LogsInAndRewritesItAsAHash()
        {
            _people.GetByDocumentResult = ActiveUser(hashed: "plain-text");

            AuthResult result = Service().Authenticate("123", "plain-text");

            Assert.Equal(AuthStatus.Ok, result.Status);
            Assert.Equal(1, _people.UpdatePasswordCall.Id);
            Assert.True(PasswordHasher.IsHashed(_people.UpdatePasswordCall.Hash));
        }

        [Fact]
        public void GetLockedDocuments_DelegatesToTheRepositoryWithTheServiceThresholds()
        {
            _attempts.LockedDocuments = new System.Collections.Generic.HashSet<string> { "123", "456" };

            Assert.Equal(new[] { "123", "456" }, Service().GetLockedDocuments().OrderBy(d => d));
        }

        [Fact]
        public void Unlock_RecordsASuccessRowTaggedAsAdminUnlockWithTheActor()
        {
            Service().Unlock("123", actorId: 42);

            var row = _attempts.Recorded.Single();
            Assert.True(row.Success);
            Assert.Equal("admin_unlock", row.Reason);
            Assert.Equal(42, row.ActorId);
            Assert.Equal("123", row.Document);
        }
    }
}
