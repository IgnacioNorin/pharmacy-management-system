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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class frmSupplier : Form
    {
        public frmSupplier()
        {
            InitializeComponent();
        }

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtdocument", "RUC/Número Documento" },
            { "txtcompanyname", "Razón Social" },
            { "txtemail", "Correo" },
            { "txtphone", "Teléfono" },
        };
        private void InitializeValidators()
        {
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtdocument, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtcompanyname, new List<string>{ "NotEmpty" } },
                { txtemail, new List<string>{ "NotEmpty", "ValidateEmail" } },
                { txtphone, new List<string>{ "NotEmpty", "OnlyNumbers" } },
            };
        }

        private void frmSupplier_Load(object sender, EventArgs e)
        {

            InitializeValidators();

            DataGridViewButtonColumn Button = new DataGridViewButtonColumn()
            {
                HeaderText = "Seleccionar",
                Width = 80,
                Text = "",
                Name = "btnSeleccionar",
                UseColumnTextForButtonValue = true,

            };
           

            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("Id", "Id");
            dgdata.Columns.Add("NumeroDocumento", "Numero Documento");
            dgdata.Columns.Add("RazonSocial", "Razon Social");
            dgdata.Columns.Add("Correo", "Correo");
            dgdata.Columns.Add("Telefono", "Telefono");

            dgdata.Columns["Id"].Visible = false;
            foreach (DataGridViewColumn cl in dgdata.Columns)
            {
                if (cl.Visible == true && cl.Name != "btnSeleccionar")
                {
                    cbosearch.Items.Add(new ComboBoxItem() { Value = cl.Name, Text = cl.HeaderText });
                }
            }
            cbosearch.DisplayMember = "Text";
            cbosearch.ValueMember = "Value";
            cbosearch.SelectedIndex = 0;
           

            foreach (Supplier p in SupplierService.Instance.ListSupplier())
            {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                row.Cells["Id"].Value = p.idSupplier.ToString();
                row.Cells["NumeroDocumento"].Value = p.document;
                row.Cells["RazonSocial"].Value = p.companyName;
                row.Cells["Correo"].Value = p.email;
                row.Cells["Telefono"].Value = p.phone;
            }

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            int txtindexParse = int.Parse(txtindex.Text);
            int txtidParse = int.Parse(txtid.Text);
            bool result = false;

            if (txtindexParse < 0 || txtindexParse > dgdata.Rows.Count) return;
 

            Supplier obj = new Supplier()
            {
                idSupplier = txtidParse,
                document = txtdocument.Text.Trim(),
                companyName = txtcompanyname.Text.Trim(),
                email = txtemail.Text.Trim(),
                phone = txtphone.Text.Trim()
            };

            if (txtidParse == 0)
            {
                int id = SupplierService.Instance.RegisterSupplier(obj);

                result = id != 0 ? true : false;

                if (!result)
                {
                    MessageBox.Show("Ya existe un proveedor con esa CI/RUC", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                row.Cells["Id"].Value = id.ToString();
                row.Cells["NumeroDocumento"].Value = txtdocument.Text.Trim();
                row.Cells["RazonSocial"].Value = txtcompanyname.Text.Trim();
                row.Cells["Correo"].Value = txtemail.Text.Trim();
                row.Cells["Telefono"].Value = txtphone.Text.Trim();

            }
            else
            {
                result = SupplierService.Instance.UpdateSupplier(obj);

                if (!result) return;

                DataGridViewRow row = dgdata.Rows[txtindexParse -1];
                row.Cells["Id"].Value = txtid.Text;
                row.Cells["NumeroDocumento"].Value = txtdocument.Text.Trim();
                row.Cells["RazonSocial"].Value = txtcompanyname.Text.Trim();
                row.Cells["Correo"].Value = txtemail.Text.Trim();
                row.Cells["Telefono"].Value = txtphone.Text.Trim();

            }

            if (result)
                Clean();
            else
                MessageBox.Show("No se pudo guardar los cambios\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0) return;
       
            dgdata.Cursor = dgdata.Columns[e.ColumnIndex].Name == "btnSeleccionar" 
                            ? Cursors.Hand 
                            : Cursors.Default;
            

        }

        private void dgdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0)
                return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.All);

            var w = Properties.Resources.check20.Width;
            var h = Properties.Resources.check20.Height;
            var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
            var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;

            e.Graphics.DrawImage(Properties.Resources.check20, new Rectangle(x, y, w, h));
            e.Handled = true;

        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            Clean();
        }

        private void Clean()
        {

            txtindex.Text = "0";
            txtid.Text = "0";
            txtdocument.Text = "";
            txtcompanyname.Text = "";
            txtemail.Text = "";
            txtphone.Text = "";
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            if (dgdata.Columns[e.ColumnIndex].Name != "btnSeleccionar" || index < 0) return;

            txtindex.Text = (index + 1).ToString();
            txtid.Text = dgdata.Rows[index].Cells["Id"].Value.ToString();
            txtdocument.Text = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
            txtcompanyname.Text = dgdata.Rows[index].Cells["RazonSocial"].Value.ToString();
            txtemail.Text = dgdata.Rows[index].Cells["Correo"].Value.ToString();
            txtphone.Text = dgdata.Rows[index].Cells["Telefono"].Value.ToString();

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            int txtindexParse = int.Parse(txtindex.Text);
            int txtintParse = int.Parse(txtid.Text);

            bool result;
            DialogResult dialogResult;
            if (txtindexParse <= 0)
            {
                MessageBox.Show("No se pudo eliminar, seleccione un proveedor", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            dialogResult = MessageBox.Show("¿Desea eliminar el proveedor?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult != DialogResult.Yes) return;

            result = SupplierService.Instance.DeleteSupplier(txtintParse);

            if (!result)
            {
                MessageBox.Show("No se pudo eliminar el registro\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
      
            dgdata.Rows.RemoveAt(txtindexParse - 1);
            Clean();
         
               


        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            if (dgdata.Rows.Count <= 0) return;

            string columnFilter = ((ComboBoxItem)cbosearch.SelectedItem).Value.ToString();
            string value;

            foreach (DataGridViewRow row in dgdata.Rows)
            {
                value = row.Cells[columnFilter].Value.ToString().Trim();

                if (row.Cells[columnFilter].Value.ToString().Trim().Contains(txtsearch.Text.Trim()))
                    row.Visible = true;
                else
                    row.Visible = false;
            }


        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtsearch.Text = "";
            foreach (DataGridViewRow row in dgdata.Rows)
            {
                row.Visible = true;
            }
        }

        private bool ValidateForm()
        {
            var errors = new List<string>();

            foreach (var camp in campWithRules)
            {
                foreach (var rulePassword in camp.Value)
                {
                    var rule = Validations.rules[rulePassword];
                    if (!rule.Validate(camp.Key.Text))
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
