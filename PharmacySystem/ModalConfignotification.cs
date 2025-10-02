using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PharmacySystem.Logical;
using PharmacySystem.Model;

namespace PharmacySystem
{
    public partial class ModalConfignotification : Form
    {
        public ModalConfignotification()
        {
            InitializeComponent();
        }

        private void ModalConfignotificacion_Load(object sender, EventArgs e)
        {
            NotificationConfigService con = new NotificationConfigService();
            txtdays.Text = con.ConfigDay().ToString();
            txtstock.Text = con.ConfigStock().ToString();
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            NotificationConfigService con = new NotificationConfigService();
            int i = 0;
            if (!string.IsNullOrEmpty(txtdays.Text) &&
                 !int.TryParse(txtdays.Text, out i)
              )
            {
                MessageBox.Show("Ingrese valores Validos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (!string.IsNullOrEmpty(txtstock.Text) &&
                 !int.TryParse(txtstock.Text, out i)
              )
                {
                    MessageBox.Show("Ingrese valores Validos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {

                    if (txtdays.Text == "" || txtstock.Text == "")
                    {
                        MessageBox.Show("No se puede ingresar campos vacios", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        NotificationConfig obj = new NotificationConfig()
                        {
                            days = int.Parse(txtdays.Text.Trim()),
                            criticalStock = int.Parse(txtstock.Text.Trim()),
                        };
                        var result = true;
                        result = con.ConfigUpdate(obj);
                        if (result)
                        {
                            MessageBox.Show("Nueva Configuracion Exitosa!!", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se pudo guardar/revise los valores", "Fallido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        
                        
                    }

                }
            }
        }



    }
}
