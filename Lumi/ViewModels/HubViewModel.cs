using System.Windows;
using System.Windows.Input;
using Lumi.Infrastructure;

namespace Lumi.ViewModels
{
    public sealed class HubViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;

        // Caching: State bleibt erhalten, Views wechseln ohne Neuinstanzierung
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

        public ICommand ShowHomeCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand CloseCommand { get; }

        public HubViewModel()
        {
            _currentViewModel = Home;

            ShowHomeCommand = new RelayCommand(() => CurrentViewModel = Home);
            ShowSettingsCommand = new RelayCommand(() => CurrentViewModel = Settings);

            ExitCommand = new RelayCommand(() => Application.Current.Shutdown());
            CloseCommand = new RelayCommand(() =>
            {
                // Close über Command ist knifflig ohne Service.
                // Minimal sauber: Window per CommandParameter übergeben (siehe XAML).
            });
        }

        public void CloseWindow(Window window) => window.Close();
    }
}
