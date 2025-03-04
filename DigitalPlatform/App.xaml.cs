using DigitalPlatform.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace DigitalPlatform
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            ViewModelLocator.AddRecord(null);
        }
    }

}
