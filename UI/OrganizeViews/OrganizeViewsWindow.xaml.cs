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

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class OrganizeViewsWindow : Window
    {
        public bool Cancelled { get; set; }

        public OrganizeViewsWindow()
        {
            InitializeComponent();
            this.Cancelled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.LoadViewNames();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Cancelled = false;
            this.Close();
        }

        private void Pan_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            OrganizeViewsViewModel data = (OrganizeViewsViewModel)this.DataContext;
            Selection selection= data.UIDocument.Selection;
            SelectionFilter selectionFilter = new SelectionFilter();
            selection.PickObjects(ObjectType.Element, selectionFilter, "Pan around the Sheet. You cannot Change or Select anything");
            this.Show();
        }
    }
    internal sealed class SelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

            if (builtInCategory == BuiltInCategory.OST_SpotElevations && builtInCategory == BuiltInCategory.OST_AdaptivePoints) return true;

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }

        public static int GetCategoryIdAsInteger(Element element)
        {
            return element?.Category?.Id?.IntegerValue ?? -1;
        }
    }
}
