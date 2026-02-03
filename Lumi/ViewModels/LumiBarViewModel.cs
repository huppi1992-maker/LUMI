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
                IconFill = (Brush)new BrushConverter().ConvertFromString("#3A7BD5"),
                IconFillHover = (Brush)new BrushConverter().ConvertFromString("#4C8EE6"),
                IconFillPressed = (Brush)new BrushConverter().ConvertFromString("#2E5FA8"),
                Command = new RelayCommand(() => { /* Settings öffnen */ })
            });

            BottomButtons.Add(new LumiBarButtonItem
            {
                Label = "",
                IconKey = "tdesign_poweroff",
                IconFill = (Brush)new BrushConverter().ConvertFromString("#C93A3A"),
                IconFillHover = (Brush)new BrushConverter().ConvertFromString("#E14B4B"),
                IconFillPressed = (Brush)new BrushConverter().ConvertFromString("#8F1D1D"),
                Command = ExitCommand
            });
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
