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
        public SplashScreen(Window owner, string fact = null)
        {
            InitializeComponent();
            this.Owner = owner;
            try
            {
                if (fact == null)
                {
                    this.SplashFact = BeyondRevit.Facts.RandomFactGenerator.DownloadRandomFact();
                }
                else
                {
                    this.SplashFact = fact;
                }
            }
            catch
            {
                this.SplashFact = "You are not connected to the Internet";
            }
            this.FactLabel.Text = this.SplashFact;
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task ShowSplash()
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            this.Show();
            System.Threading.Thread.Sleep(10000);
            this.Close();
        }
        
    }
}
