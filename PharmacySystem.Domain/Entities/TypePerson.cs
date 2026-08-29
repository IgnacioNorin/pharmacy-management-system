using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class TypePerson
    {
        public int idPersonType { get; set; }
        public string description { get; set; }

        // person_type.is_system: 1 for the four built-in roles, which the roles admin screen
        // must not let be renamed or deleted. Only populated where a query selects it.
        public bool IsSystem { get; set; }
    }
}
