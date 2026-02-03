using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Lumi.ViewModels;

namespace Lumi.Views
{
    public partial class LumiHub : Window
    {
        public HubViewModel Vm { get; }

        public LumiHub(HubStart start)
        {
            InitializeComponent();

            Vm = new HubViewModel();
            DataContext = Vm;

            // Navigation
            if (start == HubStart.Settings)
                Vm.ShowSettingsCommand.Execute(null);
            else
                Vm.ShowHomeCommand.Execute(null);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

