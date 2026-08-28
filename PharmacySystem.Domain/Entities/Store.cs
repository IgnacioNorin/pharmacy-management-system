using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class Store
    {
        public int idStore { get; set; }
        public string document { get; set; }
        public string companyName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }

        public string address { get; set; }
        public string currencyCulture { get; set; }
        // VAT rate applied to tax-affected items, as a percentage (e.g. 19.00). Country-neutral:
        // Chile is 19, but it is a setting, not a constant.
        public decimal defaultTaxRate { get; set; } = 19m;
    }
}
