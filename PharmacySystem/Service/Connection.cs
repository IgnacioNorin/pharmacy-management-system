using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Logical
{
    public class Connection
    {
        public static string CN = ConfigurationManager.ConnectionStrings["connection"].ToString();
    }
}
