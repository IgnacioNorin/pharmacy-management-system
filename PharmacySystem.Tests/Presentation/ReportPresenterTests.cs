using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class ReportPresenterTests
    {
        private static (ReportPresenter Presenter, FakeReportView View, FakeSupplierService Suppliers,
            FakeCategoryService Categories, FakeSaleService Sales, FakePurchaseService Purchases, FakeProductService Products,
            FakeNotificationConfigService Notifications, FakePersonService Persons) Create()
        {
            var view = new FakeReportView();
            var suppliers = new FakeSupplierService();
            var categories = new FakeCategoryService();
            var sales = new FakeSaleService();
            var purchases = new FakePurchaseService();
            var products = new FakeProductService();
            var notifications = new FakeNotificationConfigService();
            var persons = new FakePersonService();
            var presenter = new ReportPresenter(view, suppliers, categories, sales, purchases, products, notifications, persons,
                TestUser.With("reportes.ventas", "reportes.compras", "reportes.productos", "reportes.alertas"));
            return (presenter, view, suppliers, categories, sales, purchases, products, notifications, persons);
        }

        private static ReportPresenter PresenterFor(FakeReportView view, params string[] permissions) =>
            new ReportPresenter(view, new FakeSupplierService(), new FakeCategoryService(), new FakeSaleService(),
                new FakePurchaseService(), new FakeProductService(), new FakeNotificationConfigService(), new FakePersonService(),
                TestUser.With(permissions));

        private static object Cell<TRow>(ReportDefinition<TRow> definition, TRow row, string header) =>
            definition.Columns.First(c => c.Header == header).Value(row);

        [Fact]
        public void OnConsult_WithoutTheMatchingReportPermission_ProducesNothing()
        {
            var view = new FakeReportView();

            // A role that can see only the purchases report.
            var presenter = PresenterFor(view, "reportes.compras");

            presenter.OnConsultSale();
            presenter.OnConsultProduct();
            presenter.OnConsultAlertHistory();

            Assert.Null(view.SaleReport);
            Assert.Null(view.ProductReport);
            Assert.Null(view.AlertHistoryReport);

            // The one it is allowed to run still works.
            presenter.OnConsultPurchase();
            Assert.NotNull(view.PurchaseReport);
        }

        [Fact]
        public void OnLoad_PrependsTodosToBothCombos()
        {
            var f = Create();
            f.Suppliers.ListResult = new List<Supplier> { new Supplier { idSupplier = 5, companyName = "Acme" } };
            f.Categories.ListResult = new List<Categories> { new Categories { IdCategory = 9, description = "Meds" } };

            f.Presenter.OnLoad();

            Assert.Equal(2, f.View.SupplierOptions.Count);
            Assert.Equal("0", f.View.SupplierOptions[0].Value);
            Assert.Equal("Todos", f.View.SupplierOptions[0].Text);
            Assert.Equal(5, f.View.SupplierOptions[1].Value);

            Assert.Equal(2, f.View.CategoryOptions.Count);
            Assert.Equal("0", f.View.CategoryOptions[0].Value);
            Assert.Equal(9, f.View.CategoryOptions[1].Value);
        }

        [Fact]
        public void OnLoad_LoadsSaleClientOptions_TodosThenOnlyClients()
        {
            var f = Create();
            f.Persons.ListResult = new List<Person>
            {
                new Person { idPerson = 7, name = "Clínica Andes", Estado = true, oPersonType = new TypePerson { idPersonType = 4 } }, // Cliente
                new Person { idPerson = 8, name = "Empleado", Estado = true, oPersonType = new TypePerson { idPersonType = 3 } },
                new Person { idPerson = 9, name = "Cliente dado de baja", Estado = false, oPersonType = new TypePerson { idPersonType = 4 } }
            };

            f.Presenter.OnLoad();

            Assert.Equal(2, f.View.SaleClientOptions.Count);
            Assert.Equal("0", f.View.SaleClientOptions[0].Value);
            Assert.Equal("Todos", f.View.SaleClientOptions[0].Text);
            Assert.Equal(7, f.View.SaleClientOptions[1].Value);
            Assert.Equal("Clínica Andes", f.View.SaleClientOptions[1].Text);
        }

        [Fact]
        public void OnConsultSale_PassesTheSelectedClientIdToTheService()
        {
            var f = Create();
            f.View.SelectedSaleClientId = "7";

            f.Presenter.OnConsultSale();

            Assert.Equal(7, f.Sales.ReportClientId);
        }

        [Fact]
        public void OnConsultSale_PassesRawRowsThroughAndSumsTheTotalsFromThem()
        {
            var f = Create();
            f.Sales.ReportResult = new List<SaleReportRow>
            {
                new SaleReportRow
                {
                    DateRegistered = new DateTime(2026, 3, 17),
                    DocumentType = "Boleta",
                    DocumentNumber = "000001",
                    SellerDocument = "111",
                    SellerName = "Vendor",
                    ClientDocument = "222",
                    ClientName = "Client",
                    NetAmount = 13m, TaxAmount = 2m, ExemptAmount = 0m,
                    TotalAmount = 15m, AmountReceived = 20m, ChangeAmount = 5m
                },
                new SaleReportRow
                {
                    NetAmount = 7m, TaxAmount = 1m, ExemptAmount = 4m,
                    TotalAmount = 12m, AmountReceived = 12m, ChangeAmount = 0m
                }
            };

            f.Presenter.OnConsultSale();

            var result = f.View.SaleReport;
            Assert.Equal(2, result.Rows.Count);
            Assert.Same(f.Sales.ReportResult[0], result.Rows[0]);   // rows handed through untouched, unformatted
            Assert.Equal(13m, result.Rows[0].NetAmount);

            Assert.True(result.HasTotals);
            Assert.Equal(20m, result.Totals.NetAmount);
            Assert.Equal(3m, result.Totals.TaxAmount);
            Assert.Equal(4m, result.Totals.ExemptAmount);
            Assert.Equal(27m, result.Totals.TotalAmount);
            Assert.Equal(32m, result.Totals.AmountReceived);
            Assert.Equal(5m, result.Totals.ChangeAmount);
        }

        [Fact]
        public void SaleDefinition_HasOneNeutralClientPairNotTwo()
        {
            var f = Create();

            f.Presenter.OnConsultSale();

            var headers = f.View.SaleDefinition.Columns.Select(c => c.Header).ToList();
            Assert.Contains("Documento Vendedor", headers);
            Assert.Contains("Documento Cliente", headers);
            Assert.Contains("Cliente / Razón Social", headers);
            Assert.DoesNotContain("Documento Receptor", headers); // merged into "Documento Cliente"
            Assert.DoesNotContain("CI Vendedor", headers);
            Assert.DoesNotContain("CI Cliente", headers);
        }

        [Fact]
        public void OnConsultPurchase_SumsLineColumnsFromRowsButTakesTheHeaderTotalFromTheService()
        {
            var f = Create();
            f.Purchases.ReportResult = new List<PurchaseReportRow>
            {
                new PurchaseReportRow
                {
                    DateRegistered = new DateTime(2026, 3, 17),
                    SupplierDocument = "111", CompanyName = "Acme",
                    DocumentType = "Factura", DocumentNumber = "000001",
                    TotalAmount = 42.50m, ProductName = "Widget",
                    Quantity = 10, PurchasePrice = 3m
                },
                new PurchaseReportRow
                {
                    TotalAmount = 42.50m, ProductName = "Gadget",
                    Quantity = 4, PurchasePrice = 2m
                }
            };
            // Both lines belong to the same purchase: its header total must be counted once.
            f.Purchases.TotalAmountResult = 42.50m;

            f.Presenter.OnConsultPurchase();

            var result = f.View.PurchaseReport;
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal("Widget", result.Rows[0].ProductName);

            Assert.True(result.HasTotals);
            Assert.Equal(42.50m, result.Totals.TotalAmount);   // from GetTotalAmount, not the row sum (which would be 85)
            Assert.Equal(14, result.Totals.Quantity);
            Assert.Equal(5m, result.Totals.PurchasePrice);
        }

        [Fact]
        public void OnConsultProduct_TotalsRowCarriesUnitsAndInventoryValuation()
        {
            var f = Create();
            f.Products.ReportResult = new List<ProductReportRow>
            {
                new ProductReportRow
                {
                    DateCreated = new DateTime(2026, 1, 1),
                    Code = "A1", Name = "Aspirin", Description = "Pain relief",
                    CategoryDescription = "Meds", Stock = 20,
                    PurchasePrice = 1m, SalePrice = 2m,
                    DateExpired = new DateTime(2027, 1, 1), StatusName = "Activo"
                },
                new ProductReportRow
                {
                    Code = "B2", Name = "Gauze", CategoryDescription = "Supplies",
                    Stock = 5, PurchasePrice = 3m, SalePrice = 4m, StatusName = "Activo"
                }
            };

            f.Presenter.OnConsultProduct();

            var result = f.View.ProductReport;
            Assert.Equal(2, result.Rows.Count);
            Assert.Equal("Aspirin", result.Rows[0].Name);
            Assert.Equal(10, f.View.ProductDefinition.Columns.Count);

            Assert.True(result.HasTotals);
            Assert.Equal(25, result.Totals.Stock);              // 20 + 5 units
            Assert.Equal(35m, result.Totals.PurchasePrice);     // 20*1 + 5*3, value at cost
            Assert.Equal(60m, result.Totals.SalePrice);         // 20*2 + 5*4, value at sale price
        }

        // Fase 4 of the alerts rework (traceability). The type / severity / "Abierta" labels now
        // live in the column selectors, so they are exercised through the definition.
        [Fact]
        public void OnConsultAlertHistory_ColumnSelectorsLabelEachState()
        {
            var f = Create();
            f.Notifications.GetAlertHistoryResult = new List<ProductAlertHistoryEntry>
            {
                new ProductAlertHistoryEntry
                {
                    ProductCode = "P1", ProductName = "Paracetamol",
                    AlertType = AlertType.Stock, Severity = AlertSeverity.Critical,
                    TriggerValue = 0m, DetectedAt = new DateTime(2026, 3, 10),
                    ResolvedAt = null, AcknowledgedByName = null, AcknowledgedAt = null
                },
                new ProductAlertHistoryEntry
                {
                    ProductCode = "P2", ProductName = "Amoxicilina",
                    AlertType = AlertType.Expiration, Severity = AlertSeverity.Expired,
                    TriggerValue = null, DetectedAt = new DateTime(2026, 3, 1),
                    ResolvedAt = new DateTime(2026, 3, 5),
                    AcknowledgedByName = "Juan Pérez", AcknowledgedAt = new DateTime(2026, 3, 2)
                }
            };

            f.Presenter.OnConsultAlertHistory();

            var def = f.View.AlertHistoryDefinition;
            var rows = f.View.AlertHistoryReport.Rows;
            Assert.Equal(2, rows.Count);
            Assert.False(f.View.AlertHistoryReport.HasTotals);

            Assert.Equal("Stock", Cell(def, rows[0], "Tipo"));
            Assert.Equal("Crítico", Cell(def, rows[0], "Severidad"));
            Assert.Equal("Abierta", Cell(def, rows[0], "Fecha Resuelta"));
            Assert.Equal("", Cell(def, rows[0], "Reconocido Por"));

            Assert.Equal("Vencimiento", Cell(def, rows[1], "Tipo"));
            Assert.Equal("Vencido", Cell(def, rows[1], "Severidad"));
            Assert.Equal(new DateTime(2026, 3, 5), Cell(def, rows[1], "Fecha Resuelta"));
            Assert.Equal("Juan Pérez", Cell(def, rows[1], "Reconocido Por"));
        }

        [Fact]
        public void OnConsultAlertHistory_ForwardsWhateverTheServiceReturns()
        {
            var f = Create();
            f.View.AlertHistoryStartDate = new DateTime(2026, 1, 1);
            f.View.AlertHistoryEndDate = new DateTime(2026, 1, 31);
            f.Notifications.GetAlertHistoryResult = new List<ProductAlertHistoryEntry>();

            f.Presenter.OnConsultAlertHistory();

            Assert.Empty(f.View.AlertHistoryReport.Rows);
        }
    }
}
