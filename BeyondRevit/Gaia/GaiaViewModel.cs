using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using BeyondRevit.Models;
using Autodesk.Revit.UI;
using System.Windows;

namespace BeyondRevit.Gaia
{
    public class GaiaViewModel : INotifyPropertyChanged
    {
        private string _sourceFilePath;
        public Document SourceDocument { get; set; }
        public Autodesk.Revit.ApplicationServices.Application Application { get; set; }
        public string SourceFilePath
        {
            get
            {
                return _sourceFilePath;
            }
            set
            {
                _sourceFilePath = value;
                OnPropertyRaised("SourceFilePath");
            }
        }

        public GaiaViewModel(Autodesk.Revit.ApplicationServices.Application application)
        {
            SourceDocument = null;
            SourceFilePath = "";
            Application = application;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }
    }
}
