using System;

namespace PharmacySystem.Infrastructure
{
    // Raised by a repository when an operation could not run because the database itself is
    // unreachable (server down, network failure, login rejected, command timeout) - as opposed
    // to the operation running and yielding an empty or negative result. The login, sale and
    // purchase presenters catch it directly; for the product, client, supplier and report
    // screens it propagates to the global handler in Program.cs, which shows "the database is
    // unavailable, try again" and keeps the application running instead of showing a misleading
    // "no matches" / "check the stock" message or a silently empty grid.
    public class DataUnavailableException : Exception
    {
        public const string DefaultMessage =
            "No se pudo conectar con la base de datos. Verifique que el servidor este " +
            "disponible e intente nuevamente.";

        public DataUnavailableException()
            : base(DefaultMessage) { }

        public DataUnavailableException(string message)
            : base(message) { }

        public DataUnavailableException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
