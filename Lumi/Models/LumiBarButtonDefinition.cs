using Lumi.Infrastructure;
using Lumi.ViewModels;

namespace Lumi.Models
{
    public sealed class LumiBarButtonDefinition : ViewModelBase
    {
        private string _name = "Neuer Button";
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        private string _label = "";
        public string Label
        {
            get => _label;
            set
            {
                if (_label == value) return;
                _label = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        private string _iconKey = "";
        public string IconKey
        {
            get => _iconKey;
            set
            {
                if (_iconKey == value) return;
                _iconKey = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        private string _fillHex = "#3A7BD5";
        public string FillHex
        {
            get => _fillHex;
            set
            {
                if (_fillHex == value) return;
                _fillHex = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        private string _hoverHex = "#4C8EE6";
        public string HoverHex
        {
            get => _hoverHex;
            set
            {
                if (_hoverHex == value) return;
                _hoverHex = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        private string _pressedHex = "#2E5FA8";
        public string PressedHex
        {
            get => _pressedHex;
            set
            {
                if (_pressedHex == value) return;
                _pressedHex = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }

        public string Id { get; set; } = System.Guid.NewGuid().ToString("N");
        public int Order { get; set; }

        private string _actionId = "";
        public string ActionId
        {
            get => _actionId;
            set
            {
                if (_actionId == value) return;
                _actionId = value;
                OnPropertyChanged();
                LumiBarConfigService.NotifyPreviewChangedDebounced();
            }
        }
    }
}

