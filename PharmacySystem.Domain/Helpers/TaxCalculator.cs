using System;
using System.Collections.Generic;

namespace PharmacySystem.Helpers
{
    // VAT breakdown for a sale. Prices/subtotals are stored VAT-included (retail convention), so
    // for the tax-affected part the net is backed out: net = round(gross / (1 + rate/100)).
    // Country-neutral: the rate is a parameter (Chile 19, Peru 18, ...). Amounts round to the
    // whole currency unit (CLP and most LATAM currencies have no cents in practice).
    public static class TaxCalculator
    {
        public struct Breakdown
        {
            public decimal Net;
            public decimal Tax;
            public decimal Exempt;
            public decimal Total;
        }

        public static Breakdown Compute(IEnumerable<(decimal SubTotal, bool TaxAffected)> lines, decimal ratePercent)
        {
            decimal affectedGross = 0m;
            decimal exempt = 0m;

            foreach (var line in lines)
            {
                if (line.TaxAffected)
                {
                    affectedGross += line.SubTotal;
                }
                else
                {
                    exempt += line.SubTotal;
                }
            }

            decimal net = ratePercent <= 0m
                ? affectedGross
                : Math.Round(affectedGross / (1m + ratePercent / 100m), 0, MidpointRounding.AwayFromZero);

            decimal tax = affectedGross - net;

            return new Breakdown
            {
                Net = net,
                Tax = tax,
                Exempt = exempt,
                Total = net + tax + exempt
            };
        }
    }
}
