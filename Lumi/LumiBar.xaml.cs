using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Lumi.Views;
using Lumi.ViewModels;
using System.Windows.Threading;
using Lumi.Infrastructure;

namespace Lumi
{
    public partial class LumiBar: Window
    {
        public ObservableCollection<LumiBarRuntimeButton> TopButtons { get; } = new();

        private readonly LumiBarConfigService _configService = new();

        private const double PanelWidthRatio = 0.03; // 03 %
        private const double MaxHeightRatio = 0.90; // 90 %
        private double _expandedHeight;

        private const double CollapsedWidth = 6;       // der "Strich" (DIP)
        private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(4); // 4 Sekunden

        private readonly DispatcherTimer _idleTimer;
        private double _expandedWidth;
        private bool _isCollapsed;
        private bool _mouseInside;
        private LumiHub? _hub;

        public LumiBar()
        {
            InitializeComponent();

            DataContext = this;
            Loaded += MainWindow_Loaded;

            // Idle Timer initialisieren
            _idleTimer = new DispatcherTimer { Interval = IdleDelay };
            _idleTimer.Tick += (_, _) =>
            {
                _idleTimer.Stop();
                if (!_mouseInside) Collapse();
            };
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BuildButtonsFromConfig(); // <-- NEU

            ApplyPanelLayout();
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            MaxHeight = screenHeight * MaxHeightRatio;
            SizeToContent = SizeToContent.Height;

            Dispatcher.BeginInvoke(new Action(CenterVertically),
                DispatcherPriority.Loaded);

            SizeChanged += (_, _) => CenterVertically();
            _idleTimer.Start();
        }

        private void BuildButtonsFromConfig()
        {
            var cfg = _configService.LoadOrCreateDefault();

            TopButtons.Clear();

            var defs = cfg.Buttons
                .Where(b => b.IsEnabled)
                .OrderBy(b => b.Order)
                .ToList();

            foreach (var def in defs)
            {
                // IconKey -> Geometry Resource
                var geo = TryFindResource(def.IconKey) as Geometry ?? Geometry.Empty;

                // Hex -> Brush (deine BrushFromHex Helper, die du schon gebaut hast)
                var fill = BrushFromHex(def.FillHex);
                var hover = BrushFromHex(def.HoverHex);
                var pressed = BrushFromHex(def.PressedHex);

                var cmd = ResolveAction(def.ActionId);

                TopButtons.Add(new LumiBarRuntimeButton
                {
                    Name = def.Name,
                    Label = def.Label,
                    IconData = geo,
                    IconFill = fill,
                    IconFillHover = hover,
                    IconFillPressed = pressed,
                    Command = cmd
                });
            }
        }

        private ICommand ResolveAction(string actionId)
        {
            return actionId switch
            {
                "open_hub" => new RelayCommand(OpenHub),
                "open_lumibar_button_management" => new RelayCommand(OpenLumiBarButtonManagement),
                _ => new RelayCommand(() => { }) // Unknown Action: no-op
            };
        }

        private void OpenHub()
        {
            // Minimal: Hub öffnen und auf Home springen
            EnsureHubVisible();
            _hub?.Vm.ShowHomeCommand.Execute(null);
        }

        private void OpenLumiBarButtonManagement()
        {
            EnsureHubVisible();
            // 1) Hub auf Settings-View wechseln
            _hub?.Vm.ShowSettingsCommand.Execute(null);

            // 2) Innerhalb Settings die gewünschte Unterseite aktivieren
            _hub?.Vm.Settings.ShowLumiBarManageButtonsCommand.Execute(null);
        }

        private void EnsureHubVisible()
        {
            if (_hub == null || !_hub.IsVisible)
            {
                _hub = new LumiHub(HubStart.Home) // oder HubStart.Settings je nachdem
                {
                    Owner = this,
                    Left = this.Left + this.Width + 10,
                    Top = this.Top
                };

                _hub.Closed += (_, __) => _hub = null;
                _hub.Show();
            }
            else
            {
                _hub.Activate();
            }
        }

        private static SolidColorBrush BrushFromHex(string hex)
        {
            var obj = ColorConverter.ConvertFromString(hex);
            if (obj is not Color c)
                throw new FormatException($"Invalid color string: {hex}");

            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }

        private void CenterVertically()
        {
            if (_isCollapsed) return; // eingefroren bleiben
            var screenHeight = SystemParameters.PrimaryScreenHeight;
            Top = (screenHeight - ActualHeight) / 2;
        }


        private void ApplyPanelLayout()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;

            Width = Math.Max(screenWidth * PanelWidthRatio, 60);
            Left = screenWidth - Width;
        }

        // --- Mouse Events ---

        private void Root_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _mouseInside = true;
            _idleTimer.Stop();

            if (_isCollapsed)
                Expand();
        }

        private void Root_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _mouseInside = false;
            RestartIdleTimer();
        }

        private void Root_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Solange sich die Maus über dem Panel bewegt, Timer zurücksetzen
            if (_mouseInside && !_isCollapsed)
                RestartIdleTimer();
        }

        private void RestartIdleTimer()
        {
            _idleTimer.Stop();
            _idleTimer.Start();
        }

        // --- Panel State ---
        private void Collapse()
        {
            if (_isCollapsed) return;

            // aktuelle Höhe merken und fixieren
            _expandedHeight = ActualHeight;

            // WICHTIG: SizeToContent aus, sonst schrumpft die Höhe mit dem Content
            SizeToContent = SizeToContent.Manual;
            Height = _expandedHeight;

            _expandedWidth = Width;
            AnimateWidth(to: CollapsedWidth);

            Left = SystemParameters.PrimaryScreenWidth - CollapsedWidth;

            _isCollapsed = true;
        }

        private void Expand(bool immediate = false)
        {
            if (!_isCollapsed && !immediate) return;

            var target = _expandedWidth > 0
                ? _expandedWidth
                : Math.Max(SystemParameters.PrimaryScreenWidth * PanelWidthRatio, 60);

            if (immediate)
                Width = target;
            else
                AnimateWidth(to: target);

            Left = SystemParameters.PrimaryScreenWidth - target;

            // Höhe wieder dem Content überlassen (mit MaxHeight bleibt’s gedeckelt)
            SizeToContent = SizeToContent.Height;

            _isCollapsed = false;

            RestartIdleTimer();
        }

        private void AnimateWidth(double to)
        {
            var anim = new DoubleAnimation
            {
                To = to,
                Duration = TimeSpan.FromMilliseconds(100),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            BeginAnimation(WidthProperty, anim);
        }

        private void SquareButton_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Button btn)
            {
                btn.Height = btn.ActualWidth;
            }
        }

        // Button Funktionalität
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_hub == null || !_hub.IsVisible)
            {
                _hub = new LumiHub(HubStart.Settings)
                {
                    Owner = this,
                    Left = this.Left + this.Width + 10,
                    Top = this.Top
                };

                _hub.Closed += (_, __) => _hub = null;
                _hub.Show();
            }
            else
            {
                _hub.Activate();
                _hub.Vm.ShowSettingsCommand.Execute(null);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}