using System;
using System.Configuration;
using PharmacySystem;
using PharmacySystem.Infrastructure;
using Xunit;

namespace PharmacySystem.UiTests
{
    // StartupError is what the global exception handlers in Program.cs use to decide what to tell
    // the user. The branch that matters: a missing / broken database configuration must be
    // recognised even when it arrives wrapped in a TypeInitializationException (which is how a
    // failure in CompositionRoot's static initializer actually surfaces).
    public class StartupErrorTests
    {
        [Fact]
        public void IsDatabaseOrConfig_ConfigurationError_IsTrue()
        {
            Assert.True(StartupError.IsDatabaseOrConfig(
                new ConfigurationErrorsException("No connection string named 'connection' was found.")));
        }

        [Fact]
        public void IsDatabaseOrConfig_ConfigurationErrorWrappedInTypeInitializer_IsTrue()
        {
            var wrapped = new TypeInitializationException(
                "PharmacySystem.CompositionRoot",
                new ConfigurationErrorsException("Unable to open configSource file 'ConnectionStrings.config'."));

            Assert.True(StartupError.IsDatabaseOrConfig(wrapped));
        }

        [Fact]
        public void IsDatabaseOrConfig_EmptyConnectionStringArgument_IsTrue()
        {
            Assert.True(StartupError.IsDatabaseOrConfig(
                new ArgumentException("The connection string cannot be empty.", "connectionString")));
        }

        [Fact]
        public void IsDatabaseOrConfig_UnrelatedException_IsFalse()
        {
            Assert.False(StartupError.IsDatabaseOrConfig(new InvalidOperationException("something else")));
            Assert.False(StartupError.IsDatabaseOrConfig(new NullReferenceException()));
        }

        [Fact]
        public void DescribeForUser_PicksTheDatabaseMessageForAConfigProblem()
        {
            Assert.Equal(StartupError.Database,
                StartupError.DescribeForUser(new ConfigurationErrorsException("boom")));
        }

        [Fact]
        public void DescribeForUser_PicksTheGenericMessageOtherwise()
        {
            Assert.Equal(StartupError.Generic,
                StartupError.DescribeForUser(new InvalidOperationException("boom")));
        }

        [Fact]
        public void IsTransientDataFailure_DataUnavailableException_IsTrue()
        {
            Assert.True(StartupError.IsTransientDataFailure(new DataUnavailableException()));
        }

        [Fact]
        public void IsTransientDataFailure_WrappedInTypeInitializer_IsTrue()
        {
            var wrapped = new TypeInitializationException(
                "PharmacySystem.CompositionRoot", new DataUnavailableException());

            Assert.True(StartupError.IsTransientDataFailure(wrapped));
        }

        [Fact]
        public void IsTransientDataFailure_PlainConfigOrDbError_IsFalse()
        {
            Assert.False(StartupError.IsTransientDataFailure(new ConfigurationErrorsException("boom")));
            Assert.False(StartupError.IsTransientDataFailure(new InvalidOperationException("boom")));
        }

        [Fact]
        public void DescribeForUser_TransientDataFailure_ReturnsItsOwnRetryMessage()
        {
            var ex = new DataUnavailableException();

            string message = StartupError.DescribeForUser(ex);

            Assert.Equal(ex.Message, message);
            Assert.NotEqual(StartupError.Database, message);
        }
    }
}
