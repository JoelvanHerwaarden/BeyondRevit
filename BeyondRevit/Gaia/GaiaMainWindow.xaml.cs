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
using Forms = System.Windows.Forms;

namespace BeyondRevit.Gaia
{
    public partial class GaiaMainWindow : Window
    {
        public GaiaMainWindow(GaiaViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void BrowseButtonClick(object sender, RoutedEventArgs e)
        {
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Multiselect = false;
            dialog.Filter = "Revit File (*.RVT)|*.rvt";
            if(dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                ((GaiaViewModel)this.DataContext).SourceFilePath = dialog.FileName;
            }

        }
    }
}
