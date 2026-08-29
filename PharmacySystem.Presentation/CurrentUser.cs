using System;
using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // The signed-in user plus the flat set of permission codes resolved from their role at
    // login. Immutable for the lifetime of the session; presenters call Can(...) before running
    // a guarded action and views use it to hide or disable UI.
    public class CurrentUser
    {
        private readonly HashSet<string> _permissions;

        public Person Person { get; }

        public CurrentUser(Person person, IEnumerable<string> permissions)
        {
            Person = person ?? throw new ArgumentNullException(nameof(person));
            _permissions = new HashSet<string>(
                permissions ?? Enumerable.Empty<string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        public int PersonId => Person.idPerson;

        public int RoleId => Person.oPersonType?.idPersonType ?? 0;

        public IReadOnlyCollection<string> Permissions => _permissions;

        public bool Can(string permissionCode) =>
            !string.IsNullOrEmpty(permissionCode) && _permissions.Contains(permissionCode);
    }
}
