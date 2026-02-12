using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Lumi.Infrastructure;
using Lumi.Models;

namespace Lumi.ViewModels
{
    public sealed class LumiBarViewModel : ViewModelBase, IDisposable
    {
        private readonly LumiBarConfigService _service = new();
        private LumiBarConfig _config;

        // Optionaler Resolver: ActionId -> ICommand
        // Damit kann später z.B. LumiBar.xaml.cs oder ein ActionRegistry die Commands liefern,
        // ohne dass der ViewModel Views kennen muss.
        private readonly Func<string, ICommand> _actionResolver;

        public ObservableCollection<LumiBarButtonItem> MainButtons { get; } = new();
        public ObservableCollection<LumiBarButtonItem> BottomButtons { get; } = new();

        public ICommand ExitCommand { get; }

        // Cache für Hex -> SolidColorBrush (Frozen)
        private static readonly Dictionary<string, SolidColorBrush> BrushCache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly object BrushCacheLock = new();

        private bool _isDisposed;

        public LumiBarViewModel(Func<string, ICommand>? actionResolver = null)
        {
            _actionResolver = actionResolver ?? ResolveActionFallback;

            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

            _config = _service.LoadOrCreateDefault();

            // Bottom Buttons sind "fix" (nicht Teil der User-Config)
            BuildBottomButtons();

            // Main Buttons aus JSON/AppData
            BuildMainButtonsFromConfig(_config);

            // Live-Preview & Save-Refresh
            LumiBarConfigService.PreviewChanged += OnPreviewChanged;
            LumiBarConfigService.ConfigChanged += OnConfigChanged;
        }

        private void BuildBottomButtons()
        {
            BottomButtons.Clear();

            BottomButtons.Add(new LumiBarButtonItem
            {
                Label = "",
                IconKey = "tdesign_setting_1_filled",
                IconFill = BrushFromHexCached("#3A7BD5"),
                IconFillHover = BrushFromHexCached("#4C8EE6"),
                IconFillPressed = BrushFromHexCached("#2E5FA8"),
                Command = _actionResolver("open_settings")
            });

            BottomButtons.Add(new LumiBarButtonItem
            {
                Label = "",
                IconKey = "tdesign_poweroff",
                IconFill = BrushFromHexCached("#C93A3A"),
                IconFillHover = BrushFromHexCached("#E14B4B"),
                IconFillPressed = BrushFromHexCached("#8F1D1D"),
                Command = ExitCommand
            });
        }

        private void BuildMainButtonsFromConfig(LumiBarConfig config)
        {
            MainButtons.Clear();

            foreach (var def in config.Buttons
                         .Where(b => b.IsEnabled)
                         .OrderBy(b => b.Order))
            {
                MainButtons.Add(new LumiBarButtonItem
                {
                    Label = def.Label ?? "",
                    IconKey = def.IconKey ?? "",
                    IconFill = BrushFromHexCached(def.FillHex, fallbackHex: "#3A7BD5"),
                    IconFillHover = BrushFromHexCached(def.HoverHex, fallbackHex: "#4C8EE6"),
                    IconFillPressed = BrushFromHexCached(def.PressedHex, fallbackHex: "#2E5FA8"),
                    Command = _actionResolver(def.ActionId ?? "")
                });
            }
        }

        private void OnPreviewChanged(object? sender, EventArgs e)
        {
            // Preview: nicht von Disk laden, sondern die letzte bekannte Config verwenden.
            // Das UI der Manage-Ansicht ändert die Definitions direkt, daher reicht ein Rebuild.
            BuildMainButtonsFromConfig(_config);
        }

        private void OnConfigChanged(object? sender, EventArgs e)
        {
            // Save: von Disk neu laden, damit die laufende Bar garantiert den persisted State hat.
            _config = _service.LoadOrCreateDefault();
            BuildMainButtonsFromConfig(_config);
        }

        private static SolidColorBrush BrushFromHexCached(string? hex, string fallbackHex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                hex = fallbackHex;

            return BrushFromHexCached(hex);
        }

        private static SolidColorBrush BrushFromHexCached(string hex)
        {
            lock (BrushCacheLock)
            {
                if (BrushCache.TryGetValue(hex, out var cached))
                    return cached;

                // ColorConverter kann throwen, deshalb try/catch + klare Exception
                object? obj;
                try
                {
                    obj = ColorConverter.ConvertFromString(hex);
                }
                catch (Exception ex)
                {
                    throw new FormatException($"Invalid color string: {hex}", ex);
                }

                if (obj is not Color color)
                    throw new FormatException($"Invalid color string: {hex}");

                var brush = new SolidColorBrush(color);
                brush.Freeze();

                BrushCache[hex] = brush;
                return brush;
            }
        }

        private ICommand ResolveActionFallback(string actionId)
        {
            // Fallback-Resolver: verhindert Null-Commands und liefert Platzhalter.
            // Später ideal: ActionRegistry (ActionId -> Action/Command) in Infrastructure.
            if (string.IsNullOrWhiteSpace(actionId))
                return new RelayCommand(() => { });

            return actionId switch
            {
                // Bottom Buttons
                "open_settings" => new RelayCommand(() =>
                {
                    // Platzhalter: später z.B. Hub öffnen und Settings-Tab auswählen
                }),

                // Beispiele für Main Buttons
                "open_hub" => new RelayCommand(() =>
                {
                    // Platzhalter: Hub öffnen
                }),

                "open_lumibar_button_management" => new RelayCommand(() =>
                {
                    // Platzhalter: Manage-Ansicht öffnen
                }),

                _ => new RelayCommand(() => { /* Unknown ActionId */ })
            };
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            LumiBarConfigService.PreviewChanged -= OnPreviewChanged;
            LumiBarConfigService.ConfigChanged -= OnConfigChanged;
        }
    }

    public sealed class LumiBarButtonItem
    {
        public string Label { get; init; } = "";
        public string IconKey { get; init; } = ""; // Referenz auf Resource-Key (Geometry)
        public Brush? IconFill { get; init; }
        public Brush? IconFillHover { get; init; }
        public Brush? IconFillPressed { get; init; }
        public ICommand? Command { get; init; }
    }
}
