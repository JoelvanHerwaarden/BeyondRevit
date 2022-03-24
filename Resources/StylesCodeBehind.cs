using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BeyondRevit
{
    public partial class StylesCodeBehind
    {
        public static ResourceDictionary GetResourceDictionary()
        {
            ResourceDictionary resources = new ResourceDictionary();
            resources.Source = new Uri("/BeyondRevit;component/Resources/Styles.xaml", UriKind.RelativeOrAbsolute);
            return resources;
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            Brush background = button.Background;
            Brush foreground = button.Foreground;
            button.FontWeight = FontWeights.SemiBold;
            if (foreground.IsFrozen)
            {
                foreground = foreground.Clone();
            }
            foreground.Opacity = 0;
            button.Foreground = background;
            button.Background = foreground;
        }
        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            button.FontWeight = FontWeights.Light;
            Brush foreground = button.Background;
            if (foreground.IsFrozen)
            {
                foreground = foreground.Clone();
            }
            foreground.Opacity = 1;
            Brush background = button.Foreground;
            button.Foreground = foreground;
            button.Background = background;
        }
        private void DotButton_Click (object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string text = button.Content.ToString();
            System.Windows.Forms.Clipboard.SetText(text);
            Utils.Show("Copied " + text);

        }
    }
}
