using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Builds each report as a typed (ReportDefinition, ReportResult) pair: the definition
    // declares the columns once, the result carries raw rows plus an optional totals row of
    // the same shape. No formatting and no DataTable here - the view/exporters decide how the
    // typed values look. Totals are derived from the rows themselves; the only surviving
    // aggregate query is the purchase header total (see PurchaseTotals).
    public class ReportPresenter
    {
        private readonly IReportView _view;
        private readonly ISupplierService _supplierService;
        private readonly ICategoryService _categoryService;
        private readonly ISaleService _saleService;
        private readonly IPurchaseService _purchaseService;
        private readonly IProductService _productService;
        private readonly INotificationConfigService _notificationConfigService;
        private readonly IPersonService _personService;
        private readonly CurrentUser _currentUser;

        public ReportPresenter(
            IReportView view,
            ISupplierService supplierService,
            ICategoryService categoryService,
            ISaleService saleService,
            IPurchaseService purchaseService,
            IProductService productService,
            INotificationConfigService notificationConfigService,
            IPersonService personService,
            CurrentUser currentUser)
        {
            _view = view;
            _supplierService = supplierService;
            _categoryService = categoryService;
            _saleService = saleService;
            _purchaseService = purchaseService;
            _productService = productService;
            _notificationConfigService = notificationConfigService;
            _personService = personService;
            _currentUser = currentUser;
        }

        // Each report type has its own permission. frmReport already hides the tab a role cannot
        // see; this is the fail-closed gate for the consult action behind it.
        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            var supplierOptions = new List<ComboBoxItem> { new ComboBoxItem { Value = "0", Text = "Todos" } };
            supplierOptions.AddRange(_supplierService.List().Select(s => new ComboBoxItem { Value = s.idSupplier, Text = s.companyName }));
            _view.LoadSupplierOptions(supplierOptions);

            var categoryOptions = new List<ComboBoxItem> { new ComboBoxItem { Value = "0", Text = "Todos" } };
            categoryOptions.AddRange(_categoryService.List().Select(c => new ComboBoxItem { Value = c.IdCategory, Text = c.description }));
            _view.LoadCategoryOptions(categoryOptions);

            var clientOptions = new List<ComboBoxItem> { new ComboBoxItem { Value = "0", Text = "Todos" } };
            clientOptions.AddRange(_personService.ListClients()
                .Select(p => new ComboBoxItem { Value = p.idPerson, Text = p.name }));
            _view.LoadSaleClientOptions(clientOptions);
        }

        public void OnConsultSale()
        {
            if (!Can("reportes.ventas")) return;

            int clientId = int.TryParse(_view.SelectedSaleClientId, out int parsedClientId) ? parsedClientId : 0;
            List<SaleReportRow> rows = _saleService.ReportSale(_view.SaleStartDate, _view.SaleEndDate, clientId);
            _view.SetSaleReport(SaleDefinition, new ReportResult<SaleReportRow>(rows, SaleTotals(rows)));
        }

        public void OnConsultPurchase()
        {
            if (!Can("reportes.compras")) return;

            DateTime startDate = _view.PurchaseStartDate;
            DateTime endDate = _view.PurchaseEndDate;
            string supplierId = _view.SelectedSupplierId;

            List<PurchaseReportRow> rows = _purchaseService.ReportPurchase(supplierId, startDate, endDate);
            decimal headerTotal = _purchaseService.GetTotalAmount(supplierId, startDate, endDate);
            _view.SetPurchaseReport(PurchaseDefinition, new ReportResult<PurchaseReportRow>(rows, PurchaseTotals(rows, headerTotal)));
        }

        public void OnConsultProduct()
        {
            if (!Can("reportes.productos")) return;

            List<ProductReportRow> rows = _productService.Report(_view.SelectedCategoryId);
            _view.SetProductReport(ProductDefinition, new ReportResult<ProductReportRow>(rows, ProductTotals(rows)));
        }

        // Fase 4 of the alerts rework (traceability): every stock/expiration alert transition
        // (detected, resolved, acknowledged) that fell inside the selected date range, so a
        // pharmacy can answer "when was this flagged, and was it handled" on demand.
        public void OnConsultAlertHistory()
        {
            if (!Can("reportes.alertas")) return;

            List<ProductAlertHistoryEntry> rows = _notificationConfigService.GetAlertHistory(
                _view.AlertHistoryStartDate, _view.AlertHistoryEndDate);
            _view.SetAlertHistoryReport(AlertHistoryDefinition, new ReportResult<ProductAlertHistoryEntry>(rows));
        }

        // Rows are one per sale (ReportSale does not join sale_detail), so every column total is
        // a straight sum of the rows - no separate aggregate query is needed.
        private static SaleReportRow SaleTotals(List<SaleReportRow> rows) => new SaleReportRow
        {
            NetAmount = rows.Sum(r => r.NetAmount),
            TaxAmount = rows.Sum(r => r.TaxAmount),
            ExemptAmount = rows.Sum(r => r.ExemptAmount),
            TotalAmount = rows.Sum(r => r.TotalAmount),
            AmountReceived = rows.Sum(r => r.AmountReceived),
            ChangeAmount = rows.Sum(r => r.ChangeAmount)
        };

        // Rows are one per purchase_detail line, so quantity / prices sum straight from them.
        // "Monto Total" is a purchase-header value repeated across a purchase's lines, so it
        // comes from GetTotalAmount (which sums it once per purchase, and has its own DB
        // regression test).
        private static PurchaseReportRow PurchaseTotals(List<PurchaseReportRow> rows, decimal headerTotal) => new PurchaseReportRow
        {
            TotalAmount = headerTotal,
            Quantity = rows.Sum(r => r.Quantity),
            PurchasePrice = rows.Sum(r => r.PurchasePrice)
        };

        // The totals row reinterprets the price columns as inventory valuation: total units,
        // value at last purchase price, lot-accurate value at cost, and value at sale price.
        private static ProductReportRow ProductTotals(List<ProductReportRow> rows) => new ProductReportRow
        {
            Stock = rows.Sum(r => r.Stock),
            PurchasePrice = rows.Sum(r => r.Stock * r.PurchasePrice),
            StockCostValue = rows.Sum(r => r.StockCostValue),
            SalePrice = rows.Sum(r => r.Stock * r.SalePrice)
        };

        private static readonly ReportDefinition<SaleReportRow> SaleDefinition = new ReportDefinition<SaleReportRow>(new[]
        {
            new ReportColumn<SaleReportRow>("Fecha Venta", ReportValueType.Date, r => r.DateRegistered),
            new ReportColumn<SaleReportRow>("Tipo Documento", ReportValueType.Text, r => r.DocumentType),
            new ReportColumn<SaleReportRow>("Número Documento", ReportValueType.Text, r => r.DocumentNumber),
            new ReportColumn<SaleReportRow>("Documento Vendedor", ReportValueType.Text, r => r.SellerDocument),
            new ReportColumn<SaleReportRow>("Nombre Vendedor", ReportValueType.Text, r => r.SellerName),
            new ReportColumn<SaleReportRow>("Documento Cliente", ReportValueType.Text, r => r.ClientDocument),
            new ReportColumn<SaleReportRow>("Cliente / Razón Social", ReportValueType.Text, r => r.ClientName),
            new ReportColumn<SaleReportRow>("Neto", ReportValueType.Currency, r => r.NetAmount),
            new ReportColumn<SaleReportRow>("IVA", ReportValueType.Currency, r => r.TaxAmount),
            new ReportColumn<SaleReportRow>("Exento", ReportValueType.Currency, r => r.ExemptAmount),
            new ReportColumn<SaleReportRow>("Total Pagar", ReportValueType.Currency, r => r.TotalAmount),
            new ReportColumn<SaleReportRow>("Forma de Pago", ReportValueType.Text, r => r.PaymentMethod),
            new ReportColumn<SaleReportRow>("Pago Con", ReportValueType.Currency, r => r.AmountReceived),
            new ReportColumn<SaleReportRow>("Cambio", ReportValueType.Currency, r => r.ChangeAmount)
        });

        private static readonly ReportDefinition<PurchaseReportRow> PurchaseDefinition = new ReportDefinition<PurchaseReportRow>(new[]
        {
            new ReportColumn<PurchaseReportRow>("Fecha Compra", ReportValueType.Date, r => r.DateRegistered),
            new ReportColumn<PurchaseReportRow>("Documento Proveedor", ReportValueType.Text, r => r.SupplierDocument),
            new ReportColumn<PurchaseReportRow>("Razón Social", ReportValueType.Text, r => r.CompanyName),
            new ReportColumn<PurchaseReportRow>("Tipo Documento", ReportValueType.Text, r => r.DocumentType),
            new ReportColumn<PurchaseReportRow>("Número Documento", ReportValueType.Text, r => r.DocumentNumber),
            new ReportColumn<PurchaseReportRow>("Monto Total", ReportValueType.Currency, r => r.TotalAmount),
            new ReportColumn<PurchaseReportRow>("Nombre", ReportValueType.Text, r => r.ProductName),
            new ReportColumn<PurchaseReportRow>("Cantidad", ReportValueType.Integer, r => r.Quantity),
            new ReportColumn<PurchaseReportRow>("Precio Compra", ReportValueType.Currency, r => r.PurchasePrice)
        });

        private static readonly ReportDefinition<ProductReportRow> ProductDefinition = new ReportDefinition<ProductReportRow>(new[]
        {
            new ReportColumn<ProductReportRow>("Fecha Registro", ReportValueType.Date, r => r.DateCreated),
            new ReportColumn<ProductReportRow>("Código", ReportValueType.Text, r => r.Code),
            new ReportColumn<ProductReportRow>("Nombre", ReportValueType.Text, r => r.Name),
            new ReportColumn<ProductReportRow>("Descripción", ReportValueType.Text, r => r.Description),
            new ReportColumn<ProductReportRow>("Categoría", ReportValueType.Text, r => r.CategoryDescription),
            new ReportColumn<ProductReportRow>("Stock", ReportValueType.Integer, r => r.Stock),
            new ReportColumn<ProductReportRow>("Precio Compra", ReportValueType.Currency, r => r.PurchasePrice),
            new ReportColumn<ProductReportRow>("Valor Stock (costo)", ReportValueType.Currency, r => r.StockCostValue),
            new ReportColumn<ProductReportRow>("Precio Venta", ReportValueType.Currency, r => r.SalePrice),
            new ReportColumn<ProductReportRow>("Fecha Vencimiento", ReportValueType.Date, r => r.DateExpired),
            new ReportColumn<ProductReportRow>("Estado", ReportValueType.Text, r => r.StatusName)
        });

        private static readonly ReportDefinition<ProductAlertHistoryEntry> AlertHistoryDefinition = new ReportDefinition<ProductAlertHistoryEntry>(new[]
        {
            new ReportColumn<ProductAlertHistoryEntry>("Fecha Detectada", ReportValueType.Date, r => r.DetectedAt),
            new ReportColumn<ProductAlertHistoryEntry>("Producto", ReportValueType.Text, r => r.ProductName),
            new ReportColumn<ProductAlertHistoryEntry>("Código", ReportValueType.Text, r => r.ProductCode),
            new ReportColumn<ProductAlertHistoryEntry>("Tipo", ReportValueType.Text, r => TypeLabel(r.AlertType)),
            new ReportColumn<ProductAlertHistoryEntry>("Severidad", ReportValueType.Text, r => SeverityLabel(r.Severity)),
            new ReportColumn<ProductAlertHistoryEntry>("Valor", ReportValueType.Text, r => r.TriggerValue?.ToString() ?? ""),
            new ReportColumn<ProductAlertHistoryEntry>("Fecha Resuelta", ReportValueType.Date, r => r.ResolvedAt.HasValue ? (object)r.ResolvedAt.Value : "Abierta"),
            new ReportColumn<ProductAlertHistoryEntry>("Reconocido Por", ReportValueType.Text, r => r.AcknowledgedByName ?? ""),
            new ReportColumn<ProductAlertHistoryEntry>("Fecha Reconocimiento", ReportValueType.Date, r => r.AcknowledgedAt.HasValue ? (object)r.AcknowledgedAt.Value : "")
        });

        private static string TypeLabel(AlertType type) => type == AlertType.Stock ? "Stock" : "Vencimiento";

        private static string SeverityLabel(AlertSeverity severity)
        {
            switch (severity)
            {
                case AlertSeverity.Critical: return "Crítico";
                case AlertSeverity.Expired: return "Vencido";
                case AlertSeverity.Low: return "Bajo";
                case AlertSeverity.ExpiringSoon: return "Por vencer";
                default: return "";
            }
        }
    }
}
