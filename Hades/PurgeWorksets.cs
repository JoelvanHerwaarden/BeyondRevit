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
    public class PurgeWorksets : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Workset> emptyWorksets = GetEmptyWorksets(doc);
            if(emptyWorksets.Count == 0)
            {
                TaskDialog.Show("Hades", "No Empty Worksets Found");
            }
            else
            {
                TaskDialog dialog = new TaskDialog("Hades");
                TaskDialogResult result = HadesUtils.Show("Found " + emptyWorksets.Count.ToString() + " empty worksets found\nDo you want to delete them?", "Revit does not allow Hades to delete the worksets for you\nHades will open the workset window and mark the worksets with AAA_ at the front and _CanBeDeleted at the end.\nYou can directly delete the marked worksets from there.");
                    
                if(result== TaskDialogResult.Yes)
                {
                    List<WorksetId> worksetIds = new List<WorksetId>();
                    using (Transaction t = new Transaction(doc, "Purging Empty Worksets"))
                    {
                        t.Start();
                        foreach (Workset workset in emptyWorksets)
                        {
                            string newName = "AAA_"+workset.Name + "_CanBeDeleted";
                            WorksetTable.RenameWorkset(doc, workset.Id, newName);
                            worksetIds.Add(workset.Id);
                        }
                        t.Commit();
                    }

                    TransactWithCentralOptions options = new TransactWithCentralOptions();
                    WorksharingUtils.CheckoutWorksets(doc, new HashSet<WorksetId>(worksetIds), options);
                    commandData.Application.PostCommand(RevitCommandId.LookupPostableCommandId(PostableCommand.Worksets));
                }

            }
            return Result.Succeeded;
        }
        private List<Workset> GetEmptyWorksets(Document doc, bool Report = false)
        {
            List<Workset> emptyWorksets = new List<Workset>();
            string report = "Workset Report\n\n";
            ICollection<Workset> worksetIds = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
            foreach(Workset workset in worksetIds)
            {
                WorksetId worksetId = workset.Id;
                ElementWorksetFilter filter = new ElementWorksetFilter(worksetId);
                ICollection<ElementId> elementIds = new FilteredElementCollector(doc).WherePasses(filter).ToElementIds();
                report += elementIds.Count().ToString() + " elements in "+workset.Name+"\n";
                if (elementIds.Count() == 0)
                {
                    emptyWorksets.Add(workset);
                }
            }
            if (Report)
            {
                Utils.Show(report);
            }
            return emptyWorksets;
        }
    }   
}
