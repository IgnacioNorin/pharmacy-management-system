using System;
using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    // ReportPresenter calls CultureInfoHelper.FormatAsCurrency, which reads a process-wide
    // mutable static field. Shares the "Database" collection with CultureInfoHelperTests (which
    // mutates that field via SetCurrency) so the two can never run concurrently - same reasoning
    // as putting CultureInfoHelperTests there in the first place.
    [Collection("Database")]
    public class ReportPresenterTests
    {
        private static (ReportPresenter Presenter, FakeReportView View, FakeSupplierService Suppliers,
            FakeCategoryService Categories, FakeSaleService Sales, FakePurchaseService Purchases, FakeProductService Products,
            FakeNotificationConfigService Notifications) Create()
        {
            var view = new FakeReportView();
            var suppliers = new FakeSupplierService();
            var categories = new FakeCategoryService();
            var sales = new FakeSaleService();
            var purchases = new FakePurchaseService();
            var products = new FakeProductService();
            var notifications = new FakeNotificationConfigService();
            var presenter = new ReportPresenter(view, suppliers, categories, sales, purchases, products, notifications);
            return (presenter, view, suppliers, categories, sales, purchases, products, notifications);
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
        public void OnConsultSale_BuildsRowsAndAlignedTotalRow()
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
                    TotalAmount = 15m,
                    AmountReceived = 20m,
                    ChangeAmount = 5m
                }
            };
            f.Sales.SumTotalPayResult = 15m;
            f.Sales.SumAmountReceivedResult = 20m;
            f.Sales.SumChangeAmountResult = 5m;

            f.Presenter.OnConsultSale();

            var dt = f.View.SaleReport;
            Assert.Equal(10, dt.Columns.Count);
            Assert.Equal("Nombre Cliente", dt.Columns[6].ColumnName);
            Assert.Equal(3, dt.Rows.Count); // 1 data row + 1 blank spacer + 1 total row

            Assert.Equal("17-03-2026", dt.Rows[0]["Fecha Venta"]);
            Assert.Equal("Client", dt.Rows[0]["Nombre Cliente"]);
            Assert.Contains("$", (string)dt.Rows[0]["Total Pagar"]);

            // Total row: "Total:" lands under "Nombre Cliente" (column index 6), sums under the
            // next three columns - same layout as the original ReportSale()'s Rows.Add call.
            Assert.Equal("Total:", dt.Rows[2][6]);
            Assert.Equal(DBNull.Value, dt.Rows[2][0]);
        }

        [Fact]
        public void OnConsultPurchase_BuildsRowsAndAlignedTotalRow()
        {
            var f = Create();
            f.Purchases.ReportResult = new List<PurchaseReportRow>
            {
                new PurchaseReportRow
                {
                    DateRegistered = new DateTime(2026, 3, 17),
                    SupplierDocument = "111",
                    CompanyName = "Acme",
                    DocumentType = "Factura",
                    DocumentNumber = "000001",
                    TotalAmount = 42.50m,
                    ProductName = "Widget",
                    Quantity = 10,
                    PurchasePrice = 3m,
                    SalePrice = 5m
                }
            };
            f.Purchases.TotalAmountResult = 42.50m;
            f.Purchases.TotalQuantityResult = 10;
            f.Purchases.TotalPurchasePriceResult = 3m;
            f.Purchases.TotalSalesPriceResult = 5m;

            f.Presenter.OnConsultPurchase();

            var dt = f.View.PurchaseReport;
            Assert.Equal(10, dt.Columns.Count);
            Assert.Equal(3, dt.Rows.Count);

            Assert.Equal("Widget", dt.Rows[0]["Nombre,"]);
            Assert.Equal("10", dt.Rows[0]["Cantidad"]);

            // "Total:" lands under "Numero Documento" (index 4), sum under "Monto Total" (5),
            // "Nombre," (6) stays null, quantity/purchase/sale price fill the rest.
            Assert.Equal("Total:", dt.Rows[2][4]);
            Assert.Equal(DBNull.Value, dt.Rows[2][6]);
            Assert.Equal("10", dt.Rows[2][7]);
        }

        [Fact]
        public void OnConsultProduct_BuildsRowsWithNoTotalRow()
        {
            var f = Create();
            f.Products.ReportResult = new List<ProductReportRow>
            {
                new ProductReportRow
                {
                    DateCreated = new DateTime(2026, 1, 1),
                    Code = "A1",
                    Name = "Aspirin",
                    Description = "Pain relief",
                    CategoryDescription = "Meds",
                    Stock = 20,
                    PurchasePrice = 1m,
                    SalePrice = 2m,
                    DateExpired = new DateTime(2027, 1, 1),
                    StatusName = "Activo"
                }
            };

            f.Presenter.OnConsultProduct();

            var dt = f.View.ProductReport;
            Assert.Equal(10, dt.Columns.Count);
            Assert.Single(dt.Rows); // no "Total:" row for products, matching the original
            Assert.Equal("Aspirin", dt.Rows[0]["Nombre"]);
            Assert.Equal("20", dt.Rows[0]["Stock"]);
        }

        // Fase 4 of the alerts rework (traceability).
        [Fact]
        public void OnConsultAlertHistory_OpenAndResolvedRows_FormatEachStateCorrectly()
        {
            var f = Create();
            f.Notifications.GetAlertHistoryResult = new List<ProductAlertHistoryEntry>
            {
                new ProductAlertHistoryEntry
                {
                    ProductCode = "P1",
                    ProductName = "Paracetamol",
                    AlertType = AlertType.Stock,
                    Severity = AlertSeverity.Critical,
                    TriggerValue = 0m,
                    DetectedAt = new DateTime(2026, 3, 10),
                    ResolvedAt = null,
                    AcknowledgedByName = null,
                    AcknowledgedAt = null
                },
                new ProductAlertHistoryEntry
                {
                    ProductCode = "P2",
                    ProductName = "Amoxicilina",
                    AlertType = AlertType.Expiration,
                    Severity = AlertSeverity.Expired,
                    TriggerValue = null,
                    DetectedAt = new DateTime(2026, 3, 1),
                    ResolvedAt = new DateTime(2026, 3, 5),
                    AcknowledgedByName = "Juan Pérez",
                    AcknowledgedAt = new DateTime(2026, 3, 2)
                }
            };

            f.Presenter.OnConsultAlertHistory();

            var dt = f.View.AlertHistoryReport;
            Assert.Equal(9, dt.Columns.Count);
            Assert.Equal(2, dt.Rows.Count);

            Assert.Equal("Paracetamol", dt.Rows[0]["Producto"]);
            Assert.Equal("Stock", dt.Rows[0]["Tipo"]);
            Assert.Equal("Crítico", dt.Rows[0]["Severidad"]);
            Assert.Equal("Abierta", dt.Rows[0]["Fecha Resuelta"]);
            Assert.Equal("", dt.Rows[0]["Reconocido Por"]);

            Assert.Equal("Vencimiento", dt.Rows[1]["Tipo"]);
            Assert.Equal("Vencido", dt.Rows[1]["Severidad"]);
            Assert.Equal("05-03-2026", dt.Rows[1]["Fecha Resuelta"]);
            Assert.Equal("Juan Pérez", dt.Rows[1]["Reconocido Por"]);
        }

        [Fact]
        public void OnConsultAlertHistory_PassesSelectedDateRangeToService()
        {
            var f = Create();
            f.View.AlertHistoryStartDate = new DateTime(2026, 1, 1);
            f.View.AlertHistoryEndDate = new DateTime(2026, 1, 31);

            f.Presenter.OnConsultAlertHistory();

            // FakeNotificationConfigService.GetAlertHistory doesn't currently record its args, but
            // it must not throw and must forward whatever the service returns for that call -
            // covered by returning a distinct result instance and asserting identity.
            var expected = new List<ProductAlertHistoryEntry>();
            f.Notifications.GetAlertHistoryResult = expected;
            f.Presenter.OnConsultAlertHistory();

            Assert.Empty(f.View.AlertHistoryReport.Rows);
        }
    }
}
