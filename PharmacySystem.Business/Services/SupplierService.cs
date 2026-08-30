using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // No business rules beyond what the repository/stored procedures already enforce (the
    // document-uniqueness check lives in sp_create_supplier). This class stays thin on purpose:
    // suppliers genuinely don't have more rules today, and inventing some to look busy would
    // just be dead code. The seam is still worth it - it's where a real rule (credit limits,
    // approval workflow) would go without touching the repository or the presenter.
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _repository;

        public SupplierService(ISupplierRepository repository)
        {
            _repository = repository;
        }

        public int Register(Supplier obj) => _repository.Register(obj);

        public bool Update(Supplier obj) => _repository.Update(obj);

        public List<Supplier> List() => _repository.List();

        public PagedResult<Supplier> ListPaged(int pageNumber, int pageSize, string search) =>
            _repository.ListPaged(pageNumber, pageSize, search);

        public bool Delete(int idSupplier) => _repository.Delete(idSupplier);
    }
}
