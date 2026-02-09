using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lumi.Infrastructure;
using Lumi.Models;

namespace Lumi.ViewModels
{
    public sealed class LumiBarManageButtonViewModel : ViewModelBase
    {
        private readonly LumiBarConfigService _service = new();
        private LumiBarConfig _config;

        public ObservableCollection<LumiBarButtonDefinition> Buttons { get; }

        private LumiBarButtonDefinition? _selectedButton;
        public LumiBarButtonDefinition? SelectedButton
        {
            get => _selectedButton;
            set
            {
                _selectedButton = value;
                OnPropertyChanged();
                RaiseCommandStates();
            }
        }

        public ICommand AddButtonCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ReloadCommand { get; }

        public LumiBarManageButtonViewModel()
        {
            _config = _service.LoadOrCreateDefault();

            Buttons = new ObservableCollection<LumiBarButtonDefinition>(
                _config.Buttons.OrderBy(b => b.Order));

            SelectedButton = Buttons.FirstOrDefault();

            AddButtonCommand = new RelayCommand(AddButton);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected, () => SelectedButton != null);

            MoveUpCommand = new RelayCommand(MoveUp, () => CanMove(-1));
            MoveDownCommand = new RelayCommand(MoveDown, () => CanMove(+1));

            SaveCommand = new RelayCommand(Save);
            ReloadCommand = new RelayCommand(Reload);
        }

        private void AddButton()
        {
            var nextOrder = Buttons.Count == 0 ? 0 : Buttons.Max(b => b.Order) + 1;

            var btn = new LumiBarButtonDefinition
            {
                Order = nextOrder,
                Name = "Neuer Button",
                Label = "",
                IconKey = "tdesign_add",
                FillHex = "#3A7BD5",
                HoverHex = "#4C8EE6",
                PressedHex = "#2E5FA8",
                IsEnabled = true,
                ActionId = ""
            };

            Buttons.Add(btn);
            SelectedButton = btn;
            NormalizeOrderInCollection();
            RaiseCommandStates();
        }

        private void RemoveSelected()
        {
            if (SelectedButton == null) return;

            var idx = Buttons.IndexOf(SelectedButton);
            Buttons.Remove(SelectedButton);

            SelectedButton = Buttons.Count == 0
                ? null
                : Buttons.ElementAtOrDefault(idx) ?? Buttons.Last();

            NormalizeOrderInCollection();
            RaiseCommandStates();
        }

        private bool CanMove(int delta)
        {
            if (SelectedButton == null) return false;
            var idx = Buttons.IndexOf(SelectedButton);
            var target = idx + delta;
            return target >= 0 && target < Buttons.Count;
        }

        private void MoveUp()
        {
            if (!CanMove(-1) || SelectedButton == null) return;
            var idx = Buttons.IndexOf(SelectedButton);
            Buttons.Move(idx, idx - 1);
            NormalizeOrderInCollection();
            RaiseCommandStates();
        }

        private void MoveDown()
        {
            if (!CanMove(+1) || SelectedButton == null) return;
            var idx = Buttons.IndexOf(SelectedButton);
            Buttons.Move(idx, idx + 1);
            NormalizeOrderInCollection();
            RaiseCommandStates();
        }

        private void Save()
        {
            _config.Buttons = Buttons.ToList();
            _service.Save(_config);
        }

        private void Reload()
        {
            _config = _service.LoadOrCreateDefault();

            Buttons.Clear();
            foreach (var b in _config.Buttons.OrderBy(b => b.Order))
                Buttons.Add(b);

            SelectedButton = Buttons.FirstOrDefault();
            RaiseCommandStates();
        }

        private void NormalizeOrderInCollection()
        {
            for (int i = 0; i < Buttons.Count; i++)
                Buttons[i].Order = i;
        }

        private void RaiseCommandStates()
        {
            (RemoveSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
