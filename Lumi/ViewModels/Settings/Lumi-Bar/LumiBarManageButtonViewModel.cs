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
                // Einheitlicher Setter (Equality-Check + PropertyChanged)
                if (SetProperty(ref _selectedButton, value))
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
            // Order wird danach normalisiert, daher reicht ein plausibler Initialwert
            var btn = new LumiBarButtonDefinition
            {
                Order = Buttons.Count,
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
            NormalizeOrderInCollection();

            // Selection aktualisieren (Setter triggert Command-States)
            SelectedButton = btn;

            // Buttons-Änderungen beeinflussen MoveUp/MoveDown zusätzlich zur Selection
            RaiseCommandStates();
        }

        private void RemoveSelected()
        {
            if (SelectedButton == null) return;

            var idx = Buttons.IndexOf(SelectedButton);
            Buttons.Remove(SelectedButton);

            NormalizeOrderInCollection();

            // Neue Selection: bevorzugt gleicher Index, sonst letztes Element
            SelectedButton = Buttons.Count == 0
                ? null
                : Buttons.ElementAtOrDefault(idx) ?? Buttons.Last();

            // Collection-Änderung beeinflusst MoveUp/MoveDown auch ohne Selection-Change
            RaiseCommandStates();
        }

        private bool CanMove(int delta)
        {
            if (SelectedButton == null) return false;

            var idx = Buttons.IndexOf(SelectedButton);
            if (idx < 0) return false;

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
            // Persistiert die Reihenfolge so, wie sie in der Collection aktuell ist
            _config.Buttons = Buttons.ToList();
            _service.Save(_config);
        }

        private void Reload()
        {
            _config = _service.LoadOrCreateDefault();

            Buttons.Clear();
            foreach (var b in _config.Buttons.OrderBy(b => b.Order))
                Buttons.Add(b);

            NormalizeOrderInCollection();
            SelectedButton = Buttons.FirstOrDefault();

            RaiseCommandStates();
        }

        private void NormalizeOrderInCollection()
        {
            // Stellt sicher, dass Order immer zur sichtbaren Reihenfolge passt
            for (int i = 0; i < Buttons.Count; i++)
                Buttons[i].Order = i;
        }

        private void RaiseCommandStates()
        {
            // CanExecute hängt von Selection und von Position innerhalb der Collection ab
            (RemoveSelectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
