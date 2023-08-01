using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Autodesk.Internal.InfoCenter;
using BeyondRevit.ViewModels;

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for _3DSectionWindow.xaml
    /// </summary>
    public partial class SyncPropertiesWindow : Window
    {
        public SyncPropertiesWindow()
        {
            InitializeComponent();
            BeyondRevit.Commands.SynchronisationConfig config = BeyondRevit.Commands.SynchronisationConfig.Current;
            this.SaveInterval.Text = config.SaveInterval.ToString();
            this.ReloadInterval.Text = config.ReloadLatestInterval.ToString();
            this.SyncInterval.Text = config.SyncInterval.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BeyondRevit.Commands.SynchronisationConfig config = BeyondRevit.Commands.SynchronisationConfig.Current;
                int saveInt = int.Parse(this.SaveInterval.Text);
                int reloadInt = int.Parse(this.ReloadInterval.Text);
                int syncInt = int.Parse(this.SyncInterval.Text);
                config.SaveInterval = saveInt;
                config.ReloadLatestInterval = reloadInt;
                config.SyncInterval = syncInt;
                config.UpdateSettings();
                config.SaveConfigSettings();
                this.Close();
            }
            catch
            {
                Utils.Show("All inputs should be numbers");
            }
        }

        private void NumberCheck_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            try
            {
                int.Parse(box.Text);
                box.Background = Brushes.White;
            }
            catch
            {
                box.Background = Brushes.Red;
            }
        }
    }
}
