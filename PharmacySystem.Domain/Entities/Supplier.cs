using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class Supplier
    {
        public int idSupplier { get; set; }
        public string document { get; set; } = string.Empty;
        public string companyName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public bool state { get; set; }
    }
}
