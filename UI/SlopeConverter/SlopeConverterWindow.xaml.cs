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
using BeyondRevit.ViewModels;
using Autodesk.Windows;

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for ManageTabsWindow.xaml
    /// </summary>
    public partial class SlopeConverterWindow : Window
    {
        public SlopeConverterWindow()
        {
            InitializeComponent();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (IsValueValid(textBox))
            {
                if (ActiveTab() == "Degrees")
                {
                    double degrees = double.Parse(textBox.Text);
                    DegreesCalculation(degrees);
                }
                if (ActiveTab() == "Ratio")
                {
                    TextBox firstTB = GetTextBoxByName("Ratio", "A");
                    TextBox secondTB = GetTextBoxByName("Ratio", "O");
                    try
                    {
                        double O = double.Parse(firstTB.Text);
                        double A = double.Parse(secondTB.Text);
                        RatioCalculation(A,O);
                    }
                    catch { }
                        
                }
                if (ActiveTab() == "Percentage")
                {
                    double percent = double.Parse(textBox.Text);
                    PercentageCalculation(percent);
                }
            }
        }

        private void RatioCalculation(double A, double O)
        {
            double degrees = CalculateDegrees(A, O);
            TextBox degreesTB = GetTextBoxByName("Ratio", "Ratio2Degrees");
            degreesTB.Text = Math.Round(degrees, 2).ToString();

            TextBox percentTB = GetTextBoxByName("Ratio", "Ratio2Percent");
            double percent = CalculatePercentage(degrees);
            percentTB.Text = Math.Round(percent,2).ToString();
        }

        private void PercentageCalculation(double percent)
        {
            TextBox degreesTB = GetTextBoxByName("Percentage", "Percent2Degrees");
            double degrees = CalculateDegrees(100, percent);
            degreesTB.Text = Math.Round(degrees,2).ToString();

            TextBox ratio = GetTextBoxByName("Percentage", "Percent2Ratio");
            List<double> values = CalculateRatio(degrees);
            ratio.Text = values[0].ToString() + " : " + Math.Round(values[1],0).ToString();
        }

        private void DegreesCalculation(double degrees)
        {
            TextBox percentTB = GetTextBoxByName("Degrees", "Degrees2Percent");
            double percent = CalculatePercentage(degrees);
            percentTB.Text = Math.Round(percent,2).ToString();

            TextBox ratio = GetTextBoxByName("Degrees", "Degrees2Ratio");
            List<double> values = CalculateRatio(degrees);
            ratio.Text = values[0].ToString() + " : " + Math.Round(values[1], 0).ToString();

        }

        private bool IsValueValid(TextBox sender)
        {
            bool result = true;
            TextBox s = (TextBox)sender;
            string content = s.Text;
            try
            {
                double.Parse(content);
                s.Background = Brushes.White;
                result = true;
            }
            catch
            {
                s.Background = Brushes.Red;
                result = false;
            }
            return result;
        }

        private string ActiveTab()
        {
            ItemCollection tabs = this.TabManager.Items;
            string tabName = "";
            foreach(TabItem tab in tabs)
            {
                if (tab.IsSelected)
                {
                    tabName = tab.Header.ToString();
                    break;
                }
            }
            return tabName;
        }

        private TextBox GetTextBoxByName(string TabName, string TextBoxName)
        {
            TextBox result = null;
            ItemCollection tabs = this.TabManager.Items;
            foreach (TabItem tab in tabs)
            {
                if (tab.IsSelected)
                {
                    dynamic grid = tab.Content;
                    foreach (UIElement element in grid.Children)
                    {
                        if(element.GetType() == typeof(TextBox))
                        {
                            TextBox txtbox = (TextBox)element;
                            if(txtbox.Name == TextBoxName)
                            {
                                result = txtbox;
                                break;
                            }
                        }
                    }
                }
            }
            return result;
        }

        private List<double> CalculateRatio(double degrees)
        {
            double A = 1;
            double O = A * Math.Tan(Utils.DegreesToRadians(degrees));
            double r = Math.Round(1 / O, 0);
            List<double> result = new List<double>()
            {
                A,
                r
            };
            return result;
        }

        private double CalculatePercentage(double degrees)
        {
            List<double> ratio = CalculateRatio(degrees);
            double A = ratio[0];
            double O = ratio[1];
            double result = 100 * (A/O);
            return result;
        }

        private double CalculateDegrees(double A, double O)
        {
            double d = (O / A);
            double degrees = Utils.RadiansToDegrees(Math.Atan(d));
            return degrees;
        }

    }
}
