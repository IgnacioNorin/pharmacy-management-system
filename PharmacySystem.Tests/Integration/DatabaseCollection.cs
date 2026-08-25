using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // All integration test classes share this collection so xUnit runs them sequentially
    // against the single real SQL Server instance instead of interleaving connections.
    [CollectionDefinition("Database", DisableParallelization = true)]
    public class DatabaseCollection
    {
    }
}
