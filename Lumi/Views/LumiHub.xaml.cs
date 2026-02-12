using System.ComponentModel;
using System.Linq;
using System.Windows;
using Lumi.ViewModels;

namespace Lumi.Views
{
    public partial class LumiHub : Window
    {
        public HubViewModel Vm { get; }
        private bool _shutdownAfterClose;

        public LumiHub(HubStart start)
        {
            InitializeComponent();

            Vm = new HubViewModel();
            DataContext = Vm;
            Vm.Settings.RequestNavigateSettings += HandleSettingsNavigateRequest;

            Vm.RequestExit += () =>
            {
                _shutdownAfterClose = true;
                Close();
            };

            // NEU: Navigation Guard
            Vm.RequestNavigate += HandleNavigateRequest;

            Closing += LumiHub_Closing;
            Closed += LumiHub_Closed;

            if (start == HubStart.Settings)
            {
                Vm.ApplyNavigation(Vm.Settings);

                // Direkt auf Lumi-Bar -> Buttons verwalten routen
                Vm.Settings.ApplySettingsNavigation(
                    Vm.Settings.LumiBarManageButtons,
                    forceExpandForDeepLink: true);
            }
            else
            {
                Vm.ApplyNavigation(Vm.Home);
            }

        }

        private void HandleNavigateRequest(ViewModelBase target)
        {
            // Wenn Ziel bereits aktiv: nichts tun
            if (ReferenceEquals(Vm.CurrentViewModel, target))
                return;

            // Nur blocken, wenn man Settings verlässt
            if (ReferenceEquals(Vm.CurrentViewModel, Vm.Settings))
            {
                var manage = Vm.Settings.LumiBarManageButtons;

                var hasUnsaved =
                    manage.IsModified ||
                    manage.Buttons.Any(b => b.IsModified);

                if (hasUnsaved)
                {
                    var result = MessageBox.Show(
                        "Es gibt ungespeicherte Änderungen. Sollen die Änderungen gespeichert werden?",
                        "Ungespeicherte Änderungen",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (manage.SaveCommand.CanExecute(null))
                                manage.SaveCommand.Execute(null);
                            break;

                        case MessageBoxResult.No:
                            if (manage.ReloadCommand.CanExecute(null))
                                manage.ReloadCommand.Execute(null);
                            break;

                        default:
                            // Cancel: Navigation abbrechen
                            return;
                    }
                }
            }

            // Navigation durchführen
            Vm.ApplyNavigation(target);

            // WICHTIG: Wenn wir in Settings wechseln, gewünschte Unterseite setzen + Expander öffnen
            if (ReferenceEquals(target, Vm.Settings))
            {
                Vm.Settings.ApplySettingsNavigation(
                    Vm.Settings.LumiBarManageButtons,
                    forceExpandForDeepLink: true);
            }
        }


        private void LumiHub_Closing(object? sender, CancelEventArgs e)
        {
            var manage = Vm.Settings.LumiBarManageButtons;

            var hasUnsaved =
                manage.IsModified ||
                manage.Buttons.Any(b => b.IsModified);

            if (!hasUnsaved)
                return;

            var result = MessageBox.Show(
                "Es gibt ungespeicherte Änderungen. Sollen die Änderungen gespeichert werden?",
                "Ungespeicherte Änderungen",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    if (manage.SaveCommand.CanExecute(null))
                        manage.SaveCommand.Execute(null);
                    return;

                case MessageBoxResult.No:
                    if (manage.ReloadCommand.CanExecute(null))
                        manage.ReloadCommand.Execute(null);
                    return;

                default:
                    _shutdownAfterClose = false;
                    e.Cancel = true;
                    return;
            }
        }

        private void LumiHub_Closed(object? sender, System.EventArgs e)
        {
            if (_shutdownAfterClose)
                Application.Current.Shutdown();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _shutdownAfterClose = false;
            Close();
        }
        private void HandleSettingsNavigateRequest(ViewModelBase target)
        {
            // Wenn Ziel bereits aktiv: nichts tun
            if (ReferenceEquals(Vm.Settings.CurrentSettingsViewModel, target))
                return;

            // Guard nur, wenn man die LumiBarManageButtons-Seite verlassen will
            if (ReferenceEquals(Vm.Settings.CurrentSettingsViewModel, Vm.Settings.LumiBarManageButtons))
            {
                var manage = Vm.Settings.LumiBarManageButtons;

                var hasUnsaved =
                    manage.IsModified ||
                    manage.Buttons.Any(b => b.IsModified);

                if (hasUnsaved)
                {
                    var result = MessageBox.Show(
                        "Es gibt ungespeicherte Änderungen. Sollen die Änderungen gespeichert werden?",
                        "Ungespeicherte Änderungen",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);

                    switch (result)
                    {
                        case MessageBoxResult.Yes:
                            if (manage.SaveCommand.CanExecute(null))
                                manage.SaveCommand.Execute(null);
                            break;

                        case MessageBoxResult.No:
                            if (manage.ReloadCommand.CanExecute(null))
                                manage.ReloadCommand.Execute(null);
                            break;

                        default:
                            // Cancel -> Navigation abbrechen
                            return;
                    }
                }
            }

            // Navigation durchführen
            Vm.Settings.ApplySettingsNavigation(target);
        }

    }
}
