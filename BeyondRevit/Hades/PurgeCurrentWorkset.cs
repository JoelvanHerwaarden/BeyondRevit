using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Hades
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PurgeCurrentWorkset : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            WorksetTable table = doc.GetWorksetTable();
            WorksetId worksetId = table.GetActiveWorksetId();
            ICollection<ElementId> worksetElements = GetActiveWorksetElements(doc, worksetId);
            Workset workset = table.GetWorkset(worksetId);
            if(worksetElements.Count == 0)
            {
                TaskDialog.Show("Hades", "No Elements in this workset.\nRecommend to commence a purge for empty worksets.");
            }
            else
            {
                TaskDialogResult result = HadesUtils.Show("Found " + worksetElements.Count.ToString() + " elements in Workset:\n"+ workset.Name+"\nDo you want to delete them?", Report(doc, worksetElements));

                if (result== TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Elements in "+workset.Name))
                    {
                        t.Start();
                        foreach(ElementId element in worksetElements)
                        {
                            doc.Delete(element);
                        }
                        t.Commit();
                    }
                }

            }
            return Result.Succeeded;
        }
        private ICollection<ElementId> GetActiveWorksetElements(Document doc, WorksetId worksetId, bool Report = false)
        {

            ElementWorksetFilter filter = new ElementWorksetFilter(worksetId);
            ICollection<ElementId> elementIds = new FilteredElementCollector(doc).WherePasses(filter).ToElementIds();

            return elementIds;
        }

        private string Report(Document doc, ICollection<ElementId> elementIds)
        {
            string report = "";
            foreach(ElementId elementId in elementIds)
            {
                Element element = doc.GetElement(elementId);
                report += element.Name + " - " + element.GetType().ToString() + "\n";
            }
            return report;
        }
    }   
}
