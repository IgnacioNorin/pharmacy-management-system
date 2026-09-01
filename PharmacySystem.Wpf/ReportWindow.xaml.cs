using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using ComboBoxItem = PharmacySystem.Model.ComboBoxItem;

namespace PharmacySystem.Ui
{
    // WPF port of frmReport. Four report tabs (Ventas / Compras / Productos / Historial de
    // alertas), each with its own date range / filter, a read-only grid, a totals strip, and an
    // export button backed by IReportExporter. The presenter runs each consult on a thread-pool
    // thread, so date/combo inputs are snapshotted on the UI thread before the run and the grid
    // writes are marshalled back onto the dispatcher.
    public partial class ReportWindow : Wpf.Ui.Controls.FluentWindow, IReportView
    {
        private readonly ReportPresenter _presenter;
        private readonly ReportPermissions _permissions;

        private static readonly IReportExporter[] Exporters =
        {
            new XlsxReportExporter(),
            new CsvReportExporter(),
            new PdfReportExporter()
        };

        // Last consulted report per tab, kept typed so an export walks the same definition the
        // grid does (the exporters add the totals row; the grid stays without it).
        private ReportDefinition<SaleReportRow> _saleDefinition = null!;
        private ReportResult<SaleReportRow> _saleResult = null!;
        private ReportDefinition<PurchaseReportRow> _purchaseDefinition = null!;
        private ReportResult<PurchaseReportRow> _purchaseResult = null!;
        private ReportDefinition<ProductReportRow> _productDefinition = null!;
        private ReportResult<ProductReportRow> _productResult = null!;
        private ReportDefinition<ProductAlertHistoryEntry> _alertHistoryDefinition = null!;
        private ReportResult<ProductAlertHistoryEntry> _alertHistoryResult = null!;

        // Snapshot of the filter inputs, taken on the UI thread right before a consult runs;
        // the IReportView getters return these because the presenter reads them off-thread.
        private DateTime _saleStart, _saleEnd, _purchaseStart, _purchaseEnd, _alertStart, _alertEnd;
        private string _supplierId = "0", _categoryId = "0", _saleClientId = "0";

        public ReportWindow(Func<IReportView, ReportPresenter> presenterFactory, ReportPermissions permissions)
        {
            InitializeComponent();

            _permissions = permissions ?? new ReportPermissions();
            _presenter = presenterFactory(this);
            Loaded += ReportWindow_Loaded;
        }

        private void ReportWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _presenter.OnLoad();

            // Start the "from" pickers at the first day of the current month so a freshly opened
            // report is not empty by default; the "to" pickers stay at today.
            DateTime today = DateTime.Today;
            DateTime monthStart = new DateTime(today.Year, today.Month, 1);
            foreach (DatePicker picker in new[] { dpSaleStart, dpPurchaseStart, dpAlertStart })
            {
                picker.DisplayDateEnd = today;
                picker.SelectedDate = monthStart;
            }
            foreach (DatePicker picker in new[] { dpSaleEnd, dpPurchaseEnd, dpAlertEnd })
            {
                picker.DisplayDateEnd = today;
                picker.SelectedDate = today;
            }

            // Each report type has its own view permission: drop the tab the role cannot see, and
            // enable its export only with the matching "<tipo>.exportar" permission on top.
            if (!_permissions.Sales) tabControl.Items.Remove(tabSale);
            if (!_permissions.Purchases) tabControl.Items.Remove(tabPurchase);
            if (!_permissions.Products) tabControl.Items.Remove(tabProduct);
            if (!_permissions.AlertHistory) tabControl.Items.Remove(tabAlertHistory);

            btnExportSale.IsEnabled = _permissions.Sales && _permissions.SalesExport;
            btnExportPurchases.IsEnabled = _permissions.Purchases && _permissions.PurchasesExport;
            btnExportProduct.IsEnabled = _permissions.Products && _permissions.ProductsExport;
            btnExportAlertHistory.IsEnabled = _permissions.AlertHistory && _permissions.AlertHistoryExport;
        }

        #region IReportView

        public DateTime SaleStartDate => _saleStart;
        public DateTime SaleEndDate => _saleEnd;
        public DateTime PurchaseStartDate => _purchaseStart;
        public DateTime PurchaseEndDate => _purchaseEnd;
        public DateTime AlertHistoryStartDate => _alertStart;
        public DateTime AlertHistoryEndDate => _alertEnd;
        public string SelectedSupplierId => _supplierId;
        public string SelectedCategoryId => _categoryId;
        public string SelectedSaleClientId => _saleClientId;

        public void LoadSupplierOptions(IReadOnlyList<ComboBoxItem> options) => FillCombo(cboSupplier, options);
        public void LoadCategoryOptions(IReadOnlyList<ComboBoxItem> options) => FillCombo(cboCategory, options);
        public void LoadSaleClientOptions(IReadOnlyList<ComboBoxItem> options) => FillCombo(cboSaleClient, options);

        public void SetSaleReport(ReportDefinition<SaleReportRow> definition, ReportResult<SaleReportRow> result) => RunOnUi(() =>
        {
            _saleDefinition = definition;
            _saleResult = result;
            dgSale.ItemsSource = BuildTable(definition, result).DefaultView;
            lblSaleTotals.Text = TotalsCaption(definition, result);
        });

        public void SetPurchaseReport(ReportDefinition<PurchaseReportRow> definition, ReportResult<PurchaseReportRow> result) => RunOnUi(() =>
        {
            _purchaseDefinition = definition;
            _purchaseResult = result;
            dgPurchase.ItemsSource = BuildTable(definition, result).DefaultView;
            lblPurchaseTotals.Text = TotalsCaption(definition, result);
        });

        public void SetProductReport(ReportDefinition<ProductReportRow> definition, ReportResult<ProductReportRow> result) => RunOnUi(() =>
        {
            _productDefinition = definition;
            _productResult = result;
            dgProduct.ItemsSource = BuildTable(definition, result).DefaultView;
            lblProductTotals.Text = TotalsCaption(definition, result);
        });

        public void SetAlertHistoryReport(ReportDefinition<ProductAlertHistoryEntry> definition, ReportResult<ProductAlertHistoryEntry> result) => RunOnUi(() =>
        {
            _alertHistoryDefinition = definition;
            _alertHistoryResult = result;
            dgAlertHistory.ItemsSource = BuildTable(definition, result).DefaultView;
        });

        #endregion

        private static void FillCombo(ComboBox combo, IReadOnlyList<ComboBoxItem> options)
        {
            combo.Items.Clear();
            foreach (ComboBoxItem item in options) combo.Items.Add(item);
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }

        private static string ComboValue(ComboBox combo) =>
            (combo.SelectedItem as ComboBoxItem)?.Value?.ToString() ?? "0";

        // Marshals action onto the dispatcher. Used because a consult runs on a thread-pool
        // thread; a report window closed mid-run just drops the late result.
        private void RunOnUi(Action action)
        {
            if (Dispatcher.CheckAccess()) { action(); return; }
            try { Dispatcher.Invoke(action); }
            catch (System.ComponentModel.Win32Exception) { }
            catch (InvalidOperationException) { }
        }

        // Builds the string DataTable the grid binds to: data rows only, no totals row, so
        // sorting a column never drags the totals around. Formatting lives here, in the view:
        // the presenter only supplies raw values and their ReportValueType.
        private static DataTable BuildTable<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result)
        {
            DataTable dt = new DataTable();
            foreach (ReportColumn<TRow> column in definition.Columns)
            {
                dt.Columns.Add(column.Header, typeof(string));
            }

            foreach (TRow row in result.Rows)
            {
                dt.Rows.Add(definition.Columns.Select(c => (object)FormatCell(c.Value(row), c.Type)).ToArray());
            }

            return dt;
        }

        private static string TotalsCaption<TRow>(ReportDefinition<TRow> definition, ReportResult<TRow> result)
        {
            if (!result.HasTotals) return "";

            IEnumerable<string> parts = definition.Columns
                .Where(c => c.Type == ReportValueType.Currency || c.Type == ReportValueType.Integer)
                .Select(c => c.Header + " " + FormatCell(c.Value(result.Totals!), c.Type));
            return "Totales:      " + string.Join("       ", parts);
        }

        private static string FormatCell(object? value, ReportValueType type)
        {
            if (value == null) return "";
            if (value is string s) return s;
            if (value is DateTime d) return DateHelper.FormatDatePresentation(d);

            switch (type)
            {
                case ReportValueType.Currency: return CultureInfoHelper.FormatAsCurrency(Convert.ToDecimal(value));
                case ReportValueType.Integer: return Convert.ToInt64(value).ToString();
                default: return value.ToString() ?? "";
            }
        }

        private void SnapshotFilters()
        {
            DateTime today = DateTime.Today;
            _saleStart = dpSaleStart.SelectedDate ?? today;
            _saleEnd = dpSaleEnd.SelectedDate ?? today;
            _purchaseStart = dpPurchaseStart.SelectedDate ?? today;
            _purchaseEnd = dpPurchaseEnd.SelectedDate ?? today;
            _alertStart = dpAlertStart.SelectedDate ?? today;
            _alertEnd = dpAlertEnd.SelectedDate ?? today;
            _supplierId = ComboValue(cboSupplier);
            _categoryId = ComboValue(cboCategory);
            _saleClientId = ComboValue(cboSaleClient);
        }

        private void btnConsultSale_Click(object sender, RoutedEventArgs e) => RunConsult(_presenter.OnConsultSale);
        private void btnConsultPurchase_Click(object sender, RoutedEventArgs e) => RunConsult(_presenter.OnConsultPurchase);
        private void btnConsultProduct_Click(object sender, RoutedEventArgs e) => RunConsult(_presenter.OnConsultProduct);
        private void btnConsultAlertHistory_Click(object sender, RoutedEventArgs e) => RunConsult(_presenter.OnConsultAlertHistory);

        // Runs the consult off the UI thread so a slow date range does not freeze the window.
        // A failure surfaces as a logged error and a message, never a silent no-op. The consult
        // buttons are disabled while it runs so it cannot be launched twice.
        private async void RunConsult(Action consult)
        {
            SnapshotFilters();
            SetConsultButtonsEnabled(false);
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                await Task.Run(consult);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "No se pudo generar el reporte: " + ex.Message, "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                SetConsultButtonsEnabled(true);
            }
        }

        private void SetConsultButtonsEnabled(bool enabled)
        {
            btnConsultSale.IsEnabled = enabled;
            btnConsultPurchase.IsEnabled = enabled;
            btnConsultProduct.IsEnabled = enabled;
            btnConsultAlertHistory.IsEnabled = enabled;
        }

        private void btnExportSale_Click(object sender, RoutedEventArgs e)
        {
            if (!_permissions.SalesExport) return;
            ExportReport("Ventas", RowCount(_saleResult),
                (exporter, stream) => exporter.Export(_saleDefinition, _saleResult, "Ventas", stream));
        }

        private void btnExportPurchases_Click(object sender, RoutedEventArgs e)
        {
            if (!_permissions.PurchasesExport) return;
            ExportReport("Compras", RowCount(_purchaseResult),
                (exporter, stream) => exporter.Export(_purchaseDefinition, _purchaseResult, "Compras", stream));
        }

        private void btnExportProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!_permissions.ProductsExport) return;
            ExportReport("Productos", RowCount(_productResult),
                (exporter, stream) => exporter.Export(_productDefinition, _productResult, "Productos", stream));
        }

        private void btnExportAlertHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!_permissions.AlertHistoryExport) return;
            ExportReport("Historial_Alertas", RowCount(_alertHistoryResult),
                (exporter, stream) => exporter.Export(_alertHistoryDefinition, _alertHistoryResult, "Historial de alertas", stream));
        }

        private static int RowCount<TRow>(ReportResult<TRow> result) => result?.Rows.Count ?? 0;

        // Shared by the four export buttons: the format is chosen from the SaveFileDialog filter
        // (extension -> exporter), the write goes through the matching IReportExporter, and a
        // locked target file is reported specifically instead of as a generic failure.
        private void ExportReport(string baseName, int rowCount, Action<IReportExporter, System.IO.Stream> write)
        {
            if (rowCount == 0)
            {
                MessageBox.Show(this, "No existen datos para exportar", "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            var dialog = new SaveFileDialog
            {
                FileName = baseName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                Filter = string.Join("|", Exporters.Select(x => x.FilterLabel + "|*." + x.Extension)),
                FilterIndex = 1
            };
            if (dialog.ShowDialog(this) != true) return;

            string extension = System.IO.Path.GetExtension(dialog.FileName).TrimStart('.').ToLowerInvariant();
            IReportExporter exporter =
                Exporters.FirstOrDefault(x => x.Extension == extension) ?? Exporters[dialog.FilterIndex - 1];

            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                using (var stream = new System.IO.FileStream(dialog.FileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    write(exporter, stream);
                }
                MessageBox.Show(this, "Reporte generado", "Mensaje", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.IO.IOException ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "No se pudo escribir el archivo. Verifica que no esté abierto en otra aplicación.",
                    "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                MessageBox.Show(this, "Error al generar el reporte: " + ex.Message, "Mensaje",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
    }
}
