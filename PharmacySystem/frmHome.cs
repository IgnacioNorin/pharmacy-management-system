using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using System;
using System.Drawing;
using System.Linq;
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
            ApplyPermissions();

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

        // What the landing screen is allowed to show for a given session. The presenter still
        // gates every real action; this only decides which tiles, quick-access buttons and the
        // product drill-down the current role can see. A null session (design time / tests)
        // resolves to everything visible.
        internal struct HomeAccess
        {
            public bool SalesTile;
            public bool AlertTiles;
            public bool AttentionList;
            public bool NewSale;
            public bool NewPurchase;
            public bool ManageProducts;
            public bool ProductDrillDown;
            public bool QuickActionsPanel => NewSale || NewPurchase || ManageProducts;
        }

        internal static HomeAccess ResolveAccess(CurrentUser session)
        {
            // No session -> deny every tile / entry point (DEF-23).
            bool Can(string permission) => session?.Can(permission) ?? false;

            bool canProducts = Can("productos.acceso");
            bool canAlerts = Can("alertas.acceso");

            return new HomeAccess
            {
                SalesTile = Can("ventas.acceso"),
                AlertTiles = canAlerts,
                AttentionList = canAlerts,
                NewSale = Can("ventas.acceso"),
                NewPurchase = Can("compras.acceso"),
                ManageProducts = canProducts,
                ProductDrillDown = canProducts
            };
        }

        private void ApplyPermissions()
        {
            HomeAccess access = ResolveAccess(MainForm.Session);

            pnlTileSales.Visible = access.SalesTile;
            pnlTileAlerts.Visible = access.AlertTiles;
            pnlTileExpiring.Visible = access.AlertTiles;
            pnlTileStock.Visible = access.AlertTiles;
            Restack(new Panel[] { pnlTileSales, pnlTileAlerts, pnlTileExpiring, pnlTileStock },
                    horizontal: true, start: 20, gap: 16);

            btnNewSale.Visible = access.NewSale;
            btnNewPurchase.Visible = access.NewPurchase;
            btnManageProducts.Visible = access.ManageProducts;
            Restack(new Control[] { btnNewSale, btnNewPurchase, btnManageProducts },
                    horizontal: false, start: 12, gap: 12);

            pnlAttention.Visible = access.AttentionList;
            pnlQuickActions.Visible = access.QuickActionsPanel;

            // Pull the quick-actions panel to the left edge when the attention list next to it
            // is hidden, so it does not sit alone against the right margin.
            if (!pnlAttention.Visible && pnlQuickActions.Visible)
            {
                pnlQuickActions.Left = pnlTileSales.Left;
            }
        }

        // Re-flows the visible controls of a row/column so hiding one does not leave a gap.
        private static void Restack(Control[] items, bool horizontal, int start, int gap)
        {
            int offset = start;
            foreach (Control item in items.Where(c => c.Visible))
            {
                if (horizontal)
                {
                    item.Left = offset;
                    offset += item.Width + gap;
                }
                else
                {
                    item.Top = offset;
                    offset += item.Height + gap;
                }
            }
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
            bool clickable = e.RowIndex >= 0 && ResolveAccess(MainForm.Session).ProductDrillDown;
            dgAttention.Cursor = clickable ? Cursors.Hand : Cursors.Default;
        }

        private void dgAttention_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // The row jumps to the product tab in frmManagement; a role without that section
            // must not trigger it (the tab is not even loaded for them).
            if (!ResolveAccess(MainForm.Session).ProductDrillDown) return;

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
