using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using PharmacySystem.Model;
using PharmacySystem.Presentation;

namespace PharmacySystem.Wpf
{
    // WPF port of ModalSecurityLog. Implements the same ISecurityLogView; the presenter runs the
    // query off the UI thread (SecurityLogPresenter.OnConsultAsync) and the await resumes here on
    // the dispatcher, so ShowEvents/ShowError touch the grid on the UI thread with no marshalling.
    public partial class SecurityLogWindow : Window, ISecurityLogView
    {
        private readonly SecurityLogPresenter _presenter;

        public SecurityLogWindow(Func<ISecurityLogView, SecurityLogPresenter> presenterFactory)
        {
            InitializeComponent();

            dpFrom.SelectedDate = DateTime.Today.AddDays(-30);
            dpTo.SelectedDate = DateTime.Today;

            _presenter = presenterFactory(this);

            Loaded += async (s, e) => await ConsultAsync();
        }

        public DateTime StartDate => (dpFrom.SelectedDate ?? DateTime.Today.AddDays(-30)).Date;
        public DateTime EndDate => (dpTo.SelectedDate ?? DateTime.Today).Date;

        public void ShowEvents(IReadOnlyList<SecurityEventRow> events)
        {
            dgEvents.ItemsSource = events;
            lblCount.Text = events.Count == 0
                ? "Sin registros en el período."
                : events.Count + " registro(s).";
        }

        public void ShowError(string message) =>
            MessageBox.Show(this, message, "Bitácora", MessageBoxButton.OK, MessageBoxImage.Warning);

        private async void btnConsult_Click(object sender, RoutedEventArgs e) => await ConsultAsync();

        private async Task ConsultAsync()
        {
            btnConsult.IsEnabled = false;
            Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                await _presenter.OnConsultAsync();
            }
            finally
            {
                Cursor = null;
                btnConsult.IsEnabled = true;
            }
        }
    }
}
