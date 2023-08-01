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
            EZInstallerViewModel viewmodel = new EZInstallerViewModel("2DRebar4Revit", "You hereby agree that you will only use the 2DRebar4Revit add-in, which is developed by HaskoningDHV Nederland b.v. (further mentioned as “RHDHV”), for the “'Oosterweel” Project.\n\nDuring project you are allowed to use the Addin for free to make reinforcement drawings. 2DRebar4Revit may therefore only be used for this project.\n\nYou acknowledge our intellectual property rights to 2DRebar4Revit. These property rights are and shall remain vested in us, and you shall refrain from any action that could prejudice these rights.\n\nRHDHV cannot be held liable in any way for the operation of 2DRebar4Revit, nor for the results produced with 2DRebar4Revit, because you use 2DRebar4Revit under the delivery condition 'actual state in which the service is then' ('as is').\n\nOur liability in the event of a shortcoming attributable to us is limited to a maximum amount of €10,000.", true, new List<string>() { "Revit"});
            this.DataContext = viewmodel;
            this.StartView.DataContext = viewmodel;
            this.InstallationView.DataContext = viewmodel;

            if (!viewmodel.CanRunInstaller())
            {
                this.NextButton.Visibility = Visibility.Hidden;
                this.KillProgramsButton.Visibility = Visibility.Visible;
            }
            Status = 0;
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
