using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    public enum UserAction { None, ResetPassword, Unlock, ToggleActive }

    // Per-user admin actions, opened from the "Acciones" column of the Usuarios grid. A dumb
    // dialog: it shows the selected user's name and state and returns which action the admin
    // picked - frmUser / UserPresenter run it (with all the guards).
    public class ModalUserActions : Form
    {
        public UserAction SelectedAction { get; private set; } = UserAction.None;

        public ModalUserActions(string userName, string statusText, bool isActive)
        {
            Text = "Acciones de usuario";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(360, 230);
            Font = new Font("Segoe UI", 9F);

            Controls.Add(new Label
            {
                Location = new Point(16, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Text = userName
            });
            Controls.Add(new Label
            {
                Location = new Point(16, 38),
                AutoSize = true,
                ForeColor = Color.Gray,
                Text = "Estado: " + statusText
            });

            AddActionButton("Restablecer contraseña", 66, UserAction.ResetPassword);
            AddActionButton("Desbloquear", 108, UserAction.Unlock);
            AddActionButton(isActive ? "Suspender cuenta" : "Reactivar cuenta", 150, UserAction.ToggleActive);

            var btnClose = new Button
            {
                Text = "Cerrar",
                Location = new Point(232, 192),
                Size = new Size(112, 28),
                DialogResult = DialogResult.Cancel
            };
            Controls.Add(btnClose);
            CancelButton = btnClose;
        }

        private void AddActionButton(string text, int top, UserAction action)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(16, top),
                Size = new Size(328, 34),
                TextAlign = ContentAlignment.MiddleLeft,
                UseVisualStyleBackColor = true
            };
            btn.Click += (s, e) =>
            {
                SelectedAction = action;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btn);
        }
    }
}
