using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EZInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class EZInstallerWindow : Window
    {
        public int Status { get; set; }
        public EZInstallerWindow()
        {
            InitializeComponent();
            bool showDisclaimer = false;
            EZInstallerViewModel viewmodel = new EZInstallerViewModel("Beyond Revit", "", showDisclaimer, new List<string>() { "Revit"});
            this.DataContext = viewmodel;
            this.StartView.DataContext = viewmodel;
            this.InstallationView.DataContext = viewmodel;

            if (!viewmodel.CanRunInstaller())
            {
                this.NextButton.Visibility = Visibility.Hidden;
                this.KillProgramsButton.Visibility = Visibility.Visible;
            }
            if (showDisclaimer)
            {
                Status = 0;
            }
            else
            {
                Status = 1;
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Status)
            {
                case 0:
                    this.StartView.Visibility = Visibility.Hidden;
                    this.LicenseView.Visibility = Visibility.Visible;
                    this.startLabel.FontWeight = FontWeights.Light;
                    this.licenseLabel.FontWeight = FontWeights.DemiBold;
                    this.NextButton.IsEnabled = false;
                    this.NextButton.Content = "Install";
                    break;

                case 1:
                    this.StartView.Visibility = Visibility.Hidden;
                    this.LicenseView.Visibility = Visibility.Hidden;
                    this.licenseLabel.FontWeight = FontWeights.Light;
                    this.installationLabel.FontWeight = FontWeights.DemiBold;
                    this.InstallationView.Visibility = Visibility.Visible;
                    EZInstallerViewModel viewmodel = (EZInstallerViewModel)this.DataContext;
                    this.NextButton.IsEnabled = false;
                    Progress<double> progress = new Progress<double>(value =>
                    {
                        this.InstallationView.ProgressBar.Value = value;
                    });

                    Progress<string> progressName = new Progress<string>(value =>
                    {
                        this.InstallationView.ProcessLabel.Content = string.Format("Installing {0}",value);
                    });
                    await viewmodel.RunActions(progress, progressName);

                    this.InstallationView.ProcessLabel.Content = "Installation Finished";
                    this.InstallationView.ProgressBar.Value = 100;
                    this.NextButton.Content = "Close";
                    this.NextButton.IsEnabled = true;
                    this.installationLabel.FontWeight = FontWeights.Light;
                    this.finishLabel.FontWeight = FontWeights.DemiBold;
                    break;
                case 2:
                    this.Close();
                    break;
            }

            this.Status += 1;
        }

        private void KillProgramsButton_Click(object sender, RoutedEventArgs e)
        {

            EZInstallerViewModel viewmodel = (EZInstallerViewModel)this.DataContext;
            viewmodel.KillPrograms();
            this.NextButton.Visibility = Visibility.Visible;
            this.KillProgramsButton.Visibility = Visibility.Hidden;
        }
    }
}
