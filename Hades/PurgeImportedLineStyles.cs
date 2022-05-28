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
    public class PurgeImportedLineStyles : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            IList<Element> linesPatternElement = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement)).ToElements().Where(pattern => pattern.Name.StartsWith("IMPORT-")).ToList();
            if (linesPatternElement.Count == 0)
            {
                TaskDialog.Show("Hades", "No Imported Line Styles Found");
            }
            else
            {
                TaskDialogResult result = HadesUtils.Show("Found " + linesPatternElement.Count.ToString() + " Imported Line Styles Found\nDo you want to delete them?");

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Imported Line Styles"))
                    {
                        t.Start();
                        foreach (Element element in linesPatternElement)
                        {
                            doc.Delete(element.Id);
                        }
                        t.Commit();
                    }
                }
            }
            return Result.Succeeded;
        }
    }
}
