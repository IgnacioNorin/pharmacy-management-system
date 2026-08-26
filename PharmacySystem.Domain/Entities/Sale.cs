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
        public DateTime registrationDate { get; set; }
        public List<SaleDetail> oSaleDetail { get; set; }
    }
}
