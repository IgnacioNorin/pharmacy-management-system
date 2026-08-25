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

            dgdata.Columns.Add(viewButton);
            dgdata.Columns.Add(ackButton);
            dgdata.Columns.Add("Severidad", "Severidad");
            dgdata.Columns.Add("Codigo", "Código");
            dgdata.Columns.Add("HistoryId", "HistoryId");
            dgdata.Columns.Add("Producto", "Producto");
            dgdata.Columns.Add("Detalle", "Detalle");

            dgdata.Columns["Codigo"].Visible = false;
            dgdata.Columns["HistoryId"].Visible = false;
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

                row.Cells["Severidad"].Style.ForeColor = SeverityColor(alert.Severity);
                row.Cells["Severidad"].Style.Font = new Font(dgdata.Font, FontStyle.Bold);

                // No history row to acknowledge against (a DB hiccup on the history write - the
                // alert itself is still shown, fail-open, just not acknowledgeable this round).
                if (alert.HistoryId == null)
                {
                    row.Cells["btnAck"].Value = "";
                    row.Cells["btnAck"].ReadOnly = true;
                }
            }

            lblEmpty.Visible = dgdata.Rows.Count == 0;
            dgdata.Visible = dgdata.Rows.Count > 0;
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
            dgdata.Cursor = colName == "btnVer" || colName == "btnAck" ? Cursors.Hand : Cursors.Default;
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
        }

        private void AcknowledgeRow(DataGridViewRow row)
        {
            if (_notificationService == null) return;

            string rawHistoryId = row.Cells["HistoryId"].Value?.ToString();
            if (string.IsNullOrEmpty(rawHistoryId) || !int.TryParse(rawHistoryId, out int historyId)) return;

            if (_notificationService.AcknowledgeAlert(historyId, _currentPersonId))
            {
                row.Cells["btnAck"].Value = "Hecho";
                row.Cells["btnAck"].ReadOnly = true;
            }
            else
            {
                MessageBox.Show("No se pudo registrar el reconocimiento de la alerta.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
