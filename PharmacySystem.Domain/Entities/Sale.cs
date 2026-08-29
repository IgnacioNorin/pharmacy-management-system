using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class Sale
    {
        public int idSale { get; set; }
        public string typeDocument { get; set; }
        public string numberDocument { get; set; }
        public Person oPerson { get; set; }
        public string documentClient { get; set; }
        public string nameClient { get; set; }
        public decimal totalPay { get; set; }
        public decimal payWith { get; set; }
        public decimal change { get; set; }
        // VAT breakdown of totalPay: netAmount + taxAmount + exemptAmount == totalPay.
        public decimal netAmount { get; set; }
        public decimal taxAmount { get; set; }
        public decimal exemptAmount { get; set; }
        // Recipient fiscal data - only filled when typeDocument is a Factura.
        public string recipientTaxId { get; set; }
        public string recipientBusinessName { get; set; }
        public string recipientActivity { get; set; }
        public string recipientAddress { get; set; }
        public string recipientCommune { get; set; }
        // The client this sale was made to (a person of type Cliente). Null for a walk-in /
        // consumidor final, and for every sale registered before the link existed.
        public int? clientId { get; set; }
        // Set on a Nota de Credito: the id of the sale it reverses, plus the reason.
        public int? referenceId { get; set; }
        public string referenceReason { get; set; }
        // Fiscal issuance status. Defaults to "interno": the receipt is numbered by the local
        // sequence and no tax authority is contacted. A DTE-provider issuer fills trackId /
        // barcode and moves the status to pendiente / aceptado / rechazado.
        public string fiscalStatus { get; set; } = "interno";
        public string fiscalTrackId { get; set; }
        public string fiscalBarcode { get; set; }
        public DateTime registrationDate { get; set; }
        public List<SaleDetail> oSaleDetail { get; set; }
    }
}
