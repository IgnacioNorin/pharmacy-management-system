using System;
using System.Collections.Generic;
using System.Data;
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

        void LoadSupplierOptions(IReadOnlyList<ComboBoxItem> options);
        void LoadCategoryOptions(IReadOnlyList<ComboBoxItem> options);

        void SetSaleReport(DataTable table);
        void SetPurchaseReport(DataTable table);
        void SetProductReport(DataTable table);
        void SetAlertHistoryReport(DataTable table);
    }
}
