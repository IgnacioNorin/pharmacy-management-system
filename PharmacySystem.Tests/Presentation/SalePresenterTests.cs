using System.Collections.Generic;
using System.Linq;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class SalePresenterTests
    {
        // countryCode defaults to "CL" so the existing Factura tests (which use Chilean RUT
        // vectors) keep the modulo-11 validation; pass "" for the generic preset.
        private static SalePresenter CreatePresenter(FakeSaleView view, FakeSaleService saleService, FakeProductService productService,
            int idPerson = 1, decimal taxRate = 19m, string countryCode = "CL")
            => new SalePresenter(view, saleService, productService,
                new FakeStoreService { ListStoreResult = new Store { defaultTaxRate = taxRate, countryCode = countryCode } }, idPerson);

        // The cart lives inside the Presenter now, not the View, so tests that need an existing
        // cart line build it through the real OnAddProduct() path instead of pre-seeding view state.
        private static void AddLine(SalePresenter presenter, FakeSaleView view, int productId, decimal amount, string priceSaleText, int stock = 100)
        {
            view.SelectedProductId = productId;
            view.SelectedProductName = "Product " + productId;
            view.Stock = stock;
            view.Amount = amount;
            view.PriceSaleText = priceSaleText;
            presenter.OnAddProduct();
        }

        [Fact]
        public void OnLoad_SetsDocumentTypeOptionsAndDefaultFromStore()
        {
            var view = new FakeSaleView();
            var presenter = new SalePresenter(view, new FakeSaleService(), new FakeProductService(),
                new FakeStoreService { ListStoreResult = new Store { defaultDocumentType = "Factura" } }, 1);

            presenter.OnLoad();

            Assert.Equal(new[] { "Boleta", "Factura" }, view.DocumentTypeOptions);
            Assert.Equal("Factura", view.SelectedDocumentTypeOption);
            Assert.Equal(new[] { "Efectivo", "Tarjeta", "Transferencia" }, view.PaymentMethodOptions);
            Assert.Equal("Efectivo", view.SelectedPaymentMethodOption);
        }

        [Fact]
        public void OnProductCodeEntered_KnownCode_SelectsProduct()
        {
            var view = new FakeSaleView();
            var productService = new FakeProductService
            {
                ListResult = new List<Product> { new Product { idProduct = 5, code = "P1", name = "Paracetamol", stock = 20, salePrice = 3.5m } }
            };

            CreatePresenter(view, new FakeSaleService(), productService).OnProductCodeEntered("P1");

            Assert.Equal(5, view.SelectedProductSetTo.Value.Id);
            Assert.Equal(20, view.SelectedProductSetTo.Value.Stock);
        }

        [Fact]
        public void OnAddProduct_NoProductSelected_ShowsMessage()
        {
            var view = new FakeSaleView { SelectedProductId = 0 };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Debe seleccionar un producto primero" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_InsufficientStock_ShowsMessage()
        {
            var view = new FakeSaleView { SelectedProductId = 1, Stock = 2, Amount = 5 };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "No hay suficiente stock del producto" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_InvalidSalePrice_ShowsMessage()
        {
            var view = new FakeSaleView { SelectedProductId = 1, Stock = 10, Amount = 1, PriceSaleText = "not-a-number" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Precio Venta\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Empty(view.RenderedCartLines);
        }

        [Fact]
        public void OnAddProduct_ValidEntry_AddsLineRecalculatesTotalAndClearsEntry()
        {
            var view = new FakeSaleView { SelectedProductId = 1, SelectedProductName = "Paracetamol", Stock = 10, Amount = 3, PriceSaleText = "2.00" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnAddProduct();

            Assert.Single(view.RenderedCartLines);
            Assert.Equal(6m, view.RenderedCartLines[0].SubTotal);
            Assert.True(view.ProductEntryCleared);
            Assert.NotNull(view.TotalText);
        }

        [Fact]
        public void OnAddProduct_ProductAlreadyInCart_ShowsMessageAndDoesNotAdd()
        {
            var view = new FakeSaleView();
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "2.00");

            view.SelectedProductId = 1;
            view.Stock = 10;
            view.Amount = 1;
            view.PriceSaleText = "2.00";
            presenter.OnAddProduct();

            Assert.Equal(new[] { "El producto ya fue agregado\nElimínelo e ingrese el nuevo si quiere cambiar la cantidad." }, view.ShownMessages);
            Assert.Single(view.RenderedCartLines);
        }

        [Fact]
        public void OnRemoveProduct_RemovesLineAndRecalculatesTotal()
        {
            var view = new FakeSaleView();
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");
            AddLine(presenter, view, productId: 2, amount: 1, priceSaleText: "20.00");

            presenter.OnRemoveProduct(0);

            Assert.Single(view.RenderedCartLines);
            Assert.Equal(2, view.RenderedCartLines[0].ProductId);
        }

        // Regression test: CalculateChange() used to parse "Paga con" with Convert.ToDecimal
        // instead of the culture-aware converter used for totalPay right next to it, and never
        // actually returned false, so this error message was unreachable dead code. Now a bad
        // "Paga con" value is caught and reported like every other currency-parsing error on this
        // screen.
        [Fact]
        public void OnCalculateChangeRequested_InvalidPayWith_ShowsMessage()
        {
            var view = new FakeSaleView { PayWithText = "not-a-number", TotalPayText = "10.00" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnCalculateChangeRequested();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##" }, view.ShownMessages);
            Assert.Null(view.ChangeTextSet);
        }

        [Fact]
        public void OnCalculateChangeRequested_ValidPayWith_SetsChangeText()
        {
            var view = new FakeSaleView { PayWithText = "15.00", TotalPayText = "10.00" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnCalculateChangeRequested();

            Assert.Empty(view.ShownMessages);
            Assert.NotNull(view.ChangeTextSet);
        }

        [Fact]
        public void OnFinishSale_InvalidPayWith_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "not-a-number";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Error al convertir el tipo de moneda - Paga con\nEjemplo Formato ##.##" }, view.ShownMessages);
        }

        private static FakeSaleView FacturaView() => new FakeSaleView
        {
            DocumentType = "Factura",
            RecipientTaxId = "12.345.678-5",
            RecipientBusinessName = "Acme SpA",
            RecipientActivity = "Comercio",
            RecipientAddress = "Calle 1",
            RecipientCommune = "Santiago",
            PayWithText = "10.00",
            TotalPayText = "10.00"
        };

        [Fact]
        public void OnDocumentTypeChanged_TogglesTheFacturaPanel()
        {
            var view = new FakeSaleView { DocumentType = "Factura" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());

            presenter.OnDocumentTypeChanged();
            Assert.True(view.FacturaFieldsVisible);

            view.DocumentType = "Boleta";
            presenter.OnDocumentTypeChanged();
            Assert.False(view.FacturaFieldsVisible);
        }

        private static ClientRow ClientRowSample() => new ClientRow
        {
            Id = 42,
            Document = "12.345.678-5",
            Name = "Contacto Ejemplo",
            Address = "Calle 1",
            BusinessName = "Ejemplo SpA",
            Activity = "Comercio",
            Commune = "Santiago",
            IsCompany = true
        };

        [Fact]
        public void OnClientSelected_SetsTheClientFieldsFromThePickedRow()
        {
            var view = new FakeSaleView { DocumentType = "Boleta" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());

            presenter.OnClientSelected(ClientRowSample());

            Assert.Equal(("12.345.678-5", "Contacto Ejemplo"), view.ClientSetTo);
            Assert.Null(view.RecipientSetTo); // Boleta: recipient block untouched
        }

        [Fact]
        public void OnClientSelected_Factura_PrefillsTheRecipientFromTheClientFiscalProfile()
        {
            var view = new FakeSaleView { DocumentType = "Factura" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());

            presenter.OnClientSelected(ClientRowSample());

            Assert.Equal(("12.345.678-5", "Ejemplo SpA", "Comercio", "Calle 1", "Santiago"), view.RecipientSetTo);
        }

        [Fact]
        public void OnClientSelected_Factura_ClientWithoutBusinessName_UsesTheName()
        {
            var view = new FakeSaleView { DocumentType = "Factura" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());

            var client = ClientRowSample();
            client.BusinessName = "   ";
            presenter.OnClientSelected(client);

            Assert.Equal("Contacto Ejemplo", view.RecipientSetTo.Value.BusinessName);
        }

        [Fact]
        public void OnDocumentTypeChanged_ToFactura_PrefillsFromTheAlreadySelectedClient()
        {
            var view = new FakeSaleView { DocumentType = "Boleta" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());

            presenter.OnClientSelected(ClientRowSample());
            Assert.Null(view.RecipientSetTo);

            view.DocumentType = "Factura";
            presenter.OnDocumentTypeChanged();

            Assert.Equal("Ejemplo SpA", view.RecipientSetTo.Value.BusinessName);
        }

        [Fact]
        public void OnFinishSale_WithASelectedClient_LinksTheSaleToThatClient()
        {
            var view = FacturaView();
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            presenter.OnClientSelected(ClientRowSample());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            Assert.Equal(42, saleService.RegisteredWith.clientId);
        }

        [Fact]
        public void OnFinishSale_WalkInWithNoSelectedClient_LeavesClientIdNull()
        {
            var view = FacturaView();
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            Assert.Null(saleService.RegisteredWith.clientId);
        }

        [Fact]
        public void OnFinishSale_Factura_GenericPreset_AcceptsANonRutRecipientDocument()
        {
            var view = FacturaView();
            view.RecipientTaxId = "AB-1234.5"; // not a Chilean RUT, but a valid generic document
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true }, countryCode: "");
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            Assert.NotNull(saleService.RegisteredWith);
            Assert.DoesNotContain(view.ShownMessages, m => m.Contains("no es válido"));
        }

        [Fact]
        public void OnFinishSale_Factura_GenericPreset_RejectsAMalformedRecipientDocument()
        {
            var view = FacturaView();
            view.RecipientTaxId = "@@"; // too short and invalid characters for the generic check
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService { VerifyResult = true }, countryCode: "");
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            Assert.Contains(view.ShownMessages, m => m.Contains("documento del receptor no es válido"));
        }

        [Fact]
        public void OnFinishSale_Factura_MissingRecipientData_ShowsMessage()
        {
            var view = FacturaView();
            view.RecipientActivity = "";
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            Assert.Contains(view.ShownMessages, m => m.Contains("receptor de la factura"));
        }

        [Fact]
        public void OnFinishSale_Factura_InvalidRut_ShowsMessage()
        {
            var view = FacturaView();
            view.RecipientTaxId = "12.345.678-9"; // wrong check digit
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            Assert.Contains(view.ShownMessages, m => m.Contains("RUT del receptor no es válido"));
        }

        [Fact]
        public void OnFinishSale_Factura_ValidRecipient_RegistersWithRecipientData()
        {
            var view = FacturaView();
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnFinishSale();

            var sale = saleService.RegisteredWith;
            Assert.NotNull(sale);
            Assert.Equal("12.345.678-5", sale.recipientTaxId);
            Assert.Equal("Acme SpA", sale.recipientBusinessName);
            Assert.Equal("Santiago", sale.recipientCommune);
            // On a Factura the identity is not duplicated into document_client / name_client.
            Assert.Equal("", sale.documentClient);
            Assert.Equal("", sale.nameClient);
        }

        [Fact]
        public void OnFinishSale_MissingClientData_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "", NameClient = "" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar todos los datos del cliente" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_EmptyCart_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };

            CreatePresenter(view, new FakeSaleService(), new FakeProductService()).OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar un producto como minimo\npara registrar una venta" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_CashSaleWithoutTenderedAmount_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan", PaymentMethod = "Efectivo" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.TotalPayText = "10.00";
            view.PayWithText = "0";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar con cuánto paga el cliente en efectivo" }, view.ShownMessages);
        }

        [Fact]
        public void OnFinishSale_CardOnlySale_DoesNotRequireATenderedCashAmount()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan", PaymentMethod = "Tarjeta" };
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.TotalPayText = "10.00";
            view.PayWithText = "0"; // no cash tendered - and that is fine for a card sale
            presenter.OnFinishSale();

            Assert.NotNull(saleService.RegisteredWith);
            Assert.Equal("Tarjeta", saleService.RegisteredWith.paymentMethod);
            Assert.Equal(0m, saleService.RegisteredWith.payWith);
            Assert.Equal(10m, saleService.RegisteredWith.payments.Single().amount);
        }

        [Fact]
        public void OnFinishSale_NotEnoughMoney_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "5.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Falta dinero para pagar" }, view.ShownMessages);
        }

        // Regression test: OnFinishSale used to run an extra stock check against the product-entry
        // fields (SelectedProductId/Amount) before touching the cart, and those are "0"/1 after
        // the last CleanProduct(). A real cashier hits this on every sale - after adding their
        // last item, the entry fields reset, yet the sale must still register. The presenter now
        // only reads the cart; stock is enforced inside Register's transaction.
        [Fact]
        public void OnFinishSale_ProductEntryFieldsResetAfterLastAdd_StillRegistersSale()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 5 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            // Mirrors CleanProduct() having reset the entry fields after the add above.
            view.SelectedProductId = 0;
            view.Amount = 1;
            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.NotNull(saleService.RegisteredWith);
            Assert.True(view.SaleCleared);
        }

        [Fact]
        public void OnFinishSale_LineProductNoLongerExists_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService();
            var productService = new FakeProductService { VerifyResult = false };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "No se pudo registrar la venta\n Problema con producto" }, view.ShownMessages);
            Assert.Null(saleService.RegisteredWith);
        }

        // Register returns 0 when a line's stock ran out (the guard is now inside its
        // transaction). The presenter reports it and leaves the sale uncommitted.
        [Fact]
        public void OnFinishSale_RegisterReportsStockShortage_ShowsMessageAndDoesNotClear()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 0 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "No se pudo registrar la venta.\nVerifique el stock disponible." }, view.ShownMessages);
            Assert.False(view.SaleCleared);
        }

        [Fact]
        public void OnFinishSale_Succeeds_SetsVatBreakdownOnTheSale()
        {
            var view = new FakeSaleView { DocumentClient = "1", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "1190.00");

            view.PayWithText = "1190.00";
            view.TotalPayText = "1190.00";
            presenter.OnFinishSale();

            Assert.Equal(1000m, saleService.RegisteredWith.netAmount);
            Assert.Equal(190m, saleService.RegisteredWith.taxAmount);
            Assert.Equal(0m, saleService.RegisteredWith.exemptAmount);
            Assert.True(saleService.RegisteredWith.oSaleDetail[0].taxAffected);
        }

        [Fact]
        public void OnFinishSale_ExemptProduct_SetsExemptAmountAndNoTax()
        {
            var view = new FakeSaleView { DocumentClient = "1", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var productService = new FakeProductService
            {
                VerifyResult = true,
                ListResult = new List<Product> { new Product { idProduct = 1, name = "Exento", taxAffected = false } }
            };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "1000.00");

            view.PayWithText = "1000.00";
            view.TotalPayText = "1000.00";
            presenter.OnFinishSale();

            Assert.Equal(0m, saleService.RegisteredWith.netAmount);
            Assert.Equal(0m, saleService.RegisteredWith.taxAmount);
            Assert.Equal(1000m, saleService.RegisteredWith.exemptAmount);
            Assert.False(saleService.RegisteredWith.oSaleDetail[0].taxAffected);
        }

        [Fact]
        public void OnFinishSale_Succeeds_RegistersSaleClearsAndNotifiesView()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 99 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService, idPerson: 7);
            AddLine(presenter, view, productId: 1, amount: 2, priceSaleText: "5.00");

            view.DocumentClient = " 123 ";
            view.NameClient = " Juan ";
            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            view.ChangeText = "0.00";
            presenter.OnFinishSale();

            Assert.NotNull(saleService.RegisteredWith);
            Assert.Equal("123", saleService.RegisteredWith.documentClient);
            Assert.Equal("Juan", saleService.RegisteredWith.nameClient);
            Assert.Equal(7, saleService.RegisteredWith.oPerson.idPerson);
            Assert.Single(saleService.RegisteredWith.oSaleDetail);
            Assert.True(view.SaleCleared);
            Assert.Equal(99, view.RegisteredSaleId);
        }

        [Fact]
        public void OnFinishSale_CarriesTheSelectedPaymentMethodOntoTheSale()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan", PaymentMethod = "Tarjeta" };
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal("Tarjeta", saleService.RegisteredWith.paymentMethod);
        }

        // Fase 2 of the alerts rework: a successful sale must let MainForm know stock just
        // changed, instead of it waiting up to 5 minutes for the next safety-net timer tick.
        [Fact]
        public void OnFinishSale_Succeeds_RaisesInventoryChangedNotification()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan", PayWithText = "10.00", TotalPayText = "10.00" };
            var saleService = new FakeSaleService { RegisterResult = 99 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            bool raised = false;
            void Handler() => raised = true;
            InventoryChangeNotifier.StockChanged += Handler;
            try
            {
                presenter.OnFinishSale();
            }
            finally
            {
                InventoryChangeNotifier.StockChanged -= Handler;
            }

            Assert.True(raised);
        }

        [Fact]
        public void OnFinishSale_RegisterFails_DoesNotRaiseInventoryChangedNotification()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan", PayWithText = "10.00", TotalPayText = "10.00" };
            var saleService = new FakeSaleService { RegisterResult = 0 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            bool raised = false;
            void Handler() => raised = true;
            InventoryChangeNotifier.StockChanged += Handler;
            try
            {
                presenter.OnFinishSale();
            }
            finally
            {
                InventoryChangeNotifier.StockChanged -= Handler;
            }

            Assert.False(raised);
        }

        [Fact]
        public void OnFinishSale_RegisterFails_ShowsMessage()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 0 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { "No se pudo registrar la venta.\nVerifique el stock disponible." }, view.ShownMessages);
            Assert.False(view.SaleCleared);
        }

        [Fact]
        public void OnFinishSale_DatabaseUnavailable_ShowsConnectionErrorAndDoesNotClear()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterThrows = new DataUnavailableException() };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            Assert.Equal(new[] { DataUnavailableException.DefaultMessage }, view.ShownMessages);
            Assert.False(view.SaleCleared);
        }

        [Fact]
        public void OnFinishSale_Succeeds_ClearsThePresenterOwnedCartToo()
        {
            var view = new FakeSaleView { DocumentClient = "123", NameClient = "Juan" };
            var saleService = new FakeSaleService { RegisterResult = 5 };
            var productService = new FakeProductService { VerifyResult = true };
            var presenter = CreatePresenter(view, saleService, productService);
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            view.PayWithText = "10.00";
            view.TotalPayText = "10.00";
            presenter.OnFinishSale();

            // A second finish attempt with nothing re-added must behave as an empty cart again -
            // proves the cart was actually cleared, not just the grid the View renders.
            view.ShownMessages.Clear();
            presenter.OnFinishSale();

            Assert.Equal(new[] { "Debe ingresar un producto como minimo\npara registrar una venta" }, view.ShownMessages);
        }

        // --- Pago mixto ---

        [Fact]
        public void OnSplitPaymentRequested_PromptsWithTheCartTotalAndKeepsTheResult()
        {
            var view = new FakeSaleView
            {
                PaymentSplitToReturn = new[] { new SalePaymentEntry("Efectivo", 6m), new SalePaymentEntry("Tarjeta", 4m) }
            };
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnSplitPaymentRequested();

            Assert.Equal(10m, view.PromptPaymentSplitArgs?.Total);
            Assert.Equal(2, view.ShownPaymentSplit.Count);
        }

        [Fact]
        public void OnSplitPaymentRequested_Cancelled_LeavesNoSplit()
        {
            var view = new FakeSaleView { PaymentSplitToReturn = null }; // dialog cancelled
            var presenter = CreatePresenter(view, new FakeSaleService(), new FakeProductService());
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");

            presenter.OnSplitPaymentRequested();

            Assert.Null(view.ShownPaymentSplit);
        }

        [Fact]
        public void OnFinishSale_WithAValidSplit_RegistersEachPaymentAndTheCashPortionDrivesTheChange()
        {
            var view = new FakeSaleView
            {
                DocumentClient = "123", NameClient = "Juan",
                PaymentSplitToReturn = new[] { new SalePaymentEntry("Efectivo", 6m), new SalePaymentEntry("Tarjeta", 4m) }
            };
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");
            view.TotalPayText = "10.00";
            presenter.OnSplitPaymentRequested();

            view.PayWithText = "10.00"; // customer hands 10 in cash for the 6 cash portion
            presenter.OnFinishSale();

            Sale sale = saleService.RegisteredWith;
            Assert.NotNull(sale);
            Assert.Equal(2, sale.payments.Count);
            Assert.Equal(6m, sale.payments.Single(p => p.paymentMethod == "Efectivo").amount);
            Assert.Equal(4m, sale.payments.Single(p => p.paymentMethod == "Tarjeta").amount);
            Assert.Equal("Efectivo", sale.paymentMethod);   // primary = largest amount
            Assert.Equal(10m, sale.payWith);
            Assert.Equal(4m, sale.change);                  // 10 tendered - 6 cash portion
        }

        [Fact]
        public void OnFinishSale_AfterTheCartChanges_TheStaleSplitIsDroppedAndTheComboIsUsed()
        {
            var view = new FakeSaleView
            {
                DocumentClient = "123", NameClient = "Juan", PaymentMethod = "Tarjeta",
                PaymentSplitToReturn = new[] { new SalePaymentEntry("Efectivo", 6m), new SalePaymentEntry("Tarjeta", 4m) }
            };
            var saleService = new FakeSaleService { RegisterResult = 1 };
            var presenter = CreatePresenter(view, saleService, new FakeProductService { VerifyResult = true });
            AddLine(presenter, view, productId: 1, amount: 1, priceSaleText: "10.00");
            view.TotalPayText = "10.00";
            presenter.OnSplitPaymentRequested();

            // Another product goes in - the split was for the old total, so it must be dropped.
            AddLine(presenter, view, productId: 2, amount: 1, priceSaleText: "5.00");
            view.TotalPayText = "15.00";
            presenter.OnFinishSale();

            Sale sale = saleService.RegisteredWith;
            Assert.Single(sale.payments);
            Assert.Equal("Tarjeta", sale.payments[0].paymentMethod);
            Assert.Equal(15m, sale.payments[0].amount);
        }
    }
}
