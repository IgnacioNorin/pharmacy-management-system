using System.Collections.Generic;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    internal class FakeCategoryService : ICategoryService
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public List<Categories> ListResult { get; set; } = new List<Categories>();

        public int Register(Categories obj) => RegisterResult;
        public bool Update(Categories obj) => UpdateResult;
        public List<Categories> List() => ListResult;
        public bool Delete(int idCategory) => DeleteResult;
    }
}
