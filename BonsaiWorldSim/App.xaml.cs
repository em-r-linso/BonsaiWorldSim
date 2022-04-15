using System.Windows;
using System.Windows.Threading;

namespace BonsaiWorldSim
{
	/// <summary>
	///     Interaction logic for App.xaml
	/// </summary>
	public partial class App
	{
		void App_OnStartup(object sender, StartupEventArgs e)
		{
			var mainWindow = new MainWindow();
			mainWindow.Show();
		}

		void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			MessageBox.Show(
				"An unhandled exception just occurred: " + e.Exception.Message,
				"Exception Sample",
				MessageBoxButton.OK,
				MessageBoxImage.Warning
			);
			e.Handled = true;
		}
	}
}