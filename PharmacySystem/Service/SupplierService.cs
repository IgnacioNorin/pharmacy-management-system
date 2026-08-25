using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Logical
{
    // Thin adapter kept only for the screens not migrated yet (ModalSupplier.cs, frmReport.cs),
    // which still call SupplierService.Instance.ListSupplier(). It no longer talks to SQL
    // itself - it delegates to the real implementation in PharmacySystem.Business, which is what
    // frmSupplier now uses directly through SupplierPresenter. Delete this class once nothing
    // calls .Instance anymore.
    public class SupplierService
    {
        private static SupplierService instance = null;
        private readonly Business.ISupplierService _inner;

        public SupplierService()
        {
            _inner = new Business.SupplierService(new SupplierRepository(CompositionRoot.ConnectionFactory));
        }

        public static SupplierService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SupplierService();
                }

                return instance;
            }
        }

        public int RegisterSupplier(Supplier obj) => _inner.Register(obj);

        public bool UpdateSupplier(Supplier obj) => _inner.Update(obj);

        public List<Supplier> ListSupplier() => _inner.List();

        public bool DeleteSupplier(int idSupplier) => _inner.Delete(idSupplier);
    }
}
