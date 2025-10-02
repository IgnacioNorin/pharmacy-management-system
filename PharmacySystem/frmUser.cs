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
    public partial class frmUser : Form
    {
        public frmUser()
        {
            InitializeComponent();
        }

        private Dictionary<TextBox, List<string>> campWithRules = new Dictionary<TextBox, List<string>>();
        private Dictionary<string, string> namesMessages = new Dictionary<string, string>
        {
            { "txtdocument", "Numero Documento" },
            { "txtname", "Nombre Completo" },
            { "txtpassword", "Contraseña" },
            { "txtconfirmpassword", "Confirmar Contraseña" },
        };
        private void InitializeValidators()
        {
            campWithRules = new Dictionary<TextBox, List<string>>
            {
                { txtdocument, new List<string>{ "NotEmpty", "ValidatorRUC/CI" } },
                { txtname, new List<string>{ "NotEmpty" } },
                { txtpassword, new List<string>{ "NotEmpty" } },
                { txtconfirmpassword, new List<string>{ "NotEmpty" } },
            };
        }

        
        private void frmUser_Load(object sender, EventArgs e)
        {
            // Initializes validations for the form

            InitializeValidators();

            var roles = new[]
            {
                new ComboBoxItem() { Value = 1, Text = "Administrador" },
                new ComboBoxItem() { Value = 2, Text = "Empleado" }
            };
            foreach (var rol in roles)
            {
                cborol.Items.Add(rol);
            }

            cborol.DisplayMember = "Text";
            cborol.ValueMember = "Value";
            cborol.SelectedIndex = 0;

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
            dgdata.Columns.Add("NombreCompleto", "Nombre Completo");
            dgdata.Columns.Add("Rol", "Rol");
            dgdata.Columns.Add("Clave", "Clave");

            dgdata.Columns["btnSeleccionar"].Width = 80;
            dgdata.Columns["NumeroDocumento"].Width = 150;
            dgdata.Columns["NombreCompleto"].Width = 260;
            dgdata.Columns["Rol"].Width = 300;
            dgdata.Columns["Id"].Visible = false;
            dgdata.Columns["Clave"].Visible = false;

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

            foreach (Person p in PersonService.Instance.ListPerson().Where(p => p.oPersonType.idPersonType != 3).ToList())
            {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                row.Cells["Id"].Value = p.idPerson.ToString();
                row.Cells["NumeroDocumento"].Value = p.document;
                row.Cells["NombreCompleto"].Value = p.name;
                row.Cells["Rol"].Value = p.oPersonType.description;
                row.Cells["Clave"].Value = p.password;
            }

            

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateForm() || !SpecialValidators()) return;
            bool result = false;
            int txtidParse = int.Parse(txtid.Text);
            int txtindexParse = int.Parse(txtindex.Text);

            if (txtindexParse < 0 || txtindexParse > dgdata.Rows.Count) return;


            var comBoxCborol = (ComboBoxItem)cborol.SelectedItem;

            Person obj = new Person()
            {
                idPerson = txtidParse,
                document = txtdocument.Text.Trim(),
                name = txtname.Text.Trim(),
                address = "",
                phone = "",
                password = txtpassword.Text,
                oPersonType = new TypePerson() { idPersonType = Convert.ToInt32((comBoxCborol.Value.ToString())) }
            };

            
            if (txtidParse  == 0)
            {
                result = PersonService.Instance.RegisterPerson(obj);

                if (!result)
                {
                    MessageBox.Show("Ya existe un usuario con esa Cedula de Identidad", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];
                row.Cells["Id"].Value = txtid.Text;
                row.Cells["NumeroDocumento"].Value = txtdocument.Text.Trim();
                row.Cells["NombreCompleto"].Value = txtname.Text.Trim();
                row.Cells["Rol"].Value = comBoxCborol.Text;
                row.Cells["Clave"].Value = txtpassword.Text;



            }
            else
            {
                result = PersonService.Instance.UpdatePerson(obj);

                if (!result) return;

                DataGridViewRow row = dgdata.Rows[txtindexParse - 1];
                row.Cells["Id"].Value = txtid.Text;
                row.Cells["NumeroDocumento"].Value = txtdocument.Text.Trim();
                row.Cells["NombreCompleto"].Value = txtname.Text.Trim();
                row.Cells["Rol"].Value = comBoxCborol.Text;
                row.Cells["Clave"].Value = txtpassword.Text;


            }

            if (result)
                Clean();
            else
                MessageBox.Show("No se pudo guardar los cambios\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            dgdata.Cursor = dgdata.Columns[e.ColumnIndex].Name == "btnSleccionar" 
                        ? Cursors.Hand 
                        : Cursors.Default;
          

        }

        private void dgdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;


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
            txtname.Text = "";
            txtpassword.Text = "";
            txtconfirmpassword.Text = "";
            if(cborol.SelectedValue != null)
            {
                cborol.SelectedIndex = 0;
            }
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            int item_index;
            if (dgdata.Columns[e.ColumnIndex].Name != "btnSeleccionar" || index < 0) return;

            txtindex.Text = (index + 1).ToString();
            txtid.Text = dgdata.Rows[index].Cells["Id"].Value.ToString();
            txtdocument.Text = dgdata.Rows[index].Cells["NumeroDocumento"].Value.ToString();
            txtname.Text = dgdata.Rows[index].Cells["NombreCompleto"].Value.ToString();
            txtpassword.Text = dgdata.Rows[index].Cells["Clave"].Value.ToString();
            txtconfirmpassword.Text = dgdata.Rows[index].Cells["Clave"].Value.ToString();
            foreach (ComboBoxItem item in cborol.Items)
            {
                if (item.Text == dgdata.Rows[index].Cells["Rol"].Value.ToString())
                {
                    item_index = cborol.Items.IndexOf(item);
                    cborol.SelectedIndex = item_index;
                    break;
                }
            }




        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult;
            bool result;
            int parseTxtId = int.Parse(txtid.Text);
            int parseTxtIndex = int.Parse(txtindex.Text);

            if (parseTxtIndex <= 0) {
                MessageBox.Show("No se pudo eliminar, seleccione un usuario","Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            
            }
            
            dialogResult = MessageBox.Show("¿Desea eliminar el usuario?", "Mensaje", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult != DialogResult.Yes) return;

            result = PersonService.Instance.DeletePerson(parseTxtId);
            if (!result) {
                MessageBox.Show("No se pudo eliminar el registro\nRevise los datos", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            dgdata.Rows.RemoveAt(parseTxtIndex - 1);
            Clean();

        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string columnFilter = ((ComboBoxItem)cbosearch.SelectedItem).Value.ToString();
            string value;

            if (dgdata.Rows.Count <= 0) return;

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

        private bool SpecialValidators()
        {
            if (txtpassword.Text != txtconfirmpassword.Text) {
                 MessageBox.Show("Las contraseñas no coinciden\nRevise nuevamente", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

             return true;
        }

    }
}
