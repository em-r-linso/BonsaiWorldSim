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
			var result = MessageBox.Show(
				e.Exception.Message,
				"Unhandled Exception",
				MessageBoxButton.OKCancel,
				MessageBoxImage.Error
			);
			switch (result)
			{
				case MessageBoxResult.OK:
					e.Handled = true;
					break;
				case MessageBoxResult.Cancel:
					Current.Shutdown();
					break;
			}
		}
	}
}