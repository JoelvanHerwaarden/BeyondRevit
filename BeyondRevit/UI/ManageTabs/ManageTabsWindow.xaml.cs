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
    public partial class ManageTabsWindow : Window
    {
        public static string SaveFileLocation = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BeyondRevit\\ManageTabsSettings.json");
        public List<RibbonTab> Tabs { get; set; }
        public Dictionary<string, bool> Settings {get;set;} 
        public ManageTabsWindow(List<RibbonTab> tabs)
        {
            InitializeComponent();
            if (IO.File.Exists(SaveFileLocation))
            {
                string jsonContent = IO.File.ReadAllText(SaveFileLocation);
                this.Settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, bool>>(jsonContent);
            }
            else
            {
                Settings = new Dictionary<string, bool>();
            }
            this.Tabs = tabs;
            PopulateTabs();
        }
        private void PopulateTabs()
        {
            foreach(RibbonTab tab in this.Tabs)
            {
                bool check = true;
                if (Settings != null)
                {
                    if (Settings.ContainsKey(tab.Id))
                    {
                        check = Settings[tab.Id];
                    }
                    else
                    {
                        Settings.Add(tab.Id, tab.IsVisible);
                    }
                }

                else
                {
                    check = tab.IsVisible;
                }
                CheckBox cb = new CheckBox()
                {
                    Content = tab.Name,
                    IsChecked = check,
                    Foreground = (Brush)new BrushConverter().ConvertFrom(" #00BFA5"),
                    Margin = new Thickness(10),
                    FontWeight = FontWeights.DemiBold
                };
                stackPanel.Children.Add(cb);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < this.stackPanel.Children.Count; i++)
            {
                RibbonTab tab = this.Tabs[i];
                CheckBox cb = (CheckBox)this.stackPanel.Children[i];
                Settings[tab.Id] = cb.IsChecked.Value;
                if (cb.IsChecked == true)
                {
                    tab.IsVisible = true;
                }
                else
                {
                    tab.IsVisible = false;
                }
            }

            string settingsJson = Newtonsoft.Json.JsonConvert.SerializeObject(this.Settings, Newtonsoft.Json.Formatting.Indented);
            IO.File.WriteAllText(SaveFileLocation, settingsJson);

            this.Close();
           
        }
    }
}
