using System.Collections.Generic;
using PharmacySystem.Data;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    // Thin like SupplierService: the dedupe/reactivate rule for categories lives inside
    // sp_create_category and sp_delete_category, not here.
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public int Register(Categories obj) => _repository.Register(obj);

        public bool Update(Categories obj) => _repository.Update(obj);

        public List<Categories> List() => _repository.List();

        public bool Delete(int idCategory) => _repository.Delete(idCategory);
    }
}
