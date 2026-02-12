using System;
using System.Windows.Input;
using Lumi.Infrastructure;

namespace Lumi.ViewModels
{
    public sealed class HubViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;

        public HomeViewModel Home { get; } = new();
        public SettingsViewModel Settings { get; } = new();

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        // Exit: hast du schon
        public event Action? RequestExit;

        // NEU: Navigation-Request
        public event Action<ViewModelBase>? RequestNavigate;

        public ICommand ShowHomeCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ExitCommand { get; }

        public HubViewModel()
        {
            _currentViewModel = Home;

            // Nur Request auslösen, View entscheidet
            ShowHomeCommand = new RelayCommand(() => RequestNavigate?.Invoke(Home));
            ShowSettingsCommand = new RelayCommand(() => RequestNavigate?.Invoke(Settings));

            ExitCommand = new RelayCommand(() => RequestExit?.Invoke());
        }

        // NEU: nur die View darf das aufrufen, nachdem sie validiert hat
        public void ApplyNavigation(ViewModelBase target)
        {
            CurrentViewModel = target;
        }
    }
}
