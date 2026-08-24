using System.Data.SqlClient;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class SupplierServiceTests
    {
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
        public void RegisterSupplier_New_IsListed()
        {
            int id = SupplierService.Instance.RegisterSupplier(NewSupplier());

            try
            {
                Assert.True(id > 0);
                Assert.Contains(SupplierService.Instance.ListSupplier(), s => s.idSupplier == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void RegisterSupplier_DuplicateDocument_ReturnsZero()
        {
            string document = SqlTestHelper.NewTag();
            int firstId = SupplierService.Instance.RegisterSupplier(NewSupplier(document));

            try
            {
                int secondId = SupplierService.Instance.RegisterSupplier(NewSupplier(document));

                Assert.Equal(0, secondId);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", firstId));
            }
        }

        [Fact]
        public void UpdateSupplier_ChangesFields()
        {
            int id = SupplierService.Instance.RegisterSupplier(NewSupplier());

            try
            {
                bool result = SupplierService.Instance.UpdateSupplier(new Supplier
                {
                    idSupplier = id,
                    document = SqlTestHelper.NewTag(),
                    companyName = "Updated supplier",
                    email = "updated@test.local",
                    phone = "0888888888"
                });

                Assert.True(result);
                Assert.Contains(SupplierService.Instance.ListSupplier(), s => s.idSupplier == id && s.companyName == "Updated supplier");
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM supplier WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void DeleteSupplier_RemovesRow()
        {
            int id = SupplierService.Instance.RegisterSupplier(NewSupplier());

            bool result = SupplierService.Instance.DeleteSupplier(id);

            Assert.True(result);
            Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM supplier WHERE id = @id", new SqlParameter("@id", id)));
        }
    }
}
