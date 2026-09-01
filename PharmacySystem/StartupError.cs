using System;
using Microsoft.Data.SqlClient;
using PharmacySystem.Infrastructure;

namespace PharmacySystem
{
    // Turns an exception into a message the person at the till can act on. The global handlers in
    // Program.cs use this so an unexpected failure shows something better than the raw .NET crash
    // dialog, and a missing or broken database configuration is called out specifically instead of
    // looking like an application bug.
    public static class StartupError
    {
        public const string Generic =
            "Ocurrio un error inesperado y la operacion no pudo completarse.\n\n" +
            "El detalle quedo registrado en el archivo error.log, junto al ejecutable.";

        public const string Database =
            "No se pudo conectar con la base de datos.\n\n" +
            "Verifique que exista la cadena de conexion (appsettings.Local.json junto al " +
            "ejecutable, o la variable de entorno ConnectionStrings__connection), que sus datos " +
            "sean correctos y que el servidor de base de datos este disponible.\n\n" +
            "El detalle quedo registrado en el archivo error.log.";

        // True if this exception, or any exception it wraps, points at a database or configuration
        // problem rather than an application bug. Walks InnerException because a failure in a static
        // initializer (e.g. CompositionRoot building its repositories) arrives wrapped in a
        // TypeInitializationException.
        public static bool IsDatabaseOrConfig(Exception ex)
        {
            for (Exception? e = ex; e != null; e = e.InnerException)
            {
                if (e is SqlException || e is MissingConfigurationException)
                {
                    return true;
                }

                if (e is ArgumentException &&
                    e.Message.IndexOf("connection string", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static string DescribeForUser(Exception ex)
        {
            // A transient connection loss carries its own actionable message ("the server is
            // unavailable, try again") - the appsettings advice in Database would only mislead,
            // since the configuration is present and correct.
            for (Exception? e = ex; e != null; e = e.InnerException)
            {
                if (e is DataUnavailableException)
                {
                    return e.Message;
                }
            }

            return IsDatabaseOrConfig(ex) ? Database : Generic;
        }

        // True when the failure is a DataUnavailableException raised by a repository because the
        // database was briefly unreachable during an operation. Unlike a missing ConnectionStrings
        // file or a startup failure, this is transient: the operation failed, but the application
        // can keep running and the user can retry. Program.cs uses this so a lost connection while
        // loading a grid shows a message instead of closing the application.
        public static bool IsTransientDataFailure(Exception ex)
        {
            for (Exception? e = ex; e != null; e = e.InnerException)
            {
                if (e is DataUnavailableException)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
