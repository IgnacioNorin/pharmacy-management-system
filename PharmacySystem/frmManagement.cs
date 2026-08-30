using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using PharmacySystem.Validators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;

namespace PharmacySystem
{
    public partial class frmManagement : Form, ICategoryManagementView, IProductManagementView, IProductPriceView, IStoreManagementView
    {
        private readonly CategoryManagementPresenter _categoryPresenter;
        private readonly ProductManagementPresenter _productPresenter;
        private readonly ProductPricePresenter _productPricePresenter;
        private readonly StoreManagementPresenter _storePresenter;
        // True only while _storePresenter.OnLoad() runs, so populating the preset combo does not
        // fire OnCountryPresetChanged and overwrite the just-loaded saved values.
        private bool _loadingStore;

        public frmManagement()
        {
            InitializeComponent();
            BuildPricesTab();
            BuildProductPager();
            _categoryPresenter = CompositionRoot.CreateCategoryManagementPresenter(this);
            _productPresenter = CompositionRoot.CreateProductManagementPresenter(this);
            _productPricePresenter = CompositionRoot.CreateProductPricePresenter(this);
            _storePresenter = CompositionRoot.CreateStoreManagementPresenter(this);
        }

        private Dictionary<string, Dictionary<Control, List<string>>> sectionWithRules;
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            #region STORE
            { "txttaxid", "Número Documento" },
            { "txtlegalName", "Razón Social" },
            { "txtemail", "Correo" },
            { "txtphone", "Teléfono"},
            { "txtaddress", "Dirección"},
            #endregion
            #region PRODUCT
            { "txtsearchproduct", "Buscar"},
            { "txtcodeproduct", "Codigo"},
            { "txtnameproduct", "Nombre"},
            { "txtdescriptionproduct", "Descripcion"},
            { "cbocategory", "Categoria"},
            #endregion
            #region CATEGORY
            { "txtdescriptioncategory", "Descripcion"}
            #endregion
        };
        private void InitializeValidators()
        {
            sectionWithRules = new Dictionary<string, Dictionary<Control, List<string>>>()
            {
                #region STORE
                ["tabStore"] = new Dictionary<Control, List<string>>
                {
                    { txttaxid, new List<string>{ "NotEmpty", "ValidateDocument" } },
                    { txtlegalName, new List<string>{ "NotEmpty", "ValidateMaxLength" } },
                    { txtemail, new List<string>{ "NotEmpty","ValidateEmail", "ValidateMaxLength" } },
                    { txtphone, new List<string>{ "NotEmpty","OnlyNumbers" } },
                    { txtaddress, new List<string>{ "NotEmpty" , "ValidateMaxLength" } },
                },
                #endregion
                #region PRODUCT
                ["tabProduct"] = new Dictionary<Control, List<string>>
                {
                    { txtcodeproduct , new List<string>{ "NotEmpty", "ValidateMaxLength" } },
                    { txtnameproduct , new List<string>{ "NotEmpty", "ValidateMaxLength" } },
                    { txtdescriptionproduct , new List<string>{ "NotEmpty", "ValidateMaxLength" } },
                    { cbocategory , new List<string>{ "ComboNotEmpty" } },
                },
                #endregion
                #region CATEGORY
                ["tabCategory"] = new Dictionary<Control, List<string>>
                {
                    {txtdescriptioncategory, new List<string> {"NotEmpty", "ValidateMaxLength"}}
                }
                #endregion
            };
        }

        // Shared by the three presenters: each button click only ever fires while its own tab is
        // selected, so tabManagement.SelectedTab.Name at that moment is always the same tab this
        // handler belongs to - same as the original.
        private List<string> ValidateForm()
        {
            var errors = new List<string>();

            foreach (var camp in sectionWithRules[tabManagement.SelectedTab.Name])
            {
                Control control = camp.Key;

                foreach (var rulePassword in camp.Value)
                {
                    var rule = Validations.rules[rulePassword];
                    string value = "";
                    if (control is TextBox txt)
                    {
                        value = txt.Text.Trim();
                    }
                    else if (control is ComboBox cbo)
                    {
                        value = (cbo.SelectedItem as ComboBoxItem)?.Value?.ToString() ?? "";
                    }

                    if (!rule.Validate(value))
                    {
                        errors.Add($"{namesMessages[camp.Key.Name]} : {rule.MessageError}");
                    }
                }
            }

            return errors;
        }

        private void frmManagement_Load(object sender, EventArgs e)
        {
            InitializeValidators();

            #region CATEGORY
            DataGridViewButtonColumn Boton = new DataGridViewButtonColumn();
            Boton.HeaderText = "Seleccionar";
            Boton.Width = 80;
            Boton.Text = "";
            Boton.Name = "btnSeleccionar";
            Boton.UseColumnTextForButtonValue = true;

            dgdatacategory.Columns.Add(Boton);
            dgdatacategory.Columns.Add("Id", "Id");
            dgdatacategory.Columns.Add("Descripcion", "Descripción");

            dgdatacategory.Columns["btnSeleccionar"].Width = 100;
            dgdatacategory.Columns["Descripcion"].Width = 600;
            dgdatacategory.Columns["Id"].Visible = false;
            #endregion

            #region PRODUCT
            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();
            Button.HeaderText = "Seleccionar";
            Button.Width = 80;
            Button.Text = "";
            Button.Name = "btnSeleccionar";
            Button.UseColumnTextForButtonValue = true;

            dgdataproduct.Columns.Add(Button);
            dgdataproduct.Columns.Add("Id", "Id");
            dgdataproduct.Columns.Add("Codigo", "Código");
            dgdataproduct.Columns.Add("Nombre", "Nombre");
            dgdataproduct.Columns.Add("Descripcion", "Descripción");
            dgdataproduct.Columns.Add("Categoria", "Categoria");
            dgdataproduct.Columns.Add("Stock", "Stock");
            dgdataproduct.Columns.Add("FechaVencimiento", "FechaVencimiento");
            dgdataproduct.Columns.Add("TaxAffected", "TaxAffected");

            dgdataproduct.Columns["Id"].Visible = false;
            dgdataproduct.Columns["TaxAffected"].Visible = false;

            foreach (DataGridViewColumn cl in dgdataproduct.Columns)
            {
                if (cl.Visible == true && cl.Name != "btnSeleccionar")
                {
                    cbosearchproduct.Items.Add(new ComboBoxItem() { Value = cl.Name, Text = cl.HeaderText });
                }
            }
            cbosearchproduct.DisplayMember = "Text";
            cbosearchproduct.ValueMember = "Value";
            cbosearchproduct.SelectedIndex = 0;
            #endregion

            // Each tab is shown only if the user's role grants access to that section; a tab the
            // user cannot use is removed rather than left disabled.
            bool canCategories = CanSee("categorias.acceso");
            bool canProducts = CanSee("productos.acceso");

            // The category list also backs the product form's category combo, so load it whenever
            // either tab is going to be shown.
            if (canCategories || canProducts)
            {
                _categoryPresenter.OnLoad();
            }
            if (!canCategories)
            {
                tabManagement.TabPages.Remove(tabCategory);
            }

            if (canProducts)
            {
                _productPresenter.OnLoad();
            }
            else
            {
                tabManagement.TabPages.Remove(tabProduct);
            }

            if (CanSee("productos.editar_precios"))
            {
                _productPricePresenter.OnLoad();
            }
            else
            {
                tabManagement.TabPages.Remove(_tabPrices);
            }

            if (CanSee("tienda.acceso"))
            {
                _loadingStore = true;
                try { _storePresenter.OnLoad(); }
                finally { _loadingStore = false; }
            }
            else
            {
                tabManagement.TabPages.Remove(tabStore);
            }
        }

        // Falls back to allowed when there is no session (e.g. the form-construction smoke test),
        // which never reaches this code path in the real app.
        private static bool CanSee(string permission) => MainForm.Session?.Can(permission) ?? true;

        #region ICategoryManagementView

        int ICategoryManagementView.SelectedIndex => int.Parse(txtindexcategory.Text);
        int ICategoryManagementView.RowCount => dgdatacategory.Rows.Count;
        public int CategoryId => int.Parse(txtidcategory.Text);
        string ICategoryManagementView.Description => txtdescriptioncategory.Text;

        List<string> ICategoryManagementView.Validate() => ValidateForm();

        bool ICategoryManagementView.ConfirmDelete() =>
            MessageBox.Show("¿Desea eliminar la categoria?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void LoadCategories(IEnumerable<CategoryRow> categories)
        {
            foreach (CategoryRow row in categories)
            {
                ((ICategoryManagementView)this).AddRow(row);
            }
        }

        void ICategoryManagementView.AddRow(CategoryRow row)
        {
            int rowId = dgdatacategory.Rows.Add();
            WriteCategoryRow(dgdatacategory.Rows[rowId], row);
        }

        void ICategoryManagementView.ReplaceRow(int index, CategoryRow row) => WriteCategoryRow(dgdatacategory.Rows[index], row);

        void ICategoryManagementView.RemoveRow(int index) => dgdatacategory.Rows.RemoveAt(index);

        void ICategoryManagementView.ClearForm() => CleanCategory();

        void ICategoryManagementView.ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        void ICategoryManagementView.ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void RefreshProductCategoryOptions(IEnumerable<ComboBoxItem> options) => LoadCategoryComboItems(options.ToList());

        private static void WriteCategoryRow(DataGridViewRow gridRow, CategoryRow row)
        {
            gridRow.Cells["Id"].Value = row.Id.ToString();
            gridRow.Cells["Descripcion"].Value = row.Description;
        }

        private void CleanCategory()
        {
            txtindexcategory.Text = "0";
            txtidcategory.Text = "0";
            txtdescriptioncategory.Text = "";
        }

        private void btnSaveCategory_Click(object sender, EventArgs e) => _categoryPresenter.OnSave();

        private void btnDeleteCategory_Click(object sender, EventArgs e) => _categoryPresenter.OnDelete();

        private void dgdataCategory_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                string colname = dgdatacategory.Columns[e.ColumnIndex].Name;
                dgdatacategory.Cursor = colname != "btnSeleccionar" ? Cursors.Default : Cursors.Hand;
            }
        }

        private void dgdataCategory_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);

                var w = Properties.Resources.check20.Width;
                var h = Properties.Resources.check20.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

                e.Graphics.DrawImage(Properties.Resources.check20, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void btnCleanCategory_Click(object sender, EventArgs e) => CleanCategory();

        private void dgdataCategory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdatacategory.Columns[e.ColumnIndex].Name == "btnSeleccionar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    txtindexcategory.Text = (index + 1).ToString();
                    txtidcategory.Text = dgdatacategory.Rows[index].Cells["Id"].Value.ToString();
                    txtdescriptioncategory.Text = dgdatacategory.Rows[index].Cells["Descripcion"].Value.ToString();
                }
            }
        }

        #endregion

        #region IProductManagementView

        int IProductManagementView.SelectedIndex => int.Parse(txtindexproduct.Text);
        int IProductManagementView.RowCount => dgdataproduct.Rows.Count;
        public string SearchText => txtsearchproduct.Text;
        public int ProductId => int.Parse(txtidproduct.Text);
        public string Code => txtcodeproduct.Text;
        string IProductManagementView.Name => txtnameproduct.Text;
        string IProductManagementView.Description => txtdescriptionproduct.Text;
        public int SelectedCategoryId => (int)((ComboBoxItem)cbocategory.SelectedItem).Value;
        public string SelectedCategoryText => ((ComboBoxItem)cbocategory.SelectedItem).Text;
        public bool TaxAffected => chkTaxAffected.Checked;

        List<string> IProductManagementView.Validate() => ValidateForm();

        bool IProductManagementView.ConfirmDelete() =>
            MessageBox.Show("¿Desea eliminar el producto?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        public void LoadCategoryOptions(IEnumerable<ComboBoxItem> options) => LoadCategoryComboItems(options.ToList());

        private void LoadCategoryComboItems(List<ComboBoxItem> options)
        {
            cbocategory.Items.Clear();
            if (options.Count > 0)
            {
                foreach (ComboBoxItem item in options)
                {
                    cbocategory.Items.Add(item);
                }
                cbocategory.DisplayMember = "Text";
                cbocategory.ValueMember = "Value";
                cbocategory.SelectedIndex = 0;
            }
        }

        public void LoadProducts(IEnumerable<ManagementProductRow> products)
        {
            dgdataproduct.Rows.Clear();
            foreach (ManagementProductRow row in products)
            {
                int rowId = dgdataproduct.Rows.Add();
                WriteProductRow(dgdataproduct.Rows[rowId], row, isNewRow: true);
            }
        }

        public void SetPageInfo(int currentPage, int totalPages, int totalCount)
        {
            lblProductPage.Text = totalCount == 0
                ? "Sin resultados"
                : $"Página {currentPage} de {totalPages}  ·  {totalCount} producto(s)";

            btnProductFirst.Enabled = btnProductPrev.Enabled = currentPage > 1;
            btnProductNext.Enabled = btnProductLast.Enabled = currentPage < totalPages;
        }

        void IProductManagementView.ClearForm() => CleanProduct();

        void IProductManagementView.ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        void IProductManagementView.ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private static void WriteProductRow(DataGridViewRow gridRow, ManagementProductRow row, bool isNewRow)
        {
            gridRow.Cells["Id"].Value = row.Id.ToString();
            gridRow.Cells["Codigo"].Value = row.Code;
            gridRow.Cells["Nombre"].Value = row.Name;
            gridRow.Cells["Descripcion"].Value = row.Description;
            gridRow.Cells["Categoria"].Value = row.CategoryText;
            gridRow.Cells["TaxAffected"].Value = row.TaxAffected.ToString();

            // On a new row the original always sets Stock ("0") and leaves FechaVencimiento
            // untouched (defaults to blank). On an update it rewrites neither cell.
            if (isNewRow)
            {
                gridRow.Cells["Stock"].Value = row.Stock;
            }
            else if (row.Stock != null)
            {
                gridRow.Cells["Stock"].Value = row.Stock;
                gridRow.Cells["FechaVencimiento"].Value = row.ExpirationDateText;
            }
        }

        private void CleanProduct()
        {
            txtindexproduct.Text = "0";
            txtidproduct.Text = "0";
            txtcodeproduct.Text = "";
            txtnameproduct.Text = "";
            txtdescriptionproduct.Text = "";
            chkTaxAffected.Checked = true;
            if (cbocategory.SelectedValue != null)
            {
                cbocategory.SelectedIndex = 0;
            }
        }

        private void btnSaveProduct_Click(object sender, EventArgs e) => _productPresenter.OnSave();

        private void dgdataProduct_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            string colname = dgdataproduct.Columns[e.ColumnIndex].Name;

            dgdataproduct.Cursor = (colname != "btnSeleccionar")
                                    ? Cursors.Hand
                                    : Cursors.Default;
        }

        private void dgdataProduct_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != 0) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            var w = Properties.Resources.check20.Width;
            var h = Properties.Resources.check20.Height;
            var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
            var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

            e.Graphics.DrawImage(Properties.Resources.check20, new Rectangle(x, y, w, h));
            e.Handled = true;
        }

        private void btnCleanProduct_Click(object sender, EventArgs e) => CleanProduct();

        private void dgdataProduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdataproduct.Columns[e.ColumnIndex].Name == "btnSeleccionar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    txtindexproduct.Text = (index + 1).ToString();
                    txtidproduct.Text = dgdataproduct.Rows[index].Cells["Id"].Value.ToString();
                    txtcodeproduct.Text = dgdataproduct.Rows[index].Cells["Codigo"].Value.ToString();
                    txtnameproduct.Text = dgdataproduct.Rows[index].Cells["Nombre"].Value.ToString();
                    txtdescriptionproduct.Text = dgdataproduct.Rows[index].Cells["Descripcion"].Value.ToString();
                    chkTaxAffected.Checked =
                        !string.Equals(dgdataproduct.Rows[index].Cells["TaxAffected"].Value?.ToString(), "False", StringComparison.OrdinalIgnoreCase);
                    foreach (ComboBoxItem item in cbocategory.Items)
                    {
                        if (item.Text == dgdataproduct.Rows[index].Cells["Categoria"].Value.ToString())
                        {
                            int item_index = cbocategory.Items.IndexOf(item);
                            cbocategory.SelectedIndex = item_index;
                            break;
                        }
                    }
                }
            }
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e) => _productPresenter.OnDelete();

        private void btnsearch_Click(object sender, EventArgs e) => _productPresenter.OnSearch();

        // Entry point for the notification center's click-through (Fase 3 of the alerts rework):
        // jumps straight to the Producto tab filtered to this code. The search is now a
        // server-side query, so the "buscar por" column selector no longer applies - the term
        // is matched against code, name and description at once.
        public void ShowProductByCode(string code)
        {
            tabManagement.SelectedTab = tabProduct;
            txtsearchproduct.Text = code;
            _productPresenter.OnSearch();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtsearchproduct.Text = "";
            _productPresenter.OnSearch();
        }

        #endregion

        #region IStoreManagementView

        public string Document => txttaxid.Text;
        string IStoreManagementView.CompanyName => txtlegalName.Text;
        public string Email => txtemail.Text;
        public string Phone => txtphone.Text;
        public string Address => txtaddress.Text;
        public string SelectedCurrency => ((ComboBoxItem)cbocurrency.SelectedItem).Value.ToString();
        public string TaxRate => txttaxrate.Text;
        public string DefaultDocumentType => cbodefaultdoctype.SelectedItem?.ToString() ?? "";
        public string SelectedCountryCode => (cbocountrypreset.SelectedItem as ComboBoxItem)?.Value?.ToString() ?? "";

        public void SetTaxRate(string value) => txttaxrate.Text = value;

        public void LoadCountryPresetOptions(IReadOnlyList<ComboBoxItem> options, int selectedIndex)
        {
            cbocountrypreset.DataSource = options.ToList();
            cbocountrypreset.DisplayMember = "Text";
            cbocountrypreset.ValueMember = "Value";
            if (cbocountrypreset.Items.Count > 0)
            {
                cbocountrypreset.SelectedIndex = selectedIndex;
            }
        }

        public void SelectCurrency(string currencyCulture)
        {
            for (int i = 0; i < cbocurrency.Items.Count; i++)
            {
                if (cbocurrency.Items[i] is ComboBoxItem item &&
                    string.Equals((string)item.Value, currencyCulture, StringComparison.OrdinalIgnoreCase))
                {
                    cbocurrency.SelectedIndex = i;
                    return;
                }
            }
        }

        private void cbocountrypreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingStore) return;
            _storePresenter.OnCountryPresetChanged();
        }

        public void LoadDocumentTypeOptions(IReadOnlyList<string> options, string selected)
        {
            cbodefaultdoctype.Items.Clear();
            foreach (string option in options)
            {
                cbodefaultdoctype.Items.Add(option);
            }
            int index = 0;
            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i], selected, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }
            if (cbodefaultdoctype.Items.Count > 0)
            {
                cbodefaultdoctype.SelectedIndex = index;
            }
        }

        List<string> IStoreManagementView.Validate() => ValidateForm();

        public void LoadStoreFields(string document, string companyName, string email, string phone, string address)
        {
            txttaxid.Text = document;
            txtlegalName.Text = companyName;
            txtemail.Text = email;
            txtphone.Text = phone;
            txtaddress.Text = address;
        }

        public void LoadCurrencyOptions(IReadOnlyList<ComboBoxItem> options, int selectedIndex)
        {
            cbocurrency.DataSource = options.ToList();
            cbocurrency.DisplayMember = "Text";
            cbocurrency.ValueMember = "Value";
            cbocurrency.SelectedIndex = selectedIndex;
        }

        public void SetCurrencyEditable(bool enabled) => cbocurrency.Enabled = enabled;

        public void ShowInfo(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void ShowError(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        void IStoreManagementView.ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        private void btnSaveStore_Click(object sender, EventArgs e) => _storePresenter.OnSave();

        #endregion

        #region Product grid pager

        // The Producto grid is server-paged (ProductManagementPresenter). This bar sits just
        // below the grid; the search box above it now triggers a server-side query instead of
        // hiding rows in place. Built in code to leave the Designer untouched, same as the
        // Prices tab below.
        private Button btnProductFirst;
        private Button btnProductPrev;
        private Button btnProductNext;
        private Button btnProductLast;
        private Button btnProductLots;
        private Label lblProductPage;

        private void BuildProductPager()
        {
            int top = dgdataproduct.Bottom + 8;
            int left = dgdataproduct.Left;

            btnProductFirst = MakePagerButton("|<", left, top);
            btnProductPrev = MakePagerButton("<", left + 44, top);
            btnProductNext = MakePagerButton(">", left + 88, top);
            btnProductLast = MakePagerButton(">|", left + 132, top);

            lblProductPage = new Label
            {
                AutoSize = true,
                Location = new Point(left + 188, top + 6),
                Text = string.Empty
            };

            btnProductLots = new Button
            {
                Text = "Ver lotes",
                Location = new Point(left + 176, top - 34),
                Size = new Size(110, 25),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.WhiteSmoke,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnProductLots.Click += (s, e) => ShowSelectedProductLots();

            btnProductFirst.Click += (s, e) => _productPresenter.OnFirstPage();
            btnProductPrev.Click += (s, e) => _productPresenter.OnPreviousPage();
            btnProductNext.Click += (s, e) => _productPresenter.OnNextPage();
            btnProductLast.Click += (s, e) => _productPresenter.OnLastPage();

            Control host = dgdataproduct.Parent ?? tabProduct;
            host.Controls.Add(btnProductFirst);
            host.Controls.Add(btnProductPrev);
            host.Controls.Add(btnProductNext);
            host.Controls.Add(btnProductLast);
            host.Controls.Add(lblProductPage);
            host.Controls.Add(btnProductLots);
        }

        // Opens the lots of the product currently loaded into the edit fields (via the grid's
        // "Seleccionar" button). Read-only view of quantity / expiry / cost per batch.
        private void ShowSelectedProductLots()
        {
            if (!int.TryParse(txtidproduct.Text, out int productId) || productId <= 0)
            {
                MessageBox.Show("Seleccione un producto de la grilla primero.", "Lotes",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var modal = new ModalProductLots(productId, txtnameproduct.Text))
            {
                modal.ShowDialog(this);
            }
        }

        private static Button MakePagerButton(string text, int x, int y) => new Button
        {
            Text = text,
            Location = new Point(x, y),
            Size = new Size(40, 25),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.WhiteSmoke,
            Cursor = Cursors.Hand
        };

        #endregion

        #region Prices tab (IProductPriceView)

        // The Prices tab is built in code (not the Designer) to keep this large form's
        // InitializeComponent untouched. It has two product grids - "to release" and
        // "commercialized" - a small form to set a price, and the selected product's history.
        private TabPage _tabPrices;
        private DataGridView _dgvReleasable;
        private DataGridView _dgvCommercialized;
        private DataGridView _dgvPriceHistory;
        private Label _lblSelectedProduct;
        private TextBox _txtNewPrice;
        private TextBox _txtPriceReason;
        private int _priceSelectedId;

        private void BuildPricesTab()
        {
            _tabPrices = new TabPage("Precios") { Name = "tabPrices", BackColor = Color.FromArgb(245, 246, 248) };

            _dgvReleasable = BuildPriceGrid(new Point(6, 26), new Size(560, 210));
            _dgvCommercialized = BuildPriceGrid(new Point(6, 276), new Size(560, 225));
            _dgvReleasable.SelectionChanged += (s, e) => OnPriceRowSelected(_dgvReleasable);
            _dgvCommercialized.SelectionChanged += (s, e) => OnPriceRowSelected(_dgvCommercialized);

            _dgvPriceHistory = new DataGridView
            {
                Location = new Point(586, 210),
                Size = new Size(645, 291),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _dgvPriceHistory.Columns.Add("Fecha", "Fecha");
            _dgvPriceHistory.Columns.Add("Evento", "Evento");
            _dgvPriceHistory.Columns.Add("Precio", "Precio");
            _dgvPriceHistory.Columns.Add("Costo", "Costo");
            _dgvPriceHistory.Columns.Add("Usuario", "Usuario");
            _dgvPriceHistory.Columns.Add("Motivo", "Motivo");

            _lblSelectedProduct = new Label { Location = new Point(586, 6), AutoSize = true, Text = "Producto: (ninguno seleccionado)" };
            var lblNewPrice = new Label { Location = new Point(586, 34), AutoSize = true, Text = "Nuevo precio de venta:" };
            _txtNewPrice = new TextBox { Location = new Point(586, 52), Size = new Size(140, 21) };
            _txtNewPrice.KeyPress += PriceEntry_KeyPress;
            var lblReason = new Label { Location = new Point(586, 82), AutoSize = true, Text = "Motivo (opcional):" };
            _txtPriceReason = new TextBox { Location = new Point(586, 100), Size = new Size(400, 21) };

            var btnApply = new Button { Location = new Point(586, 134), Size = new Size(200, 28), Text = "Guardar precio / Liberar" };
            btnApply.Click += (s, e) => _productPricePresenter.OnApplyPrice();
            var btnUnrelease = new Button { Location = new Point(796, 134), Size = new Size(200, 28), Text = "Retirar de comercialización" };
            btnUnrelease.Click += (s, e) => _productPricePresenter.OnUnrelease();

            var lblRel = new Label { Location = new Point(6, 6), AutoSize = true, Text = "Por liberar (en stock, sin precio de venta)" };
            var lblCom = new Label { Location = new Point(6, 256), AutoSize = true, Text = "En comercialización" };
            var lblHist = new Label { Location = new Point(586, 188), AutoSize = true, Text = "Historial de precios del producto seleccionado" };

            _tabPrices.Controls.AddRange(new Control[]
            {
                lblRel, _dgvReleasable, lblCom, _dgvCommercialized,
                _lblSelectedProduct, lblNewPrice, _txtNewPrice, lblReason, _txtPriceReason,
                btnApply, btnUnrelease, lblHist, _dgvPriceHistory
            });

            tabManagement.TabPages.Add(_tabPrices);
        }

        private static DataGridView BuildPriceGrid(Point location, Size size)
        {
            var grid = new DataGridView
            {
                Location = location,
                Size = size,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            grid.Columns.Add("Id", "Id");
            grid.Columns.Add("Codigo", "Código");
            grid.Columns.Add("Nombre", "Producto");
            grid.Columns.Add("Stock", "Stock");
            grid.Columns.Add("Costo", "Costo");
            grid.Columns.Add("Precio", "Precio venta");
            grid.Columns.Add("Margen", "Margen %");
            grid.Columns.Add("IVA", "IVA");
            grid.Columns["Id"].Visible = false;
            grid.ClearSelection();
            return grid;
        }

        private void OnPriceRowSelected(DataGridView grid)
        {
            if (_productPricePresenter == null || grid.CurrentRow == null || grid.CurrentRow.Cells["Id"].Value == null)
            {
                return;
            }

            _priceSelectedId = int.Parse(grid.CurrentRow.Cells["Id"].Value.ToString());
            _lblSelectedProduct.Text = "Producto: " + grid.CurrentRow.Cells["Nombre"].Value + " (" + grid.CurrentRow.Cells["Codigo"].Value + ")";
            _productPricePresenter.OnSelectProduct(_priceSelectedId);
        }

        private void PriceEntry_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar) || char.IsDigit(e.KeyChar))
            {
                return;
            }

            var box = (TextBox)sender;
            bool dotAllowed = e.KeyChar == '.' && box.Text.Trim().Length > 0 && !box.Text.Contains(".");
            e.Handled = !dotAllowed;
        }

        private static void FillPriceGrid(DataGridView grid, IEnumerable<ProductPriceRow> rows)
        {
            grid.Rows.Clear();
            foreach (ProductPriceRow r in rows)
            {
                grid.Rows.Add(
                    r.Id.ToString(),
                    r.Code,
                    r.Name,
                    r.Stock.ToString(),
                    CultureInfoHelper.FormatAsCurrency(r.Cost),
                    r.SalePrice.HasValue ? CultureInfoHelper.FormatAsCurrency(r.SalePrice.Value) : "-",
                    r.MarginPercent.HasValue ? r.MarginPercent.Value.ToString("0.0") + " %" : "-",
                    r.TaxAffected ? "Sí" : "No");
            }
            grid.ClearSelection();
        }

        int IProductPriceView.SelectedProductId => _priceSelectedId;
        string IProductPriceView.NewPriceText => _txtNewPrice.Text;
        string IProductPriceView.Reason => _txtPriceReason.Text;

        void IProductPriceView.LoadReleasable(IEnumerable<ProductPriceRow> rows) => FillPriceGrid(_dgvReleasable, rows);
        void IProductPriceView.LoadCommercialized(IEnumerable<ProductPriceRow> rows) => FillPriceGrid(_dgvCommercialized, rows);

        void IProductPriceView.LoadHistory(IEnumerable<ProductPriceHistoryRow> entries)
        {
            _dgvPriceHistory.Rows.Clear();
            foreach (ProductPriceHistoryRow e in entries)
            {
                _dgvPriceHistory.Rows.Add(e.DateText, e.EventText, e.SalePriceText, e.CostText, e.UserName, e.Reason);
            }
            _dgvPriceHistory.ClearSelection();
        }

        void IProductPriceView.ClearEntry()
        {
            _txtNewPrice.Text = "";
            _txtPriceReason.Text = "";
        }

        void IProductPriceView.ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        void IProductPriceView.ShowValidationErrors(IReadOnlyList<string> errors) =>
            MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        #endregion
    }
}
