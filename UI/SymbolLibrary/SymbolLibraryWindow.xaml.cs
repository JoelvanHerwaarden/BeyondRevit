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

namespace BeyondRevit.UI
{
    /// <summary>
    /// Interaction logic for SymbolLibraryWindow.xaml
    /// </summary>
    public partial class SymbolLibraryWindow : Window
    {
        public Symbols SymbolList { get; set; }
       
        public SymbolLibraryWindow()
        {
            InitializeComponent();
            string filename = "BeyondRevitSymbols.json";
            string folderpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string filepath = System.IO.Path.Combine(folderpath, "BeyondRevit", filename);
            Symbols symbols = new Symbols(filepath);
            SymbolList = symbols;
            this.PopulateWindow();
        }

        public void PopulateWindow()
        {
            ResourceDictionary resources = BeyondRevit.StylesCodeBehind.GetResourceDictionary();
            foreach (string symbol in this.SymbolList.SymbolLibrary)
            {
                Button button = new Button()
                {
                    Style = (Style)resources.FindName("DotButton"),
                    Content = symbol
                };
                this.wrapPanel.Children.Add(button);
            }
        }

        public void SaveSymbols()
        {
            List<string> symbols = new List<string>();
            foreach(UIElement element in this.wrapPanel.Children)
            {
                Button button = (Button)element;
                symbols.Add(button.Content.ToString());
            }
            this.SymbolList.SaveSymbols(symbols);
        }


    }
}
