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
    public class PurgeViewFilters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<ElementId> unusedViewFilters = GetUnusedViewFilters(doc);
            if (unusedViewFilters.Count == 0)
            {
                TaskDialog.Show("Hades", "No Unused View Filters Found");
            }
            else
            {
                TaskDialogResult result = HadesUtils.Show("Found " + unusedViewFilters.Count.ToString() + " Unused View Filters found\nDo you want to delete them?");

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Unused View Filters"))
                    {
                        t.Start();
                        foreach(ElementId id in unusedViewFilters)
                        {
                            doc.Delete(id);
                        }
                        t.Commit();
                    }

                }

            }
            return Result.Succeeded;
        }
        private List<ElementId> GetUnusedViewFilters(Document doc, bool Report = false)
        {
            List<ElementId> UnusedViewFilters = new List<ElementId>();
            List<ElementId> UsedViewFilters = new List<ElementId>();
            IList<Element> viewElements = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
            foreach(Element viewElement in viewElements)
            {
                View view = (View)viewElement;
                try
                {
                    UsedViewFilters.AddRange(view.GetFilters());
                }
                catch { }
            }
            IList<Element> viewFilterElements = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).ToElements();
            foreach(Element viewFilterElement in viewFilterElements)
            {
                ParameterFilterElement parameterFilterElement = (ParameterFilterElement)viewFilterElement;
                if (!UsedViewFilters.Contains(parameterFilterElement.Id))
                {
                    UnusedViewFilters.Add(parameterFilterElement.Id);
                }
            }
            return UnusedViewFilters;


        }
    }
}
