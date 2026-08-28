using System;
using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    public interface IReportView
    {
        DateTime SaleStartDate { get; }
        DateTime SaleEndDate { get; }
        DateTime PurchaseStartDate { get; }
        DateTime PurchaseEndDate { get; }
        DateTime AlertHistoryStartDate { get; }
        DateTime AlertHistoryEndDate { get; }
        string SelectedSupplierId { get; }
        string SelectedCategoryId { get; }
        string SelectedSaleClientId { get; }

        void LoadSupplierOptions(IReadOnlyList<ComboBoxItem> options);
        void LoadCategoryOptions(IReadOnlyList<ComboBoxItem> options);
        void LoadSaleClientOptions(IReadOnlyList<ComboBoxItem> options);

        void SetSaleReport(ReportDefinition<SaleReportRow> definition, ReportResult<SaleReportRow> result);
        void SetPurchaseReport(ReportDefinition<PurchaseReportRow> definition, ReportResult<PurchaseReportRow> result);
        void SetProductReport(ReportDefinition<ProductReportRow> definition, ReportResult<ProductReportRow> result);
        void SetAlertHistoryReport(ReportDefinition<ProductAlertHistoryEntry> definition, ReportResult<ProductAlertHistoryEntry> result);
    }
}
