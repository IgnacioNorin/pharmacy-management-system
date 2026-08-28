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
        // Set on a Nota de Credito: the id of the sale it reverses, plus the reason.
        public int? referenceId { get; set; }
        public string referenceReason { get; set; }
        public DateTime registrationDate { get; set; }
        public List<SaleDetail> oSaleDetail { get; set; }
    }
}
