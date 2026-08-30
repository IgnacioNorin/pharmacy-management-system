using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Model;

namespace PharmacySystem.Tests.Presentation
{
    // Hand-written test double instead of a mocking library: with one interface and four
    // methods, a fake is less code than learning/reading a Moq setup, and it's plain C# anyone
    // touching this test can follow without knowing the library.
    internal class FakeSupplierService : ISupplierService
    {
        public int RegisterResult { get; set; } = 1;
        public bool UpdateResult { get; set; } = true;
        public bool DeleteResult { get; set; } = true;
        public List<Supplier> ListResult { get; set; } = new List<Supplier>();

        public Supplier RegisteredWith { get; private set; }
        public Supplier UpdatedWith { get; private set; }
        public int? DeletedId { get; private set; }
        public bool ListCalled { get; private set; }

        public int Register(Supplier obj)
        {
            RegisteredWith = obj;
            return RegisterResult;
        }

        public bool Update(Supplier obj)
        {
            UpdatedWith = obj;
            return UpdateResult;
        }

        public List<Supplier> List()
        {
            ListCalled = true;
            return ListResult;
        }

        public (int Page, int PageSize, string Search)? LastPagedCall { get; private set; }

        // Pages over ListResult in memory, applying the same company/document/email text match
        // the real query does.
        public PagedResult<Supplier> ListPaged(int pageNumber, int pageSize, string search)
        {
            LastPagedCall = (pageNumber, pageSize, search);

            string term = (search ?? string.Empty).Trim();
            List<Supplier> matches = string.IsNullOrEmpty(term)
                ? ListResult
                : ListResult.Where(x =>
                    (x.companyName ?? "").Contains(term) ||
                    (x.document ?? "").Contains(term) ||
                    (x.email ?? "").Contains(term)).ToList();

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = PagedResult<Supplier>.DefaultPageSize;

            return new PagedResult<Supplier>
            {
                Items = matches.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
                TotalCount = matches.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public bool Delete(int idSupplier)
        {
            DeletedId = idSupplier;
            return DeleteResult;
        }
    }
}
