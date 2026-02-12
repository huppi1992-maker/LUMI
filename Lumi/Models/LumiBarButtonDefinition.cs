using Lumi.Infrastructure;
using Lumi.ViewModels;

namespace Lumi.Models
{
    public sealed class LumiBarButtonDefinition : ViewModelBase
    {
        private static void PreviewChanged()
            => LumiBarConfigService.NotifyPreviewChangedDebounced();

        private string _name = "Neuer Button";
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value, PreviewChanged);
        }

        private string _label = "";
        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value, PreviewChanged);
        }

        private string _iconKey = "";
        public string IconKey
        {
            get => _iconKey;
            set => SetProperty(ref _iconKey, value, PreviewChanged);
        }

        private string _fillHex = "#3A7BD5";
        public string FillHex
        {
            get => _fillHex;
            set => SetProperty(ref _fillHex, value, PreviewChanged);
        }

        private string _hoverHex = "#4C8EE6";
        public string HoverHex
        {
            get => _hoverHex;
            set => SetProperty(ref _hoverHex, value, PreviewChanged);
        }

        private string _pressedHex = "#2E5FA8";
        public string PressedHex
        {
            get => _pressedHex;
            set => SetProperty(ref _pressedHex, value, PreviewChanged);
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value, PreviewChanged);
        }

        public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
        public int Order { get; set; }

        private string _actionId = "";
        public string ActionId
        {
            get => _actionId;
            set => SetProperty(ref _actionId, value, PreviewChanged);
        }
    }
}
