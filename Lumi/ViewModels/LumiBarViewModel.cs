using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using Lumi.Infrastructure;

namespace Lumi.ViewModels
{
    public sealed class LumiBarViewModel : ViewModelBase
    {
        public ObservableCollection<LumiBarButtonItem> MainButtons { get; } = new();
        public ObservableCollection<LumiBarButtonItem> BottomButtons { get; } = new();

        public ICommand ExitCommand { get; }

        public LumiBarViewModel()
        {
            ExitCommand = new RelayCommand(() =>
            {
                System.Windows.Application.Current.Shutdown();
            });

            // Beispiel-Buttons (später aus JSON/AppData laden)
            MainButtons.Add(new LumiBarButtonItem
            {
                Label = "1",
                IconKey = "tdesign_houses_2",
                Command = new RelayCommand(() => { /* Placeholder */ })
            });

            MainButtons.Add(new LumiBarButtonItem
            {
                Label = "2",
                IconKey = "tdesign_add",
                Command = new RelayCommand(() => { /* Placeholder */ })
            });

            BottomButtons.Add(new LumiBarButtonItem
            {
                Label = "",
                IconKey = "tdesign_setting_1_filled",
                IconFill = BrushFromHex("#3A7BD5"),
                IconFillHover = BrushFromHex("#4C8EE6"),
                IconFillPressed = BrushFromHex("#2E5FA8"),
                Command = new RelayCommand(() => { /* Settings öffnen */ })
            });

            BottomButtons.Add(new LumiBarButtonItem
            {
                Label = "",
                IconKey = "tdesign_poweroff",
                IconFill = BrushFromHex("#C93A3A"),
                IconFillHover = BrushFromHex("#E14B4B"),
                IconFillPressed = BrushFromHex("#8F1D1D"),
                Command = ExitCommand
            });
        }

        // Hilfsmethode zum Erstellen von Brushes aus Hex-Farbcodes
        private static SolidColorBrush BrushFromHex(string hex)
        {
            // ColorConverter liefert object? -> wir prüfen explizit
            var obj = ColorConverter.ConvertFromString(hex);
            if (obj is not Color c)
                throw new FormatException($"Invalid color string: {hex}");

            // Freezable: frieren = weniger Memory/CPU, keine Änderungen mehr möglich (für UI-Brush perfekt)
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
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
