using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Business
{
    public class NotificationConfigServiceTests
    {
        private static NotificationConfigService CreateService(FakeNotificationConfigRepository repository, FakeProductAlertHistoryRepository historyRepository = null)
            => new NotificationConfigService(repository, historyRepository ?? new FakeProductAlertHistoryRepository());

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

        // Fase 4 (traceability): GetActiveAlerts() must diff the live alert list against
        // product_alert_history's open rows and write only the transitions.

        [Fact]
        public void GetActiveAlerts_NewAlert_InsertsHistoryRowAndAttachesId()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListStockResult = new List<Product> { new Product { idProduct = 1, code = "P1", name = "Paracetamol", stock = 0 } }
            };
            var historyRepository = new FakeProductAlertHistoryRepository { NextInsertedId = 42 };

            var alerts = CreateService(repository, historyRepository).GetActiveAlerts();

            var inserted = Assert.Single(historyRepository.Inserted);
            Assert.Equal(1, inserted.ProductId);
            Assert.Equal(AlertType.Stock, inserted.AlertType);
            Assert.Equal(AlertSeverity.Critical, inserted.Severity);
            Assert.Equal(42, Assert.Single(alerts).HistoryId);
        }

        [Fact]
        public void GetActiveAlerts_AlreadyOpenAlertSameSeverity_DoesNotWriteAgain()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListStockResult = new List<Product> { new Product { idProduct = 1, code = "P1", name = "Paracetamol", stock = 3 } }
            };
            var historyRepository = new FakeProductAlertHistoryRepository
            {
                OpenAlerts = new List<ProductAlertHistoryEntry>
                {
                    new ProductAlertHistoryEntry { Id = 7, ProductId = 1, AlertType = AlertType.Stock, Severity = AlertSeverity.Low }
                }
            };

            var alerts = CreateService(repository, historyRepository).GetActiveAlerts();

            Assert.Empty(historyRepository.Inserted);
            Assert.Empty(historyRepository.SeverityUpdates);
            Assert.Equal(7, Assert.Single(alerts).HistoryId);
        }

        [Fact]
        public void GetActiveAlerts_OpenAlertSeverityWorsened_UpdatesSeverityInPlace()
        {
            var repository = new FakeNotificationConfigRepository
            {
                ListStockResult = new List<Product> { new Product { idProduct = 1, code = "P1", name = "Paracetamol", stock = 0 } }
            };
            var historyRepository = new FakeProductAlertHistoryRepository
            {
                OpenAlerts = new List<ProductAlertHistoryEntry>
                {
                    new ProductAlertHistoryEntry { Id = 7, ProductId = 1, AlertType = AlertType.Stock, Severity = AlertSeverity.Low }
                }
            };

            CreateService(repository, historyRepository).GetActiveAlerts();

            var update = Assert.Single(historyRepository.SeverityUpdates);
            Assert.Equal(7, update.HistoryId);
            Assert.Equal(AlertSeverity.Critical, update.Severity);
            Assert.Empty(historyRepository.Inserted);
        }

        [Fact]
        public void GetActiveAlerts_PreviouslyOpenAlertNoLongerQualifies_IsResolved()
        {
            var repository = new FakeNotificationConfigRepository(); // nothing qualifies any more
            var historyRepository = new FakeProductAlertHistoryRepository
            {
                OpenAlerts = new List<ProductAlertHistoryEntry>
                {
                    new ProductAlertHistoryEntry { Id = 7, ProductId = 1, AlertType = AlertType.Stock, Severity = AlertSeverity.Low }
                }
            };

            CreateService(repository, historyRepository).GetActiveAlerts();

            Assert.Equal(new[] { 7 }, historyRepository.Resolved);
        }

        [Fact]
        public void AcknowledgeAlert_DelegatesToHistoryRepository()
        {
            var historyRepository = new FakeProductAlertHistoryRepository { AcknowledgeResult = true };

            bool result = CreateService(new FakeNotificationConfigRepository(), historyRepository).AcknowledgeAlert(7, 3);

            Assert.True(result);
            Assert.Equal((7, 3), historyRepository.AcknowledgedWith);
        }

        [Fact]
        public void GetAlertHistory_DelegatesToHistoryRepository()
        {
            var expected = new List<ProductAlertHistoryEntry> { new ProductAlertHistoryEntry { Id = 1 } };
            var historyRepository = new FakeProductAlertHistoryRepository { HistoryResult = expected };

            var result = CreateService(new FakeNotificationConfigRepository(), historyRepository)
                .GetAlertHistory(DateTime.Today.AddDays(-7), DateTime.Today);

            Assert.Same(expected, result);
        }
    }
}
