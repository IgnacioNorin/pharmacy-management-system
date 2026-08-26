using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was PersonServiceTests. The hash-if-not-already-hashed rule moved to Business - see
    // PersonServiceTests in PharmacySystem.Tests.Business for that, tested with no database.
    // This file only covers what the repository does: it persists person.password exactly as
    // given, so every password here is already in its final (hashed) form.
    [Collection("Database")]
    public class PersonRepositoryTests
    {
        private static readonly IPersonRepository Repository = new PersonRepository(SqlConnectionFactory.FromConfiguration());

        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        private static Person NewPerson(string document = null, string password = "already-hashed-value")
        {
            return new Person
            {
                document = document ?? SqlTestHelper.NewTag(),
                name = "Test person",
                address = "Test address",
                phone = "0999999999",
                password = password,
                oPersonType = new TypePerson { idPersonType = PersonTypeId() }
            };
        }

        [Fact]
        public void Register_New_PersistsPasswordVerbatim()
        {
            string document = SqlTestHelper.NewTag();
            bool result = Repository.Register(NewPerson(document, "some-already-hashed-string"));

            try
            {
                Assert.True(result);

                Person stored = Repository.GetByDocument(document);

                Assert.NotNull(stored);
                Assert.Equal("some-already-hashed-string", stored.password); // no hashing happens here
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void Register_DuplicateDocument_ReturnsFalse()
        {
            string document = SqlTestHelper.NewTag();
            Repository.Register(NewPerson(document));

            try
            {
                bool result = Repository.Register(NewPerson(document));

                Assert.False(result);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void Update_ChangesFields()
        {
            string document = SqlTestHelper.NewTag();
            Repository.Register(NewPerson(document));
            Person created = Repository.GetByDocument(document);

            try
            {
                bool result = Repository.Update(new Person
                {
                    idPerson = created.idPerson,
                    document = document,
                    name = "Updated name",
                    address = "Updated address",
                    phone = "0888888888",
                    password = "already-hashed-value",
                    oPersonType = new TypePerson { idPersonType = PersonTypeId() }
                });

                Assert.True(result);
                Assert.Equal("Updated name", Repository.GetByDocument(document).name);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void UpdatePassword_ChangesStoredValue()
        {
            string document = SqlTestHelper.NewTag();
            Repository.Register(NewPerson(document));
            Person created = Repository.GetByDocument(document);

            try
            {
                bool result = Repository.UpdatePassword(created.idPerson, "new-hashed-value");

                Assert.True(result);
                Assert.Equal("new-hashed-value", Repository.GetByDocument(document).password);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void Delete_RemovesRow()
        {
            string document = SqlTestHelper.NewTag();
            Repository.Register(NewPerson(document));
            Person created = Repository.GetByDocument(document);

            bool result = Repository.Delete(created.idPerson);

            Assert.True(result);
            Assert.Null(Repository.GetByDocument(document));
        }

        [Fact]
        public void GetByDocument_UnknownDocument_ReturnsNull()
        {
            Assert.Null(Repository.GetByDocument(SqlTestHelper.NewTag()));
        }
    }
}
