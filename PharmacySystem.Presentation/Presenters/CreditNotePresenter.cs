using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // frmCreditNote: look a sale up by document type + number, then issue a Nota de Credito that
    // reverses it (negative document + stock restored). This is the "anular venta" flow.
    public class CreditNotePresenter
    {
        private readonly ICreditNoteView _view;
        private readonly ISaleService _service;
        private readonly IStoreService _storeService;
        private readonly CurrentUser _currentUser;
        private readonly int _currentPersonId;

        private int? _selectedSaleId;

        public CreditNotePresenter(ICreditNoteView view, ISaleService service, IStoreService storeService, CurrentUser currentUser, int currentPersonId)
        {
            _view = view;
            _service = service;
            _storeService = storeService;
            _currentUser = currentUser;
            _currentPersonId = currentPersonId;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            _view.SetDocumentTypeOptions(CountryPresets.ForCode(_storeService.ListStore()?.countryCode).SaleDocumentTypes);
            _view.SetGenerateEnabled(false);
        }

        public void OnSearch()
        {
            _selectedSaleId = null;
            _view.SetGenerateEnabled(false);

            string type = (_view.DocumentTypeInput ?? "").Trim();
            string number = (_view.DocumentNumberInput ?? "").Trim();

            if (number.Length == 0)
            {
                _view.ShowMessage("Ingrese el número de comprobante.");
                _view.ClearSale();
                return;
            }

            SaleLookup sale = _service.FindByDocument(type, number);
            if (sale == null)
            {
                _view.ShowMessage("No se encontró el comprobante.");
                _view.ClearSale();
                return;
            }

            _view.ShowSale(sale);

            if (sale.IsCreditNote)
            {
                _view.ShowMessage("El comprobante seleccionado es una nota de crédito.");
                return;
            }
            if (sale.AlreadyCreditNoted)
            {
                _view.ShowMessage("El comprobante ya tiene una nota de crédito emitida.");
                return;
            }

            _selectedSaleId = sale.Id;
            _view.SetGenerateEnabled(true);
        }

        public void OnGenerate()
        {
            if (!Can("ventas.nota_credito"))
            {
                _view.ShowMessage("No tiene permiso para emitir notas de crédito.");
                return;
            }
            if (_selectedSaleId == null)
            {
                _view.ShowMessage("Busque primero un comprobante.");
                return;
            }

            string reason = (_view.ReasonInput ?? "").Trim();
            if (reason.Length == 0)
            {
                _view.ShowMessage("Ingrese el motivo de la nota de crédito.");
                return;
            }

            if (!_view.ConfirmGenerate())
            {
                return;
            }

            switch (_service.CreateCreditNote(_selectedSaleId.Value, _currentPersonId, reason))
            {
                case CreditNoteResult.Ok:
                    _view.ShowMessage("Nota de crédito emitida. Se devolvió el stock.");
                    InventoryChangeNotifier.NotifyStockChanged();
                    _view.CreditNoteCompleted();
                    break;
                case CreditNoteResult.AlreadyCreditNoted:
                    _view.ShowMessage("El comprobante ya tiene una nota de crédito emitida.");
                    break;
                case CreditNoteResult.NotFound:
                    _view.ShowMessage("No se encontró el comprobante.");
                    break;
                case CreditNoteResult.NotAllowedOnCreditNote:
                    _view.ShowMessage("No se puede anular una nota de crédito.");
                    break;
                default:
                    _view.ShowMessage("No se pudo emitir la nota de crédito.");
                    break;
            }
        }
    }
}
