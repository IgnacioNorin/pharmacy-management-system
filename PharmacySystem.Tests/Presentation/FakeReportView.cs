using System;
using System.Collections.Generic;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeReportView : IReportView
    {
        public DateTime SaleStartDate { get; set; } = new DateTime(2026, 1, 1);
        public DateTime SaleEndDate { get; set; } = new DateTime(2026, 1, 31);
        public DateTime PurchaseStartDate { get; set; } = new DateTime(2026, 1, 1);
        public DateTime PurchaseEndDate { get; set; } = new DateTime(2026, 1, 31);
        public DateTime AlertHistoryStartDate { get; set; } = new DateTime(2026, 1, 1);
        public DateTime AlertHistoryEndDate { get; set; } = new DateTime(2026, 1, 31);
        public string SelectedSupplierId { get; set; } = "0";
        public string SelectedCategoryId { get; set; } = "0";

        public IReadOnlyList<ComboBoxItem> SupplierOptions { get; private set; }
        public IReadOnlyList<ComboBoxItem> CategoryOptions { get; private set; }

        public ReportDefinition<SaleReportRow> SaleDefinition { get; private set; }
        public ReportResult<SaleReportRow> SaleReport { get; private set; }
        public ReportDefinition<PurchaseReportRow> PurchaseDefinition { get; private set; }
        public ReportResult<PurchaseReportRow> PurchaseReport { get; private set; }
        public ReportDefinition<ProductReportRow> ProductDefinition { get; private set; }
        public ReportResult<ProductReportRow> ProductReport { get; private set; }
        public ReportDefinition<ProductAlertHistoryEntry> AlertHistoryDefinition { get; private set; }
        public ReportResult<ProductAlertHistoryEntry> AlertHistoryReport { get; private set; }

        public void LoadSupplierOptions(IReadOnlyList<ComboBoxItem> options) => SupplierOptions = options;
        public void LoadCategoryOptions(IReadOnlyList<ComboBoxItem> options) => CategoryOptions = options;

        public void SetSaleReport(ReportDefinition<SaleReportRow> definition, ReportResult<SaleReportRow> result)
        {
            SaleDefinition = definition;
            SaleReport = result;
        }

        public void SetPurchaseReport(ReportDefinition<PurchaseReportRow> definition, ReportResult<PurchaseReportRow> result)
        {
            PurchaseDefinition = definition;
            PurchaseReport = result;
        }

        public void SetProductReport(ReportDefinition<ProductReportRow> definition, ReportResult<ProductReportRow> result)
        {
            ProductDefinition = definition;
            ProductReport = result;
        }

        public void SetAlertHistoryReport(ReportDefinition<ProductAlertHistoryEntry> definition, ReportResult<ProductAlertHistoryEntry> result)
        {
            AlertHistoryDefinition = definition;
            AlertHistoryReport = result;
        }
    }
}
