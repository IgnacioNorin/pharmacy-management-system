using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Fills the HTML ticket template - PrintSale's default (normal-printer) path. Kept beside
    // PharmacyTicketBuilder (the thermal / plain-text path) so the two receipts do not drift:
    // both now carry the RECEPTOR block and the NETO / IVA / EXENTO breakdown (DEF-11), and
    // every data-derived string is HTML-encoded (DEF-12).
    //
    // The template still uses the legacy "!...!"-style tokens for the plain fields; the two
    // blocks that expand to a variable number of <tr> rows use ASCII "{{...}}" tokens so they
    // are encoding-independent (the .html resource is Windows-1252).
    public static class HtmlTicketBuilder
    {
        public static string Build(string template, Store store, Sale sale, List<SaleDetail> saleDetails)
        {
            string t = template;

            t = t.Replace("¡nombreempresa!", Enc((store?.companyName ?? string.Empty).ToUpper()));
            t = t.Replace("¡documentoempresa!", Enc(store?.document));
            t = t.Replace("¡correoempresa!", Enc(store?.email));
            t = t.Replace("!telefonoempresa¡", Enc(store?.phone));

            t = t.Replace("¡tipodocumento!", Enc(sale.typeDocument));
            t = t.Replace("¡numerodocumento!", Enc(sale.numberDocument));
            t = t.Replace("¡fechaventa!", Enc(sale.registrationDate.ToString("dd/MM/yyyy HH:mm")));

            t = t.Replace("{{receptor}}", RecipientRows(sale));
            t = t.Replace("¡detalleventa!", DetailRows(saleDetails));
            t = t.Replace("{{desgloseiva}}", VatRows(store, sale));

            t = t.Replace("¡totalpagar!", Money(sale.totalPay));
            t = t.Replace("¡formapago!", Enc(PaymentText(sale)));
            t = t.Replace("¡pagocon!", Money(sale.payWith));
            t = t.Replace("¡cambio!", Money(sale.change));

            return t;
        }

        private static string Enc(string s) => WebUtility.HtmlEncode(s ?? string.Empty);

        private static string Money(decimal v) => Enc(CultureInfoHelper.FormatAsCurrency(v));

        // label is encoded here; encodedValue must already be HTML-safe (Enc/Money output).
        private static string Row(string label, string encodedValue) =>
            "<tr><td align=\"left\">" + Enc(label) + "</td><td align=\"right\">" + encodedValue + "</td></tr>";

        private static string RecipientRows(Sale sale)
        {
            var sb = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(sale.recipientTaxId))
            {
                sb.AppendLine("<tr><td colspan=\"2\">-------------------------------------------------</td></tr>");
                sb.AppendLine("<tr><td align=\"center\" colspan=\"2\" style=\"font-weight:bold\">RECEPTOR</td></tr>");
                sb.AppendLine(Row("RUT:", Enc(sale.recipientTaxId)));
                if (!string.IsNullOrWhiteSpace(sale.recipientBusinessName)) sb.AppendLine(Row("Razón Social:", Enc(sale.recipientBusinessName)));
                if (!string.IsNullOrWhiteSpace(sale.recipientActivity)) sb.AppendLine(Row("Giro:", Enc(sale.recipientActivity)));
                if (!string.IsNullOrWhiteSpace(sale.recipientAddress)) sb.AppendLine(Row("Dirección:", Enc(sale.recipientAddress)));
                if (!string.IsNullOrWhiteSpace(sale.recipientCommune)) sb.AppendLine(Row("Comuna:", Enc(sale.recipientCommune)));
            }
            else
            {
                sb.AppendLine("<tr><td align=\"center\" colspan=\"2\">Cliente: Público General</td></tr>");
            }

            if (sale.referenceId.HasValue)
            {
                sb.AppendLine(Row("Anula venta N°:", Enc(sale.referenceId.Value.ToString())));
                if (!string.IsNullOrWhiteSpace(sale.referenceReason)) sb.AppendLine(Row("Motivo:", Enc(sale.referenceReason)));
            }

            return sb.ToString();
        }

        private static string DetailRows(List<SaleDetail> saleDetails)
        {
            var sb = new StringBuilder();
            foreach (SaleDetail detail in saleDetails ?? Enumerable.Empty<SaleDetail>())
            {
                sb.AppendLine("<tr>");
                sb.AppendLine("<td width=\"20\">" + detail.amount + "</td>");
                sb.AppendLine("<td width=\"180\">" + Enc(detail.oProduct?.name) + "</td>");
                sb.AppendLine("<td style=\"font-size:14px\">" + Money(detail.salePrice) + "</td>");
                sb.AppendLine("<td style=\"font-size:14px\">" + Money(detail.subtotal) + "</td>");
                sb.AppendLine("</tr>");
            }
            return sb.ToString();
        }

        private static string VatRows(Store store, Sale sale)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Row("Neto:", Money(sale.netAmount)));
            string ivaLabel = (store?.defaultTaxRate ?? 0m) > 0m
                ? "IVA (" + store.defaultTaxRate.ToString("0.##") + "%):"
                : "IVA:";
            sb.AppendLine(Row(ivaLabel, Money(sale.taxAmount)));
            if (sale.exemptAmount > 0m)
            {
                sb.AppendLine(Row("Exento:", Money(sale.exemptAmount)));
            }
            return sb.ToString();
        }

        private static string PaymentText(Sale sale)
        {
            if (sale.payments != null && sale.payments.Count > 0)
            {
                return sale.payments.Count > 1
                    ? "Mixto (" + string.Join(", ", sale.payments.Select(p => p.paymentMethod + " " + CultureInfoHelper.FormatAsCurrency(p.amount))) + ")"
                    : sale.payments[0].paymentMethod;
            }
            return string.IsNullOrWhiteSpace(sale.paymentMethod) ? PaymentMethods.Default : sale.paymentMethod;
        }
    }
}
