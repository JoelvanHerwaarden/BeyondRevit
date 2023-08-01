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

namespace BeyondRevit.ViewModels
{
    public class OrganizeViewsViewModel : INotifyPropertyChanged
    {
        private List<OrganizeViewsModel> _views;
        
        public List<OrganizeViewsModel> Views
        {
            get
            {
                return _views;
            }
            set
            {
                _views = value;
                OnPropertyRaised("Views");
            }
        }

        public Document document;
        public UIDocument UIDocument;

        public OrganizeViewsViewModel(UIDocument uidoc, ViewSheet sheet)
        {
            this.UIDocument = uidoc;
            this.document = uidoc.Document;
            this.Views = new List<OrganizeViewsModel>();
            List<Viewport> vps = new List<Viewport>();
            List<View> views = new List<View>();
            List<string> viewNames = new List<string>();
            foreach (ElementId viewportId in sheet.GetAllViewports())
            {
                Viewport viewport = (Viewport)this.document.GetElement(viewportId);
                View view = (View)this.document.GetElement(viewport.ViewId);
                vps.Add(viewport);
                views.Add(view);
                viewNames.Add(view.Name);
            }
            for (int i=0; i < views.Count; i++)
            {
                Viewport viewport = vps[i];
                View view = views[i];
                OrganizeViewsModel viewModel = new OrganizeViewsModel(viewport, view);
                this.Views.Add(viewModel);
            }
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
