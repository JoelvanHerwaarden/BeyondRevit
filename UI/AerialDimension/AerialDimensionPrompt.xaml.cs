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
namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for _3DSectionWindow.xaml
    /// </summary>
    public partial class AerialDimensionPrompt : Window
    {
        public UIDocument Document { get; set; }
        public double Dimension { get; set; }
        public double Pixels { get; set; }
        public XYZ Point { get; set; }
        public AerialDimensionPrompt(UIDocument doc)
        {
            InitializeComponent();

            this.Document = doc;
            BasePoint PB = new FilteredElementCollector(this.Document.Document).OfClass(typeof(BasePoint)).ToElements()[1] as BasePoint;
            XYZ point = Utils.XYZFeetToMm(PB.Position);
            point /= 1000;
            point = Utils.ToWCS(point, PB);
            SetPoint(point);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            try
            {
                message = "Dimension";
                this.Dimension = double.Parse(this.dimension_box.Text);
                message = "Pixels";
                this.Pixels = double.Parse(this.pixel_box.Text);
                message = "X Coordinate";
                double x = double.Parse(this.X_Coordinate.Text);
                message = "Y Coordinate";
                double y = double.Parse(this.Y_Coordinate.Text);
                this.Point = new XYZ(x, y, 0);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception exceptiom)
            {
                Utils.Show(exceptiom.Message);
            }
        }
        private void PickPointButton_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            XYZ point = this.Document.Selection.PickPoint();
            BasePoint PB = new FilteredElementCollector(this.Document.Document).OfClass(typeof(BasePoint)).ToElements()[1] as BasePoint;
            point = Utils.ToWCS(point, PB);
            SetPoint(point);
            this.ShowDialog();
        }

        private void SetPoint(XYZ point)
        {
            this.Point = point;
            this.X_Coordinate.Text = point.X.ToString();
            this.Y_Coordinate.Text = point.Y.ToString();
        }

        private void textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox textbox = (System.Windows.Controls.TextBox)sender;
            try
            {
                double.Parse(textbox.Text);
                textbox.Background = Brushes.White;
            }
            catch (Exception)
            {
                textbox.Background = Brushes.Red;
            }
        }
    }
}
