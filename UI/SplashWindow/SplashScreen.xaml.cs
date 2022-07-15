using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public string SplashFact { get; set; }
        public SplashScreen(Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            try
            {

                this.SplashFact = DownloadRandomFact();
            }
            catch
            {
                this.SplashFact = "You are not connected to the Internet";
            }
            this.FactLabel.Text = this.SplashFact;
        }

        public async Task ShowSplash()
        {
            this.Show();
            System.Threading.Thread.Sleep(10000);
            this.Close();
        }
        public static string DownloadRandomFact()
        {
            WebClient client = new WebClient();
            string webcontent = client.DownloadString("https://fungenerators.com/random/facts/");
            int index = webcontent.IndexOf("wow fadeInUp animated\"  data");
            string sub = webcontent.Substring(index);
            int startIndex = sub.IndexOf(">") + 1;
            int endIndex = sub.IndexOf("<");
            string fact = sub.Substring(startIndex, endIndex - startIndex);
            client.Dispose();

            return fact;
        }
    }
}
