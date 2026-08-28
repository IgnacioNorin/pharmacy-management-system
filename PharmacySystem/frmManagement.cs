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
    public partial class frmManagement : Form, ICategoryManagementView, IProductManagementView, IStoreManagementView
    {
        private readonly CategoryManagementPresenter _categoryPresenter;
        private readonly ProductManagementPresenter _productPresenter;
        private readonly StoreManagementPresenter _storePresenter;
        // True only while _storePresenter.OnLoad() runs, so populating the preset combo does not
        // fire OnCountryPresetChanged and overwrite the just-loaded saved values.
        private bool _loadingStore;

        public frmManagement()
        {
            InitializeComponent();
            _categoryPresenter = CompositionRoot.CreateCategoryManagementPresenter(this);
            _productPresenter = CompositionRoot.CreateProductManagementPresenter(this);
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
            foreach (ManagementProductRow row in products)
            {
                ((IProductManagementView)this).AddRow(row);
            }
        }

        void IProductManagementView.AddRow(ManagementProductRow row)
        {
            int rowId = dgdataproduct.Rows.Add();
            WriteProductRow(dgdataproduct.Rows[rowId], row, isNewRow: true);
        }

        void IProductManagementView.ReplaceRow(int index, ManagementProductRow row) =>
            WriteProductRow(dgdataproduct.Rows[index], row, isNewRow: false);

        void IProductManagementView.RemoveRow(int index) => dgdataproduct.Rows.RemoveAt(index);

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

        private void btnsearch_Click(object sender, EventArgs e) => FilterProducts();

        private void FilterProducts()
        {
            string columnFilter = ((ComboBoxItem)cbosearchproduct.SelectedItem).Value.ToString();

            if (dgdataproduct.Rows.Count <= 0)
            {
                MessageBox.Show("No hay datos para buscar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtsearchproduct.Text))
            {
                MessageBox.Show("Ingrese un valor al campo antes de buscar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            foreach (DataGridViewRow row in dgdataproduct.Rows)
            {
                string value = row.Cells[columnFilter].Value.ToString().Trim();

                if (row.Cells[columnFilter].Value.ToString().Trim().Contains(txtsearchproduct.Text.Trim()))
                    row.Visible = true;
                else
                    row.Visible = false;
            }
        }

        // Entry point for the notification center's click-through (Fase 3 of the alerts rework):
        // jumps straight to the Producto tab filtered to this code, reusing the exact same search
        // the user already has instead of building a separate "select this row" mechanism.
        public void ShowProductByCode(string code)
        {
            tabManagement.SelectedTab = tabProduct;

            foreach (ComboBoxItem item in cbosearchproduct.Items)
            {
                if ((string)item.Value == "Codigo")
                {
                    cbosearchproduct.SelectedItem = item;
                    break;
                }
            }

            txtsearchproduct.Text = code;
            FilterProducts();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtsearchproduct.Text = "";
            foreach (DataGridViewRow row in dgdataproduct.Rows)
            {
                row.Visible = true;
            }
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
    }
}
