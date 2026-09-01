using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class Purchase
    {
        public int idPurchase { get; set; }
        public Person? oPerson { get; set; }
        public Supplier? oSupplier { get; set; }
        public decimal totalAmount { get; set; }
        // VAT breakdown of the invoice. Line prices are entered VAT-included, so the net is
        // backed out (net = total / (1 + taxRate/100)) and taxRate is captured at purchase time.
        public decimal netAmount { get; set; }
        public decimal taxAmount { get; set; }
        public decimal exemptAmount { get; set; }
        public decimal taxRate { get; set; }
        public string documentType { get; set; } = string.Empty;
        public string documentNumber { get; set; } = string.Empty;
        public List<PurchaseDetail> oPurchaseDetail { get; set; } = new List<PurchaseDetail>();
    }
}
