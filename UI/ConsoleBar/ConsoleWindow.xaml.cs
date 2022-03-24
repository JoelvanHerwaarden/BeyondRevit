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
using BeyondRevit.ViewModels;
using Autodesk.Revit.DB;
using RevitUI = Autodesk.Revit.UI;
using NUnit.Framework;

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for _3DSectionWindow.xaml
    /// </summary>
    public partial class ConsoleWindow : Window
    {
        private bool Searching { get; set; }
        public List<RevitUI.PostableCommand> AllCommands {get;set;}
        public RevitUI.PostableCommand Command { get; set; }

        public ConsoleWindow()
        {
            InitializeComponent();
            this.Searching = true;
            this.SearchBox.Focus();
            this.AllCommands = new List<RevitUI.PostableCommand>();

            //Get all Commands
            foreach(dynamic command in Enum.GetValues(typeof(RevitUI.PostableCommand)))
            {
                AllCommands.Add(command);
            }

            //Sort the Commands Alphabeticaly
            AllCommands = AllCommands.OrderBy(o => o.ToString()).ToList();
            foreach (RevitUI.PostableCommand command in AllCommands)
            {
                this.SearchList.Items.Add(command);
            }

            this.PreviewKeyDown += new KeyEventHandler(HandleKeyPressed);
        }

        private void HandleKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
            else if(e.Key == Key.Enter)
            {
                if (!this.Searching)
                {
                    Submit();
                }
            }
            else if(e.Key == Key.Down)
            {
                if (this.SearchBox.IsFocused)
                {
                    this.SearchList.Focus();
                    this.SearchList.SelectedIndex = 0;
                }
                else if (this.SearchList.IsFocused)
                {
                    this.SearchList.SelectedIndex++;
                }
            }
            else if(e.Key == Key.Up)
            {
                if (this.SearchList.SelectedIndex == 0)
                {
                    this.SearchBox.Focus();
                }
                else if (this.SearchList.IsFocused)
                {
                    this.SearchList.SelectedIndex--;
                }
            }
            else
            {
                this.SearchBox.Focus();
            }
        }

        private void Search(string searchText)
        {
            this.SearchList.Items.Clear();
            List<string> searchTerms = searchText.Split(' ').ToList<string>();
            foreach(RevitUI.PostableCommand command in AllCommands)
            {
                if(ContainsAll(command.ToString(), searchTerms))
                {
                    this.SearchList.Items.Add(command);
                }
            }
        }

        private bool ContainsAll(string evaluate, List<string> searchTerms)
        {
            bool result = true;
            evaluate = evaluate.ToLower();
            foreach(string search in searchTerms)
            {
                string lowerString = search.ToLower();
                if (!evaluate.Contains(lowerString))
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.Searching = false;
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.Searching = true;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchBox = (TextBox)sender;
            Search(searchBox.Text);
        }

        private void SearchList_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Submit();
        }

        private void Submit()
        {
            RevitUI.PostableCommand command = (RevitUI.PostableCommand)this.SearchList.SelectedItem;
            if (command != null)
            {
                this.Command = command;
                this.Close();
            }
        }
    }
}
