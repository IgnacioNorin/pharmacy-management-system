using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Tests.Presentation
{
    // Builds a CurrentUser with a given permission set for presenter tests. With() (no args) is a
    // user that can do nothing - used to test the "no tiene permiso" guards.
    internal static class TestUser
    {
        public static CurrentUser With(params string[] permissions) =>
            new CurrentUser(
                new Person { idPerson = 1, name = "Test", oPersonType = new TypePerson { idPersonType = 1, description = "Test" } },
                permissions);
    }
}
