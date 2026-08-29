using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeProductService : IProductService
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public bool VerifyResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public List<Product> ListResult { get; set; } = new List<Product>();
        public List<Product> ListSellableResult { get; set; }
        public List<ProductReportRow> ReportResult { get; set; } = new List<ProductReportRow>();

        public bool SetSalePriceResult { get; set; } = true;
        public bool UnreleaseResult { get; set; } = true;
        public List<ProductPriceHistoryEntry> PriceHistoryResult { get; set; } = new List<ProductPriceHistoryEntry>();

        public (int Id, decimal Price, string Reason, int? UserId)? SetSalePriceCall { get; private set; }
        public (int Id, string Reason, int? UserId)? UnreleaseCall { get; private set; }
        public int? PriceHistoryRequestedFor { get; private set; }

        public int Register(Product obj) => RegisterResult;
        public bool Update(Product obj) => UpdateResult;
        public List<Product> List() => ListResult;
        public List<Product> ListSellable() => ListSellableResult ?? ListResult;
        public Product GetSellableByCode(string code) => (ListSellableResult ?? ListResult).Find(p => p.code == code);
        public Product GetSellableById(int idProduct) => (ListSellableResult ?? ListResult).Find(p => p.idProduct == idProduct);
        public bool Verify(int idProduct) => VerifyResult;
        public bool Delete(int idProduct) => DeleteResult;
        public List<ProductReportRow> Report(string categoryId) => ReportResult;

        public bool SetSalePrice(int idProduct, decimal salePrice, string reason, int? userId)
        {
            SetSalePriceCall = (idProduct, salePrice, reason, userId);
            return SetSalePriceResult;
        }

        public bool Unrelease(int idProduct, string reason, int? userId)
        {
            UnreleaseCall = (idProduct, reason, userId);
            return UnreleaseResult;
        }

        public List<ProductPriceHistoryEntry> GetPriceHistory(int idProduct)
        {
            PriceHistoryRequestedFor = idProduct;
            return PriceHistoryResult;
        }
    }
}
