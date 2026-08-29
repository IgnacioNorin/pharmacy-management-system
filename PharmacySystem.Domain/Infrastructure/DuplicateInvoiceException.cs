using System;

namespace PharmacySystem.Infrastructure
{
    // Raised by PurchaseRepository.Register when the (supplier, document type, document number)
    // of the purchase already exists. The unique index UX_purchase_supplier_document stops the
    // second registration at the database; this exception lets the presenter tell the user the
    // invoice was already recorded instead of showing a generic "could not register" message.
    public class DuplicateInvoiceException : Exception
    {
        public const string DefaultMessage =
            "Esa factura de ese proveedor ya fue registrada.";

        public DuplicateInvoiceException()
            : base(DefaultMessage) { }

        public DuplicateInvoiceException(string message)
            : base(message) { }

        public DuplicateInvoiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
