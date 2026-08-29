using PharmacySystem.Helpers;
using PharmacySystem.Model;
using Xunit;
using BusinessPersonService = PharmacySystem.Business.PersonService;

namespace PharmacySystem.Tests.Business
{
    // The hash-if-not-already-hashed rule, isolated from SQL Server. The DB-backed round trip
    // (does the hashed value actually survive a real insert/read) stays in
    // Integration/PersonRepositoryTests.cs.
    public class PersonServiceTests
    {
        private static Person NewPerson(string password) => new Person
        {
            document = "123",
            name = "Test",
            address = "Address",
            phone = "0999999999",
            password = password,
            oPersonType = new TypePerson { idPersonType = 1 }
        };

        [Fact]
        public void Register_PlainTextPassword_HashesBeforePersisting()
        {
            var repository = new FakePersonRepository();
            var service = new BusinessPersonService(repository);

            service.Register(NewPerson("Passw0rd!"));

            Assert.True(PasswordHasher.IsHashed(repository.RegisteredWith.password));
            Assert.True(PasswordHasher.Verify("Passw0rd!", repository.RegisteredWith.password));
        }

        [Fact]
        public void Register_AlreadyHashedPassword_DoesNotHashAgain()
        {
            string alreadyHashed = PasswordHasher.Hash("Passw0rd!");
            var repository = new FakePersonRepository();
            var service = new BusinessPersonService(repository);

            service.Register(NewPerson(alreadyHashed));

            Assert.Equal(alreadyHashed, repository.RegisteredWith.password); // not re-hashed
        }

        [Fact]
        public void Update_PlainTextPassword_HashesBeforePersisting()
        {
            var repository = new FakePersonRepository();
            var service = new BusinessPersonService(repository);

            service.Update(NewPerson("NewPassw0rd!"));

            Assert.True(PasswordHasher.IsHashed(repository.UpdatedWith.password));
            Assert.True(PasswordHasher.Verify("NewPassw0rd!", repository.UpdatedWith.password));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_BlankPassword_PassesNullThroughToKeepCurrentOne(string blank)
        {
            var repository = new FakePersonRepository();
            var service = new BusinessPersonService(repository);

            service.Update(NewPerson(blank));

            Assert.Null(repository.UpdatedWith.password);
        }
    }
}
