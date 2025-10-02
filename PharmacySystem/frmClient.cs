using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PharmacySystem.Model;
using PharmacySystem.Logical;
using PharmacySystem.Validators;

namespace PharmacySystem


{
    public partial class frmClient : Form
    {
        public frmClient()
        {
            InitializeComponent();
        }

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtdocument", "Numero Documento" },
            { "txtname", "Nombre Completo" },
            { "txtaddress", "Dirección" },
            { "txtphone", "Teléfono" },
        };
        private void InitializeValidators()
        {
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtdocument, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtname, new List<string>{ "NotEmpty" } },
                { txtaddress, new List<string>{ "NotEmpty" } },
                { txtphone, new List<string>{ "NotEmpty" } },
            };
        }

        private void frmClient_Load(object sender, EventArgs e)
        {
            InitializeValidators();

            DataGridViewButtonColumn Button = new DataGridViewButtonColumn();
            Button.HeaderText = "Seleccionar";
            Button.Width = 80;
            Button.Text = "";
            Button.Name = "btnSeleccionar";
            Button.UseColumnTextForButtonValue = true;

            dgdata.Columns.Add(Button);
            dgdata.Columns.Add("Id", "Id");
            dgdata.Columns.Add("NumeroDocumento", "Numero Documento");
            dgdata.Columns.Add("NombreCompleto", "Nombre Completo");
            dgdata.Columns.Add("Direccion", "Dirección");
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

            foreach (Person p in PersonService.Instance.ListPerson().Where(p => p.oPersonType.idPersonType == 3).ToList())
            {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                row.Cells["Id"].Value = p.idPerson.ToString();
                row.Cells["NumeroDocumento"].Value = p.document;
                row.Cells["NombreCompleto"].Value = p.name;
                row.Cells["Direccion"].Value = p.address;
                row.Cells["Telefono"].Value = p.phone;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;
            Person obj = new Person()
            {
                idPerson = int.Parse(txtid.Text),
                document = txtdocument.Text.Trim(),
                name = txtname.Text.Trim(),
                address = txtaddress.Text.Trim(),
                phone = txtphone.Text.Trim(),
                password = "",
                oPersonType = new TypePerson() { idPersonType = 3}
            };

            var result = false;
            if (int.Parse(txtid.Text) == 0)
            {
                result = PersonService.Instance.RegisterPerson(obj);

                if (result) {
                    int rowId = dgdata.Rows.Add();
                    DataGridViewRow row = dgdata.Rows[rowId];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["NumeroDocumento"].Value = txtdocument.Text.Trim();
                    row.Cells["NombreCompleto"].Value = txtname.Text.Trim();
                    row.Cells["Direccion"].Value = txtaddress.Text.Trim();
                    row.Cells["Telefono"].Value = txtphone.Text.Trim();
                }
                

            }
            else {
                result = PersonService.Instance.UpdatePerson(obj);
                if (result)
                {
                    DataGridViewRow row = dgdata.Rows[int.Parse(txtindex.Text) - 1];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["NumeroDocumento"].Value = txtdocument.Text.Trim();
                    row.Cells["NombreCompleto"].Value = txtname.Text.Trim();
                    row.Cells["Direccion"].Value = txtaddress.Text.Trim();
                    row.Cells["Telefono"].Value = txtphone.Text.Trim();
                }
                
            }

            if (result)
                Clean();
            else
                MessageBox.Show("No se pudo guardar los cambios\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0){
                string colname = dgdata.Columns[e.ColumnIndex].Name;
                if (colname != "btnSeleccionar")
                {
                    dgdata.Cursor = Cursors.Default;
                }
                else
                {
                    dgdata.Cursor = Cursors.Hand;
                }
            }
           
        }

        private void dgdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if(e.RowIndex < 0)
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

        private void btnClean_Click(object sender, EventArgs e)
        {
            Clean();
        }
        private void Clean() {
            
            txtindex.Text = "0";
            txtid.Text = "0";
            txtdocument.Text = "";
            txtname.Text = "";
            txtaddress.Text = "";
            txtphone.Text = "";
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgdata.Columns[e.ColumnIndex].Name == "btnSeleccionar")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    txtindex.Text = (index + 1).ToString();
                    txtid.Text = dgdata.Rows[index].Cells["Id"].Value.ToString();
                    txtdocument.Text = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
                    txtname.Text = dgdata.Rows[index].Cells["NombreCompleto"].Value.ToString();
                    txtaddress.Text = dgdata.Rows[index].Cells["Direccion"].Value.ToString();
                    txtphone.Text = dgdata.Rows[index].Cells["Telefono"].Value.ToString();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (int.Parse(txtindex.Text) > 0) {

                if (MessageBox.Show("¿Desea eliminar el cliente?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                    bool result = PersonService.Instance.DeletePerson(int.Parse(txtid.Text));
                    if (result)
                    {
                        dgdata.Rows.RemoveAt(int.Parse(txtindex.Text) - 1);
                        Clean();
                    }
                    else
                        MessageBox.Show("No se pudo eliminar el registro\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }

                

            }
            
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((ComboBoxItem)cbosearch.SelectedItem).Value.ToString();

            if (dgdata.Rows.Count > 0) {
                foreach (DataGridViewRow row in dgdata.Rows)
                {
                    string valor = row.Cells[columnFilter].Value.ToString().Trim();

                    if (row.Cells[columnFilter].Value.ToString().Trim().Contains(txtsearch.Text.Trim()))
                        row.Visible = true;
                    else
                        row.Visible = false;
                }
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
