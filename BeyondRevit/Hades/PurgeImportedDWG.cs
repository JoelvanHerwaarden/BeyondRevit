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
    public class PurgeImportedDWG : IExternalCommand, IHadesCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Command(commandData);
            return Result.Succeeded;
        }
        private static IList<dynamic> GetUnusedDWGs(Document doc, bool Report = false)
        {
            IList<dynamic> categories = new List<dynamic>();
            foreach(Category category in doc.Settings.Categories)
            {
                if (category.Name.Contains(".dwg"))
                {
                    IList<Element> elements = new FilteredElementCollector(doc).OfCategoryId(category.Id).ToElements();
                    if(elements.Count == 0)
                    {
                        categories.Add(category);
                    }
                    else if (elements[1].LookupParameter("Shared Site") == null)
                    {
                        categories.Add(category);
                    }
                }
            }
            return categories;


        }

        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            List<dynamic> unusedDwg = GetUnusedDWGs(doc) as List<dynamic>;
            if (unusedDwg.Count == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Imported/Unused DWG Files Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + unusedDwg.Count.ToString() + " Unused Imported/Unused DWG Files found\nDo you want to delete them?");
                }

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Imported/Unused DWG Files"))
                    {
                        t.Start();
                        foreach (Category category in unusedDwg)
                        {
                            doc.Delete(category.Id);
                        }
                        t.Commit();
                    }

                }
                else if (result == TaskDialogResult.Retry)
                {
                    List<dynamic> filters = HadesUtils.SelectivePurge(unusedDwg, commandData);
                    using (Transaction t = new Transaction(doc, "Purging Imported/Unused DWG Files"))
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
