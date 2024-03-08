using BeyondRevit.UI.SectioningTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace BeyondRevit.UI
{
    public partial class CreateSectionsAlongCurveControl : UserControl
    {
        public CreateSectionsAlongCurveControl()
        {
            this.InitializeComponent();
        }

        private void NumbersOnlyInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex _regex = new Regex("[^0-9.-]+");
            e.Handled = _regex.IsMatch(e.Text);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SectioningToolsViewModel viewModel = (SectioningToolsViewModel)this.DataContext;
            Utils.Show(string.Format("Prefix = {0}", viewModel.ViewNamePrefix));
        }
    }
}
