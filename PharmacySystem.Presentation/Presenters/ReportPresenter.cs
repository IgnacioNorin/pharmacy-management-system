using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from frmReport.cs. The three Report*/ReportPurchase/ReportSale methods used to
    // build a pre-formatted-string DataTable directly inside the WinForms-side adapter, because
    // that's where CultureInfoHelper/DateHelper lived. Now that those helpers are in Domain, the
    // repositories return raw typed rows and this presenter builds the exact same DataTable
    // shape (same column names/order, same "Total:" summary row) - it's what both the grid
    // binding and the Excel export in frmReport.cs's Export*_Click handlers still consume.
    public class ReportPresenter
    {
        private readonly IReportView _view;
        private readonly ISupplierService _supplierService;
        private readonly ICategoryService _categoryService;
        private readonly ISaleService _saleService;
        private readonly IPurchaseService _purchaseService;
        private readonly IProductService _productService;
        private readonly INotificationConfigService _notificationConfigService;
        private readonly CurrentUser _currentUser;

        public ReportPresenter(
            IReportView view,
            ISupplierService supplierService,
            ICategoryService categoryService,
            ISaleService saleService,
            IPurchaseService purchaseService,
            IProductService productService,
            INotificationConfigService notificationConfigService,
            CurrentUser currentUser)
        {
            _view = view;
            _supplierService = supplierService;
            _categoryService = categoryService;
            _saleService = saleService;
            _purchaseService = purchaseService;
            _productService = productService;
            _notificationConfigService = notificationConfigService;
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
        }

        public void OnConsultSale()
        {
            if (!Can("reportes.ventas")) return;

            DateTime startDate = _view.SaleStartDate;
            DateTime endDate = _view.SaleEndDate;

            decimal sumTotalPay = _saleService.SumTotalPay(startDate, endDate);
            decimal sumAmountReceived = _saleService.SumAmountReceived(startDate, endDate);
            decimal sumChangeAmount = _saleService.SumChangeAmount(startDate, endDate);
            List<SaleReportRow> rows = _saleService.ReportSale(startDate, endDate);

            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha Venta", typeof(string));
            dt.Columns.Add("Tipo Documento", typeof(string));
            dt.Columns.Add("Numero Documento", typeof(string));
            dt.Columns.Add("CI Vendedor", typeof(string));
            dt.Columns.Add("Nombre Vendedor", typeof(string));
            dt.Columns.Add("CI Cliente", typeof(string));
            dt.Columns.Add("Nombre Cliente", typeof(string));
            dt.Columns.Add("Neto", typeof(string));
            dt.Columns.Add("IVA", typeof(string));
            dt.Columns.Add("Exento", typeof(string));
            dt.Columns.Add("Total Pagar", typeof(string));
            dt.Columns.Add("Pago Con", typeof(string));
            dt.Columns.Add("Cambio", typeof(string));

            foreach (SaleReportRow r in rows)
            {
                dt.Rows.Add(
                    DateHelper.FormatDatePresentation(r.DateRegistered),
                    r.DocumentType,
                    r.DocumentNumber,
                    r.SellerDocument,
                    r.SellerName,
                    r.ClientDocument,
                    r.ClientName,
                    CultureInfoHelper.FormatAsCurrency(r.NetAmount),
                    CultureInfoHelper.FormatAsCurrency(r.TaxAmount),
                    CultureInfoHelper.FormatAsCurrency(r.ExemptAmount),
                    CultureInfoHelper.FormatAsCurrency(r.TotalAmount),
                    CultureInfoHelper.FormatAsCurrency(r.AmountReceived),
                    CultureInfoHelper.FormatAsCurrency(r.ChangeAmount));
            }

            dt.Rows.Add(null, null);
            dt.Rows.Add(null, null, null, null, null, null, "Total:",
                CultureInfoHelper.FormatAsCurrency(rows.Sum(r => r.NetAmount)),
                CultureInfoHelper.FormatAsCurrency(rows.Sum(r => r.TaxAmount)),
                CultureInfoHelper.FormatAsCurrency(rows.Sum(r => r.ExemptAmount)),
                CultureInfoHelper.FormatAsCurrency(sumTotalPay),
                CultureInfoHelper.FormatAsCurrency(sumAmountReceived),
                CultureInfoHelper.FormatAsCurrency(sumChangeAmount));

            _view.SetSaleReport(dt);
        }

        public void OnConsultPurchase()
        {
            if (!Can("reportes.compras")) return;

            DateTime startDate = _view.PurchaseStartDate;
            DateTime endDate = _view.PurchaseEndDate;
            string supplierId = _view.SelectedSupplierId;

            List<PurchaseReportRow> rows = _purchaseService.ReportPurchase(supplierId, startDate, endDate);
            decimal sumTotalAmount = _purchaseService.GetTotalAmount(supplierId, startDate, endDate);
            int sumQuantityProduct = _purchaseService.GetTotalQuantity(supplierId, startDate, endDate);
            decimal sumPurchasePrice = _purchaseService.GetTotalPurchasePrice(supplierId, startDate, endDate);
            decimal sumSalePrice = _purchaseService.GetTotalSalesPrice(supplierId, startDate, endDate);

            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha Compra", typeof(string));
            dt.Columns.Add("Documento Proveedor", typeof(string));
            dt.Columns.Add("Razon Social", typeof(string));
            dt.Columns.Add("Tipo Documento", typeof(string));
            dt.Columns.Add("Numero Documento", typeof(string));
            dt.Columns.Add("Monto Total", typeof(string));
            dt.Columns.Add("Nombre,", typeof(string));
            dt.Columns.Add("Cantidad", typeof(string));
            dt.Columns.Add("Precio Compra", typeof(string));
            dt.Columns.Add("Precio Venta", typeof(string));

            foreach (PurchaseReportRow r in rows)
            {
                dt.Rows.Add(
                    DateHelper.FormatDatePresentation(r.DateRegistered),
                    r.SupplierDocument,
                    r.CompanyName,
                    r.DocumentType,
                    r.DocumentNumber,
                    CultureInfoHelper.FormatAsCurrency(r.TotalAmount),
                    r.ProductName,
                    r.Quantity.ToString(),
                    CultureInfoHelper.FormatAsCurrency(r.PurchasePrice),
                    CultureInfoHelper.FormatAsCurrency(r.SalePrice));
            }

            dt.Rows.Add(null, null);
            dt.Rows.Add(null, null, null, null, "Total:",
                CultureInfoHelper.FormatAsCurrency(sumTotalAmount), null,
                sumQuantityProduct.ToString(),
                CultureInfoHelper.FormatAsCurrency(sumPurchasePrice),
                CultureInfoHelper.FormatAsCurrency(sumSalePrice));

            _view.SetPurchaseReport(dt);
        }

        public void OnConsultProduct()
        {
            if (!Can("reportes.productos")) return;

            List<ProductReportRow> rows = _productService.Report(_view.SelectedCategoryId);

            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha Registro", typeof(string));
            dt.Columns.Add("Codigo", typeof(string));
            dt.Columns.Add("Nombre", typeof(string));
            dt.Columns.Add("Descripcion", typeof(string));
            dt.Columns.Add("Categoria", typeof(string));
            dt.Columns.Add("Stock", typeof(string));
            dt.Columns.Add("Precio Compra", typeof(string));
            dt.Columns.Add("Precio Venta", typeof(string));
            dt.Columns.Add("Fecha Vencimiento", typeof(string));
            dt.Columns.Add("Estado", typeof(string));

            foreach (ProductReportRow r in rows)
            {
                dt.Rows.Add(
                    DateHelper.FormatDatePresentation(r.DateCreated),
                    r.Code,
                    r.Name,
                    r.Description,
                    r.CategoryDescription,
                    r.Stock.ToString(),
                    CultureInfoHelper.FormatAsCurrency(r.PurchasePrice),
                    CultureInfoHelper.FormatAsCurrency(r.SalePrice),
                    DateHelper.FormatDatePresentation(r.DateExpired),
                    r.StatusName);
            }

            _view.SetProductReport(dt);
        }

        // Fase 4 of the alerts rework (traceability): every stock/expiration alert transition
        // (detected, resolved, acknowledged) that fell inside the selected date range, so a
        // pharmacy can answer "when was this flagged, and was it handled" on demand.
        public void OnConsultAlertHistory()
        {
            if (!Can("reportes.alertas")) return;

            List<ProductAlertHistoryEntry> rows = _notificationConfigService.GetAlertHistory(
                _view.AlertHistoryStartDate, _view.AlertHistoryEndDate);

            DataTable dt = new DataTable();
            dt.Columns.Add("Fecha Detectada", typeof(string));
            dt.Columns.Add("Producto", typeof(string));
            dt.Columns.Add("Codigo", typeof(string));
            dt.Columns.Add("Tipo", typeof(string));
            dt.Columns.Add("Severidad", typeof(string));
            dt.Columns.Add("Valor", typeof(string));
            dt.Columns.Add("Fecha Resuelta", typeof(string));
            dt.Columns.Add("Reconocido Por", typeof(string));
            dt.Columns.Add("Fecha Reconocimiento", typeof(string));

            foreach (ProductAlertHistoryEntry r in rows)
            {
                dt.Rows.Add(
                    DateHelper.FormatDatePresentation(r.DetectedAt),
                    r.ProductName,
                    r.ProductCode,
                    TypeLabel(r.AlertType),
                    SeverityLabel(r.Severity),
                    r.TriggerValue?.ToString() ?? "",
                    r.ResolvedAt.HasValue ? DateHelper.FormatDatePresentation(r.ResolvedAt.Value) : "Abierta",
                    r.AcknowledgedByName ?? "",
                    r.AcknowledgedAt.HasValue ? DateHelper.FormatDatePresentation(r.AcknowledgedAt.Value) : "");
            }

            _view.SetAlertHistoryReport(dt);
        }

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
