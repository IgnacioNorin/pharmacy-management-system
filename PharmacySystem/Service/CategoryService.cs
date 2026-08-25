using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Logical
{
    // Thin adapter kept for screens not migrated yet (frmManagement.cs, frmReport.cs). Delegates
    // to PharmacySystem.Business; delete once nothing calls .Instance anymore.
    public class CategoryService
    {
        private static CategoryService instance = null;
        private readonly Business.ICategoryService _inner;

        public CategoryService()
        {
            _inner = new Business.CategoryService(new CategoryRepository(CompositionRoot.ConnectionFactory));
        }

        public static CategoryService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CategoryService();
                }

                return instance;
            }
        }

        public int RegisterCategory(Categories obj) => _inner.Register(obj);

        public bool UpdateCategory(Categories obj) => _inner.Update(obj);

        public List<Categories> ListCategory() => _inner.List();

        public bool DeleteCategory(int idCategory) => _inner.Delete(idCategory);
    }
}
