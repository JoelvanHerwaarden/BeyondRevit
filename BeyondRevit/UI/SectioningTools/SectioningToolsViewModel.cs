using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BeyondRevit.UI;

namespace BeyondRevit.UI.SectioningTools
{
    public class SectioningToolsViewModel : INotifyPropertyChanged
    {
        #region FIELDS
        private Curve _sectionPath { get; set; }
        private string _viewNamePrefix { get; set; }
        private int _startingNumber { get; set; }
        private double _sectionfrequency { get; set; }
        private double _viewDepth { get; set; }
        private double _topOffset { get; set; }
        private double _bottomOffset { get; set; }
        private double _leftOffset { get; set; }
        private double _rightOffset { get; set; }
        private bool _flipSectionPath { get; set; }
        private bool _flipSectionDirection { get; set; }
        #endregion

        #region Properties
        public SectioningToolsExternalEventHandler EventHandler { get; }
        public ExternalEvent ExternalEvent { get; }
        public Document Document { get; }
        public Curve SectionPath { get { return _sectionPath; } }
        public string ViewNamePrefix{   get { return ViewNamePrefix; } set {_viewNamePrefix = value; OnPropertyRaised("ViewNamePrefix");}}
        public int StartingNumber { get { return _startingNumber; } set { _startingNumber = value; OnPropertyRaised("StartingNumber"); } }  
        public double SectionFrequency { get { return _sectionfrequency; } set { _sectionfrequency = value; OnPropertyRaised("SectionFrequency"); } }
        public double ViewDepth { get { return _viewDepth; } set { _viewDepth = value; OnPropertyRaised("ViewDepth"); } }
        public double TopOffset { get { return _topOffset; } set { _topOffset = value; OnPropertyRaised("TopOffset"); } }
        public double BottomOffset { get { return _bottomOffset; } set { _bottomOffset = value; OnPropertyRaised("BottomOffset"); } }
        public double LeftOffset { get { return _leftOffset; } set { _leftOffset = value; OnPropertyRaised("LeftOffset"); } }
        public double RightOffset { get { return _rightOffset; } set { _rightOffset = value; OnPropertyRaised("RightOffset"); } }
        public bool FlipSectionPath { get { return _flipSectionPath; } set { _flipSectionPath = value; OnPropertyRaised("FlipSectionPath"); } }
        public bool FlipSectionDirection { get { return _flipSectionDirection; } set { _flipSectionDirection = value; OnPropertyRaised("FlipSectionDirection"); } }
        #endregion
        public SectioningToolsViewModel(Document document, SectioningToolsExternalEventHandler eventHandler, ExternalEvent externalEvent)
        {
            Document = document; 
            EventHandler = eventHandler;
            ExternalEvent = externalEvent;
        }

        public void SelectCurveInDocument()
        {
            UIDocument uidoc = new UIDocument(this.Document);
            Selection selection = uidoc.Selection;
            Utils.TypeSelectionFilter filter = new Utils.TypeSelectionFilter(this.Document, new List<Type>() { typeof(DetailCurve) });
            Reference element = selection.PickObject(ObjectType.Element, filter, "Select Detail Curve as Section Path");
            DetailCurve curve = (DetailCurve)this.Document.GetElement(element);
            this._sectionPath = curve.GeometryCurve;
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
