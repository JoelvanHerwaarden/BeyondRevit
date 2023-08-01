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
    public class PurgeViewTemplates : IExternalCommand, IHadesCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Command(commandData);
            return Result.Succeeded;
        }
        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<ElementId> UnusedViewTemplates = GetUnusedViewTemplates(doc);
            if (UnusedViewTemplates.Count == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Unused View Templates Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + UnusedViewTemplates.Count.ToString() + " Unused View Templates found\nDo you want to delete them?");
                }

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Unused View Templates"))
                    {
                        t.Start();
                        foreach (ElementId id in UnusedViewTemplates)
                        {
                            doc.Delete(id);
                        }
                        t.Commit();
                    }

                }
            }
        }

        private List<ElementId> GetUnusedViewTemplates(Document doc, bool Report = false)
        {
            List<ElementId> UnusedViewTemplates = new List<ElementId>();
            List<ElementId> UsedViewTemplates = new List<ElementId>();
            IList<Element> viewElements = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            foreach (Element viewElement in viewElements)
            {
                View view = (View)viewElement;
                if (!view.IsTemplate & view.ViewTemplateId != ElementId.InvalidElementId)
                {
                    UsedViewTemplates.Add(view.ViewTemplateId);
                }
            }
            foreach (Element viewElement in viewElements)
            {
                View view = (View)viewElement;
                if (view.IsTemplate & !UsedViewTemplates.Contains(view.Id))
                {
                    UnusedViewTemplates.Add(view.Id);
                }
            }
            return UnusedViewTemplates;


        }
    }
}
