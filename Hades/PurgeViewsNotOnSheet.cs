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
    public class PurgeViewsNotOnSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            IList<ElementId> unplacedViews = GetUnplacedViews(doc);
            if (unplacedViews.Count == 0)
            {
                TaskDialog.Show("Hades", "No Unplaced Views Found");
            }
            else
            {
                TaskDialogResult result = HadesUtils.Show("Found " + unplacedViews.Count.ToString() + " Unplaced Views\nDo you want to delete them?");

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Unplaced Views"))
                    {
                        t.Start();
                        foreach (ElementId id in unplacedViews)
                        {
                            try
                            {
                                doc.Delete(id);

                            }
                            catch { }
                        }
                        t.Commit();
                    }
                }
            }
            return Result.Succeeded;
        }

        public static List<ElementId> GetUnplacedViews(Document doc)
        {
            IList<ViewType> AllowedTypes = new List<ViewType>
            {
                ViewType.Report,
                ViewType.Legend,
                ViewType.Schedule,
                ViewType.ColumnSchedule,
                ViewType.PanelSchedule,
                ViewType.SystemBrowser,
                ViewType.SystemsAnalysisReport,
                ViewType.CostReport,
                ViewType.LoadsReport,
                ViewType.PresureLossReport,
                ViewType.Walkthrough,
                ViewType.Rendering,
                ViewType.Internal,
                ViewType.ProjectBrowser,
                ViewType.DrawingSheet
            };
            List<ElementId> unplacedViews = new List<ElementId>();
            List<ElementId> placedViews = new List<ElementId>();
            IList<Element> viewSheetElements = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements();
            foreach (Element viewElement in viewSheetElements)
            {
                ViewSheet sheet = (ViewSheet)viewElement;
                placedViews.AddRange(sheet.GetAllPlacedViews());
            }
            IList<Element> views = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();

            foreach (View view in views)
            {
                if(!AllowedTypes.Contains(view.ViewType) & !view.Name.StartsWith("{3D}") & !view.IsTemplate)
                {
                    Parameter p = GetSheetNumberParameter(view);
                    if (p != null & p.AsString() == "---")
                    {
                        unplacedViews.Add(view.Id);
                    }
                }
                
            }
            return unplacedViews;
        }

        public static Parameter GetSheetNumberParameter(View view)
        {
            Parameter Result = null;
            foreach (Parameter p in view.Parameters)
            {
                if(p.Definition.Name == "Sheet Number")
                {
                    Result = p;
                    break;
                }
            }
            return Result;
        }
    }
}
