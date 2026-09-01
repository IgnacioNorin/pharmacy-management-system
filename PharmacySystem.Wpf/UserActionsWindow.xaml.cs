using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PharmacySystem.Ui
{
    public enum UserAction { None, ResetPassword, Unlock, ToggleActive }

    // Small dialog: shows the user's name / state and returns which per-user admin action the
    // operator picked. Built in code (no XAML).
    public class UserActionsWindow : System.Windows.Window
    {
        public UserAction SelectedAction { get; private set; } = UserAction.None;

        private readonly string _userName;
        private readonly bool _isActive;

        public UserActionsWindow(string userName, string statusText, bool isActive)
        {
            _userName = userName;
            _isActive = isActive;

            Title = "Acciones de usuario";
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;
            SizeToContent = SizeToContent.WidthAndHeight;

            var root = new StackPanel { Margin = new Thickness(24), Width = 360 };
            root.Children.Add(new TextBlock { Text = userName, FontWeight = FontWeights.Bold, FontSize = 16 });
            root.Children.Add(new TextBlock
            {
                Text = "Estado: " + statusText,
                Foreground = (Application.Current?.TryFindResource("MutedTextBrush") as Brush) ?? Brushes.Gray,
                Margin = new Thickness(0, 2, 0, 16)
            });

            root.Children.Add(ActionButton("Restablecer contraseña", UserAction.ResetPassword));
            root.Children.Add(ActionButton("Desbloquear", UserAction.Unlock));
            root.Children.Add(ActionButton(isActive ? "Suspender cuenta" : "Reactivar cuenta", UserAction.ToggleActive));

            var close = new Button
            {
                Content = "Cerrar",
                Height = 38,
                Margin = new Thickness(0, 8, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                IsCancel = true
            };
            root.Children.Add(close);

            Content = root;
        }

        private Button ActionButton(string text, UserAction action)
        {
            var btn = new Button
            {
                Content = text,
                Height = 38,
                Margin = new Thickness(0, 0, 0, 8),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            btn.Click += (s, e) =>
            {
                if (MessageBox.Show(this, ConfirmText(action), "Confirmar",
                        MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                SelectedAction = action;
                DialogResult = true;
            };
            return btn;
        }

        private string ConfirmText(UserAction action) => action switch
        {
            UserAction.ResetPassword => $"¿Restablecer la contraseña de «{_userName}»? Se generará una contraseña temporal.",
            UserAction.Unlock => $"¿Desbloquear la cuenta de «{_userName}»?",
            UserAction.ToggleActive => _isActive
                ? $"¿Suspender la cuenta de «{_userName}»? No podrá iniciar sesión."
                : $"¿Reactivar la cuenta de «{_userName}»?",
            _ => "¿Continuar?"
        };
    }
}
