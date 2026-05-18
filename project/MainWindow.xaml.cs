using System.Windows;
using project.ViewModels;

namespace project
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Связываем главное окно (View) с его логикой (ViewModel)
            DataContext = new MainViewModel();
        }
    }
}
