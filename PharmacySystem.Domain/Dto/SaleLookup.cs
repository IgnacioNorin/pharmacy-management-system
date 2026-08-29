using System;

namespace PharmacySystem.Model
{
    // What frmCreditNote shows after looking a sale up by document type + number.
    public class SaleLookup
    {
        public int Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime Date { get; set; }
        public string ClientName { get; set; }
        public decimal TotalAmount { get; set; }
        // true if a Nota de Credito already references this sale.
        public bool AlreadyCreditNoted { get; set; }
        // true if this row is itself a Nota de Credito.
        public bool IsCreditNote { get; set; }
    }

    public enum CreditNoteResult
    {
        Ok,
        NotFound,
        AlreadyCreditNoted,
        NotAllowedOnCreditNote,
        Error
    }
}
