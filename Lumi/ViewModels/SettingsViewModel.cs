using System;
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

        // Unterseiten
        public GeneralAppearanceViewModel Appearance { get; } = new();
        public GeneralLanguageViewModel Language { get; } = new();
        public LumiBarManageButtonViewModel LumiBarManageButtons { get; } = new();

        // -------- Selected (für optisches Highlight) --------
        private bool _isGeneralLanguageSelected;
        public bool IsGeneralLanguageSelected
        {
            get => _isGeneralLanguageSelected;
            private set => SetProperty(ref _isGeneralLanguageSelected, value);
        }

        private bool _isGeneralAppearanceSelected;
        public bool IsGeneralAppearanceSelected
        {
            get => _isGeneralAppearanceSelected;
            private set => SetProperty(ref _isGeneralAppearanceSelected, value);
        }

        private bool _isLumiBarButtonsSelected;
        public bool IsLumiBarButtonsSelected
        {
            get => _isLumiBarButtonsSelected;
            private set => SetProperty(ref _isLumiBarButtonsSelected, value);
        }

        // -------- Expander Open State (User darf togglen) --------
        private bool _isGeneralExpanded = true;
        public bool IsGeneralExpanded
        {
            get => _isGeneralExpanded;
            set => SetProperty(ref _isGeneralExpanded, value);
        }

        private bool _isLumiHubExpanded;
        public bool IsLumiHubExpanded
        {
            get => _isLumiHubExpanded;
            set => SetProperty(ref _isLumiHubExpanded, value);
        }

        private bool _isLumiBarExpanded;
        public bool IsLumiBarExpanded
        {
            get => _isLumiBarExpanded;
            set => SetProperty(ref _isLumiBarExpanded, value);
        }

        // -------- Navigation Guard Hook --------
        public event Action<ViewModelBase>? RequestNavigateSettings;

        public ICommand ShowGeneralAppearanceCommand { get; }
        public ICommand ShowGeneralLanguageCommand { get; }
        public ICommand ShowLumiBarManageButtonsCommand { get; }

        public SettingsViewModel()
        {
            _currentSettingsViewModel = Appearance;

            // Commands lösen nur Request aus, View entscheidet (Guard)
            ShowGeneralAppearanceCommand = new RelayCommand(() => RequestNavigateSettings?.Invoke(Appearance));
            ShowGeneralLanguageCommand = new RelayCommand(() => RequestNavigateSettings?.Invoke(Language));
            ShowLumiBarManageButtonsCommand = new RelayCommand(() => RequestNavigateSettings?.Invoke(LumiBarManageButtons));

            // Default Auswahl/Highlight
            ApplySettingsNavigation(Appearance, forceExpandForDeepLink: true);
        }

        /// <summary>
        /// Wird von der View aufgerufen, nachdem Guard/Unsaved bestätigt wurde.
        /// </summary>
        public void ApplySettingsNavigation(ViewModelBase target, bool forceExpandForDeepLink = false)
        {
            // Navigation/Highlight soll nicht als "Modified" gelten
            using (SuppressIsModified())
            {
                CurrentSettingsViewModel = target;

                // Selected reset
                IsGeneralLanguageSelected = false;
                IsGeneralAppearanceSelected = false;
                IsLumiBarButtonsSelected = false;

                if (ReferenceEquals(target, Language))
                    IsGeneralLanguageSelected = true;
                else if (ReferenceEquals(target, Appearance))
                    IsGeneralAppearanceSelected = true;
                else if (ReferenceEquals(target, LumiBarManageButtons))
                    IsLumiBarButtonsSelected = true;

                // Expanders NICHT resetten, damit User-Open-State erhalten bleibt.
                // Nur beim Deep-Link gezielt öffnen.
                if (forceExpandForDeepLink)
                {
                    if (ReferenceEquals(target, Language) || ReferenceEquals(target, Appearance))
                        IsGeneralExpanded = true;

                    if (ReferenceEquals(target, LumiBarManageButtons))
                        IsLumiBarExpanded = true;
                }
            }
        }
    }
}
