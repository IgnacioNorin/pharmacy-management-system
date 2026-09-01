using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Model
{
    public class Person
    {
        public int idPerson { get; set; }
        public string document { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public TypePerson? oPersonType { get; set; }
        public bool Estado { get; set; }
        // When true, the next successful login forces a password change before the app opens.
        public bool mustChangePassword { get; set; }
    }
}
