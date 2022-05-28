using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllInstancesInProject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            IList<Reference> sourceElements = Utils.GetCurrentSelection(commandData.Application.ActiveUIDocument, null, "Select Elements");
            List<ElementId> typesToSelect = SelectionUtils.GetUniqueTypeIds(doc, sourceElements);
            List<ElementId> idsToSelect = SelectionUtils.GetAllInstances(doc, typesToSelect);
            uidoc.Selection.SetElementIds(idsToSelect);
            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllInstancesInCurrentView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;
            IList<Reference> sourceElements = Utils.GetCurrentSelection(commandData.Application.ActiveUIDocument, null, "Select Elements");
            List<ElementId> typesToSelect = SelectionUtils.GetUniqueTypeIds(doc, sourceElements);
            List<ElementId> idsToSelect = SelectionUtils.GetAllInstances(doc, typesToSelect, new List<View> { currentView });
            uidoc.Selection.SetElementIds(idsToSelect);
            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllInstancesInCurrentSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;
            ViewSheet viewSheet = null;
            if (currentView.ViewType == ViewType.DrawingSheet)
            {
                viewSheet = (ViewSheet)currentView;
            }
            else
            {
                viewSheet = SelectionUtils.GetSheetFromView(currentView);
            }
            if (viewSheet != null)
            {
                IList<View> viewsOnSheet = SelectionUtils.GetViewsOnSheet(viewSheet);
                IList<Reference> sourceElements = Utils.GetCurrentSelection(commandData.Application.ActiveUIDocument, null, "Select Elements");
                List<ElementId> typesToSelect = SelectionUtils.GetUniqueTypeIds(doc, sourceElements);
                List<ElementId> idsToSelect = SelectionUtils.GetAllInstances(doc, typesToSelect, viewsOnSheet);
                uidoc.Selection.SetElementIds(idsToSelect);
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllTypesInProject : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            IList<Reference> sourceElements = Utils.GetCurrentSelection(commandData.Application.ActiveUIDocument, null, "Select Elements");
            List<ElementId> typesToSelect = SelectionUtils.GetUniqueTypeIds(doc, sourceElements);
            List<ElementId> allTypesToSelect = SelectionUtils.GetAllSymbolIds(doc, typesToSelect);
            List<ElementId> idsToSelect = SelectionUtils.GetAllInstances(doc, allTypesToSelect);
            uidoc.Selection.SetElementIds(idsToSelect);
            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllTypesInCurrentView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;
            IList<Reference> sourceElements = Utils.GetCurrentSelection(commandData.Application.ActiveUIDocument, null, "Select Elements");
            List<ElementId> typesToSelect = SelectionUtils.GetUniqueTypeIds(doc, sourceElements);
            List<ElementId> allTypesToSelect = SelectionUtils.GetAllSymbolIds(doc, typesToSelect);
            List<ElementId> idsToSelect = SelectionUtils.GetAllInstances(doc, allTypesToSelect, new List<View> { currentView });
            uidoc.Selection.SetElementIds(idsToSelect);
            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllTypesInCurrentSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;
            ViewSheet viewSheet = null;
            if (currentView.ViewType == ViewType.DrawingSheet)
            {
                viewSheet = (ViewSheet)currentView;
            }
            else
            {
                viewSheet = SelectionUtils.GetSheetFromView(currentView);
            }
            if (viewSheet != null)
            {
                IList<View> viewsOnSheet = SelectionUtils.GetViewsOnSheet(viewSheet);
                IList<Reference> sourceElements = Utils.GetCurrentSelection(commandData.Application.ActiveUIDocument, null, "Select Elements");
                List<ElementId> typesToSelect = SelectionUtils.GetUniqueTypeIds(doc, sourceElements);
                List<ElementId> allTypesToSelect = SelectionUtils.GetAllSymbolIds(doc, typesToSelect);
                List<ElementId> idsToSelect = SelectionUtils.GetAllInstances(doc, allTypesToSelect, viewsOnSheet);
                uidoc.Selection.SetElementIds(idsToSelect);
            }
            return Result.Succeeded;
        }
    }
    

    public class SelectionUtils
    {

        public static List<ElementId> GetUniqueTypeIds(Document document, IList<Reference> elements)
        {
            List<ElementId> typeList = new List<ElementId>();
            foreach (Reference reference in elements)
            {
                Element e = document.GetElement(reference.ElementId);
                ElementId typeId = e.GetTypeId();
                if (!typeList.Contains(typeId))
                {
                    typeList.Add(typeId);
                }
            }
            return typeList;
        }

        public static List<ElementId> GetAllSymbolIds(Document document, IList<ElementId> elementIds)
        {
            List<ElementId> typeList = new List<ElementId>();
            foreach (ElementId id in elementIds)
            {
                Element element = document.GetElement(id);
                try
                {
                    FamilySymbol symbol = (FamilySymbol)element;
                    Family family = symbol.Family;
                    typeList.AddRange(family.GetFamilySymbolIds());
                }
                catch
                {
                    typeList.Add(id);
                }
            }
            return typeList;
        }

        public static List<ElementId> GetAllInstances(Document doc, List<ElementId> typeIds, IList<View> views = null)
        {
            List<ElementId> idsToSelect = new List<ElementId>();
            if(views != null)
            {
                foreach(View view in views)
                {
                    IEnumerable<Element> elementsInView = new FilteredElementCollector(doc, view.Id).WhereElementIsNotElementType().ToElements().Where(element => typeIds.Contains(element.GetTypeId()));
                    foreach(Element e in elementsInView)
                    {
                        idsToSelect.Add(e.Id);
                    }
                }
            }
            else
            {
                IEnumerable<Element> elementsInView = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements().Where(element => typeIds.Contains(element.GetTypeId()));
                foreach (Element e in elementsInView)
                {
                    idsToSelect.Add(e.Id);
                }
            }
            return idsToSelect;
        }

        public static ViewSheet GetSheetFromView(View view)
        {
            ViewSheet Result = null;
            IList<Element> sheets = new FilteredElementCollector(view.Document).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements();
            foreach(ViewSheet sheet in sheets)
            {
                if (sheet.GetAllPlacedViews().Contains(view.Id))
                {
                    Result = sheet;
                    break;
                }
            }
            return Result;
        }

        public static IList<View> GetViewsOnSheet(ViewSheet sheet)
        {
            IList<View> views = new List<View>();
            foreach(ElementId id in sheet.GetAllPlacedViews())
            {
                views.Add((View)sheet.Document.GetElement(id));
            }
            return views;
        }

    }
}
