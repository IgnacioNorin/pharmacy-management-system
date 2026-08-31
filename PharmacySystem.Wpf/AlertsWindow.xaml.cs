using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PharmacySystem.Business;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of ModalAlerts. Notification center for the alerts rework: renders the alert list
    // MainForm already computed via MainFormPresenter.RefreshAlerts(), reports back which product
    // (if any) the user picked to view, and lets the user acknowledge or mute an alert by calling
    // straight into the notification service it is handed - no Presenter of its own, same as the
    // WinForms version.
    public partial class AlertsWindow : Window
    {
        // One grid row. Mutable fields (status / button captions) raise change notifications so an
        // acknowledge or mute updates the row in place without rebuilding the grid.
        public sealed class AlertRowVm : INotifyPropertyChanged
        {
            public string SeverityLabel { get; set; }
            public Brush SeverityBrush { get; set; }
            public string Product { get; set; }
            public string Detail { get; set; }
            public string Code { get; set; }
            public int? HistoryId { get; set; }

            private string _status;
            public string Status { get => _status; set { _status = value; Raise(nameof(Status)); } }

            private string _ackText;
            public string AckText { get => _ackText; set { _ackText = value; Raise(nameof(AckText)); } }

            private bool _ackEnabled;
            public bool AckEnabled { get => _ackEnabled; set { _ackEnabled = value; Raise(nameof(AckEnabled)); } }

            private string _muteText;
            public string MuteText { get => _muteText; set { _muteText = value; Raise(nameof(MuteText)); } }

            private bool _muteEnabled;
            public bool MuteEnabled { get => _muteEnabled; set { _muteEnabled = value; Raise(nameof(MuteEnabled)); } }

            public event PropertyChangedEventHandler PropertyChanged;
            private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly INotificationConfigService _notificationService;
        private readonly int _currentPersonId;
        private readonly bool _canAcknowledge;
        private readonly bool _canMute;
        private readonly bool _canConfigure;
        private readonly Func<INotificationConfigView, NotificationConfigPresenter> _configPresenterFactory;
        private readonly ObservableCollection<AlertRowVm> _rows = new ObservableCollection<AlertRowVm>();

        // The product code the user clicked "Ver" on; null if the window was just closed.
        public string SelectedProductCode { get; private set; }

        public AlertsWindow(
            IReadOnlyList<ProductAlert> alerts,
            INotificationConfigService notificationService,
            int currentPersonId,
            bool canAcknowledge,
            bool canMute,
            bool canConfigure,
            Func<INotificationConfigView, NotificationConfigPresenter> configPresenterFactory)
        {
            InitializeComponent();

            _notificationService = notificationService;
            _currentPersonId = currentPersonId;
            _canAcknowledge = canAcknowledge;
            _canMute = canMute;
            _canConfigure = canConfigure;
            _configPresenterFactory = configPresenterFactory;

            foreach (ProductAlert alert in alerts ?? new List<ProductAlert>())
            {
                _rows.Add(BuildRow(alert));
            }

            dgData.ItemsSource = _rows;
            lblEmpty.Visibility = _rows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            dgData.Visibility = _rows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private AlertRowVm BuildRow(ProductAlert alert)
        {
            var vm = new AlertRowVm
            {
                SeverityLabel = SeverityText(alert.Severity),
                SeverityBrush = new SolidColorBrush(SeverityColor(alert.Severity)),
                Product = alert.Name,
                Detail = alert.Detail,
                Code = alert.Code,
                HistoryId = alert.HistoryId,
                Status = StatusText(alert)
            };

            if (alert.HistoryId == null)
            {
                // No history row to act against (a DB hiccup on the history write - the alert
                // itself is still shown, fail-open, just not actionable this round).
                vm.AckText = "";
                vm.AckEnabled = false;
                vm.MuteText = "";
                vm.MuteEnabled = false;
            }
            else if (alert.AcknowledgedAt != null)
            {
                vm.AckText = "Hecho";
                vm.AckEnabled = false;
                vm.MuteText = alert.MutedAt != null ? "Desmutear" : "Mutear";
                vm.MuteEnabled = _canMute;
            }
            else
            {
                vm.AckText = "Reconocer";
                vm.AckEnabled = _canAcknowledge;
                vm.MuteText = alert.MutedAt != null ? "Desmutear" : "Mutear";
                vm.MuteEnabled = _canMute;
            }

            return vm;
        }

        private static string StatusText(ProductAlert alert)
        {
            if (alert.MutedAt != null) return "Muteada";
            if (alert.AcknowledgedAt != null) return "Leída";
            return "Pendiente";
        }

        private static string SeverityText(AlertSeverity severity)
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

        private static AlertRowVm RowOf(object sender) =>
            (sender as FrameworkElement)?.DataContext as AlertRowVm;

        private void btnView_Click(object sender, RoutedEventArgs e)
        {
            AlertRowVm row = RowOf(sender);
            if (row == null) return;

            // product.code is nullable: no code -> nothing to search for in Gestión (DEF-17).
            if (string.IsNullOrEmpty(row.Code))
            {
                MessageBox.Show(this,
                    "El producto no tiene un código asignado; no se puede abrir en Gestión.",
                    "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedProductCode = row.Code;
            DialogResult = true;
        }

        private void btnAck_Click(object sender, RoutedEventArgs e)
        {
            AlertRowVm row = RowOf(sender);
            if (row == null || _notificationService == null || row.HistoryId == null) return;

            if (!_canAcknowledge)
            {
                MessageBox.Show(this, "No tiene permiso para reconocer alertas.", "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (!Confirm($"¿Marcar como reconocida la alerta de \"{row.Product}\"?")) return;

            if (_notificationService.AcknowledgeAlert(row.HistoryId.Value, _currentPersonId))
            {
                row.AckText = "Hecho";
                row.AckEnabled = false;
                if (row.Status != "Muteada") row.Status = "Leída";
            }
            else
            {
                MessageBox.Show(this, "No se pudo registrar el reconocimiento de la alerta.", "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void btnMute_Click(object sender, RoutedEventArgs e)
        {
            AlertRowVm row = RowOf(sender);
            if (row == null || _notificationService == null || row.HistoryId == null) return;

            if (!_canMute)
            {
                MessageBox.Show(this, "No tiene permiso para silenciar alertas.", "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            bool currentlyMuted = row.MuteText == "Desmutear";
            string confirmMessage = currentlyMuted
                ? $"¿Desmutear la alerta de \"{row.Product}\"? Volverá a contar en el aviso de notificaciones."
                : $"¿Mutear la alerta de \"{row.Product}\"? Dejará de contar en el aviso de notificaciones hasta que cambie.";
            if (!Confirm(confirmMessage)) return;

            bool ok = currentlyMuted
                ? _notificationService.UnmuteAlert(row.HistoryId.Value)
                : _notificationService.MuteAlert(row.HistoryId.Value, _currentPersonId);

            if (ok)
            {
                row.MuteText = currentlyMuted ? "Mutear" : "Desmutear";
                row.Status = currentlyMuted
                    ? (row.AckText == "Hecho" ? "Leída" : "Pendiente")
                    : "Muteada";
            }
            else
            {
                MessageBox.Show(this, "No se pudo actualizar la alerta.", "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private bool Confirm(string message) =>
            MessageBox.Show(this, message, "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question)
            == MessageBoxResult.Yes;

        private void btnConfig_Click(object sender, RoutedEventArgs e)
        {
            if (_configPresenterFactory == null) return;

            var window = new NotificationConfigWindow(_canConfigure, _configPresenterFactory) { Owner = this };
            window.ShowDialog();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
