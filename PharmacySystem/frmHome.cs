using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PharmacySystem
{
    // Landing screen (Fase 6 of the alerts rework, dashboard follow-up): the day's sales plus the
    // same active-alert computation MainForm's bell badge already runs, so both always agree on
    // what counts as open. Navigation is handed in as callbacks instead of going through
    // CompositionRoot/ShowForm directly - MainForm owns the MDI child lifecycle and highlight
    // state, this form just asks for it.
    public partial class frmHome : Form, IHomeView
    {
        private readonly HomePresenter _presenter;
        private readonly Action _onNewSale;
        private readonly Action _onNewPurchase;
        private readonly Action _onManageProducts;
        private readonly Action<string> _onViewProduct;

        public frmHome(Action onNewSale, Action onNewPurchase, Action onManageProducts, Action<string> onViewProduct)
        {
            InitializeComponent();
            _presenter = CompositionRoot.CreateHomePresenter(this);
            _onNewSale = onNewSale;
            _onNewPurchase = onNewPurchase;
            _onManageProducts = onManageProducts;
            _onViewProduct = onViewProduct;
        }

        public void SetSummary(HomeSummary summary)
        {
            lblSalesValue.Text = CultureInfoHelper.FormatAsCurrency(summary.SalesTodayTotal);
            lblSalesSub.Text = $"{summary.SalesTodayCount} operaciones";

            lblAlertsValue.Text = (summary.UrgentAlertsCount + summary.OtherAlertsCount).ToString();
            lblAlertsSub.Text = $"{summary.UrgentAlertsCount} urgentes · {summary.OtherAlertsCount} pendientes";

            lblExpiringValue.Text = summary.ExpiringSoonCount.ToString();
            lblStockValue.Text = summary.CriticalStockCount.ToString();

            dgAttention.Rows.Clear();
            foreach (ProductAlert alert in summary.AttentionList)
            {
                int rowId = dgAttention.Rows.Add();
                DataGridViewRow row = dgAttention.Rows[rowId];
                row.Cells["Severidad"].Value = SeverityLabel(alert.Severity);
                row.Cells["Severidad"].Style.ForeColor = SeverityColor(alert.Severity);
                row.Cells["Severidad"].Style.Font = new Font(dgAttention.Font, FontStyle.Bold);
                row.Cells["Producto"].Value = alert.Name;
                row.Cells["Detalle"].Value = alert.Detail;
                row.Cells["Codigo"].Value = alert.Code;
            }

            bool hasAttention = dgAttention.Rows.Count > 0;
            dgAttention.Visible = hasAttention;
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            dgAttention.Columns.Add("Severidad", "Severidad");
            dgAttention.Columns.Add("Producto", "Producto");
            dgAttention.Columns.Add("Detalle", "Detalle");
            dgAttention.Columns.Add("Codigo", "Código");

            dgAttention.Columns["Codigo"].Visible = false;
            dgAttention.Columns["Severidad"].Width = 90;
            dgAttention.Columns["Producto"].Width = 220;
            dgAttention.Columns["Detalle"].Width = 380;

            _presenter.OnLoad();
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

        private void dgAttention_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            dgAttention.Cursor = e.RowIndex >= 0 ? Cursors.Hand : Cursors.Default;
        }

        private void dgAttention_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string code = dgAttention.Rows[e.RowIndex].Cells["Codigo"].Value?.ToString();
            if (!string.IsNullOrEmpty(code))
            {
                _onViewProduct(code);
            }
        }

        private void btnNewSale_Click(object sender, EventArgs e) => _onNewSale();

        private void btnNewPurchase_Click(object sender, EventArgs e) => _onNewPurchase();

        private void btnManageProducts_Click(object sender, EventArgs e) => _onManageProducts();
    }
}
