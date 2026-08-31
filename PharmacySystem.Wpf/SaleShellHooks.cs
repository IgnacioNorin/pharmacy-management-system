using System;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // Everything the WPF sale window needs from the (still WinForms) shell and that it cannot
    // build itself: the sub-picker factories, the credit-note presenter factory, the "print
    // ticket" action (PrintSale lives in the exe), and whether the current user may issue
    // credit notes.
    public sealed class SaleShellHooks
    {
        public PickerFactories Pickers { get; }
        public Func<ICreditNoteView, CreditNotePresenter> CreditNoteFactory { get; }
        public Action<int> PrintTicket { get; }
        public bool CanCreditNote { get; }

        public SaleShellHooks(
            PickerFactories pickers,
            Func<ICreditNoteView, CreditNotePresenter> creditNoteFactory,
            Action<int> printTicket,
            bool canCreditNote)
        {
            Pickers = pickers;
            CreditNoteFactory = creditNoteFactory;
            PrintTicket = printTicket;
            CanCreditNote = canCreditNote;
        }
    }
}
