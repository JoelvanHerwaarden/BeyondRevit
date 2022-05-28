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
    public partial class GenericDropdownWindow : Window
    {
        public bool Cancelled { get; set; }
        public Dictionary<string, dynamic> Items { get; set; }
        public List<dynamic> SelectedItems {get;set;}

        public GenericDropdownWindow(string windowTitle, string instruction, List<string> itemNames, dynamic items, Window owner, bool selectMultiple)
        {
            InitializeComponent();
            Items = new Dictionary<string, dynamic>();
            for(int i = 0; i < itemNames.Count; i++) 
            {
                string name = itemNames[i];
                var item = items[i];
                try
                {
                    Items.Add(name, item);
                }
                catch { }
            }
            PopulateListbox();
            if (!selectMultiple)
            {
                this.SelectAllButton.Visibility = System.Windows.Visibility.Hidden;
                this.SelectNoneButton.Visibility = System.Windows.Visibility.Hidden;
                this.ItemNamesListBox.SelectionMode = SelectionMode.Single;
            }
            this.Cancelled = true;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            this.SelectedItems = new List<dynamic>();
            this.Cancelled = false;
            var selectedItems = ItemNamesListBox.SelectedItems;
            foreach (string item in selectedItems)
            {
                SelectedItems.Add(Items[item]);
            }
            this.Close();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            this.ItemNamesListBox.SelectAll();
        }
        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {

            this.ItemNamesListBox.SelectedItem = null;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            string searchString = box.Text;
            if(searchString == "" | searchString == "Search...")
            {
                try
                {
                    PopulateListbox();
                }
                catch { }
                box.Text = "Search...";
            }
            else
            {
                SearchList(searchString);
            }
        }
        private void PopulateListbox()
        {
            this.ItemNamesListBox.Items.Clear();
            foreach (string key in this.Items.Keys)
            {
                this.ItemNamesListBox.Items.Add(key);
            }
        }

        private void SearchList(string searchTerm)
        {
            this.ItemNamesListBox.Items.Clear();
            string[] searchTerms = searchTerm.Split(' ');
            foreach(string key in this.Items.Keys)
            {
                bool match = true;
                foreach(string searchString in searchTerms)
                {
                    if (!key.ToLower().Contains(searchString.ToLower()))
                    {
                        match = false;
                    }
                }
                if (match)
                {
                    this.ItemNamesListBox.Items.Add(key);
                }
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            string searchString = box.Text;
            if (searchString == "Search...")
            {
                this.SearchBox.SelectAll();
            }

        }
    }
}
