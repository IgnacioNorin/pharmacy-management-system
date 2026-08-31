using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // frmCreditNote: look a sale up by document type + number, choose how many units of each line
    // to credit, then issue a Nota de Credito (negative document + stock restored) for that part.
    // A sale can be credited across several notes until every line is fully credited.
    public class CreditNotePresenter
    {
        private readonly ICreditNoteView _view;
        private readonly ISaleService _service;
        private readonly CurrentUser _currentUser;
        private readonly int _currentPersonId;

        private int? _selectedSaleId;

        public CreditNotePresenter(ICreditNoteView view, ISaleService service, CurrentUser currentUser, int currentPersonId)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
            _currentPersonId = currentPersonId;
        }

        private bool Can(string permission) => _currentUser?.Can(permission) ?? false;

        public void OnLoad()
        {
            _view.SetDocumentTypeOptions(DocumentTypes.Selectable);
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
            if (sale.FullyCreditNoted)
            {
                _view.ShowMessage("El comprobante ya fue acreditado por completo.");
                return;
            }

            _view.ShowCreditableLines(_service.GetCreditableLines(sale.Id));

            if (sale.AlreadyCreditNoted)
            {
                _view.ShowMessage("El comprobante ya tiene una nota de crédito parcial. Puede acreditar el resto.");
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

            List<CreditNoteLineRequest> lines = (_view.GetRequestedQuantities() ?? new List<CreditNoteLineRequest>())
                .Where(l => l.Quantity > 0)
                .ToList();
            if (lines.Count == 0)
            {
                _view.ShowMessage("Indique cuántas unidades acreditar en al menos una línea.");
                return;
            }

            if (!_view.ConfirmGenerate())
            {
                return;
            }

            switch (_service.CreateCreditNote(_selectedSaleId.Value, _currentPersonId, reason, lines))
            {
                case CreditNoteResult.Ok:
                    _view.ShowMessage("Nota de crédito emitida. Se devolvió el stock.");
                    InventoryChangeNotifier.NotifyStockChanged();
                    _view.CreditNoteCompleted();
                    break;
                case CreditNoteResult.NothingToCredit:
                    _view.ShowMessage("Indique cuántas unidades acreditar en al menos una línea.");
                    break;
                case CreditNoteResult.QuantityExceedsRemaining:
                    _view.ShowMessage("Una de las cantidades supera lo que queda por acreditar de esa línea. Vuelva a buscar el comprobante.");
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
