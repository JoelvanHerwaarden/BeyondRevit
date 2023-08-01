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
    public partial class GenericDropdownWindow : Window
    {
        public bool Cancelled { get; set; }
        public Dictionary<string, dynamic> Items { get; set; }
        public List<dynamic> SelectedItems {get;set;}

        public GenericDropdownWindow(string windowTitle, string instruction, Dictionary<string, dynamic> items, Window owner, bool selectMultiple)
        {
            InitializeComponent();
            this.Items = Utils.SortDictionary(items);
            this.InstructionLabel.Content = instruction;
            this.Title = windowTitle;
            this.Owner = owner;
            SearchList("");
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
            if(searchString == "")
            {
                try
                {
                    SearchList("");
                }
                catch { }
                SearchLabel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                SearchList(searchString);
                SearchLabel.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void SearchList(string searchTerm)
        {

            IList<object> SelectedItems = GetCurrentSelection();
            this.ItemNamesListBox.Items.Clear();
            string[] searchTerms = searchTerm.Split(' ');
            foreach(string key in this.Items.Keys)
            {
                bool match = true;
                foreach(string searchString in searchTerms)
                {
                    if (!key.ToLower().Contains(searchString.ToLower()))
                    {
                        if (Regex.Match(key, searchTerm).Success)
                        {
                            match = true;
                            break;
                        }
                        match = false;
                    }
                }
                if (match)
                {
                    this.ItemNamesListBox.Items.Add(key);
                    if (SelectedItems.Contains(key))
                    {
                        this.ItemNamesListBox.SelectedItems.Add(key);
                    }
                }
            }
        }

        private IList<object> GetCurrentSelection()
        {
            IList<object> SelectedItems = new List<object>();
            foreach(object obj in this.ItemNamesListBox.SelectedItems)
            {
                SelectedItems.Add(obj);
            }
            return SelectedItems;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            SearchLabel.Visibility = System.Windows.Visibility.Hidden;

        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            if(box.Text.Length == 0)
            {
                SearchLabel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                SearchLabel.Visibility = System.Windows.Visibility.Hidden;
            }

        }
    }
}
