using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Hades
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PurgeViewsNotOnSheet : IExternalCommand, IHadesCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Command(commandData);
            return Result.Succeeded;
        }
        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            IList<ElementId> unplacedViews = PurgeUnplacedViewsUtils.GetUnplacedViews(doc);
            IList<dynamic> unplacedViewsToDelete = new List<dynamic>();

            if (unplacedViews.Count == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Unplaced Views Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + unplacedViews.Count.ToString() + " Unplaced Views\nDo you want to delete them?");
                }

                if (result == TaskDialogResult.Yes)
                {
                    unplacedViewsToDelete = (List<dynamic>)unplacedViews;
                    
                }
                else if (result == TaskDialogResult.Retry) 
                {
                    Dictionary<string, dynamic> map = new Dictionary<string, dynamic>();
                    foreach(ElementId id in unplacedViews)
                    {
                        View v = (View)doc.GetElement(id);
                        map.Add(v.Name + " - " +v.Id, id);   
                    }
                    GenericDropdownWindow window = new GenericDropdownWindow("Select Views", "Select unused views to delete", map, Utils.RevitWindow(commandData), true);
                    window.ShowDialog();
                    if (!window.Cancelled)
                    {
                        unplacedViewsToDelete = window.SelectedItems;
                    }
                }
                using (Transaction t = new Transaction(doc, "Purging Unplaced Views"))
                {
                    t.Start();
                    foreach (ElementId id in unplacedViewsToDelete)
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
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PurgeLegendsAndSchedulesNotOnSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Command(commandData);
            return Result.Succeeded;
        }
        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            IList<ElementId> unplacedViews = PurgeUnplacedViewsUtils.GetUnplacedLegendsAndSchedules(doc, uidoc.Selection.GetElementIds());
            if (unplacedViews.Count == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Unplaced Legends & Schedules Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + unplacedViews.Count.ToString() + " Unplaced Legends & Schedules\nDo you want to delete them?");
                }

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Unplaced Legends and Schedules"))
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
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PurgeViewsNotOnSheetBySelection : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Command(commandData);
            return Result.Succeeded;
        }
        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            IList<ElementId> unplacedViews = PurgeUnplacedViewsUtils.GetUnplacedViews(doc, uidoc.Selection.GetElementIds());
            if (unplacedViews.Count == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Unplaced Views Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + unplacedViews.Count.ToString() + " Unplaced Views\nDo you want to delete them?");
                }

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
        }
    }

    public class PurgeUnplacedViewsUtils
    {
        public static List<ElementId> GetUnplacedViews(Document doc, ICollection<ElementId> selection = null)
        {
            //Define a List with View Types which cannot be purged
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

            //Create a List for Unplaced Views
            List<ElementId> unplacedViews = new List<ElementId>();

            List<ElementId> placedViews = new List<ElementId>();

            //Get all Sheets in the Model and Check which views are on a Sheet.
            IList<Element> viewSheetElements = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements();
            foreach (Element viewElement in viewSheetElements)
            {
                ViewSheet sheet = (ViewSheet)viewElement;
                placedViews.AddRange(sheet.GetAllPlacedViews());
            }

            //Get All the Views in the model
            IEnumerable<Element> views = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            if (selection != null)
            {
                views = selection.Select(v => doc.GetElement(v));
            }

            foreach (View view in views)
            {
                if (!AllowedTypes.Contains(view.ViewType) & !view.Name.StartsWith("{3D}") & !view.IsTemplate)
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


        public static List<ElementId> GetUnplacedLegendsAndSchedules(Document doc, ICollection<ElementId> selection = null)
        {
            //Define a List with View Types which cannot be purged
            IList<ViewType> AllowedTypes = new List<ViewType>
            {
                ViewType.Legend,
                ViewType.Schedule,
                ViewType.ColumnSchedule,
                ViewType.PanelSchedule
            };

            //Create a List for Unplaced Views
            List<ElementId> unplacedViews = new List<ElementId>();

            List<ElementId> placedViews = new List<ElementId>();

            //Get all Sheets in the Model and Check which views are on a Sheet.
            IList<Element> viewSheetElements = new FilteredElementCollector(doc).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements();
            foreach (Element viewElement in viewSheetElements)
            {
                ViewSheet sheet = (ViewSheet)viewElement;
                placedViews.AddRange(sheet.GetAllPlacedViews());
            }

            //Get All the Views in the model
            IEnumerable<Element> views = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            if (selection != null)
            {
                views = selection.Select(v => doc.GetElement(v));
            }

            foreach (View view in views)
            {
                if (AllowedTypes.Contains(view.ViewType) & !view.IsTemplate)
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
                if (p.Definition.Name == "Sheet Number")
                {
                    Result = p;
                    break;
                }
            }
            return Result;
        }
    }
}
