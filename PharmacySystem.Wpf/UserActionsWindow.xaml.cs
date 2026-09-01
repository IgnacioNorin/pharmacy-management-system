using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PharmacySystem.Ui
{
    public enum UserAction { None, ResetPassword, Unlock, ToggleActive }

    // WPF port of ModalUserActions. Dumb dialog: shows the user's name/state and returns which
    // per-user admin action the operator picked. Built in code (no XAML).
    public class UserActionsWindow : Window
    {
        public UserAction SelectedAction { get; private set; } = UserAction.None;

        public UserActionsWindow(string userName, string statusText, bool isActive)
        {
            Title = "Acciones de usuario";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;
            FontFamily = new FontFamily("Segoe UI");
            FontSize = 13;

            var root = new StackPanel { Margin = new Thickness(16), Width = 340 };
            root.Children.Add(new TextBlock { Text = userName, FontWeight = FontWeights.Bold, FontSize = 15 });
            root.Children.Add(new TextBlock { Text = "Estado: " + statusText, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 12) });

            root.Children.Add(ActionButton("Restablecer contraseña", UserAction.ResetPassword));
            root.Children.Add(ActionButton("Desbloquear", UserAction.Unlock));
            root.Children.Add(ActionButton(isActive ? "Suspender cuenta" : "Reactivar cuenta", UserAction.ToggleActive));

            var close = new Button { Content = "Cerrar", Width = 110, Height = 28, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 8, 0, 0), IsCancel = true };
            root.Children.Add(close);

            Content = root;
        }

        private Button ActionButton(string text, UserAction action)
        {
            var btn = new Button
            {
                Content = text,
                Height = 34,
                Margin = new Thickness(0, 0, 0, 6),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(8, 0, 0, 0)
            };
            btn.Click += (s, e) => { SelectedAction = action; DialogResult = true; };
            return btn;
        }
    }
}
