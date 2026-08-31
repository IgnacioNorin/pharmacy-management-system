using System;
using Microsoft.Data.SqlClient;
using System.Linq;
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

        [Fact]
        public void List_ReturnsRowsInTheRange_NewestFirst_WithinTheCap()
        {
            string marker = "test-" + SqlTestHelper.NewTag();
            try
            {
                Repository.Record(1, "user.create", "person", 11, marker + "-a", "BOX");
                Repository.Record(1, "user.update", "person", 12, marker + "-b", "BOX");

                // Wide range: the dev SQL Server clock can run a few hours ahead of this box.
                var rows = Repository.List(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(2), 500)
                    .Where(r => r.Summary != null && r.Summary.StartsWith(marker))
                    .ToList();

                Assert.Equal(2, rows.Count);
                Assert.True(rows[0].At >= rows[1].At);
                Assert.Contains(rows, r => r.Summary == marker + "-a" && r.EntityId == 11);
                Assert.Contains(rows, r => r.ActorName != null); // LEFT JOIN person resolved or ""
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM security_event WHERE summary LIKE @s", new SqlParameter("@s", marker + "%"));
            }
        }
    }
}
