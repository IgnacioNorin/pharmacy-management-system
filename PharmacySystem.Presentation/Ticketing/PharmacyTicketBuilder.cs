using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from PrintSale.cs's GenerateFormattedPharmacyTicket(). Pure text assembly with no
    // I/O, so the Form only fetches the Store/Sale/SaleDetail rows and hands them here.
    public static class PharmacyTicketBuilder
    {
        public static string Build(Store store, Sale sale, List<SaleDetail> saleDetails, int width = 50)
        {
            if (sale == null || saleDetails == null || !saleDetails.Any())
            {
                return "Error: Sale not found or no details available.";
            }

            var formatter = new PlainTextTicketFormatter(width);

            string date = sale.registrationDate.ToString("dd/MM/yyyy");
            string time = sale.registrationDate.ToString("HH:mm:ss");

            // Company header
            formatter.AddCenteredText(store.companyName.ToUpper());
            formatter.AddCenteredText($"Documento: {store.document}");
            formatter.AddCenteredText(store.address.ToUpper());
            if (!string.IsNullOrEmpty(store.phone))
                formatter.AddCenteredText($"Tel: {store.phone}");
            if (!string.IsNullOrEmpty(store.email))
                formatter.AddCenteredText(store.email);

            formatter.AddCharacter("-");

            // Sale information
            formatter.AddTwoColumns("Tipo Doc:", sale.typeDocument);
            formatter.AddTwoColumns("Número:", sale.numberDocument);
            formatter.AddTwoColumns("Fecha:", date);
            formatter.AddTwoColumns("Hora:", time);
            if (sale.referenceId.HasValue)
            {
                formatter.AddTwoColumns("Anula venta id:", sale.referenceId.Value.ToString());
                if (!string.IsNullOrWhiteSpace(sale.referenceReason))
                {
                    formatter.AddTwoColumns("Motivo:", sale.referenceReason);
                }
            }

            if (!string.IsNullOrWhiteSpace(sale.recipientTaxId))
            {
                formatter.AddCharacter("-");
                formatter.AddCenteredText("RECEPTOR");
                formatter.AddTwoColumns("RUT:", sale.recipientTaxId);
                formatter.AddTwoColumns("Razón Social:", sale.recipientBusinessName ?? "");
                formatter.AddTwoColumns("Giro:", sale.recipientActivity ?? "");
                formatter.AddTwoColumns("Dirección:", sale.recipientAddress ?? "");
                formatter.AddTwoColumns("Comuna:", sale.recipientCommune ?? "");
            }
            else
            {
                formatter.AddCenteredText("Cliente: Público General");
            }

            formatter.AddCharacter("-");

            // Product headers
            string header = string.Format("{0,-4} {1,-20} {2,-8} {3,8}", "Cant", "Producto", "Precio", "Subtotal");
            if (header.Length > width)
            {
                formatter.AppendLine("Cant Producto           P.Unit  Subtot");
            }
            else
            {
                formatter.AppendLine(header);
            }
            formatter.AddCharacter("-");

            // Product details
            foreach (SaleDetail detail in saleDetails)
            {
                string productName = detail.oProduct.name.Length > 20 ?
                    detail.oProduct.name.Substring(0, 17) + "..." : detail.oProduct.name;

                string priceStr = CultureInfoHelper.FormatAsCurrency(detail.salePrice);
                string subtotalStr = CultureInfoHelper.FormatAsCurrency(detail.subtotal);

                string productLine = string.Format("{0,-4} {1,-20} {2,-8} {3,8}",
                    detail.amount.ToString(),
                    productName,
                    priceStr,
                    subtotalStr);

                if (productLine.Length > width)
                {
                    // Alternative format for very long lines
                    formatter.AppendLine($"{detail.amount} {productName}");
                    formatter.AddTwoColumns($"  {priceStr} x {detail.amount}", subtotalStr);
                }
                else
                {
                    formatter.AppendLine(productLine);
                }
            }

            formatter.AddCharacter("-");

            // VAT breakdown (net + tax + exempt == totalPay)
            formatter.AddTwoColumns("NETO:", CultureInfoHelper.FormatAsCurrency(sale.netAmount));
            string ivaLabel = store.defaultTaxRate > 0
                ? $"IVA ({store.defaultTaxRate:0.##}%):"
                : "IVA:";
            formatter.AddTwoColumns(ivaLabel, CultureInfoHelper.FormatAsCurrency(sale.taxAmount));
            if (sale.exemptAmount > 0)
            {
                formatter.AddTwoColumns("EXENTO:", CultureInfoHelper.FormatAsCurrency(sale.exemptAmount));
            }

            // Totals
            formatter.AddTwoColumns("TOTAL A PAGAR:", CultureInfoHelper.FormatAsCurrency(sale.totalPay));

            if (sale.payments != null && sale.payments.Count > 0)
            {
                formatter.AddTwoColumns("FORMA DE PAGO:", sale.payments.Count > 1 ? "Mixto" : sale.payments[0].paymentMethod);
                if (sale.payments.Count > 1)
                {
                    foreach (SalePayment p in sale.payments)
                    {
                        formatter.AddTwoColumns("  " + p.paymentMethod + ":", CultureInfoHelper.FormatAsCurrency(p.amount));
                    }
                }
            }
            else
            {
                formatter.AddTwoColumns("FORMA DE PAGO:", string.IsNullOrWhiteSpace(sale.paymentMethod) ? PaymentMethods.Default : sale.paymentMethod);
            }

            // The cash-tendered / change lines only make sense when part of the sale was cash.
            if (sale.payWith > 0m || sale.change > 0m)
            {
                formatter.AddTwoColumns("PAGO CON:", CultureInfoHelper.FormatAsCurrency(sale.payWith));
                formatter.AddTwoColumns("CAMBIO:", CultureInfoHelper.FormatAsCurrency(sale.change));
            }

            formatter.AddCharacter("-");
            formatter.AddCenteredText("¡Gracias por su compra!");
            formatter.AddCenteredText("¡Vuelva pronto!");

            // Final spaces for paper cutting
            formatter.AppendLine("\n\n\n");

            return formatter.ToString();
        }
    }
}
