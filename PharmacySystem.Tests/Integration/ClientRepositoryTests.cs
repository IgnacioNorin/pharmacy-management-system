using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PharmacySystem.Data;
using PharmacySystem.Model;
using Xunit;

namespace PharmacySystem.Tests.Integration
{
    // Clients were person rows with person_type_id = 4 until the person/client split (migration
    // 029). Same shape as SupplierRepositoryTests: register / update / paged list / soft-delete.
    [Collection("Database")]
    public class ClientRepositoryTests
    {
        private static readonly IClientRepository Repository = new ClientRepository(SqlConnectionFactory.FromConfiguration());

        private static Client NewClient(string document = null) => new Client
        {
            document = document ?? SqlTestHelper.NewTag(),
            name = "Test client",
            address = "Test address",
            phone = "0999999999"
        };

        [Fact]
        public void Register_New_IsListed()
        {
            int id = Repository.Register(NewClient());

            try
            {
                Assert.True(id > 0);
                Assert.Contains(Repository.ListClients(), c => c.idClient == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Register_DuplicateDocument_ReturnsZero()
        {
            string document = SqlTestHelper.NewTag();
            int firstId = Repository.Register(NewClient(document));

            try
            {
                Assert.Equal(0, Repository.Register(NewClient(document)));
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE id = @id", new SqlParameter("@id", firstId));
            }
        }

        [Fact]
        public void FiscalProfile_RoundTripsThroughRegisterAndUpdate()
        {
            var toRegister = NewClient();
            toRegister.businessName = "Comercial Ejemplo SpA";
            toRegister.activity = "Venta al por menor";
            toRegister.commune = "Providencia";
            toRegister.email = "contacto@ejemplo.cl";
            toRegister.isCompany = true;

            int id = Repository.Register(toRegister);

            try
            {
                Client stored = Repository.ListClients().Single(c => c.idClient == id);
                Assert.Equal("Comercial Ejemplo SpA", stored.businessName);
                Assert.Equal("Venta al por menor", stored.activity);
                Assert.Equal("Providencia", stored.commune);
                Assert.Equal("contacto@ejemplo.cl", stored.email);
                Assert.True(stored.isCompany);

                stored.businessName = "Otra Razon Ltda";
                stored.activity = "Servicios";
                stored.commune = "Nunoa";
                stored.email = "nuevo@ejemplo.cl";
                stored.isCompany = false;

                Assert.True(Repository.Update(stored));

                Client reread = Repository.ListClients().Single(c => c.idClient == id);
                Assert.Equal("Otra Razon Ltda", reread.businessName);
                Assert.Equal("Servicios", reread.activity);
                Assert.Equal("Nunoa", reread.commune);
                Assert.Equal("nuevo@ejemplo.cl", reread.email);
                Assert.False(reread.isCompany);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void ListClients_ExcludesDeactivated()
        {
            int id = Repository.Register(NewClient());

            try
            {
                Assert.Contains(Repository.ListClients(), c => c.idClient == id);

                SqlTestHelper.ExecuteNonQuery("UPDATE client SET status = 0 WHERE id = @id", new SqlParameter("@id", id));
                Assert.DoesNotContain(Repository.ListClients(), c => c.idClient == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void Delete_NotReferencedByASale_HardDeletesRow()
        {
            int id = Repository.Register(NewClient());

            bool result = Repository.Delete(id);

            Assert.True(result);
            Assert.Null(SqlTestHelper.ExecuteScalar("SELECT id FROM client WHERE id = @id", new SqlParameter("@id", id)));
        }

        [Fact]
        public void Delete_ReferencedByASale_SoftDeletesAndDropsFromTheList()
        {
            int id = Repository.Register(NewClient());
            SqlTestHelper.ExecuteNonQuery(
                "INSERT INTO sale(client_id, amount_received) VALUES (@c, 0)", new SqlParameter("@c", id));
            int saleId = SqlTestHelper.ExecuteScalarInt("SELECT MAX(id) FROM sale WHERE client_id = @c", new SqlParameter("@c", id));

            try
            {
                bool result = Repository.Delete(id);

                Assert.True(result);
                Assert.Equal(0, SqlTestHelper.ExecuteScalarInt("SELECT status FROM client WHERE id = @id", new SqlParameter("@id", id)));
                Assert.DoesNotContain(Repository.ListClients(), c => c.idClient == id);
            }
            finally
            {
                SqlTestHelper.ExecuteNonQuery("DELETE FROM sale WHERE id = @id", new SqlParameter("@id", saleId));
                SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE id = @id", new SqlParameter("@id", id));
            }
        }

        [Fact]
        public void ListClientsPaged_SlicesTheResult_ReportsTheTotal_AndFiltersBySearch()
        {
            string tag = SqlTestHelper.NewTag();
            var ids = new List<int>();

            try
            {
                for (int i = 0; i < 5; i++)
                {
                    var c = NewClient();
                    c.name = tag + " client " + i;
                    ids.Add(Repository.Register(c));
                }

                PagedResult<Client> page1 = Repository.ListClientsPaged(1, 2, tag);
                Assert.Equal(5, page1.TotalCount);
                Assert.Equal(2, page1.Items.Count);
                Assert.Equal(3, page1.TotalPages);

                Assert.Single(Repository.ListClientsPaged(3, 2, tag).Items);
                Assert.Equal(0, Repository.ListClientsPaged(1, 10, "missing-" + tag).TotalCount);
            }
            finally
            {
                foreach (int id in ids)
                {
                    SqlTestHelper.ExecuteNonQuery("DELETE FROM client WHERE id = @id", new SqlParameter("@id", id));
                }
            }
        }
    }
}
