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
        public string code { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Categories? oCategory { get; set; }
        public int stock { get; set; }
        public decimal  purchasePrice { get; set; }
        // Moving weighted average cost, recomputed on every purchase. 0 until the first purchase.
        public decimal  averageCost { get; set; }
        public decimal  salePrice{ get; set; }
        public DateTime expirationDate { get; set; }
        // true = the sale price is affected by VAT (the default); false = VAT-exempt item.
        public bool taxAffected { get; set; } = true;
        // false = in stock but not for sale yet; true = released for sale from the Prices screen.
        public bool isReleased { get; set; }

    }
}
