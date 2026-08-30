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

        public PurchaseReportTotals GetTotals(string idSupplier, DateTime startDate, DateTime endDate) =>
            _repository.GetTotals(idSupplier, startDate, endDate);
    }
}
