using System;

namespace PharmacySystem.Infrastructure
{
    // Raised by a repository when an operation could not run because the database itself is
    // unreachable (server down, network failure, login rejected, command timeout) - as opposed
    // to the operation running and yielding an empty or negative result. Presenters on the
    // critical paths (login, sale, purchase) catch this to tell the user the database is down
    // instead of showing a misleading "no matches" / "check the stock" message.
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
