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
        public string document { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string phone { get; set; }
        // Optional fiscal profile, used when the person is a client on a Factura.
        // Null for users, suppliers and boleta-only clients.
        public string businessName { get; set; }
        public string activity { get; set; }
        public string commune { get; set; }
        public string email { get; set; }
        public bool isCompany { get; set; }
        public string password { get; set; }
        public TypePerson oPersonType { get; set; }
        public bool Estado { get; set; }
    }
}
