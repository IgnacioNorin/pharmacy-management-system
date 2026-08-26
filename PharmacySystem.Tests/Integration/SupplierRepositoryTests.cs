using System.Data.SqlClient;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Was SupplierServiceTests, calling SupplierService.Instance. Now that the SQL moved into
    // SupplierRepository (PharmacySystem.Data), this exercises it directly instead of going
    // through the WinForms-side adapter facade - same database round trip, same assertions.
    [Collection("Database")]
    public class SupplierRepositoryTests
    {
        private static readonly ISupplierRepository Repository = new SupplierRepository(SqlConnectionFactory.FromConfiguration());

        private static Supplier NewSupplier(string document = null)
        {
            return new Supplier
            {
                document = document ?? SqlTestHelper.NewTag(),
                companyName = "Test supplier",
                email = "supplier@test.local",
                phone = "0999999999"
            };
        }

        [Fact]
        public void Register_New_IsListed()
        {
            int id = Repository.Register(NewSupplier());

            try
            {
                Assert.True(id > 0);
                Assert.Contains(Repository.List(), s => s.idSupplier == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Register_DuplicateDocument_ReturnsZero()
        {
            string document = SqlTestHelper.NewTag();
            int firstId = Repository.Register(NewSupplier(document));

            try
            {
                int secondId = Repository.Register(NewSupplier(document));

                Assert.Equal(0, secondId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", firstId));
            }
        }

        [Fact]
        public void Update_ChangesFields()
        {
            int id = Repository.Register(NewSupplier());

            try
            {
                bool result = Repository.Update(new Supplier
                {
                    idSupplier = id,
                    document = SqlTestHelper.NewTag(),
                    companyName = "Updated supplier",
                    email = "updated@test.local",
                    phone = "0888888888"
                });

                Assert.True(result);
                Assert.Contains(Repository.List(), s => s.idSupplier == id && s.companyName == "Updated supplier");
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Delete_RemovesRow()
        {
            int id = Repository.Register(NewSupplier());

            bool result = Repository.Delete(id);

            Assert.True(result);
            Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM supplier WHERE id = @id", new SqlParameter("@id", id)));
        }
    }
}
