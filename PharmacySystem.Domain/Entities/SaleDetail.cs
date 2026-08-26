using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class SaleDetail
    {
        public int idSaleDetail { get; set; }
        public int idSale { get; set; }
        public Product oProduct { get; set; }
        public int amount { get; set; }
        public decimal salePrice { get; set; }
        public decimal subtotal { get; set; }
    }
}
