using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Forms = System.Windows.Forms;
using System.Windows.Controls;

namespace BeyondRevit.Batcher
{
    public class BatcherModel
    {
        #region fields
        private Guid _projectGuid { get; set; }
        private Guid _modelGuid { get; set; }
        private string _cloudRegion { get; set; }
        private string _fileName { get; set; }
        private string _fileDirectory { get; set; }
        private bool _IsCloudModel { get; set; }
        private string _modelTitle { get; set; }
        private string _dwgExportSettings { get; set; }
        private List<BatcherView> _views { get; set; }  
        #endregion

        #region Properties
        public Guid ProjectGuid
        {
            get { return _projectGuid; }
        }
        public Guid ModelGuid
        {
            get { return _modelGuid; }
        }
        public string CloudRegion
        {
            get { return _cloudRegion; }
        }
        public string FileName
        {
            get { return _fileName; }
        }
        public string FileDirectory
        {
            get { return _fileDirectory; }
        }
        public string ModelTitle
        {
            get { return _modelTitle; }
        }
        public string DWGExportSettings
        {
            get { return _dwgExportSettings; }
        }
        public bool IsCloudModel
        {
            get { return _IsCloudModel; }
        }
        public List<BatcherView> Views
        {
            get { return _views; }
        }
        #endregion
        [JsonConstructor]
        public BatcherModel(string ModelTitle, bool IsCloudModel, string ProjectGuid, string ModelGuid, string CloudRegion, string FileDirectory, string FileName, List<BatcherView> views, string DWGExportSettings)
        {
            _modelTitle = ModelTitle;
            _IsCloudModel = IsCloudModel;
            _cloudRegion   = CloudRegion;
            _projectGuid = Guid.Parse(ProjectGuid);
            _modelGuid = Guid.Parse(ModelGuid);
            _fileDirectory = FileDirectory;
            _fileName = FileName;
            _views = views;
            _dwgExportSettings = DWGExportSettings;
        }
        public BatcherModel(Document document, IList<View> views)
        {
            _modelTitle = document.Title;
            _IsCloudModel = document.IsModelInCloud;
            _dwgExportSettings = "";
            this._views = new List<BatcherView>();
            if (_IsCloudModel)
            {
                ModelPath path = document.GetCloudModelPath();
                _projectGuid = path.GetProjectGUID();
                _modelGuid = path.GetModelGUID();
                _cloudRegion = path.Region;
            }
            else
            {
                _fileName = Path.GetFileName(document.PathName);
                _fileDirectory = Path.GetDirectoryName(document.PathName);
            }
            foreach(View v in views)
            {
                BatcherView batcherView = new BatcherView(v);
                this.Views.Add(batcherView);
            }

        }
        public void SaveAsFile(string filepath)
        {

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
            
            File.WriteAllText(filepath, json);
            Utils.Show("Model Saved to Badger");
        }
        public Document OpenAsRevitModel(Application application)
        {
            ModelPath path = ModelPathUtils.ConvertCloudGUIDsToCloudPath(this.CloudRegion, this.ProjectGuid, this.ModelGuid);
            if (path == null)
            {
                return null;
            }
            OpenOptions options = new OpenOptions() { DetachFromCentralOption = DetachFromCentralOption.DoNotDetach };
            Document document = application.OpenDocumentFile(path, options);
            return document;
        }
        public UIDocument OpenAsRevitModelInUI(UIApplication application)
        {
            OpenOptions options = new OpenOptions() { DetachFromCentralOption = DetachFromCentralOption.DoNotDetach };
            ModelPath path = ModelPathUtils.ConvertCloudGUIDsToCloudPath(this.CloudRegion, this.ProjectGuid, this.ModelGuid);
            UIDocument batchDocument = application.OpenAndActivateDocument(path, options, false);
            return batchDocument;
        }
        public bool IsModelOpened(UIApplication application)
        {
            Document currentDocument = application.ActiveUIDocument.Document;
            bool result = false;
            if (currentDocument.IsModelInCloud)
            {
                ModelPath path = currentDocument.GetCloudModelPath();
                if (this._projectGuid == path.GetProjectGUID() && this.ModelGuid == path.GetModelGUID() && !currentDocument.IsLinked)
                {
                    result = true;
                }
            }
            else
            {
                if (this.ModelTitle == currentDocument.Title && !currentDocument.IsLinked)
                {
                    result = true;
                }
            }
            return result;
        }
        public List<View> GetViews(Document document)
        {
            List<View> views = new List<View>();
            foreach(BatcherView view in this.Views)
            {
                Element element = document.GetElement(new ElementId(view.ViewId));
                if(element != null && element.Name == view.ViewName)
                {
                    views.Add((View)element);
                }
                else
                {
                    Element matching = new FilteredElementCollector(document).OfClass(typeof(View)).WhereElementIsNotElementType().Where(v => ((View)v).Name == view.ViewName).FirstOrDefault();
                    if(matching != null)
                    {
                        views.Add((View)matching);
                    }
                }
            }
            return views;
        }
    }
    public class BatcherView
    {
        #region fields
        private string _viewName { get; set; }
        private string _exportFileName { get; set; }
        private int _viewId { get; set; }
        #endregion

        #region Properties
        public string ViewName
        {
            get { return _viewName; }
        }
        public string ExportFileName
        {
            get { return _exportFileName; }
        }
        public int ViewId
        {
            get { return _viewId; }
        }
        #endregion
        [JsonConstructor]
        public BatcherView(string ViewName, int ViewId, string ExportFileName = "")
        {
            _exportFileName = ExportFileName;
            _viewName = ViewName;   
            _viewId = ViewId;
        }
        public BatcherView(View view)
        {
            _exportFileName = string.Format("{0} - {1}", view.Document.Title, view.Name);
            _viewName = view.Name;
            _viewId = view.Id.IntegerValue;
        }

        public View GetView(Document document)
        {
            View result = null;
            Element element = document.GetElement(new ElementId(this.ViewId));
            if (element != null && element.Name == this.ViewName)
            {
                result = (View)element;
            }
            else
            {
                Element matching = new FilteredElementCollector(document).OfClass(typeof(View)).WhereElementIsNotElementType().Where(v => ((View)v).Name == this.ViewName).FirstOrDefault();
                if (matching != null)
                {
                    result = (View)matching;
                }
            }
            return result;
        }
    }
}
