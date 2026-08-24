using System.Data.SqlClient;
using PharmacySystem.Helpers;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class PersonServiceTests
    {
        private static int PersonTypeId()
        {
            return SqlTestHelper.ExecuteScalarInt("SELECT TOP 1 id FROM person_type");
        }

        private static Person NewPerson(string document = null, string password = "Passw0rd!")
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
        public void RegisterPerson_New_StoresPasswordHashedNotPlainText()
        {
            string document = SqlTestHelper.NewTag();
            bool result = PersonService.Instance.RegisterPerson(NewPerson(document, "Passw0rd!"));

            try
            {
                Assert.True(result);

                Person stored = PersonService.Instance.GetPersonByDocument(document);

                Assert.NotNull(stored);
                Assert.True(PasswordHasher.IsHashed(stored.password));
                Assert.True(PasswordHasher.Verify("Passw0rd!", stored.password));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void RegisterPerson_DuplicateDocument_ReturnsFalse()
        {
            string document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(NewPerson(document));

            try
            {
                bool result = PersonService.Instance.RegisterPerson(NewPerson(document));

                Assert.False(result);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void UpdatePerson_ChangesFields()
        {
            string document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(NewPerson(document));
            Person created = PersonService.Instance.GetPersonByDocument(document);

            try
            {
                bool result = PersonService.Instance.UpdatePerson(new Person
                {
                    idPerson = created.idPerson,
                    document = document,
                    name = "Updated name",
                    address = "Updated address",
                    phone = "0888888888",
                    password = "Passw0rd!",
                    oPersonType = new TypePerson { idPersonType = PersonTypeId() }
                });

                Assert.True(result);
                Assert.Equal("Updated name", PersonService.Instance.GetPersonByDocument(document).name);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void UpdatePassword_ChangesStoredHash()
        {
            string document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(NewPerson(document, "Passw0rd!"));
            Person created = PersonService.Instance.GetPersonByDocument(document);
            string newHash = PasswordHasher.Hash("NewPassw0rd!");

            try
            {
                bool result = PersonService.Instance.UpdatePassword(created.idPerson, newHash);

                Assert.True(result);
                Assert.True(PasswordHasher.Verify("NewPassw0rd!", PersonService.Instance.GetPersonByDocument(document).password));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM person WHERE document_number = @document", new SqlParameter("@document", document));
            }
        }

        [Fact]
        public void DeletePerson_RemovesRow()
        {
            string document = SqlTestHelper.NewTag();
            PersonService.Instance.RegisterPerson(NewPerson(document));
            Person created = PersonService.Instance.GetPersonByDocument(document);

            bool result = PersonService.Instance.DeletePerson(created.idPerson);

            Assert.True(result);
            Assert.Null(PersonService.Instance.GetPersonByDocument(document));
        }

        [Fact]
        public void GetPersonByDocument_UnknownDocument_ReturnsNull()
        {
            Assert.Null(PersonService.Instance.GetPersonByDocument(SqlTestHelper.NewTag()));
        }
    }
}
