using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using PharmacySystem.Helpers;
using PharmacySystem.Logical;
using PharmacySystem.Model;
using PharmacySystem.Validators;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Control = System.Windows.Forms.Control;


namespace PharmacySystem
{
    public partial class frmManagement : Form
    {
        public frmManagement()
        {
            InitializeComponent();
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
                   
                    { txttaxid, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
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



        private void frmManagement_Load(object sender, EventArgs e)
        {
            InitializeValidators();
            #region CATEGORY
            //AGREGAR BOTON ELIMINAR
            DataGridViewButtonColumn Boton = new DataGridViewButtonColumn();

            Boton.HeaderText = "Seleccionar";
            Boton.Width = 80;
            Boton.Text = "";
            Boton.Name = "btnSeleccionar";
            Boton.UseColumnTextForButtonValue = true;

            //AGREGAMOS LOS BOTONES
            dgdatacategory.Columns.Add(Boton);
            dgdatacategory.Columns.Add("Id", "Id");
            dgdatacategory.Columns.Add("Descripcion", "Descripción");

            dgdatacategory.Columns["btnSeleccionar"].Width = 100;
            dgdatacategory.Columns["Descripcion"].Width = 600;
            dgdatacategory.Columns["Id"].Visible = false;


            foreach (Categories p in CategoryService.Instance.ListCategory())
            {
                int rowId = dgdatacategory.Rows.Add();
                DataGridViewRow row = dgdatacategory.Rows[rowId];
                row.Cells["Id"].Value = p.IdCategory.ToString();
                row.Cells["Descripcion"].Value = p.description;
            }
            #endregion

            #region PRODUCT
            List<Categories> lstc = CategoryService.Instance.ListCategory();
            if (lstc.Count > 0)
            {
                foreach (Categories c in lstc)
                {
                    cbocategory.Items.Add(new ComboBoxItem() { Value = c.IdCategory, Text = c.description });
                }
                cbocategory.DisplayMember = "Text";
                cbocategory.ValueMember = "Value";
                cbocategory.SelectedIndex = 0;
            }

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

            dgdataproduct.Columns["Id"].Visible = false;

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

            foreach (Product p in ProductService.Instance.ListProduct())
            {
                int rowId = dgdataproduct.Rows.Add();
                DataGridViewRow row = dgdataproduct.Rows[rowId];
                row.Cells["Id"].Value = p.idProduct.ToString();
                row.Cells["Codigo"].Value = p.code;
                row.Cells["Nombre"].Value = p.name;
                row.Cells["Descripcion"].Value = p.description;
                row.Cells["Categoria"].Value = p.oCategory.description;
                row.Cells["Stock"].Value = p.stock;
                if (p.expirationDate.ToShortDateString() == "01/01/0001")
                {
                    row.Cells["FechaVencimiento"].Value = "";
                }
                else
                {
                    row.Cells["FechaVencimiento"].Value = p.expirationDate.ToShortDateString();
                }

            }

            #endregion

            #region TIENDA


            Store objeto = StoreService.Instance.ListStore();
            txttaxid.Text = objeto.document;
            txtlegalName.Text = objeto.companyName;
            txtemail.Text = objeto.email;
            txtphone.Text = objeto.phone;
            txtaddress.Text = objeto.address;

            cbocurrency.DataSource = CultureInfoHelper.SupportedCurrencies.ToList();
            cbocurrency.DisplayMember = "Text";
            cbocurrency.ValueMember = "Value";
            int currencyIndex = CultureInfoHelper.SupportedCurrencies
                .ToList()
                .FindIndex(c => string.Equals((string)c.Value, objeto.currencyCulture, StringComparison.OrdinalIgnoreCase));
            cbocurrency.SelectedIndex = currencyIndex >= 0 ? currencyIndex : 0;
            #endregion

        }

        private void btnSaveCategory_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            Categories obj = new Categories()
            {
                IdCategory = int.Parse(txtidcategory.Text),
                description = txtdescriptioncategory.Text.Trim(),
            };

            var result = false;
            if (int.Parse(txtidcategory.Text) == 0)
            {
                int id = CategoryService.Instance.RegisterCategory(obj);
                result = id != 0 ? true : false;
                if (result)
                {
                    int rowId = dgdatacategory.Rows.Add();
                    DataGridViewRow row = dgdatacategory.Rows[rowId];
                    row.Cells["Id"].Value = id.ToString();
                    row.Cells["Descripcion"].Value = txtdescriptioncategory.Text.Trim();
                }
            }
            else
            {
                result = CategoryService.Instance.UpdateCategory(obj);
                if (result)
                {
                    DataGridViewRow row = dgdatacategory.Rows[int.Parse(txtindexcategory.Text) - 1];
                    row.Cells["Id"].Value = txtidcategory.Text;
                    row.Cells["Descripcion"].Value = txtdescriptioncategory.Text.Trim();

                }
            }

            if (result)
            {
                cbocategory.Items.Clear();
                List<Categories> lstc = CategoryService.Instance.ListCategory();
                if (lstc.Count > 0)
                {
                    foreach (Categories c in lstc)
                    {
                        cbocategory.Items.Add(new ComboBoxItem() { Value = c.IdCategory, Text = c.description });
                    }
                    cbocategory.DisplayMember = "Text";
                    cbocategory.ValueMember = "Value";
                    cbocategory.SelectedIndex = 0;
                }
                CleanCategory();
            }
            else
                MessageBox.Show("No se pudo guardar los cambios\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void dgdataCategory_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0)
            {
                string colname = dgdatacategory.Columns[e.ColumnIndex].Name;
                if (colname != "btnSeleccionar")
                {
                    dgdatacategory.Cursor = Cursors.Default;
                }
                else
                {
                    dgdatacategory.Cursor = Cursors.Hand;
                }
            }

        }

        private void dgdataCategory_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

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

        private void btnCleanCategory_Click(object sender, EventArgs e)
        {
            CleanCategory();
        }

        private void CleanCategory()
        {

            txtindexcategory.Text = "0";
            txtidcategory.Text = "0";
            txtdescriptioncategory.Text = "";
        }

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

        private void btnDeleteCategory_Click(object sender, EventArgs e)
        {
            if (int.Parse(txtindexcategory.Text) > 0)
            {
                if (MessageBox.Show("¿Desea eliminar la categoria?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    bool result = CategoryService.Instance.DeleteCategory(int.Parse(txtidcategory.Text));
                    if (result)
                    {
                        cbocategory.Items.Clear();
                        List<Categories> lstc = CategoryService.Instance.ListCategory();
                        if (lstc.Count > 0)
                        {
                            foreach (Categories c in lstc)
                            {
                                cbocategory.Items.Add(new ComboBoxItem() {Value = c.IdCategory, Text = c.description });
                            }
                            cbocategory.DisplayMember = "Text";
                            cbocategory.ValueMember = "Value";
                            cbocategory.SelectedIndex = 0;
                        }
                        dgdatacategory.Rows.RemoveAt(int.Parse(txtindexcategory.Text) - 1);
                        CleanCategory();
                    }
                    else
                        MessageBox.Show("No se pudo eliminar el registro\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

            }
        }

        private void btnSaveProduct_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            int txtIdProductParse = int.Parse(txtidproduct.Text);
            int txtIndexProductParse = int.Parse(txtindexproduct.Text);
            ComboBoxItem comboboxItem = ((ComboBoxItem)cbocategory.SelectedItem);

            Product product = new Product()
            {
                idProduct = txtIdProductParse,
                code = txtcodeproduct.Text.Trim(),
                name = txtnameproduct.Text.Trim(),
                description = txtdescriptionproduct.Text.Trim(),
                oCategory = new Categories() { IdCategory = (int)comboboxItem.Value },
            };

            var result = false;
            if (txtIdProductParse == 0)
            {
                int id = ProductService.Instance.RegisterProduct(product);
                result = id != 0 ? true : false;

                if (!result) return;

                int rowId = dgdataproduct.Rows.Add();
                DataGridViewRow row = dgdataproduct.Rows[rowId];
                row.Cells["Id"].Value = id.ToString();
                row.Cells["Codigo"].Value = txtcodeproduct.Text.Trim();
                row.Cells["Nombre"].Value = txtnameproduct.Text.Trim();
                row.Cells["Descripcion"].Value = txtdescriptionproduct.Text.Trim();
                row.Cells["Categoria"].Value = comboboxItem.Text;
                row.Cells["Stock"].Value = "0";


            }
            else
            {
                result = ProductService.Instance.UpdateProduct(product);
                if (!result) return;

                DataGridViewRow row = dgdataproduct.Rows[txtIndexProductParse - 1];
                row.Cells["Id"].Value = txtidproduct.Text;
                row.Cells["Codigo"].Value = txtcodeproduct.Text.Trim();
                row.Cells["Nombre"].Value = txtnameproduct.Text.Trim();
                row.Cells["Descripcion"].Value = txtdescriptionproduct.Text.Trim();
                row.Cells["Categoria"].Value = comboboxItem.Text;


            }

            if (result)
                CleanProduct();
            else
                MessageBox.Show("No se pudo guardar los cambios\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

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

            if (e.ColumnIndex != 0) return ;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            var w = Properties.Resources.check20.Width;
            var h = Properties.Resources.check20.Height;
            var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
            var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

            e.Graphics.DrawImage(Properties.Resources.check20, new Rectangle(x, y, w, h));
            e.Handled = true;

        }

        private void btnCleanProduct_Click(object sender, EventArgs e)
        {
            CleanProduct();
        }


        private void CleanProduct()
        {
            txtindexproduct.Text = "0";
            txtidproduct.Text = "0";
            txtcodeproduct.Text = "";
            txtnameproduct.Text = "";
            txtdescriptionproduct.Text = "";
            if(cbocategory.SelectedValue != null)
            {
               cbocategory.SelectedIndex = 0;
            }
        }

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

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            int txtIndexParse = int.Parse(txtindexproduct.Text);
            if (txtIndexParse > 0)
            {
                if (MessageBox.Show("¿Desea eliminar el producto?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {

                    bool result = ProductService.Instance.DeleteProduct(int.Parse(txtidproduct.Text));
                    if (result)
                    {
                        dgdataproduct.Rows.RemoveAt(txtIndexParse - 1);
                        CleanProduct();
                    }
                    else
                        MessageBox.Show("No se pudo eliminar el registro\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }



            }
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((ComboBoxItem)cbosearchproduct.SelectedItem).Value.ToString();

            if (dgdataproduct.Rows.Count <= 0) {
                MessageBox.Show("No hay datos para buscar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (string.IsNullOrEmpty(txtsearchproduct.Text)) {
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

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtsearchproduct.Text = "";
            foreach (DataGridViewRow row in dgdataproduct.Rows)
            {
                row.Visible = true;
            }
        }


        #region STORE
        private void btnSaveStore_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
 

            Store store = StoreService.Instance.ListStore();
            string selectedCurrency = ((ComboBoxItem)cbocurrency.SelectedItem).Value.ToString();
            bool isSuccess;
            isSuccess = StoreService.Instance.UpdateStore(new Store()
            {
                document = txttaxid.Text,
                companyName = txtlegalName.Text,
                email = txtemail.Text,
                phone = txtphone.Text,
                address = txtaddress.Text,
                currencyCulture = selectedCurrency,
            });
            if (isSuccess)
            {
                CultureInfoHelper.SetCurrency(selectedCurrency);
            }
            if (store == null && isSuccess) {

                MessageBox.Show("Se guardaron los datos ingresados", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (store != null && isSuccess)
            {
                MessageBox.Show("Se actualizaron los datos ingresados exitosamente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se pudo guardar los datos ingresados\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }





        }
        #endregion
        private bool ValidateForm()
        {
            var errors = new List<string>();


            
            foreach (var camp in sectionWithRules[tabManagement.SelectedTab.Name])
            {
                Control control = camp.Key;

                foreach (var rulePassword in camp.Value)
                {
                    var rule = Validations.rules[rulePassword];
                    string value = "";
                    if(control is TextBox txt)
                    {
                        value = txt.Text.Trim();
                    }else if (control is ComboBox cbo)
                    {
                        value = (cbo.SelectedItem as ComboBoxItem)?.Value?.ToString() ?? "";
                    }

                    if (!rule.Validate(value))
                    {
                        errors.Add($"{namesMessages[camp.Key.Name]} : {rule.MessageError}");
                    }
                }

            }
            if (errors.Count > 0)
            {
                MessageBox.Show(string.Join("\n", errors), "Errores de Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

    }
}
