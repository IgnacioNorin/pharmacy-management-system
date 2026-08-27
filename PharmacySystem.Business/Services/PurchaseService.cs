using System;
using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin: the atomicity of the purchase + its detail rows + the stock update is a persistence
    // concern (the SQL transaction), not a business decision, so it stays entirely inside the
    // repository. Nothing here branches on it.
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _repository;

        public PurchaseService(IPurchaseRepository repository)
        {
            _repository = repository;
        }

        public bool Register(Purchase purchase) => _repository.Register(purchase);

        public List<PurchaseReportRow> ReportPurchase(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.ReportPurchase(idSupplier, startDate, endDate);

        public decimal GetTotalAmount(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.GetTotalAmount(idSupplier, startDate, endDate);

        public decimal GetTotalPurchasePrice(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.GetTotalPurchasePrice(idSupplier, startDate, endDate);

        public int GetTotalQuantity(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.GetTotalQuantity(idSupplier, startDate, endDate);

        public decimal GetTotalSalesPrice(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.GetTotalSalesPrice(idSupplier, startDate, endDate);

        public decimal GetSubTotal(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.GetSubTotal(idSupplier, startDate, endDate);
    }
}
