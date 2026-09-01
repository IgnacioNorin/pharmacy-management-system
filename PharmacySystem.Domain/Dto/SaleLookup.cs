using System;

namespace PharmacySystem.Model
{
    // What frmCreditNote shows after looking a sale up by document type + number.
    public class SaleLookup
    {
        public int Id { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        // true if at least one Nota de Credito already references this sale (fully or partially).
        public bool AlreadyCreditNoted { get; set; }
        // true if every line of this sale has been credited in full - nothing left to credit.
        public bool FullyCreditNoted { get; set; }
        // true if this row is itself a Nota de Credito.
        public bool IsCreditNote { get; set; }
    }

    // One line of a sale as offered on the credit-note screen: how many units were sold, how many
    // have already been credited across earlier notes, and therefore how many remain.
    public class SaleCreditDetail
    {
        public int SourceDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public bool TaxAffected { get; set; }
        public int SoldQuantity { get; set; }
        public int CreditedQuantity { get; set; }

        public int RemainingQuantity => SoldQuantity - CreditedQuantity;
    }

    // A request to credit a given number of units of one original sale line.
    public class CreditNoteLineRequest
    {
        public int SourceDetailId { get; set; }
        public int Quantity { get; set; }
    }

    public enum CreditNoteResult
    {
        Ok,
        NotFound,
        AlreadyCreditNoted,
        NotAllowedOnCreditNote,
        // No line was asked to be credited (every requested quantity was zero).
        NothingToCredit,
        // A requested quantity is above what is still creditable for that line.
        QuantityExceedsRemaining,
        Error
    }
}
