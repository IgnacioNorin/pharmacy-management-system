using PharmacySystem.Business;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    // Notification center for the alerts rework (Fase 3/4): a plain data-display dialog, not an
    // MVP screen - it renders the alert list MainForm already computed via
    // MainFormPresenter.RefreshAlerts(), reports back which product (if any) the user picked to
    // view, and lets the user acknowledge an alert (Fase 4: traceability) by calling straight into
    // the notification service it's handed - there is no separate business decision to make here
    // worth a Presenter of its own.
    public partial class ModalAlerts : Form
    {
        public string SelectedProductCode { get; private set; }

        private readonly IReadOnlyList<ProductAlert> _alerts;
        private readonly INotificationConfigService _notificationService;
        private readonly int _currentPersonId;

        public ModalAlerts(IReadOnlyList<ProductAlert> alerts = null, INotificationConfigService notificationService = null, int currentPersonId = 0)
        {
            InitializeComponent();
            _alerts = alerts ?? new List<ProductAlert>();
            _notificationService = notificationService;
            _currentPersonId = currentPersonId;
        }

        private void ModalAlerts_Load(object sender, EventArgs e)
        {
            DataGridViewButtonColumn viewButton = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Width = 60,
                Text = "Ver",
                Name = "btnVer",
                UseColumnTextForButtonValue = true
            };
            DataGridViewButtonColumn ackButton = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Width = 90,
                Text = "Reconocer",
                Name = "btnAck",
                UseColumnTextForButtonValue = true
            };
            DataGridViewButtonColumn muteButton = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Width = 90,
                Text = "Mutear",
                Name = "btnMute",
                UseColumnTextForButtonValue = true
            };

            // Data columns first, action buttons on the right - they read like consequences of the
            // row, not the row's primary identity.
            dgdata.Columns.Add("Severidad", "Severidad");
            dgdata.Columns.Add("Producto", "Producto");
            dgdata.Columns.Add("Detalle", "Detalle");
            dgdata.Columns.Add("Estado", "Estado");
            dgdata.Columns.Add("Codigo", "Código");
            dgdata.Columns.Add("HistoryId", "HistoryId");
            dgdata.Columns.Add(viewButton);
            dgdata.Columns.Add(ackButton);
            dgdata.Columns.Add(muteButton);

            dgdata.Columns["Codigo"].Visible = false;
            dgdata.Columns["HistoryId"].Visible = false;
            dgdata.Columns["Estado"].Width = 90;
            dgdata.Columns["Severidad"].Width = 90;
            dgdata.Columns["Producto"].Width = 180;
            dgdata.Columns["Detalle"].Width = 160;

            foreach (ProductAlert alert in _alerts)
            {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];

                row.Cells["Severidad"].Value = SeverityLabel(alert.Severity);
                row.Cells["Codigo"].Value = alert.Code;
                row.Cells["HistoryId"].Value = alert.HistoryId?.ToString() ?? "";
                row.Cells["Producto"].Value = alert.Name;
                row.Cells["Detalle"].Value = alert.Detail;
                row.Cells["Estado"].Value = StatusLabel(alert);
                row.Cells["btnMute"].Value = alert.MutedAt != null ? "Desmutear" : "Mutear";

                row.Cells["Severidad"].Style.ForeColor = SeverityColor(alert.Severity);
                row.Cells["Severidad"].Style.Font = new Font(dgdata.Font, FontStyle.Bold);

                // No history row to act against (a DB hiccup on the history write - the alert
                // itself is still shown, fail-open, just not actionable this round).
                if (alert.HistoryId == null)
                {
                    row.Cells["btnAck"].Value = "";
                    row.Cells["btnAck"].ReadOnly = true;
                    row.Cells["btnMute"].Value = "";
                    row.Cells["btnMute"].ReadOnly = true;
                }
                else if (alert.AcknowledgedAt != null)
                {
                    row.Cells["btnAck"].Value = "Hecho";
                    row.Cells["btnAck"].ReadOnly = true;
                }
            }

            lblEmpty.Visible = dgdata.Rows.Count == 0;
            dgdata.Visible = dgdata.Rows.Count > 0;
        }

        private static string StatusLabel(ProductAlert alert)
        {
            if (alert.MutedAt != null) return "Muteada";
            if (alert.AcknowledgedAt != null) return "Leída";
            return "Pendiente";
        }

        private static string SeverityLabel(AlertSeverity severity)
        {
            switch (severity)
            {
                case AlertSeverity.Critical: return "Crítico";
                case AlertSeverity.Expired: return "Vencido";
                case AlertSeverity.Low: return "Bajo";
                case AlertSeverity.ExpiringSoon: return "Por vencer";
                default: return "";
            }
        }

        private static Color SeverityColor(AlertSeverity severity)
        {
            switch (severity)
            {
                case AlertSeverity.Critical:
                case AlertSeverity.Expired:
                    return Color.FromArgb(167, 62, 56);
                default:
                    return Color.FromArgb(176, 106, 34);
            }
        }

        private void dgdata_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0) return;

            string colName = dgdata.Columns[e.ColumnIndex].Name;
            dgdata.Cursor = colName == "btnVer" || colName == "btnAck" || colName == "btnMute" ? Cursors.Hand : Cursors.Default;
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            DataGridViewRow row = dgdata.Rows[e.RowIndex];
            string columnName = dgdata.Columns[e.ColumnIndex].Name;

            if (columnName == "btnVer")
            {
                SelectedProductCode = row.Cells["Codigo"].Value.ToString();
                DialogResult = DialogResult.OK;
                Close();
            }
            else if (columnName == "btnAck")
            {
                AcknowledgeRow(row);
            }
            else if (columnName == "btnMute")
            {
                ToggleMuteRow(row);
            }
        }

        private void AcknowledgeRow(DataGridViewRow row)
        {
            if (_notificationService == null) return;

            int? historyId = ParseHistoryId(row);
            if (historyId == null) return;

            string product = row.Cells["Producto"].Value?.ToString() ?? "";
            if (!Confirm($"¿Marcar como reconocida la alerta de \"{product}\"?"))
            {
                return;
            }

            if (_notificationService.AcknowledgeAlert(historyId.Value, _currentPersonId))
            {
                row.Cells["btnAck"].Value = "Hecho";
                row.Cells["btnAck"].ReadOnly = true;
                if (row.Cells["Estado"].Value as string != "Muteada")
                {
                    row.Cells["Estado"].Value = "Leída";
                }
            }
            else
            {
                MessageBox.Show("No se pudo registrar el reconocimiento de la alerta.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void ToggleMuteRow(DataGridViewRow row)
        {
            if (_notificationService == null) return;

            int? historyId = ParseHistoryId(row);
            if (historyId == null) return;

            bool currentlyMuted = (string)row.Cells["btnMute"].Value == "Desmutear";
            string product = row.Cells["Producto"].Value?.ToString() ?? "";
            string confirmMessage = currentlyMuted
                ? $"¿Desmutear la alerta de \"{product}\"? Volverá a contar en el aviso de notificaciones."
                : $"¿Mutear la alerta de \"{product}\"? Dejará de contar en el aviso de notificaciones hasta que cambie.";
            if (!Confirm(confirmMessage))
            {
                return;
            }

            bool ok = currentlyMuted
                ? _notificationService.UnmuteAlert(historyId.Value)
                : _notificationService.MuteAlert(historyId.Value);

            if (ok)
            {
                row.Cells["btnMute"].Value = currentlyMuted ? "Mutear" : "Desmutear";
                row.Cells["Estado"].Value = currentlyMuted
                    ? (row.Cells["btnAck"].Value as string == "Hecho" ? "Leída" : "Pendiente")
                    : "Muteada";
            }
            else
            {
                MessageBox.Show("No se pudo actualizar la alerta.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private static bool Confirm(string message) =>
            MessageBox.Show(message, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        private static int? ParseHistoryId(DataGridViewRow row)
        {
            string rawHistoryId = row.Cells["HistoryId"].Value?.ToString();
            return !string.IsNullOrEmpty(rawHistoryId) && int.TryParse(rawHistoryId, out int historyId) ? historyId : (int?)null;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
