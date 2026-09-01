using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of frmHome. Landing content hosted inside MainWindow: the day's sales plus the
    // same active-alert computation the bell badge runs. HomePresenter is unchanged. Navigation
    // from the quick-action buttons and the attention list is handed in as callbacks - MainWindow
    // owns the actual navigation.
    public partial class HomeView : UserControl, IHomeView
    {
        public sealed class AttentionRowVm
        {
            public string Severity { get; set; } = string.Empty;
            public Brush SeverityBrush { get; set; } = Brushes.Transparent;
            public string Product { get; set; } = string.Empty;
            public string Detail { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }

        private readonly HomePresenter _presenter;
        private readonly Func<CurrentUser> _currentSession;
        private readonly Action _onNewSale;
        private readonly Action _onNewPurchase;
        private readonly Action _onManageProducts;
        private readonly Action<string> _onViewProduct;
        private readonly ObservableCollection<AttentionRowVm> _attention = new ObservableCollection<AttentionRowVm>();

        public HomeView(
            Func<IHomeView, HomePresenter> presenterFactory,
            Func<CurrentUser> currentSession,
            Action onNewSale,
            Action onNewPurchase,
            Action onManageProducts,
            Action<string> onViewProduct)
        {
            InitializeComponent();

            _currentSession = currentSession;
            _onNewSale = onNewSale;
            _onNewPurchase = onNewPurchase;
            _onManageProducts = onManageProducts;
            _onViewProduct = onViewProduct;
            _presenter = presenterFactory(this);

            dgAttention.ItemsSource = _attention;

            Loaded += (s, e) =>
            {
                ApplyPermissions();
                _presenter.OnLoad();
            };
        }

        public void SetSummary(HomeSummary summary)
        {
            lblSalesValue.Text = CultureInfoHelper.FormatAsCurrency(summary.SalesTodayTotal);
            lblSalesSub.Text = $"{summary.SalesTodayCount} operaciones";

            lblAlertsValue.Text = (summary.UrgentAlertsCount + summary.OtherAlertsCount).ToString();
            lblAlertsSub.Text = $"{summary.UrgentAlertsCount} urgentes · {summary.OtherAlertsCount} pendientes";

            lblExpiringValue.Text = summary.ExpiringSoonCount.ToString();
            lblStockValue.Text = summary.CriticalStockCount.ToString();

            _attention.Clear();
            foreach (ProductAlert alert in summary.AttentionList)
            {
                _attention.Add(new AttentionRowVm
                {
                    Severity = SeverityLabel(alert.Severity),
                    SeverityBrush = new SolidColorBrush(SeverityColor(alert.Severity)),
                    Product = alert.Name,
                    Detail = alert.Detail,
                    Code = alert.Code
                });
            }
            dgAttention.Visibility = _attention.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // What the landing screen may show for the current session. The presenter still gates
        // every real action; this only decides which tiles / buttons / drill-down are visible.
        private void ApplyPermissions()
        {
            HomeAccess access = HomeAccess.Resolve(_currentSession?.Invoke());

            tileSales.Visibility = Vis(access.SalesTile);
            tileAlerts.Visibility = tileExpiring.Visibility = tileStock.Visibility = Vis(access.AlertTiles);

            btnNewSale.Visibility = Vis(access.NewSale);
            btnNewPurchase.Visibility = Vis(access.NewPurchase);
            btnManageProducts.Visibility = Vis(access.ManageProducts);

            pnlAttention.Visibility = Vis(access.AttentionList);
            pnlQuickActions.Visibility = Vis(access.QuickActionsPanel);
        }

        private static Visibility Vis(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;

        private bool CanDrillDown() => HomeAccess.Resolve(_currentSession?.Invoke()).ProductDrillDown;

        private void dgAttention_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!CanDrillDown()) return;
            if (!(dgAttention.SelectedItem is AttentionRowVm row) || string.IsNullOrEmpty(row.Code)) return;
            _onViewProduct(row.Code);
        }

        private void btnNewSale_Click(object sender, RoutedEventArgs e) => _onNewSale();
        private void btnNewPurchase_Click(object sender, RoutedEventArgs e) => _onNewPurchase();
        private void btnManageProducts_Click(object sender, RoutedEventArgs e) => _onManageProducts();

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
                    return Color.FromRgb(167, 62, 56);
                default:
                    return Color.FromRgb(176, 106, 34);
            }
        }
    }
}
