using System.Collections.Generic;
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

        public bool Delete(int idSupplier)
        {
            DeletedId = idSupplier;
            return DeleteResult;
        }
    }
}
