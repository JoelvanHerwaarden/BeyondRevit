using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.UI
{
    public class Symbols
    {
        public List<string> SymbolLibrary { get; set; }
        private string FilePath { get; set; }
        public Symbols(string filePath=null)
        {
            if (filePath != null && File.Exists(filePath))
            {
                FilePath = filePath;
                string json = File.ReadAllText(filePath);
                List<string> symbols = JsonConvert.DeserializeObject<List<string>>(json);
                this.SymbolLibrary = symbols;
            }
            else
            {
                this.SymbolLibrary = GetDefaultSymbols();
            }
        }

        public List<string> GetDefaultSymbols()
        {
            return new List<string>
            {
                "ø",
                "⚠️"
            };
        }
        public void SaveSymbols(List<string> symbols)
        {
            if (!File.Exists(this.FilePath))
            {
                File.Create(FilePath);
            }
            string json = JsonConvert.SerializeObject(symbols, Formatting.Indented);
            File.WriteAllText(this.FilePath, json);
        }
    }
}
