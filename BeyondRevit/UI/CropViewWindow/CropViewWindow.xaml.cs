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
using System.Windows.Navigation;
using System.Windows.Shapes;
using BeyondRevit.ViewModels;
using BeyondRevit.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Text.RegularExpressions;

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class CropViewWindow : Window
    {
        public bool Cancelled { get; set; }

        public double TopExtension { get; set; }
        public double BottemExtension { get; set; }
        public double LeftExtension { get; set; }
        public double RightExtension { get; set; }
        public CropViewWindow(Window owner)
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ExtensionText_TextChanged(object sender, EventArgs e)
        {
            TextBox txtbox = (TextBox)sender;
            if(txtbox.Name == "TopExtend")
            {
                this.TopExtension = double.Parse(txtbox.Text);
            }
            else if (txtbox.Name == "BottomExtend")
            {
                this.BottemExtension = double.Parse(txtbox.Text);
            }
            else if (txtbox.Name == "LeftExtend")
            {
                this.LeftExtension = double.Parse(txtbox.Text);
            }
            else
            {
                this.RightExtension = double.Parse(txtbox.Text);
            }
        }

    }
}
