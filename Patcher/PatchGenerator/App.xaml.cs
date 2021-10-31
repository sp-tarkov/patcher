using System.Windows;

namespace PatchGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            GenStartupArgs startupArgs = null;

            if (e.Args != null && e.Args.Length > 0)
            {
                if(e.Args[0].ToLower() == "help")
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder()
                        .AppendLine("Help - Shows this message box if in position 1")
                        .AppendLine("")
                        .AppendLine("Parameters below can be used like this: \"Name::Value\"")
                        .AppendLine("OutputFolderName -  The output file for the patch")
                        .AppendLine("TargetFolderPath - The target folder path")
                        .AppendLine("CompareFolderPath - The compare folder path")
                        .AppendLine("AutoZip - Set if the output folder should be zipped up after patch generation. Defaults to true");

                    MessageBox.Show(sb.ToString(), "Parameter Help Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    Application.Current.Shutdown(0);
                }

                startupArgs = GenStartupArgs.Parse(e.Args);
            }

            MainWindow mw = new MainWindow(startupArgs);

            mw.ShowDialog();
        }
    }
}
