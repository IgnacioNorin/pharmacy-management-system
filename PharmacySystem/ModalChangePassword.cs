using System;
using System.Drawing;
using System.Windows.Forms;
using PharmacySystem.Presentation;

namespace PharmacySystem
{
    // Password change dialog. Built in code (no Designer file). Two entry points:
    //  - mandatory: opened from the forced-change flow on login; cannot be dismissed without
    //    a successful change (no Cancel button, the X is blocked).
    //  - optional: the "Cambiar contraseña" self-service from MainForm.
    public class ModalChangePassword : Form, IChangePasswordView
    {
        private readonly ChangePasswordPresenter _presenter;
        private readonly bool _mandatory;
        private bool _changed;

        private readonly TextBox _txtCurrent = new TextBox();
        private readonly TextBox _txtNew = new TextBox();
        private readonly TextBox _txtConfirm = new TextBox();

        public ModalChangePassword(int personId, bool mandatory)
        {
            _mandatory = mandatory;
            BuildLayout();
            _presenter = CompositionRoot.CreateChangePasswordPresenter(this, personId);
        }

        private void BuildLayout()
        {
            Text = "Cambiar contraseña";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = !_mandatory;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(400, 220);
            Font = new Font("Segoe UI", 9F);

            if (_mandatory)
            {
                Controls.Add(new Label
                {
                    Location = new Point(16, 12),
                    Size = new Size(368, 34),
                    Text = "Debe cambiar su contraseña antes de continuar."
                });
            }

            int y = _mandatory ? 52 : 16;
            y = AddRow("Contraseña actual", _txtCurrent, y);
            y = AddRow("Nueva contraseña", _txtNew, y);
            y = AddRow("Confirmar contraseña", _txtConfirm, y);

            var btnSave = new Button
            {
                Text = "Guardar",
                Location = new Point(_mandatory ? 224 : 144, y + 8),
                Size = new Size(150, 30)
            };
            btnSave.Click += (s, e) => _presenter.OnSave();
            Controls.Add(btnSave);
            AcceptButton = btnSave;

            if (!_mandatory)
            {
                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    Location = new Point(16, y + 8),
                    Size = new Size(120, 30)
                };
                btnCancel.Click += (s, e) => _presenter.OnCancel();
                Controls.Add(btnCancel);
                CancelButton = btnCancel;
            }

            FormClosing += (s, e) =>
            {
                if (_mandatory && !_changed && e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                }
            };
        }

        private int AddRow(string caption, TextBox box, int y)
        {
            Controls.Add(new Label { Location = new Point(16, y + 4), Size = new Size(150, 20), Text = caption });
            box.Location = new Point(172, y);
            box.Size = new Size(202, 22);
            box.UseSystemPasswordChar = true;
            Controls.Add(box);
            return y + 32;
        }

        string IChangePasswordView.CurrentPassword => _txtCurrent.Text;
        string IChangePasswordView.NewPassword => _txtNew.Text;
        string IChangePasswordView.ConfirmPassword => _txtConfirm.Text;
        bool IChangePasswordView.Mandatory => _mandatory;

        public void ShowError(string message) =>
            MessageBox.Show(message, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public void Close(bool changed)
        {
            _changed = changed;
            DialogResult = changed ? DialogResult.OK : DialogResult.Cancel;
            Close();
        }
    }
}
