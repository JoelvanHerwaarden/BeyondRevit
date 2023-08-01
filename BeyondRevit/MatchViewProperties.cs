using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI.Selection;
using BeyondRevit.UI;
using Autodesk.Revit.Creation;

namespace BeyondRevit
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MatchWorksetVisibilities:IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.DB.Document doc = commandData.Application.ActiveUIDocument.Document;
            Dictionary<string, dynamic> views = MatchViewPropertiesUtils.GetViewsInModel(doc);
            GenericDropdownWindow window = new GenericDropdownWindow("Select Views", "Select Views", views, Utils.RevitWindow(commandData), false);
            window.ShowDialog();
            if (!window.Cancelled)
            {
                using(Transaction transaction = new Transaction(doc, "Match Workset Visibilties"))
                {
                    transaction.Start();
                    View sourceView = window.SelectedItems[0];
                    foreach (WorksetId id in new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksetIds())
                    {
                        doc.ActiveView.SetWorksetVisibility(id, sourceView.GetWorksetVisibility(id));
                    }
                    transaction.Commit();
                }
            }
            return Result.Succeeded;
        }
    }

    public class MatchViewPropertiesUtils
    {
        public static Dictionary<string, dynamic> GetViewsInModel(Autodesk.Revit.DB.Document doc)
        {
            IList<Element> collector = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            Dictionary<string, dynamic> filteredViews = new Dictionary<string, dynamic>();
            foreach (View view in collector)
            {
                if (!view.IsTemplate)
                {
                    try
                    {

                        filteredViews.Add(view.Name, view);
                    }
                    catch { }
                }
            }
            return filteredViews;
        }
    }
}
