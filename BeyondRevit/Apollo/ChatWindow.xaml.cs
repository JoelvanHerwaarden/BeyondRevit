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

namespace BeyondRevit.Apollo.UI
{
    public sealed partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            this.InitializeComponent();
        }

        private void SendPromptButton_Click(object sender, RoutedEventArgs e)
        {
            string prompt = "Write c# code using the Revit API where the Variable \"this\" is the Current Application: \"" + this.PromptTextbox.Text + "\"";
            string response = ChatGPTEndPoints.SendMessage(prompt);
            int i = response.IndexOf("public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)");
            //response.Substring(i, response.Length);
            Utils.Show(i.ToString()); 
            Utils.Show(response);
            //Utils.Show(response.Substring(i, response.Length));
        }
    }
}
