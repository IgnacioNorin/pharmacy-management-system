using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeProductPriceView : IProductPriceView
    {
        public int SelectedProductId { get; set; }
        public string NewPriceText { get; set; } = "";
        public string Reason { get; set; } = "";

        public List<ProductPriceRow> Releasable { get; private set; }
        public List<ProductPriceRow> Commercialized { get; private set; }
        public List<ProductPriceHistoryRow> History { get; private set; }
        public bool EntryCleared { get; private set; }
        public List<string> ShownMessages { get; } = new List<string>();
        public List<string> ShownValidationErrors { get; private set; }

        public void LoadReleasable(IEnumerable<ProductPriceRow> rows) => Releasable = rows.ToList();
        public void LoadCommercialized(IEnumerable<ProductPriceRow> rows) => Commercialized = rows.ToList();
        public void LoadHistory(IEnumerable<ProductPriceHistoryRow> entries) => History = entries.ToList();
        public void ClearEntry() => EntryCleared = true;
        public void ShowMessage(string message) => ShownMessages.Add(message);
        public void ShowValidationErrors(IReadOnlyList<string> errors) => ShownValidationErrors = errors.ToList();
    }
}
