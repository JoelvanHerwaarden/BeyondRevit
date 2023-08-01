using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace BeyondRevit.Models
{
    public class OrganizeViewsModel : INotifyPropertyChanged
    {
        private string _viewName;
        private string _viewTitle;
        private string _viewNumber;
        private Viewport _viewport;
        private View _view;
        public string ViewName
        {
            get
            {
                return _viewName;
            }
            set
            {
                _viewName = value;
                OnPropertyRaised("ViewName");
            }
        }
        public string ViewTitle
        {
            get
            {
                return _viewTitle;
            }
            set
            {
                _viewTitle = value;
                OnPropertyRaised("ViewTitle");
            }
        }
        public string ViewNumber
        {
            get
            {
                return _viewNumber;
            }
            set
            {
                _viewNumber = value;
                OnPropertyRaised("ViewNumber");
            }
        }
        public Viewport ViewPort
        {
            get
            {
                return _viewport;
            }
            set
            {
                _viewport = value;
                OnPropertyRaised("ViewElementId");
            }
        }
        public View View
        {
            get
            {
                return _view;
            }
            set
            {
                _view = value;
                OnPropertyRaised("ViewElementId");
            }
        }
        public OrganizeViewsModel(Viewport viewport, View view)
        {
            this._viewName = view.Name;
            this._viewNumber = viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).AsString();
            this._viewTitle = view.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).AsString();
            this._viewport = viewport;
            this._view = view;
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
