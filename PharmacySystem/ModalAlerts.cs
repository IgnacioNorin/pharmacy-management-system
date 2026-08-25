using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    // Notification center for the alerts rework (Fase 3): a plain data-display dialog, not an MVP
    // screen - it has no business decisions of its own, it only renders the alert list MainForm
    // already computed via MainFormPresenter.RefreshAlerts(), and reports back which product (if
    // any) the user picked to view.
    public partial class ModalAlerts : Form
    {
        public string SelectedProductCode { get; private set; }

        private IReadOnlyList<ProductAlert> _alerts = new List<ProductAlert>();

        public ModalAlerts()
        {
            InitializeComponent();
        }

        public void LoadAlerts(IReadOnlyList<ProductAlert> alerts)
        {
            _alerts = alerts;
        }

        private void ModalAlerts_Load(object sender, EventArgs e)
        {
            DataGridViewButtonColumn viewButton = new DataGridViewButtonColumn
            {
                HeaderText = "",
                Width = 70,
                Text = "Ver",
                Name = "btnVer",
                UseColumnTextForButtonValue = true
            };

            dgdata.Columns.Add(viewButton);
            dgdata.Columns.Add("Severidad", "Severidad");
            dgdata.Columns.Add("Codigo", "Código");
            dgdata.Columns.Add("Producto", "Producto");
            dgdata.Columns.Add("Detalle", "Detalle");

            dgdata.Columns["Codigo"].Visible = false;
            dgdata.Columns["Severidad"].Width = 90;
            dgdata.Columns["Producto"].Width = 200;
            dgdata.Columns["Detalle"].Width = 200;

            foreach (ProductAlert alert in _alerts)
            {
                int rowId = dgdata.Rows.Add();
                DataGridViewRow row = dgdata.Rows[rowId];

                row.Cells["Severidad"].Value = SeverityLabel(alert.Severity);
                row.Cells["Codigo"].Value = alert.Code;
                row.Cells["Producto"].Value = alert.Name;
                row.Cells["Detalle"].Value = alert.Detail;

                row.Cells["Severidad"].Style.ForeColor = SeverityColor(alert.Severity);
                row.Cells["Severidad"].Style.Font = new Font(dgdata.Font, FontStyle.Bold);
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

            dgdata.Cursor = dgdata.Columns[e.ColumnIndex].Name == "btnVer" ? Cursors.Hand : Cursors.Default;
        }

        private void dgdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgdata.Columns[e.ColumnIndex].Name != "btnVer") return;

            SelectedProductCode = dgdata.Rows[e.RowIndex].Cells["Codigo"].Value.ToString();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
