using System.Windows;
using System.Windows.Input;
using MTGAHelper.Tool.OutputLogMessageViewer.ViewModels;

namespace MTGAHelper.Tool.OutputLogMessageViewer
{
    /// <summary>
    /// Interaction logic for SimulationWindow.xaml
    /// </summary>
    public partial class SimulationWindow : Window
    {
        MainWindowVM vm;

        public SimulationWindow()
        {
            InitializeComponent();
        }

        public void SetVM(MainWindowVM vm)
        {
            this.vm = vm;
            DataContext = vm;
        }

        private void TextBoxMatchId_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.SimulationInit();
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            vm.Simulate();
        }

        private void ButtonGoTo_Click(object sender, RoutedEventArgs e)
        {
            vm.GoTo();
        }
    }
}
