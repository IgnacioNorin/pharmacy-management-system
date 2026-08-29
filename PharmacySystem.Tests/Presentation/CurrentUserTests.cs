using System;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class CurrentUserTests
    {
        private static Person Someone() => new Person
        {
            idPerson = 7,
            name = "Test",
            oPersonType = new TypePerson { idPersonType = 3 }
        };

        [Fact]
        public void Can_GrantedPermission_IsTrue_AndCaseInsensitive()
        {
            var user = new CurrentUser(Someone(), new[] { "ventas.acceso", "clientes.gestionar" });

            Assert.True(user.Can("ventas.acceso"));
            Assert.True(user.Can("Ventas.Acceso"));
        }

        [Fact]
        public void Can_MissingOrNullOrEmpty_IsFalse()
        {
            var user = new CurrentUser(Someone(), new[] { "ventas.acceso" });

            Assert.False(user.Can("productos.eliminar"));
            Assert.False(user.Can(null));
            Assert.False(user.Can(""));
        }

        [Fact]
        public void NullPermissions_DeniesEverythingWithoutThrowing()
        {
            var user = new CurrentUser(Someone(), null);

            Assert.False(user.Can("ventas.acceso"));
            Assert.Empty(user.Permissions);
        }

        [Fact]
        public void ExposesPersonIdAndRoleId()
        {
            var user = new CurrentUser(Someone(), new string[0]);

            Assert.Equal(7, user.PersonId);
            Assert.Equal(3, user.RoleId);
        }

        [Fact]
        public void NullPerson_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new CurrentUser(null, new string[0]));
        }
    }
}
