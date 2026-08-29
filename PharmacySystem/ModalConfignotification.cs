using PharmacySystem.Presentation;
using System;
using System.Windows.Forms;

namespace PharmacySystem
{
    public partial class ModalConfignotification : Form, INotificationConfigView
    {
        private readonly NotificationConfigPresenter _presenter;

        public ModalConfignotification()
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateNotificationConfigPresenter(this);
        }

        #region INotificationConfigView

        public string DaysText => txtdays.Text;
        public string StockText => txtstock.Text;

        public void SetDays(string value) => txtdays.Text = value;
        public void SetStock(string value) => txtstock.Text = value;

        public void ShowInvalidValueError() =>
            MessageBox.Show("Ingrese valores Validos", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowEmptyFieldsError() =>
            MessageBox.Show("No se puede ingresar campos vacios", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        public void ShowSaveSucceeded() =>
            MessageBox.Show("Nueva Configuracion Exitosa!!", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public void ShowSaveFailed() =>
            MessageBox.Show("No se pudo guardar/revise los valores", "Fallido", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void ShowMessage(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

        #endregion

        private void ModalConfignotificacion_Load(object sender, EventArgs e)
        {
            _presenter.OnLoad();

            btnSaveConfig.Enabled = MainForm.Session?.Can("alertas.configurar") ?? false;
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            _presenter.OnSave();
        }
    }
}
