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

        public int Register(Product obj) => RegisterResult;
        public bool Update(Product obj) => UpdateResult;
        public List<Product> List() => ListResult;
        public bool Verify(int idProduct) => VerifyResult;
        public bool Delete(int idProduct) => DeleteResult;
    }
}
