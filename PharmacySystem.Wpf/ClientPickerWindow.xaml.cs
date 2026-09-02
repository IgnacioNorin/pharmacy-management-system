using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of ModalPerson. Read-only client picker: implements IClientPickerView (initial
    // load only); filtering and selection stay in the view. Picked is the full ClientRow so a
    // caller that needs the fiscal profile has it.
    public partial class ClientPickerWindow : Wpf.Ui.Controls.FluentWindow, IClientPickerView
    {
        private readonly ClientPickerPresenter _presenter;
        private List<ClientRow> _all = new List<ClientRow>();

        public ClientRow? Picked { get; private set; }

        public ClientPickerWindow(Func<IClientPickerView, ClientPickerPresenter> presenterFactory)
        {
            InitializeComponent();
            _presenter = presenterFactory(this);
            Loaded += (s, e) => _presenter.OnLoad();
        }

        public void LoadClients(IEnumerable<ClientRow> clients)
        {
            _all = clients.ToList();
            dgClients.ItemsSource = _all;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string term = txtSearch.Text.Trim();
            dgClients.ItemsSource = string.IsNullOrEmpty(term)
                ? _all
                : _all.Where(c => Contains(c.Document, term) || Contains(c.Name, term)
                                  || Contains(c.BusinessName, term) || Contains(c.Email, term)).ToList();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            dgClients.ItemsSource = _all;
        }

        private static bool Contains(string value, string term) =>
            (value ?? "").IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        private void dgClients_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => SelectCurrent();
        private void btnSelect_Click(object sender, RoutedEventArgs e) => SelectCurrent();

        private void SelectCurrent()
        {
            if (dgClients.SelectedItem is ClientRow row)
            {
                Picked = row;
                DialogResult = true;
            }
        }
    }
}
