using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Arqueo de caja: on load, asks the service for the current open period and the expected
    // total per payment method; the user types what was physically counted and registers it.
    // Sales are never touched - this only writes a cash_count record for audit.
    public class CashCountPresenter
    {
        private readonly ICashCountView _view;
        private readonly ICashCountService _service;
        private readonly CurrentUser _currentUser;

        private const string Permission = "caja.acceso";

        private CashCount? _current;

        public CashCountPresenter(ICashCountView view, ICashCountService service, CurrentUser currentUser)
        {
            _view = view;
            _service = service;
            _currentUser = currentUser;
        }

        public void OnLoad()
        {
            _current = _service.PrepareCurrent();

            _view.ShowPeriod(_current.periodStart, _current.periodEnd);
            _view.ShowLines(_current.lines.Select(l => new CashCountRow
            {
                PaymentMethod = l.paymentMethod,
                Expected = l.expectedAmount,
                Counted = 0m
            }).ToList());

            RecomputeTotals();
        }

        // Called while the user edits the counted fields, to keep the totals line live.
        public void OnCountedChanged() => RecomputeTotals();

        public void OnRegister()
        {
            if (_current == null)
            {
                return;
            }

            if (!(_currentUser?.Can(Permission) ?? false))
            {
                _view.ShowMessage("No tiene permiso para registrar el arqueo de caja.");
                return;
            }

            if (!TryReadCounted(out List<CashCountLine> lines, out string error))
            {
                _view.ShowMessage(error);
                return;
            }

            var cashCount = new CashCount
            {
                periodStart = _current.periodStart,
                periodEnd = _current.periodEnd,
                userId = _currentUser?.PersonId,
                notes = string.IsNullOrWhiteSpace(_view.Notes) ? string.Empty : _view.Notes.Trim(),
                lines = lines
            };

            int id = _service.Register(cashCount);
            if (id == 0)
            {
                _view.ShowMessage("No se pudo registrar el arqueo. Intente nuevamente.");
                return;
            }

            _view.ShowMessage("Arqueo registrado.");
            _view.CountRegistered();
        }

        private void RecomputeTotals()
        {
            if (_current == null)
            {
                return;
            }

            decimal expected = _current.lines.Sum(l => l.expectedAmount);
            decimal counted = 0m;

            foreach (CashCountLine line in _current.lines)
            {
                if (TryParseAmount(_view.GetCountedText(line.paymentMethod), out decimal amount))
                {
                    counted += amount;
                }
            }

            _view.ShowTotals(expected, counted, counted - expected);
        }

        private bool TryReadCounted(out List<CashCountLine> lines, out string error)
        {
            lines = new List<CashCountLine>();
            error = string.Empty;

            foreach (CashCountLine source in _current!.lines)
            {
                string text = _view.GetCountedText(source.paymentMethod);

                if (!TryParseAmount(text, out decimal counted))
                {
                    error = $"El monto contado de {source.paymentMethod} no es un número válido.";
                    return false;
                }

                if (counted < 0m)
                {
                    error = $"El monto contado de {source.paymentMethod} no puede ser negativo.";
                    return false;
                }

                lines.Add(new CashCountLine
                {
                    paymentMethod = source.paymentMethod,
                    expectedAmount = source.expectedAmount,
                    countedAmount = counted
                });
            }

            return true;
        }

        // A blank field counts as 0. Accepts both "1,234.50" and "1234,50" style separators by
        // trying invariant first, then the current culture.
        private static bool TryParseAmount(string text, out decimal amount)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                amount = 0m;
                return true;
            }

            text = text.Trim();
            return decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount)
                   || decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);
        }
    }
}
