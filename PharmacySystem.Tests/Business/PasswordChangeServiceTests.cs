using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class PasswordChangeServiceTests
    {
        private readonly FakePersonRepository _people = new FakePersonRepository();
        private readonly FakeLoginAttemptRepository _attempts = new FakeLoginAttemptRepository();

        private PasswordChangeService Service() => new PasswordChangeService(_people, _attempts);

        private static Person WithPassword(string plain) => new Person
        {
            idPerson = 5,
            document = "777",
            password = PasswordHasher.Hash(plain)
        };

        [Fact]
        public void ChangeOwnPassword_WrongCurrent_ReturnsWrongCurrent_AndDoesNotWrite()
        {
            _people.GetByIdResult = WithPassword("oldpass");

            PasswordChangeResult result = Service().ChangeOwnPassword(5, "bad", "brandnew");

            Assert.Equal(PasswordChangeResult.WrongCurrent, result);
            Assert.Null(_people.SetPasswordAndFlagCall);
        }

        [Fact]
        public void ChangeOwnPassword_UnknownPerson_ReturnsWrongCurrent()
        {
            _people.GetByIdResult = null;

            Assert.Equal(PasswordChangeResult.WrongCurrent, Service().ChangeOwnPassword(5, "x", "brandnew"));
        }

        [Fact]
        public void ChangeOwnPassword_NewTooShort_ReturnsTooShort()
        {
            _people.GetByIdResult = WithPassword("oldpass");

            Assert.Equal(PasswordChangeResult.TooShort, Service().ChangeOwnPassword(5, "oldpass", "abc"));
        }

        [Fact]
        public void ChangeOwnPassword_NewEqualsCurrent_ReturnsSameAsOld()
        {
            _people.GetByIdResult = WithPassword("oldpass");

            Assert.Equal(PasswordChangeResult.SameAsOld, Service().ChangeOwnPassword(5, "oldpass", "oldpass"));
        }

        [Fact]
        public void ChangeOwnPassword_Valid_HashesTheNewPasswordAndClearsTheFlag()
        {
            _people.GetByIdResult = WithPassword("oldpass");

            PasswordChangeResult result = Service().ChangeOwnPassword(5, "oldpass", "brandnew");

            Assert.Equal(PasswordChangeResult.Ok, result);
            Assert.NotNull(_people.SetPasswordAndFlagCall);
            Assert.Equal(5, _people.SetPasswordAndFlagCall.Value.Id);
            Assert.True(PasswordHasher.IsHashed(_people.SetPasswordAndFlagCall.Value.Hash));
            Assert.False(_people.SetPasswordAndFlagCall.Value.MustChange);
        }

        [Fact]
        public void AdminReset_TempTooShort_ReturnsTooShort_AndDoesNotWrite()
        {
            Assert.Equal(PasswordChangeResult.TooShort, Service().AdminReset(9, "abc", actorId: 1));
            Assert.Null(_people.SetPasswordAndFlagCall);
        }

        [Fact]
        public void AdminReset_Valid_SetsThePasswordWithTheFlagOn_AndAuditsTheReset()
        {
            _people.GetByIdResult = new Person { idPerson = 9, document = "999" };

            PasswordChangeResult result = Service().AdminReset(9, "temp123", actorId: 42);

            Assert.Equal(PasswordChangeResult.Ok, result);
            Assert.Equal(9, _people.SetPasswordAndFlagCall.Value.Id);
            Assert.True(_people.SetPasswordAndFlagCall.Value.MustChange);

            var row = _attempts.Recorded.Single();
            Assert.True(row.Success);
            Assert.Equal("admin_reset", row.Reason);
            Assert.Equal(42, row.ActorId);
            Assert.Equal("999", row.Document);
        }
    }
}
