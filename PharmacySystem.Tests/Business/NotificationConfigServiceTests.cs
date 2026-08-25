using System;
using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class NotificationConfigServiceTests
    {
        private static NotificationConfigService CreateService(FakeNotificationConfigRepository repository)
            => new NotificationConfigService(repository);

        [Fact]
        public void GetActiveAlerts_OutOfStockProduct_IsCritical()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListStockResult = new List<Product> { new Product { idProduct = 1, code = "P1", name = "Paracetamol", stock = 0 } }
            };

            var alerts = CreateService(repository).GetActiveAlerts();

            var alert = Assert.Single(alerts);
            Assert.Equal(AlertSeverity.Critical, alert.Severity);
            Assert.Equal("Sin stock", alert.Detail);
        }

        [Fact]
        public void GetActiveAlerts_LowButNonZeroStock_IsLow()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListStockResult = new List<Product> { new Product { idProduct = 1, code = "P1", name = "Paracetamol", stock = 3 } }
            };

            var alerts = CreateService(repository).GetActiveAlerts();

            var alert = Assert.Single(alerts);
            Assert.Equal(AlertSeverity.Low, alert.Severity);
            Assert.Equal("Stock: 3", alert.Detail);
        }

        [Fact]
        public void GetActiveAlerts_PastExpirationDate_IsExpired()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListExpirationDateResult = new List<Product>
                {
                    new Product { idProduct = 2, code = "P2", name = "Amoxicilina", expirationDate = DateTime.Today.AddDays(-1) }
                }
            };

            var alerts = CreateService(repository).GetActiveAlerts();

            var alert = Assert.Single(alerts);
            Assert.Equal(AlertSeverity.Expired, alert.Severity);
            Assert.StartsWith("Venció el", alert.Detail);
        }

        [Fact]
        public void GetActiveAlerts_FutureExpirationDateWithinWindow_IsExpiringSoon()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListExpirationDateResult = new List<Product>
                {
                    new Product { idProduct = 2, code = "P2", name = "Amoxicilina", expirationDate = DateTime.Today.AddDays(3) }
                }
            };

            var alerts = CreateService(repository).GetActiveAlerts();

            var alert = Assert.Single(alerts);
            Assert.Equal(AlertSeverity.ExpiringSoon, alert.Severity);
            Assert.StartsWith("Vence el", alert.Detail);
        }

        [Fact]
        public void GetActiveAlerts_TodayAsExpirationDate_IsExpiringSoonNotExpired()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListExpirationDateResult = new List<Product>
                {
                    new Product { idProduct = 2, code = "P2", name = "Amoxicilina", expirationDate = DateTime.Today }
                }
            };

            var alerts = CreateService(repository).GetActiveAlerts();

            Assert.Equal(AlertSeverity.ExpiringSoon, Assert.Single(alerts).Severity);
        }

        [Fact]
        public void GetActiveAlerts_MixOfSeverities_OrdersMostUrgentFirst()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListStockResult = new List<Product>
                {
                    new Product { idProduct = 1, code = "P1", name = "Low stock", stock = 3 },
                    new Product { idProduct = 2, code = "P2", name = "Out of stock", stock = 0 }
                },
                ListExpirationDateResult = new List<Product>
                {
                    new Product { idProduct = 3, code = "P3", name = "Expiring soon", expirationDate = DateTime.Today.AddDays(2) },
                    new Product { idProduct = 4, code = "P4", name = "Already expired", expirationDate = DateTime.Today.AddDays(-2) }
                }
            };

            var alerts = CreateService(repository).GetActiveAlerts();

            Assert.Equal(4, alerts.Count);
            Assert.Equal(AlertSeverity.Critical, alerts[0].Severity); // Out of stock
            Assert.Equal(AlertSeverity.Expired, alerts[1].Severity); // Already expired
            Assert.Equal(AlertSeverity.Low, alerts[2].Severity); // Low stock
            Assert.Equal(AlertSeverity.ExpiringSoon, alerts[3].Severity); // Expiring soon
        }

        [Fact]
        public void GetActiveAlerts_NoQualifyingProducts_ReturnsEmpty()
        {
            var repository = new FakeNotificationConfigRepository();

            var alerts = CreateService(repository).GetActiveAlerts();

            Assert.Empty(alerts);
        }

        [Fact]
        public void GetActiveAlerts_PassesConfiguredThresholdsToRepository()
        {
            var repository = new FakeNotificationConfigRepository { ConfigStockResult = 7, ConfigDayResult = 4 };

            CreateService(repository).GetActiveAlerts();

            Assert.Equal(7, repository.RequestedCriticalStock);
            Assert.Equal(4, repository.RequestedDays);
        }
    }
}
