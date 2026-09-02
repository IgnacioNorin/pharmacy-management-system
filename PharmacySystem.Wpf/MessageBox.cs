using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.Ui.Controls;
using WpfButton = System.Windows.Controls.Button;
using SwMessageBoxButton = System.Windows.MessageBoxButton;
using SwMessageBoxImage = System.Windows.MessageBoxImage;
using SwMessageBoxResult = System.Windows.MessageBoxResult;

namespace PharmacySystem.Ui
{
    // Themed replacement for System.Windows.MessageBox. Because this type sits in the same
    // namespace as every code-behind, an unqualified `MessageBox.Show(...)` binds here instead of
    // the Win32 dialog, with no call-site changes. Same signatures, same MessageBoxResult, shown
    // synchronously with ShowDialog().
    internal static class MessageBox
    {
        public static SwMessageBoxResult Show(Window? owner, string text, string caption,
            SwMessageBoxButton button, SwMessageBoxImage icon) =>
            new MessageDialog(text, caption, button, icon) { Owner = ResolveOwner(owner) }.Run();

        // 6-arg overload (defaultResult) - the themed dialog does not need the hint.
        public static SwMessageBoxResult Show(Window? owner, string text, string caption,
            SwMessageBoxButton button, SwMessageBoxImage icon, SwMessageBoxResult defaultResult) =>
            Show(owner, text, caption, button, icon);

        private static Window? ResolveOwner(Window? owner)
        {
            if (owner != null && owner.IsLoaded) return owner;

            Window? fallback = null;
            WindowCollection? windows = Application.Current?.Windows;
            if (windows != null)
            {
                foreach (Window w in windows)
                {
                    if (w is MessageDialog) continue;
                    if (w.IsActive) return w;
                    fallback ??= w;
                }
            }
            return fallback;
        }

        private sealed class MessageDialog : Window
        {
            private SwMessageBoxResult _result = SwMessageBoxResult.None;

            public MessageDialog(string text, string caption, SwMessageBoxButton button, SwMessageBoxImage icon)
            {
                Title = string.IsNullOrEmpty(caption) ? "Mensaje" : caption;
                Width = 440;
                SizeToContent = SizeToContent.Height;
                ResizeMode = ResizeMode.NoResize;
                ShowInTaskbar = false;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                Background = Token("ApplicationBackgroundBrush",
                    (Application.Current?.TryFindResource("AppCanvasBrush") as Brush) ?? Brushes.White);

                var body = new Grid { Margin = new Thickness(24, 20, 24, 20) };
                body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                (SymbolRegular glyph, Brush brush) = Visuals(icon);
                if (glyph != SymbolRegular.Empty)
                {
                    var symbol = new SymbolIcon
                    {
                        Symbol = glyph,
                        FontSize = 28,
                        Foreground = brush,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(0, 2, 16, 0)
                    };
                    Grid.SetColumn(symbol, 0);
                    Grid.SetRow(symbol, 0);
                    body.Children.Add(symbol);
                }

                var message = new System.Windows.Controls.TextBlock
                {
                    Text = text ?? string.Empty,
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(message, 1);
                Grid.SetRow(message, 0);
                body.Children.Add(message);

                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 20, 0, 0)
                };
                Grid.SetColumn(buttons, 0);
                Grid.SetColumnSpan(buttons, 2);
                Grid.SetRow(buttons, 1);
                foreach ((string label, SwMessageBoxResult value, bool primary, bool isCancel) in Choices(button))
                {
                    var b = new WpfButton
                    {
                        Content = label,
                        MinWidth = 96,
                        Margin = new Thickness(8, 0, 0, 0),
                        IsDefault = primary,
                        IsCancel = isCancel
                    };
                    if (primary) b.SetResourceReference(FrameworkElement.StyleProperty, "PrimaryButton");
                    b.Click += (_, _) => { _result = value; DialogResult = true; };
                    buttons.Children.Add(b);
                }
                body.Children.Add(buttons);

                Content = body;
            }

            public SwMessageBoxResult Run()
            {
                ShowDialog();
                return _result;
            }

            private static Brush Token(string key, Brush fallback) =>
                Application.Current?.TryFindResource(key) as Brush ?? fallback;

            private static (SymbolRegular, Brush) Visuals(SwMessageBoxImage icon) => icon switch
            {
                SwMessageBoxImage.Error =>
                    (SymbolRegular.DismissCircle24, Token("StatusDangerBrush", Brushes.Firebrick)),
                SwMessageBoxImage.Warning =>
                    (SymbolRegular.Warning24, Token("StatusWarningBrush", Brushes.DarkGoldenrod)),
                SwMessageBoxImage.Question =>
                    (SymbolRegular.QuestionCircle24, Brushes.SteelBlue),
                SwMessageBoxImage.Information =>
                    (SymbolRegular.Info24, Brushes.SteelBlue),
                _ => (SymbolRegular.Empty, Brushes.Transparent)
            };

            private static (string, SwMessageBoxResult, bool, bool)[] Choices(SwMessageBoxButton button) => button switch
            {
                SwMessageBoxButton.OKCancel => new[]
                {
                    ("Aceptar", SwMessageBoxResult.OK, true, false),
                    ("Cancelar", SwMessageBoxResult.Cancel, false, true),
                },
                SwMessageBoxButton.YesNo => new[]
                {
                    ("Sí", SwMessageBoxResult.Yes, true, false),
                    ("No", SwMessageBoxResult.No, false, true),
                },
                SwMessageBoxButton.YesNoCancel => new[]
                {
                    ("Sí", SwMessageBoxResult.Yes, true, false),
                    ("No", SwMessageBoxResult.No, false, false),
                    ("Cancelar", SwMessageBoxResult.Cancel, false, true),
                },
                _ => new[] { ("Aceptar", SwMessageBoxResult.OK, true, true) },
            };
        }
    }
}
