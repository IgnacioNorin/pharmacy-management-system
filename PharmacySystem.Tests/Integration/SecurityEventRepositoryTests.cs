using System.Data.SqlClient;
using PharmacySystem.Data;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class SecurityEventRepositoryTests
    {
        private static readonly ISecurityEventRepository Repository =
            new SecurityEventRepository(SqlConnectionFactory.FromConfiguration());

        [Fact]
        public void Record_InsertsARow_WithTheGivenFields()
        {
            string marker = "test-" + SqlTestHelper.NewTag();
            try
            {
                Repository.Record(1, "role.permissions", "person_type", 100, marker, "TESTBOX");

                var row = SqlTestHelper.ExecuteScalar(
                    "SELECT action + '|' + CAST(entity_id AS varchar(10)) + '|' + summary + '|' + station " +
                    "FROM security_event WHERE summary = @s", new SqlParameter("@s", marker));

                Assert.Equal("role.permissions|100|" + marker + "|TESTBOX", row);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM security_event WHERE summary = @s", new SqlParameter("@s", marker));
            }
        }

        [Fact]
        public void Record_ZeroActor_StoresNull()
        {
            string marker = "test-" + SqlTestHelper.NewTag();
            try
            {
                Repository.Record(0, "store.update", "store", 1, marker, null);

                int nulls = SqlTestHelper.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM security_event WHERE summary = @s AND actor_id IS NULL",
                    new SqlParameter("@s", marker));
                Assert.Equal(1, nulls);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM security_event WHERE summary = @s", new SqlParameter("@s", marker));
            }
        }
    }
}
