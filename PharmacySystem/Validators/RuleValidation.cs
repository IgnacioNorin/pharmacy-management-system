using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Validators
{
    public class RuleValidation
    {
        public Func<string, bool> Validate { get; set; }
        public string MessageError { get; set; }

        public RuleValidation(Func<string, bool> validate, string messageError)
        {
            Validate = validate;
            MessageError = messageError;
        }
    }
}
