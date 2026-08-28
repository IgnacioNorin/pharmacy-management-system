using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class Product
    {
        public int idProduct { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Categories oCategory { get; set; }
        public int stock { get; set; }
        public decimal  purchasePrice { get; set; }
        public decimal  salePrice{ get; set; }
        public DateTime expirationDate { get; set; }
        // true = the sale price is affected by VAT (the default); false = VAT-exempt item.
        public bool taxAffected { get; set; } = true;

    }
}
