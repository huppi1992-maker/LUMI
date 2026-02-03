using System.Windows.Input;
using Lumi.Infrastructure;

namespace Lumi.ViewModels
{
    public sealed class SettingsViewModel : ViewModelBase
    {
        private ViewModelBase _currentSettingsViewModel;

        public ViewModelBase CurrentSettingsViewModel
        {
            get => _currentSettingsViewModel;
            private set
            {
                _currentSettingsViewModel = value;
                OnPropertyChanged();
            }
        }

        public GeneralAppearanceViewModel Appearance { get; } = new();
        public GeneralLanguageViewModel Language { get; } = new();

        public ICommand ShowGeneralAppearanceCommand { get; }
        public ICommand ShowGeneralLanguageCommand { get; }

        public SettingsViewModel()
        {
            _currentSettingsViewModel = Appearance;

            ShowGeneralAppearanceCommand = new RelayCommand(() => CurrentSettingsViewModel = Appearance);
            ShowGeneralLanguageCommand = new RelayCommand(() => CurrentSettingsViewModel = Language);
        }
    }
}
