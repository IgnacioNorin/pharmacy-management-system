using System.Collections.Generic;
using PharmacySystem.Model;

namespace PharmacySystem.Ui
{
    // Everything PrintSaleWindow needs for one sale's ticket, resolved by the exe (which owns the
    // sale services and the HTML template resource) and handed in. Sale is null when the id does
    // not resolve.
    public sealed class PrintTicketData
    {
        public required Store Store { get; set; }
        public Sale? Sale { get; set; }
        public List<SaleDetail>? Details { get; set; }
        public required string HtmlTemplate { get; set; }
    }
}
