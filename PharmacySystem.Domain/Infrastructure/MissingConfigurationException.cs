using System;

namespace PharmacySystem.Infrastructure
{
    // Thrown when a required configuration value (the "connection" connection string) is missing
    // or empty. StartupError recognises it as a configuration problem - not an application bug -
    // and shows the user how to fix appsettings.
    public sealed class MissingConfigurationException : Exception
    {
        public MissingConfigurationException(string message) : base(message)
        {
        }
    }
}
