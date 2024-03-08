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
    public partial class BreaklinesWindow : Window
    {
        public bool Cancelled { get; set; }
        public bool TopBreakLine { get; set; }
        public bool BottomBreakLine { get; set; }
        public bool LeftBreakLine { get; set; }
        public bool RightBreakLine { get; set; }
        public FamilySymbol BreakLineFamily { get; set; }
        private Dictionary<string, FamilySymbol> BreakLineFamilies { get; set; } 

        public BreaklinesWindow(Window owner, FamilySymbol defaultElement, IList<FamilySymbol> breakLinesFamilies)
        {
            InitializeComponent();
            BreakLineFamilies = new Dictionary<string, FamilySymbol>();
            breakLinesFamilies = breakLinesFamilies.OrderBy(f => f.Name).ToList();
            foreach(FamilySymbol element in breakLinesFamilies)
            {
                this.BreakLineFamilies.Add(element.Name, element);
                this.BreaklineFamiliesBox.Items.Add(element.Name);
            }
            this.BreaklineFamiliesBox.SelectedItem = defaultElement.Name;
            
            this.Cancelled = true;
        }



        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.TopBreakLine = (FindDescendant(this, "Top") as CheckBox).IsChecked == true;
            this.BottomBreakLine = (FindDescendant(this, "Bottom") as CheckBox).IsChecked == true;
            this.LeftBreakLine = (FindDescendant(this, "Left") as CheckBox).IsChecked == true;
            this.RightBreakLine = (FindDescendant(this, "Right") as CheckBox).IsChecked == true;
            this.Close();
            this.BreakLineFamily = (FamilySymbol)this.BreakLineFamilies[(string)this.BreaklineFamiliesBox.SelectedItem];
            
            this.Cancelled = false ;
        }
        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox cb = (CheckBox)sender;
                double thickness = 0;
                if (cb.IsChecked == true) { thickness = 2; }
                Border viewport = FindDescendant(this, "Viewport") as Border;
                Thickness currentState = viewport.BorderThickness;
                if (cb.Name == "Top")
                {
                    currentState.Top = thickness;
                }
                else if (cb.Name == "Bottom")
                {
                    currentState.Bottom = thickness;
                }
                else if (cb.Name == "Left")
                {
                    currentState.Left = thickness;
                }
                else if (cb.Name == "Right")
                {
                    currentState.Right = thickness;
                }
                viewport.BorderThickness = currentState;
            }
            catch { }
            
        }
        private static DependencyObject FindDescendant(
            DependencyObject parent, string name)
        {
            // See if this object has the target name.
            FrameworkElement element = parent as FrameworkElement;
            if ((element != null) && (element.Name == name)) return parent;

            // Recursively check the children.
            int num_children = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < num_children; i++)
            {
                // See if this child has the target name.
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                DependencyObject descendant = FindDescendant(child, name);
                if (descendant != null) return descendant;
            }

            // We didn't find a descendant with the target name.
            return null;
        }

    }
}
