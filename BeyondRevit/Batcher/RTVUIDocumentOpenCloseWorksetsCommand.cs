using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forms = System.Windows.Forms;
using System.Xml.Linq;

namespace BeyondRevit.Batcher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RTVUIDocumentOpenCloseWorksetsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string[] batchFiles = OpenRevitBatchFiles();
            if (batchFiles == null)
            {
                return Result.Cancelled;
            }
            foreach (string batchFile in batchFiles)
            {
                Dictionary<string, string> info = ReadBatchFile(batchFile);
                ModelPath path = ModelPathUtils.ConvertCloudGUIDsToCloudPath(ModelPathUtils.CloudRegionEMEA, Guid.Parse(info["ProjectGuid"]), Guid.Parse(info["ModelGuid"]));
                OpenOptions options = new OpenOptions() 
                { 
                    DetachFromCentralOption = DetachFromCentralOption.DoNotDetach,
                };
                options.SetOpenWorksetsConfiguration(new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets));
                UIDocument batchDocument = commandData.Application.OpenAndActivateDocument(path, options, false);

            }
            Utils.Show("Models Opened!");
            return Result.Succeeded;
        }
        public static string[] OpenRevitBatchFiles()
        {
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog()
            {
                Filter = "Revit Batch Files (*.rbxml)|*.rbxml",
                Multiselect = true
            };
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                return dialog.FileNames;
            }
            else
            {
                return null;
            }
        }
        public static Dictionary<string, string> ReadBatchFile(string filepath)
        {
            Dictionary<string, string> result = new Dictionary<string, string>()
            {
                { "ProjectGuid", "" },
                { "ModelGuid", "" },
                { "View", "" },
            };
            XDocument batch = XDocument.Load(filepath);
            try
            {

                IEnumerator<XElement> project = batch.Descendants("Batch").Descendants("RevitModel").Descendants("WorksharingProjectGUID").GetEnumerator();

                while (project.MoveNext())
                {
                    result["ProjectGuid"] = project.Current.Value;
                }
            }
            catch
            {
                Utils.Show("Could not Find Project Guid");
            }
            try
            {
                IEnumerator<XElement> model = batch.Descendants("Batch").Descendants("RevitModel").Descendants("WorksharingCentralGUID").GetEnumerator();
                while (model.MoveNext())
                {
                    result["ModelGuid"] = model.Current.Value;
                }

            }
            catch
            {
                Utils.Show("Could not Find Model Guid");
            }
            try
            {
                IEnumerator<XElement> view = batch.Descendants("Batch").Descendants("Views").Descendants("View").Descendants("ViewName").GetEnumerator();
                while (view.MoveNext())
                {
                    result["View"] = view.Current.Value;
                }

            }
            catch
            {
                Utils.Show("Could not Find View");
            }
            return result;
        }
    }
}
