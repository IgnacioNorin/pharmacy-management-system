using PharmacySystem.Model;
using Xunit;
using BusinessClientService = PharmacySystem.Business.ClientService;

namespace PharmacySystem.Tests.Business
{
    // ClientService is a thin passthrough (clients have no password to hash). These tests just
    // pin that every method reaches the repository unchanged.
    public class ClientServiceTests
    {
        [Fact]
        public void Register_DelegatesToRepository()
        {
            var repository = new FakeClientRepository { RegisterResult = 42 };
            var service = new BusinessClientService(repository);

            int id = service.Register(new Client { name = "Ana" });

            Assert.Equal(42, id);
            Assert.Equal("Ana", repository.RegisteredWith.name);
        }

        [Fact]
        public void Update_DelegatesToRepository()
        {
            var repository = new FakeClientRepository { UpdateResult = true };
            var service = new BusinessClientService(repository);

            Assert.True(service.Update(new Client { idClient = 3, name = "Bruno" }));
            Assert.Equal(3, repository.UpdatedWith.idClient);
        }

        [Fact]
        public void ListClientsPaged_DelegatesWithTheSameArguments()
        {
            var repository = new FakeClientRepository();
            var service = new BusinessClientService(repository);

            service.ListClientsPaged(2, 25, "ana");

            Assert.Equal((2, 25, "ana"), repository.LastPagedCall);
        }

        [Fact]
        public void Delete_DelegatesToRepository()
        {
            var repository = new FakeClientRepository { DeleteResult = true };
            var service = new BusinessClientService(repository);

            Assert.True(service.Delete(9));
            Assert.Equal(9, repository.DeletedId);
        }
    }
}
