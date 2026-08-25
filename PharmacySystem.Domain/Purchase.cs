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
        public Person oPerson { get; set; }
        public Supplier oSupplier { get; set; }
        public decimal totalAmount { get; set; }
        public string documentType { get; set; }
        public string documentNumber { get; set; }
        public List<PurchaseDetail> oPurchaseDetail { get; set; }
    }
}
