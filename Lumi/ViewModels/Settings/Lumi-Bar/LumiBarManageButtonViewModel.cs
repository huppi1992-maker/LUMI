using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Lumi.Infrastructure;
using Lumi.Models;
using System.Collections.Specialized;
using System.ComponentModel;


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
                using (SuppressIsModified()) // Auswahl ist keine inhaltliche Änderung
                {
                    if (SetProperty(ref _selectedButton, value))
                        RaiseCommandStates();
                }
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
            Buttons.CollectionChanged += Buttons_CollectionChanged;

            foreach (var b in Buttons)
                b.PropertyChanged += Button_PropertyChanged;


            SelectedButton = Buttons.FirstOrDefault();

            AddButtonCommand = new RelayCommand(AddButton);
            RemoveSelectedCommand = new RelayCommand(RemoveSelected, () => SelectedButton != null);
            MoveUpCommand = new RelayCommand(MoveUp, () => CanMove(-1));
            MoveDownCommand = new RelayCommand(MoveDown, () => CanMove(+1));
            SaveCommand = new RelayCommand(Save);
            ReloadCommand = new RelayCommand(Reload);

            // Startzustand ist "gespeichert"
            AcceptChanges();
            foreach (var b in Buttons) b.AcceptChanges();
        }

        private void AddButton()
        {
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
            SelectedButton = btn;

            IsModified = true;
            RaiseCommandStates();
        }

        private void RemoveSelected()
        {
            if (SelectedButton == null) return;

            var idx = Buttons.IndexOf(SelectedButton);
            Buttons.Remove(SelectedButton);

            NormalizeOrderInCollection();

            SelectedButton = Buttons.Count == 0
                ? null
                : Buttons.ElementAtOrDefault(idx) ?? Buttons.Last();

            IsModified = true;
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

            IsModified = true;
            RaiseCommandStates();
        }


        private void MoveDown()
        {
            if (!CanMove(+1) || SelectedButton == null) return;

            var idx = Buttons.IndexOf(SelectedButton);
            Buttons.Move(idx, idx + 1);

            NormalizeOrderInCollection();

            IsModified = true;
            RaiseCommandStates();
        }

        private void Save()
        {
            _config.Buttons = Buttons.ToList();
            _service.Save(_config);

            AcceptChanges();
            foreach (var b in Buttons)
                b.AcceptChanges();
        }


        private void Reload()
        {
            using (SuppressIsModified())
            {
                // 1) Unsubscribe von alten Instanzen
                foreach (var b in Buttons)
                    b.PropertyChanged -= Button_PropertyChanged;

                // 2) Config laden
                _config = _service.LoadOrCreateDefault();

                // 3) Collection neu aufbauen
                Buttons.Clear();
                foreach (var b in _config.Buttons.OrderBy(b => b.Order))
                    Buttons.Add(b);

                // 4) Subscribe auf neue Instanzen
                foreach (var b in Buttons)
                    b.PropertyChanged += Button_PropertyChanged;

                // 5) Selection setzen
                SelectedButton = Buttons.FirstOrDefault();
                RaiseCommandStates();
            }

            // 6) Nach Reload ist alles "gespeichert"
            AcceptChanges();
            foreach (var b in Buttons)
                b.AcceptChanges();
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
        private void Buttons_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Add/Remove/Move gilt als Änderung
            IsModified = true;

            if (e.OldItems != null)
                foreach (var item in e.OldItems.OfType<LumiBarButtonDefinition>())
                    item.PropertyChanged -= Button_PropertyChanged;

            if (e.NewItems != null)
                foreach (var item in e.NewItems.OfType<LumiBarButtonDefinition>())
                    item.PropertyChanged += Button_PropertyChanged;

            RaiseCommandStates();
        }

        private void Button_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Nur reagieren, wenn eine relevante Property geändert wurde
            // (IsModified selbst feuert auch PropertyChanged, das ignorieren wir)
            if (e.PropertyName == nameof(IsModified))
            {
                if (sender is LumiBarButtonDefinition b && b.IsModified)
                    IsModified = true;

                return;
            }

            // Jede andere Property-Änderung am Button => Manage-VM modified
            IsModified = true;
        }

    }
}
