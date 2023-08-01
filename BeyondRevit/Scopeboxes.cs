using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Scopeboxes
{
    [Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DetachScopeboxFromCurrentView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            View currentView = doc.ActiveView;
            Parameter scopeboxParameter = currentView.LookupParameter("Scope Box");
            if(scopeboxParameter != null && scopeboxParameter.AsElementId() != ElementId.InvalidElementId)
            {
                Element oldScopebox = doc.GetElement(scopeboxParameter.AsElementId());
                string scopeboxName = oldScopebox.Name;
                List<View> views = GetScopeboxViews(oldScopebox, currentView);
                using(Transaction transaction = new Transaction(doc, "Detach Scopebox from Current View"))
                {
                    transaction.Start();
                    Element newScopebox = doc.GetElement(ElementTransformUtils.CopyElement(doc, oldScopebox.Id, new XYZ()).First());
                    foreach(View view in views)
                    {
                        view.LookupParameter("Scope Box").Set(newScopebox.Id);
                    }
                    doc.Delete(oldScopebox.Id);
                    newScopebox.Name = scopeboxName;
                    transaction.Commit();
                }
            }
            return Result.Succeeded;
        }

        public static List<View> GetScopeboxViews(Element scopebox, View viewToSkip)
        {
            List<View> views = new List<View>();
            IList<Element> allModelViews = new FilteredElementCollector(scopebox.Document).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            foreach(View view in allModelViews)
            {
                if(view.Id != viewToSkip.Id)
                {
                    Parameter scopeboxParameter = view.LookupParameter("Scope Box");
                    if (scopeboxParameter != null && scopeboxParameter.AsElementId() == scopebox.Id)
                    {
                        views.Add(view);
                    }
                }
                
            }
            return views;
        }

    }
}
