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
    }
}
