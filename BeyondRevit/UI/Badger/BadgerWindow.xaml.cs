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
using Autodesk.Revit.Creation;
using Autodesk.Revit.UI;
using BeyondRevit.ViewModels;
using Autodesk.Revit.DB;
using Autodesk.Windows;
using IO = System.IO;

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for ManageTabsWindow.xaml
    /// </summary>
    public partial class BadgerWindow : Window
    {
        public static string SaveFileLocation = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BeyondRevit\\ManageTabsSettings.json");

        public BadgerWindow()
        {
        }
    }
}
