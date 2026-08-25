using System;
using System.Collections.Generic;
using System.Data;
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
        public DataTable SaleReport { get; private set; }
        public DataTable PurchaseReport { get; private set; }
        public DataTable ProductReport { get; private set; }
        public DataTable AlertHistoryReport { get; private set; }

        public void LoadSupplierOptions(IReadOnlyList<ComboBoxItem> options) => SupplierOptions = options;
        public void LoadCategoryOptions(IReadOnlyList<ComboBoxItem> options) => CategoryOptions = options;
        public void SetSaleReport(DataTable table) => SaleReport = table;
        public void SetPurchaseReport(DataTable table) => PurchaseReport = table;
        public void SetProductReport(DataTable table) => ProductReport = table;
        public void SetAlertHistoryReport(DataTable table) => AlertHistoryReport = table;
    }
}
