using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeyondRevit.UI;

namespace BeyondRevit.Hades
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class PurgeScopeboxes : IExternalCommand, IHadesCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Command(commandData);
            return Result.Succeeded;
        }
        private static List<Element> GetUnusedScopeBoxes(Document doc, bool Report = false)
        {
            List<Element> unusedScopeBoxElements = new List<Element>();
            List<ElementId> scopeBoxesInused = new List<ElementId>();
            Category category = doc.Settings.Categories.get_Item("Scope Boxes");
            IList<ElementId> scopeboxes = new FilteredElementCollector(doc).OfCategoryId(category.Id).ToElementIds().ToList();
            IList<Element> viewElements = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
            foreach(Element viewElement in viewElements)
            {
                View view = (View)viewElement;
                Parameter parameter = view.LookupParameter("Scope Box");
                if(parameter!=null && parameter.AsElementId() != ElementId.InvalidElementId)
                {
                    scopeBoxesInused.Add(parameter.AsElementId());
                }
                
            }
            foreach(ElementId box in scopeboxes)
            {
                if (!scopeBoxesInused.Contains(box))
                {
                    unusedScopeBoxElements.Add(doc.GetElement(box));
                }
            }
            
            return unusedScopeBoxElements;


        }

        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<Element> unusedScopeboxes = GetUnusedScopeBoxes(doc);
            if (unusedScopeboxes.Count == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Unused Scope Boxes Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + unusedScopeboxes.Count.ToString() + " Unused Scope Boxes found\nDo you want to delete them?");
                }

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Unused Scope Boxes"))
                    {
                        t.Start();
                        foreach (Element element in unusedScopeboxes)
                        {
                            doc.Delete(element.Id);
                        }
                        t.Commit();
                    }

                }
                else if (result == TaskDialogResult.Retry)
                {
                    List<dynamic> filters = HadesUtils.SelectivePurge(unusedScopeboxes, commandData);
                    using (Transaction t = new Transaction(doc, "Purging Unused Scope Boxes"))
                    {
                        t.Start();
                        foreach (Element element in filters)
                        {
                            doc.Delete(element.Id);
                        }
                        t.Commit();
                    }
                }
            }
        }
    }
}
