using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was PersonServiceTests. The hash-if-not-already-hashed rule moved to Business - see
    // PersonServiceTests in PharmacySystem.Tests.Business for that, tested with no database.
    // This file only covers what the repository does: it persists person.password exactly as
    // given, so every password here is already in its final (hashed) form. Clients live in
    // their own table now - see ClientRepositoryTests.
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
            int newId = Repository.Register(NewPerson(document, "some-already-hashed-string"));

            try
            {
                Assert.True(newId > 0);

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
        public void Register_New_StartsWithMustChangePasswordSet_AndGetByIdReadsItBack()
        {
            string document = SqlTestHelper.NewTag();
            int newId = Repository.Register(NewPerson(document));

            try
            {
                Assert.True(Repository.GetByDocument(document).mustChangePassword);

                Person byId = Repository.GetById(newId);
                Assert.NotNull(byId);
                Assert.Equal(document, byId.document);
                Assert.True(byId.mustChangePassword);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void SetPasswordAndFlag_WritesBothColumns()
        {
            string document = SqlTestHelper.NewTag();
            int newId = Repository.Register(NewPerson(document));

            try
            {
                Assert.True(Repository.SetPasswordAndFlag(newId, "brand-new-hash", mustChangePassword: false));

                Person stored = Repository.GetById(newId);
                Assert.Equal("brand-new-hash", stored.password);
                Assert.False(stored.mustChangePassword);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void GetById_UnknownId_ReturnsNull()
        {
            Assert.Null(Repository.GetById(-1));
        }

        [Fact]
        public void Register_DuplicateDocument_ReturnsZero()
        {
            string document = SqlTestHelper.NewTag();
            Repository.Register(NewPerson(document));

            try
            {
                int result = Repository.Register(NewPerson(document));

                Assert.Equal(0, result);
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

        // --- Administrador General protection (migration 005) ---
        // The seeded account id 1 is an active Administrador General; these tests either lean on
        // it as a second active one, or deactivate it inside a try/finally to isolate the case.

        private static int InsertAdminGeneral(string document)
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO person(document_number, name, person_type_id, status) VALUES (@d, 'AG test', 1, 1)",
                new SqlParameter("@d", document));
            return SqlTestHelper.ExecuteScalarInt(
                "SELECT id FROM person WHERE document_number = @d", new SqlParameter("@d", document));
        }

        [Fact]
        public void Delete_AdminGeneral_NotTheLastActiveOne_Succeeds()
        {
            string document = SqlTestHelper.NewTag();
            int id = InsertAdminGeneral(document);

            try
            {
                Assert.True(Repository.Delete(id)); // seeded id 1 is still an active Administrador General
                Assert.Null(Repository.GetByDocument(document));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @d", new SqlParameter("@d", document));
            }
        }

        [Fact]
        public void Delete_LastActiveAdminGeneral_IsRejected()
        {
            string document = SqlTestHelper.NewTag();
            int id = InsertAdminGeneral(document);
            SqlTestHelper.ExecuteNonQuery("UPDATE person SET status = 0 WHERE id = 1");

            try
            {
                Assert.False(Repository.Delete(id));

                Person still = Repository.GetByDocument(document);
                Assert.NotNull(still);
                Assert.True(still.Estado);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("UPDATE person SET status = 1 WHERE id = 1");
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @d", new SqlParameter("@d", document));
            }
        }

        [Fact]
        public void Update_DemotingLastActiveAdminGeneral_IsRejected()
        {
            string document = SqlTestHelper.NewTag();
            int id = InsertAdminGeneral(document);
            SqlTestHelper.ExecuteNonQuery("UPDATE person SET status = 0 WHERE id = 1");

            try
            {
                bool result = Repository.Update(new Person
                {
                    idPerson = id,
                    document = document,
                    name = "AG test",
                    address = "",
                    phone = "",
                    password = "keep",
                    oPersonType = new TypePerson { idPersonType = 2 } // demote to Administrador
                });

                Assert.False(result);
                Assert.Equal(1, Repository.GetByDocument(document).oPersonType.idPersonType);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("UPDATE person SET status = 1 WHERE id = 1");
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @d", new SqlParameter("@d", document));
            }
        }
    }
}
