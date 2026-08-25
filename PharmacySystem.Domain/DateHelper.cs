using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Helpers
{
    public static class DateHelper
    {
        public static string FormatDatePresentation(DateTime date)
        {
            return date.ToString("dd-MM-yyyy");
        }

        public static string FormatDateBackend(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }
    }
}
