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
        // VAT rate applied to tax-affected items, as a percentage. Chile is 19; kept as a
        // setting so an edge case can adjust it. The system is CLP-only (no currency setting).
        public decimal defaultTaxRate { get; set; } = 19m;
        // Document type pre-selected on the sale screen.
        public string defaultDocumentType { get; set; } = DocumentTypes.Boleta;
    }
}
