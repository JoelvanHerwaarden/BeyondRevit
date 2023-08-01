using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using System.Collections.Generic;
using Forms = System.Windows.Forms;

namespace BeyondRevit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BadgerSPF : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Dictionary<string, dynamic> documents = new Dictionary<string, dynamic>();
            foreach(Document docu in commandData.Application.Application.Documents)
            {
                documents.Add(docu.Title, docu);
            }
            GenericDropdownWindow selectDocs = new GenericDropdownWindow("Badger - Select Documents", "Select Documents", documents, Utils.RevitWindow(commandData), true);
            selectDocs.ShowDialog();
            if (!selectDocs.Cancelled)
            {
                Dictionary<string, dynamic> selectedViews = new Dictionary<string, dynamic>();
                foreach(Document docu in selectDocs.SelectedItems)
                {
                    IList<Element> views = new FilteredElementCollector(docu).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
                    foreach(View v in views)
                    {
                        if (selectedViews.ContainsKey(v.Name))
                        {
                            selectedViews[v.Name].Add(v);
                        }
                        else
                        {
                            selectedViews.Add(v.Name, new List<Element>() { v });
                        }
                    }
                }
                GenericDropdownWindow viewSelection = new GenericDropdownWindow("Badger - Select Views", "Select Views to Export", selectedViews, Utils.RevitWindow(commandData), false);
                viewSelection.ShowDialog();
                if (!viewSelection.Cancelled)
                {
                    foreach(View view in selectedViews.Values)
                    {

                    }
                }
            }

            SYN.DataGenerator generator = new SYN.DataGenerator(commandData.Application.Application, commandData.Application.ActiveUIDocument.Document, commandData.Application.ActiveUIDocument.Document.ActiveView);
            generator.SaveSPFile(@"C:\Users\907335\Desktop\somefile.spf");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BadgerOpenFiles : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Filter = "Revit Files (*.rvt) | *.rvt";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                foreach(string filepath in dialog.FileNames)
                {
                    commandData.Application.OpenAndActivateDocument(ModelPathUtils.ConvertUserVisiblePathToModelPath(filepath), new OpenOptions(), false);
                }
            }
            return Result.Succeeded;
        }
    }
}
