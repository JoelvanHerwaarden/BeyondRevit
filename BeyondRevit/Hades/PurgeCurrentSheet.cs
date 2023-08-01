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
    public class PurgeCurrentSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            if (doc.ActiveView.ViewType != ViewType.DrawingSheet)
            {
                Utils.Show("This command can only run in a Sheet");
            }
            ViewSheet sheet = (ViewSheet)doc.ActiveView;
            ICollection<ElementId> viewIds = sheet.GetAllPlacedViews();
            viewIds.Add(sheet.Id);

            SetOtherView(uidoc, viewIds.ToList());
            List<ViewType> viewTypesToIgnore = new List<ViewType>()
            { 
                ViewType.Legend,
                ViewType.Schedule,
                ViewType.ColumnSchedule,
                ViewType.PanelSchedule
            };
            using (Transaction transaction = new Transaction(doc, "Purge Current Sheet"))
            {
                transaction.Start();
                foreach (ElementId viewid in viewIds)
                {
                    View view = doc.GetElement(viewid) as View;
                    if (view != null)
                    {
                        if (!viewTypesToIgnore.Contains(view.ViewType)) 
                        {
                            doc.Delete(viewid);
                        }
                    }
                }
                transaction.Commit();
            }

            return Result.Succeeded;
        }

        public View SetOtherView(UIDocument uidoc, List<ElementId> ViewsToIgnore)
        {
            View result = null;
            Document doc = uidoc.Document;
            IList<Element> views = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            IList<UIView> openViews = uidoc.GetOpenUIViews();
            for(int i = 0; i < views.Count; i++)
            {
                View view = views[i] as View;
                if (ViewsToIgnore.Contains(view.Id))
                {
                    continue;
                }
                else
                {
                    try
                    {
                        uidoc.ActiveView = view;
                        uidoc.RefreshActiveView();
                        break;
                    }
                    catch { }
                }
            }
            return result;
        }
    }
}
