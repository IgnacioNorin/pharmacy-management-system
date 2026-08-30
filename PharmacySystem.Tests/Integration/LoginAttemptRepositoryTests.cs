using System.Data.SqlClient;
using PharmacySystem.Data;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    [Collection("Database")]
    public class LoginAttemptRepositoryTests
    {
        private static readonly ILoginAttemptRepository Repository =
            new LoginAttemptRepository(SqlConnectionFactory.FromConfiguration());

        private const int Window = 15;

        private static void InsertAttempt(string document, bool success, int minutesAgo, string reason = "login")
        {
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO login_attempt (document_number, success, reason, at) " +
                "VALUES (@doc, @success, @reason, DATEADD(MINUTE, -@ago, GETDATE()))",
                new SqlParameter("@doc", document),
                new SqlParameter("@success", success),
                new SqlParameter("@reason", reason),
                new SqlParameter("@ago", minutesAgo));
        }

        [Fact]
        public void Record_InsertsARow()
        {
            string document = SqlTestHelper.NewTag();
            try
            {
                Repository.Record(document, false, "login", null, "TESTBOX");

                int count = SqlTestHelper.ExecuteScalarInt(
                    "SELECT COUNT(*) FROM login_attempt WHERE document_number = @doc",
                    new SqlParameter("@doc", document));
                Assert.Equal(1, count);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM login_attempt WHERE document_number = @doc", new SqlParameter("@doc", document));
            }
        }

        [Fact]
        public void CountFailuresSinceLastReset_CountsOnlyFailuresInsideTheWindow()
        {
            string document = SqlTestHelper.NewTag();
            try
            {
                InsertAttempt(document, success: false, minutesAgo: 5);
                InsertAttempt(document, success: false, minutesAgo: 10);
                InsertAttempt(document, success: false, minutesAgo: 20); // outside the 15-min window

                Assert.Equal(2, Repository.CountFailuresSinceLastReset(document, Window));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM login_attempt WHERE document_number = @doc", new SqlParameter("@doc", document));
            }
        }

        [Fact]
        public void CountFailuresSinceLastReset_IgnoresFailuresOlderThanTheLastSuccess()
        {
            string document = SqlTestHelper.NewTag();
            try
            {
                InsertAttempt(document, success: false, minutesAgo: 12);
                InsertAttempt(document, success: false, minutesAgo: 11);
                InsertAttempt(document, success: true, minutesAgo: 8);   // resets the count
                InsertAttempt(document, success: false, minutesAgo: 3);

                Assert.Equal(1, Repository.CountFailuresSinceLastReset(document, Window));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM login_attempt WHERE document_number = @doc", new SqlParameter("@doc", document));
            }
        }

        [Fact]
        public void MinutesUntilUnlock_IsTheWindowMinusTheAgeOfTheOldestCountingFailure()
        {
            string document = SqlTestHelper.NewTag();
            try
            {
                InsertAttempt(document, success: false, minutesAgo: 4);
                InsertAttempt(document, success: false, minutesAgo: 9);

                int? minutesLeft = Repository.MinutesUntilUnlock(document, Window);

                Assert.NotNull(minutesLeft);
                // 15 - ~9 = ~6, allow slack for minute-boundary rounding in DATEDIFF.
                Assert.InRange(minutesLeft.Value, 5, 7);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM login_attempt WHERE document_number = @doc", new SqlParameter("@doc", document));
            }
        }

        [Fact]
        public void MinutesUntilUnlock_NoCountingFailures_ReturnsNull()
        {
            Assert.Null(Repository.MinutesUntilUnlock(SqlTestHelper.NewTag(), Window));
        }
    }
}
