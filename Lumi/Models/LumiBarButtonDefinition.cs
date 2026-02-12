using System;
using Lumi.Infrastructure;
using Lumi.ViewModels;

namespace Lumi.Models
{
    public sealed class LumiBarButtonDefinition : ViewModelBase
    {
        // Zentraler Hook für Live-Preview beim Tippen/Ändern
        private static void Preview() => LumiBarConfigService.NotifyPreviewChangedDebounced();

        private string _name = "Neuer Button";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, Preview);
        }

        private string _label = "";
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, Preview);
        }

        private string _iconKey = "";
        public string IconKey
        {
            get => _iconKey;
            set => SetProperty(ref _iconKey, value, Preview);
        }

        private string _fillHex = "#3A7BD5";
        public string FillHex
        {
            get => _fillHex;
            set => SetProperty(ref _fillHex, value, Preview);
        }

        private string _hoverHex = "#4C8EE6";
        public string HoverHex
        {
            get => _hoverHex;
            set => SetProperty(ref _hoverHex, value, Preview);
        }

        private string _pressedHex = "#2E5FA8";
        public string PressedHex
        {
            get => _pressedHex;
            set => SetProperty(ref _pressedHex, value, Preview);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, Preview);
        }

        // ID bleibt setzbar für JSON-Deserialisierung, hat aber einen Default für neue Buttons.
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        // Order wird üblicherweise beim Sortieren/Normalisieren gesetzt.
        // Kein Preview-Trigger nötig, solange sich UI nicht sofort live danach richtet.
        public int Order { get; set; }

        private string _actionId = "";
        public string ActionId
        {
            get => _actionId;
            set => SetProperty(ref _actionId, value, Preview);
        }
    }
}
