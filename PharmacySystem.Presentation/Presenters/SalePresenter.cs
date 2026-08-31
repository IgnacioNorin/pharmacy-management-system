using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;
using PharmacySystem.Validators;

namespace PharmacySystem.Presentation
{
    // Ported from frmSale.cs. Two bugs from the original were fixed after the migration landed
    // (both were verified safe via the presenter test suite before changing):
    // - CalculateChange() parsed "Paga con" with a plain Convert.ToDecimal, not the culture-aware
    //   converter used for every other amount on this screen (including totalPay right next to
    //   it) - inconsistent under any culture that doesn't use "." as decimal separator. It also
    //   never actually returned false, so the "Error al convertir..." message it could show was
    //   dead code; a real try/catch now makes that message reachable.
    // - OnFinishSale used to discount stock line by line through a separate ControlStock() call
    //   (its own connection, no transaction) before persisting the sale, so a failure midway
    //   left stock already subtracted with no sale and no rollback. Stock is now discounted
    //   inside SaleRepository.Register's transaction, with a stock >= amount guard; this loop
    //   only verifies each product still exists.
    //
    // The cart is owned here rather than read back from the grid, for the same reason as
    // PurchasePresenter's cart: the Presenter is the single source of truth for cart state, the
    // View is a pure render target.
    //
    // A successful sale raises InventoryChangeNotifier.StockChanged so MainForm can recheck its
    // stock/expiration alerts immediately instead of waiting for the next timer tick - Fase 2 of
    // the alerts rework.
    public class SalePresenter
    {
        private readonly ISaleView _view;
        private readonly ISaleService _saleService;
        private readonly IProductService _productService;
        private readonly IStoreService _storeService;
        private readonly CurrentUser _session;
        private readonly int _idPerson;
        private readonly List<SaleCartLine> _cart = new List<SaleCartLine>();
        // The client picked from ModalPerson, or null for a walk-in / consumidor final.
        private ClientRow _selectedClient;
        // The "pago mixto" split the cashier entered, and the cart total it was entered for.
        // Cleared whenever the cart changes so it can never be applied to a different total.
        private List<SalePaymentEntry> _paymentSplit;
        private decimal _paymentSplitTotal;

        public SalePresenter(ISaleView view, ISaleService saleService, IProductService productService, IStoreService storeService, CurrentUser session, int idPerson)
        {
            _view = view;
            _saleService = saleService;
            _productService = productService;
            _storeService = storeService;
            _session = session;
            _idPerson = idPerson;
        }

        public void OnLoad()
        {
            Store store = _storeService.ListStore();
            _view.SetDocumentTypeOptions(DocumentTypes.Selectable, store?.defaultDocumentType ?? DocumentTypes.Boleta);
            _view.SetPaymentMethodOptions(PaymentMethods.Selectable, PaymentMethods.Default);
            _view.SetFacturaFieldsVisible(IsFactura());
        }

        public void OnProductCodeEntered(string code)
        {
            // Only released products can be sold; an unreleased one (in stock but not priced) is
            // invisible to the sale screen.
            Product product = _productService.GetSellableByCode(code);
            if (product != null)
            {
                _view.SetSelectedProduct(product.idProduct, product.code, product.name, product.stock, CultureInfoHelper.FormatAsCurrency(product.salePrice));
            }
        }

        public void OnAddProduct()
        {
            if (_view.SelectedProductId == 0)
            {
                _view.ShowMessage("Debe seleccionar un producto primero");
                return;
            }

            if (_view.Stock < _view.Amount)
            {
                _view.ShowMessage("No hay suficiente stock del producto");
                return;
            }

            decimal priceSale;
            try
            {
                priceSale = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PriceSaleText);
            }
            catch
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##");
                return;
            }

            bool productExists = _cart.Any(l => l.ProductId == _view.SelectedProductId);
            if (productExists)
            {
                _view.ShowMessage("El producto ya fue agregado\nElimínelo e ingrese el nuevo si quiere cambiar la cantidad.");
                return;
            }

            decimal subTotal = _view.Amount * priceSale;

            Product cartProduct = _productService.GetSellableById(_view.SelectedProductId);

            var line = new SaleCartLine
            {
                ProductId = _view.SelectedProductId,
                Name = _view.SelectedProductName,
                Quantity = _view.Amount,
                SalePrice = priceSale,
                SubTotal = subTotal,
                TaxAffected = cartProduct?.taxAffected ?? true
            };

            _cart.Add(line);
            _view.AddCartLine(line);
            RecalculateTotal();
            _view.ClearProductEntry();
        }

        public void OnRemoveProduct(int index)
        {
            _cart.RemoveAt(index);
            _view.RemoveCartLineAt(index);
            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            decimal total = _cart.Sum(l => l.SubTotal);
            _view.SetTotalText(CultureInfoHelper.FormatAsCurrency(total));

            // The split was for the previous total - drop it and fall back to the single combo.
            _paymentSplit = null;
            _view.ShowPaymentSplit(null);
        }

        // Opens the "pago mixto" dialog and keeps the resulting split for the current total.
        public void OnSplitPaymentRequested()
        {
            decimal total = _cart.Sum(l => l.SubTotal);
            if (total <= 0m)
            {
                _view.ShowMessage("Agregue productos antes de dividir el pago.");
                return;
            }

            IReadOnlyList<SalePaymentEntry> result = _view.PromptPaymentSplit(total, _paymentSplit, PaymentMethods.Selectable);
            if (result == null)
            {
                return;
            }

            _paymentSplit = result.ToList();
            _paymentSplitTotal = total;
            _view.ShowPaymentSplit(_paymentSplit);
        }

        public void OnCalculateChangeRequested()
        {
            if (!CalculateChange())
            {
                _view.ShowMessage("Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##");
            }
        }

        private bool CalculateChange()
        {
            decimal moneyToPay;
            decimal totalPay;
            try
            {
                moneyToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PayWithText);
                totalPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.TotalPayText);
            }
            catch
            {
                return false;
            }

            if (moneyToPay < totalPay)
            {
                _view.SetChangeText(CultureInfoHelper.FormatAsCurrency(0));
            }
            else
            {
                _view.SetChangeText(CultureInfoHelper.FormatAsCurrency(moneyToPay - totalPay));
            }

            return true;
        }

        private bool IsFactura() =>
            string.Equals(_view.DocumentType, DocumentTypes.Factura, System.StringComparison.OrdinalIgnoreCase);

        public void OnDocumentTypeChanged()
        {
            _view.SetFacturaFieldsVisible(IsFactura());
            PrefillRecipientFromClient();
        }

        // Called when a client is chosen from the picker. Fills the client fields, and - if the
        // document is a Factura - the recipient block from that client's fiscal profile.
        public void OnClientSelected(ClientRow client)
        {
            _selectedClient = client;
            if (client != null)
            {
                _view.SetClient(client.Document, client.Name);
            }
            PrefillRecipientFromClient();
        }

        private void PrefillRecipientFromClient()
        {
            if (!IsFactura() || _selectedClient == null)
            {
                return;
            }

            string businessName = string.IsNullOrWhiteSpace(_selectedClient.BusinessName)
                ? _selectedClient.Name
                : _selectedClient.BusinessName;

            _view.SetRecipient(
                _selectedClient.Document,
                businessName,
                _selectedClient.Activity,
                _selectedClient.Address,
                _selectedClient.Commune);
        }

        public void OnFinishSale()
        {
            // Defense in depth: the sidebar already gates the screen, but the action re-checks
            // the permission here like every other sensitive operation (DEF-22).
            if (!(_session?.Can("ventas.acceso") ?? false))
            {
                _view.ShowMessage("No tiene permiso para registrar ventas.");
                return;
            }

            bool isFactura = IsFactura();
            Store store = _storeService.ListStore();

            if (!isFactura && (_view.DocumentClient.Trim() == "" || _view.NameClient.Trim() == ""))
            {
                _view.ShowMessage("Debe ingresar todos los datos del cliente");
                return;
            }

            // The recipient document on a Boleta used to be free text checked only for emptiness
            // (DEF-32). Apply the same country-agnostic format check the rest of the app uses.
            if (!isFactura && !DocumentValidator.IsValid(_view.DocumentClient.Trim()))
            {
                _view.ShowMessage("El documento del cliente no tiene un formato válido");
                return;
            }

            if (isFactura)
            {
                string recipientDocument = (_view.RecipientTaxId ?? "").Trim();
                if (recipientDocument == "" || (_view.RecipientBusinessName ?? "").Trim() == "" || (_view.RecipientActivity ?? "").Trim() == ""
                    || (_view.RecipientAddress ?? "").Trim() == "" || (_view.RecipientCommune ?? "").Trim() == "")
                {
                    _view.ShowMessage("Complete los datos del receptor de la factura");
                    return;
                }
                if (!ChileanRutValidator.IsValid(recipientDocument))
                {
                    _view.ShowMessage("El RUT del receptor no es válido");
                    return;
                }
            }

            if (_cart.Count < 1)
            {
                _view.ShowMessage("Debe ingresar un producto como minimo\npara registrar una venta");
                return;
            }

            decimal totalToPay;
            try
            {
                totalToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.TotalPayText);
            }
            catch
            {
                _view.ShowMessage("Error al convertir el total de la venta.");
                return;
            }

            if (totalToPay <= 0m)
            {
                _view.ShowMessage("El total de la venta debe ser mayor a cero.");
                return;
            }

            // Payment breakdown: the "pago mixto" split when it matches this exact total,
            // otherwise a single line for the method in the combo.
            List<SalePayment> payments;
            if (_paymentSplit != null && _paymentSplit.Count > 0 && _paymentSplitTotal == totalToPay)
            {
                payments = _paymentSplit
                    .Select(e => new SalePayment { paymentMethod = e.Method, amount = e.Amount })
                    .ToList();
            }
            else
            {
                string singleMethod = string.IsNullOrWhiteSpace(_view.PaymentMethod) ? PaymentMethods.Default : _view.PaymentMethod.Trim();
                payments = new List<SalePayment> { new SalePayment { paymentMethod = singleMethod, amount = totalToPay } };
            }

            decimal cashPortion = payments
                .Where(p => string.Equals(p.paymentMethod, PaymentMethods.Efectivo, System.StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.amount);

            decimal moneyToPay;   // amount_received
            decimal changeMoney;  // change_amount

            if (cashPortion > 0m)
            {
                if (string.IsNullOrWhiteSpace(_view.PayWithText) || _view.PayWithText.Trim() == "0")
                {
                    _view.ShowMessage("Debe ingresar con cuánto paga el cliente en efectivo");
                    return;
                }

                try
                {
                    moneyToPay = CultureInfoHelper.CultureInfoConverterStringToDecimal(_view.PayWithText);
                }
                catch
                {
                    _view.ShowMessage("Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##");
                    return;
                }

                if (moneyToPay < cashPortion)
                {
                    _view.ShowMessage("Falta dinero para pagar");
                    return;
                }

                changeMoney = moneyToPay - cashPortion;
                _view.SetChangeText(CultureInfoHelper.FormatAsCurrency(changeMoney));
            }
            else
            {
                // Fully card / transfer: nothing tendered in cash, no change.
                moneyToPay = 0m;
                changeMoney = 0m;
                _view.SetChangeText(CultureInfoHelper.FormatAsCurrency(0));
            }

            var details = new List<SaleDetail>();

            try
            {
                foreach (SaleCartLine line in _cart)
                {
                    if (!_productService.Verify(line.ProductId))
                    {
                        _view.ShowMessage("No se pudo registrar la venta\n Problema con producto");
                        return;
                    }

                    details.Add(new SaleDetail
                    {
                        oProduct = new Product { idProduct = line.ProductId },
                        amount = (int)line.Quantity,
                        salePrice = line.SalePrice,
                        subtotal = line.SubTotal,
                        taxAffected = line.TaxAffected
                    });
                }

                decimal taxRate = store?.defaultTaxRate ?? 19m;
                TaxCalculator.Breakdown vat = TaxCalculator.Compute(
                    _cart.Select(l => (l.SubTotal, l.TaxAffected)), taxRate);

                Sale sale = new Sale
                {
                    typeDocument = _view.DocumentType,
                    oPerson = new Person { idPerson = _idPerson },
                    // On a Factura the fiscal identity lives in recipient_*; document_client /
                    // name_client stay empty so the same data is not stored twice.
                    documentClient = isFactura ? "" : _view.DocumentClient.Trim(),
                    nameClient = isFactura ? "" : _view.NameClient.Trim(),
                    recipientTaxId = isFactura ? (_view.RecipientTaxId ?? "").Trim() : null,
                    recipientBusinessName = isFactura ? (_view.RecipientBusinessName ?? "").Trim() : null,
                    recipientActivity = isFactura ? (_view.RecipientActivity ?? "").Trim() : null,
                    recipientAddress = isFactura ? (_view.RecipientAddress ?? "").Trim() : null,
                    recipientCommune = isFactura ? (_view.RecipientCommune ?? "").Trim() : null,
                    totalPay = totalToPay,
                    payWith = moneyToPay,
                    change = changeMoney,
                    paymentMethod = payments.OrderByDescending(p => p.amount).First().paymentMethod,
                    payments = payments,
                    netAmount = vat.Net,
                    taxAmount = vat.Tax,
                    exemptAmount = vat.Exempt,
                    clientId = (_selectedClient != null && _selectedClient.Id > 0) ? _selectedClient.Id : (int?)null,
                    oSaleDetail = details
                };

                int idSale = _saleService.Register(sale);

                if (idSale != 0)
                {
                    _cart.Clear();
                    _selectedClient = null;
                    _view.ClearSale();
                    _view.SaleRegistered(idSale);
                    InventoryChangeNotifier.NotifyStockChanged();
                }
                else
                {
                    // Register returns 0 for any reason the sale could not be committed, including a
                    // line whose stock ran out between adding it to the cart and finishing the sale
                    // (the stock check is now inside Register's transaction).
                    _view.ShowMessage("No se pudo registrar la venta.\nVerifique el stock disponible.");
                }
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowMessage(ex.Message);
            }
        }
    }
}
