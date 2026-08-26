using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class PurchaseDetail
    {
        public int idPurchaseDetail { get; set; }
        public Product oProduct { get; set; }
        public int quantity { get; set; }
        public DateTime expirationDate { get; set; }
        public decimal purchasePrice { get; set; }
        public decimal salePrice { get; set; }
        public decimal total { get; set; }
    }
}
