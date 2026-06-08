using FileOpenRouter.Services;
using System.Windows;

namespace FileOpenRouter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length > 0)
            {
                RunRouterAndExit(e.Args[0]);
                return;
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        private static void RunRouterAndExit(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show(
                        "文件路径不能为空。",
                        "FileOpenRouter",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                var config = ConfigService.Load();
                var result = RouterService.RouteFile(filePath, config);
                if (!result.Success)
                {
                    MessageBox.Show(
                        result.ErrorMessage,
                        "FileOpenRouter",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "FileOpenRouter",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                Current.Shutdown();
            }
        }
    }
}
