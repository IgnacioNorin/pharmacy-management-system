using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PharmacySystem.Presentation;

namespace PharmacySystem.Ui
{
    // WPF port of ModalConfignotification. Two thresholds (días "por vencer", stock crítico) and
    // a Save button. Built in code.
    public class NotificationConfigWindow : Wpf.Ui.Controls.FluentWindow, INotificationConfigView
    {
        private readonly NotificationConfigPresenter _presenter;
        private readonly TextBox _days = new TextBox { Width = 120, Height = 24 };
        private readonly TextBox _stock = new TextBox { Width = 120, Height = 24 };
        private readonly Button _save = new Button { Content = "Guardar", Width = 110, Height = 30 };

        public NotificationConfigWindow(bool canConfigure, Func<INotificationConfigView, NotificationConfigPresenter> presenterFactory)
        {
            Title = "Configuración de alertas";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            FontFamily = new FontFamily("Segoe UI");
            FontSize = 13;

            _presenter = presenterFactory(this);

            var root = new StackPanel { Margin = new Thickness(16), Width = 360 };

            root.Children.Add(Row("Días para \"por vencer\":", _days));
            root.Children.Add(Row("Stock crítico:", _stock));

            _save.IsEnabled = canConfigure;
            _save.Click += (s, e) => _presenter.OnSave();
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 14, 0, 0) };
            buttons.Children.Add(_save);
            buttons.Children.Add(new Button { Content = "Cerrar", Width = 90, Height = 30, Margin = new Thickness(8, 0, 0, 0), IsCancel = true });
            root.Children.Add(buttons);

            Content = root;

            Loaded += (s, e) => _presenter.OnLoad();
        }

        private static UIElement Row(string label, TextBox box)
        {
            var dp = new DockPanel { Margin = new Thickness(0, 4, 0, 4), LastChildFill = false };
            dp.Children.Add(new TextBlock { Text = label, Width = 200, VerticalAlignment = VerticalAlignment.Center });
            dp.Children.Add(box);
            return dp;
        }

        #region INotificationConfigView

        public string DaysText => _days.Text;
        public string StockText => _stock.Text;
        public void SetDays(string value) => _days.Text = value;
        public void SetStock(string value) => _stock.Text = value;

        public void ShowInvalidValueError() =>
            MessageBox.Show(this, "Ingrese valores válidos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        public void ShowEmptyFieldsError() =>
            MessageBox.Show(this, "No se puede ingresar campos vacíos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        public void ShowSaveSucceeded() =>
            MessageBox.Show(this, "Nueva configuración exitosa.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        public void ShowSaveFailed() =>
            MessageBox.Show(this, "No se pudo guardar. Revise los valores.", "Fallido", MessageBoxButton.OK, MessageBoxImage.Warning);
        public void ShowMessage(string message) =>
            MessageBox.Show(this, message, "Mensaje", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        #endregion
    }
}
