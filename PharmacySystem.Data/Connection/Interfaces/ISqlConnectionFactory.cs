using Microsoft.Data.SqlClient;

namespace PharmacySystem.Data
{
    // Repositories take this instead of reading a static connection string, so a test can point
    // them at a different database without touching global state or app configuration.
    public interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }
}
