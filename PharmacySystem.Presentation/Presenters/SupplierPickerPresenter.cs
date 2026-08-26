using PharmacySystem.Business;

namespace PharmacySystem.Presentation
{
    public class SupplierPickerPresenter
    {
        private readonly ISupplierPickerView _view;
        private readonly ISupplierService _service;

        public SupplierPickerPresenter(ISupplierPickerView view, ISupplierService service)
        {
            _view = view;
            _service = service;
        }

        public void OnLoad()
        {
            _view.LoadSuppliers(_service.List().ConvertAll(SupplierRow.From));
        }
    }
}
