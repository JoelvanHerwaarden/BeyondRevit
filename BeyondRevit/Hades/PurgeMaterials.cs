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
    public class PurgeMaterials : IExternalCommand, IHadesCommand
    {
        public void Command(ExternalCommandData commandData, bool prompt = true)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            IList<ElementId> usedMaterialIds = GetUsedMaterials(doc);
            ICollection<ElementId> allMaterials = new FilteredElementCollector(doc).OfClass(typeof(Material)).ToElementIds();
            int unusedMaterialsCount = allMaterials.Count - usedMaterialIds.Count;
            if (unusedMaterialsCount == 0)
            {
                if (prompt)
                {
                    TaskDialog.Show("Hades", "No Unused Materials Found");
                }
            }
            else
            {
                TaskDialogResult result = TaskDialogResult.Yes;
                if (prompt)
                {
                    result = HadesUtils.Show("Found " + unusedMaterialsCount.ToString() + " Unused Materials\nDo you want to delete them?");
                }

                if (result == TaskDialogResult.Yes)
                {
                    using (Transaction t = new Transaction(doc, "Purging Imported Line Styles"))
                    {
                        t.Start();
                        foreach (ElementId material in allMaterials)
                        {
                            if (!usedMaterialIds.Contains(material))
                            {
                                doc.Delete(material);
                            }
                        }
                        t.Commit();
                    }
                }
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            throw new NotImplementedException();
        }

        public static IList<ElementId> GetUsedMaterials(Document doc)
        {
            IList<Element> elementsInModel = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements();
            List<ElementId> usedMaterialIds = new List<ElementId>();

            foreach (Element element in elementsInModel)
            {
                ICollection<ElementId> materialIds = element.GetMaterialIds(false);

                foreach (ElementId id in materialIds)
                {
                    if (!usedMaterialIds.Contains(id))
                    {
                        usedMaterialIds.Add(id);
                    }
                }
                ICollection<ElementId> paintIds = element.GetMaterialIds(true);

                foreach (ElementId id in paintIds)
                {
                    if (!usedMaterialIds.Contains(id))
                    {
                        usedMaterialIds.Add(id);
                    }
                }
            }
            return usedMaterialIds;
        }
    }
}
