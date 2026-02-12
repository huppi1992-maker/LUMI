using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Lumi.Infrastructure;
using Lumi.Models;
using Lumi.ViewModels;
using Lumi.Views;

namespace Lumi
{
    public partial class LumiBar : Window
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
        private bool _isInitializing = true;

        private LumiHub? _hub;
        private bool IsHubVisible => _hub != null && _hub.IsVisible;

        // -----------------------------
        // Hub-Position Persistenz
        // -----------------------------
        private static readonly string HubPosPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LUMI", "hubpos.json");

        private sealed class HubPos
        {
            public double L { get; set; }
            public double T { get; set; }
        }

        private static HubPos? LoadHubPos()
        {
            try
            {
                if (!File.Exists(HubPosPath))
                    return null;

                var json = File.ReadAllText(HubPosPath);
                return JsonSerializer.Deserialize<HubPos>(json);
            }
            catch
            {
                return null;
            }
        }

        private static void SaveHubPos(double l, double t)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(HubPosPath)!);

                var json = JsonSerializer.Serialize(new HubPos { L = l, T = t });
                File.WriteAllText(HubPosPath, json);
            }
            catch
            {
                // no-op
            }
        }

        public LumiBar()
        {
            InitializeComponent();

            DataContext = this;
            Loaded += MainWindow_Loaded;
            LumiBarConfigService.ConfigChanged += OnConfigChanged;
            LumiBarConfigService.PreviewChanged += OnPreviewChanged;

            Closed += (_, _) =>
            {
                LumiBarConfigService.ConfigChanged -= OnConfigChanged;
                LumiBarConfigService.PreviewChanged -= OnPreviewChanged;
            };

            // Idle Timer initialisieren
            _idleTimer = new DispatcherTimer { Interval = IdleDelay };
            _idleTimer.Tick += (_, _) =>
            {
                _idleTimer.Stop();
                if (IsHubVisible) return;
                if (!_mouseInside) Collapse();
            };
        }

        private void BuildButtons(IEnumerable<LumiBarButtonDefinition> defs)
        {
            TopButtons.Clear();

            foreach (var def in defs.Where(b => b.IsEnabled).OrderBy(b => b.Order))
            {
                var geo = Application.Current.TryFindResource(def.IconKey) as Geometry ?? Geometry.Empty;

                TopButtons.Add(new LumiBarRuntimeButton
                {
                    Name = def.Name,
                    Label = def.Label,
                    IconData = geo,
                    IconFill = BrushFromHex(def.FillHex),
                    IconFillHover = BrushFromHex(def.HoverHex),
                    IconFillPressed = BrushFromHex(def.PressedHex),
                    Command = ResolveAction(def.ActionId)
                });
            }
        }

        private void OnConfigChanged(object? sender, EventArgs e)
        {
            if (_isInitializing) return;

            Dispatcher.BeginInvoke(new Action(BuildButtonsFromConfig));
        }

        private void OnPreviewChanged(object? sender, EventArgs e)
        {
            if (_isInitializing) return;

            // Wenn Maus über der Bar ist, kein Live-Rebuild (sonst Klicks werden verschluckt)
            if (_mouseInside) return;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var defs = _hub?.Vm?.Settings?.LumiBarManageButtons?.Buttons;
                if (defs != null) BuildButtons(defs);
                else BuildButtonsFromConfig();
            }));
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            BuildButtonsFromConfig();

            ApplyPanelLayout();
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            MaxHeight = screenHeight * MaxHeightRatio;
            SizeToContent = SizeToContent.Height;

            Dispatcher.BeginInvoke(new Action(CenterVertically), DispatcherPriority.ApplicationIdle);

            _idleTimer.Start();
            _isInitializing = false;
        }

        private void BuildButtonsFromConfig()
        {
            var cfg = _configService.LoadOrCreateDefault();
            BuildButtons(cfg.Buttons);
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
            EnsureHubVisible(h => h.Vm.ShowHomeCommand.Execute(null));
        }

        private void OpenLumiBarButtonManagement()
        {
            EnsureHubVisible(h =>
            {
                h.Vm.ShowSettingsCommand.Execute(null);
                h.Vm.Settings.ShowLumiBarManageButtonsCommand.Execute(null);
            });
        }

        private void EnsureHubVisible(Action<LumiHub> onReady)
        {
            if (_hub == null || !_hub.IsVisible)
            {
                var p = LoadHubPos();

                _hub = new LumiHub(HubStart.Home)
                {
                    Owner = this,
                    Left = p?.L ?? (this.Left + this.Width + 10),
                    Top = p?.T ?? this.Top
                };

                RoutedEventHandler? loadedHandler = null;
                loadedHandler = (_, __) =>
                {
                    _hub.Loaded -= loadedHandler!;
                    onReady(_hub);
                };

                _hub.Loaded += loadedHandler;

                _hub.Closed += (_, __) =>
                {
                    // Position genau einmal am Ende speichern (kein IO-Spam beim Ziehen)
                    SaveHubPos(_hub.Left, _hub.Top);

                    _hub = null;
                    RestartIdleTimer();
                };

                _hub.Show();
            }
            else
            {
                _hub.Activate();
                onReady(_hub);
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

            if (IsHubVisible) return;
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
            if (IsHubVisible) return;
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
            // Einheitlicher Codepfad: kein doppeltes Hub-Opening mehr
            EnsureHubVisible(h => h.Vm.ShowSettingsCommand.Execute(null));
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
