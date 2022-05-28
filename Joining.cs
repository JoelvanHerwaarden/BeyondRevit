using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI.Selection;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DisallowJoins : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            IList<Reference> references = Utils.GetCurrentSelection(uiDoc, new JoiningUtils.WallBeamFilter(), "Select Walls or Beams");

            using (Transaction t = new Transaction(doc, "Disallow Joints"))
            {
                t.Start();
                foreach (Reference reference in references)
                {
                    Element element = doc.GetElement(reference.ElementId);
                    Category category = element.Category;
                    if (category.Name == "Structural Framing")
                    {
                        FamilyInstance f = (FamilyInstance)element;
                        Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(f, 0);
                        Autodesk.Revit.DB.Structure.StructuralFramingUtils.DisallowJoinAtEnd(f, 1);
                    }
                    else if (category.Name == "Walls")
                    {
                        Wall w = (Wall)element;
                        Autodesk.Revit.DB.WallUtils.DisallowWallJoinAtEnd(w, 0);
                        Autodesk.Revit.DB.WallUtils.DisallowWallJoinAtEnd(w, 1);
                    }


                }
                doc.Regenerate();
                t.Commit();
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AllowJoins : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            IList<Reference> references = Utils.GetCurrentSelection(uiDoc, new JoiningUtils.WallBeamFilter(), "Select Walls or Beams");
            IList<Element> el = new List<Element>();

            using (Transaction t = new Transaction(doc, "Disallow Joints"))
            {
                t.Start();
                foreach (Reference reference in references)
                {
                    Element element = doc.GetElement(reference.ElementId);
                    Category category = element.Category;
                    if (category.Name == "Structural Framing")
                    {
                        FamilyInstance f = (FamilyInstance)element;
                        Autodesk.Revit.DB.Structure.StructuralFramingUtils.AllowJoinAtEnd(f, 0);
                        Autodesk.Revit.DB.Structure.StructuralFramingUtils.AllowJoinAtEnd(f, 1);
                    }
                    else if (category.Name == "Walls")
                    {
                        Wall w = (Wall)element;
                        Autodesk.Revit.DB.WallUtils.AllowWallJoinAtEnd(w, 0);
                        Autodesk.Revit.DB.WallUtils.AllowWallJoinAtEnd(w, 1);
                    }
                }
                doc.Regenerate();
                t.Commit();
            }

            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CutMultipleElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Selection selection = uiDoc.Selection;
            IList<Reference> cuttingElements = selection.PickObjects(ObjectType.Element, "Select the Elements which are being cut");
            IList<Reference> cuttedElements = selection.PickObjects(ObjectType.Element, "Select the Cutting Elements");
            using (Transaction t = new Transaction(doc, "Cut MultipleElements"))
            {
                t.Start();
                foreach (Reference cuttingRef in cuttingElements)
                {
                    foreach (Reference cuttedRef in cuttedElements)
                    {
                        Element cut = doc.GetElement(cuttingRef.ElementId);
                        Element cutted = doc.GetElement(cuttedRef.ElementId);
                        try
                        {
                            SolidSolidCutUtils.AddCutBetweenSolids(doc, cutted, cut, false);
                        }
                        catch
                        {
                            try
                            {
                                InstanceVoidCutUtils.AddInstanceVoidCut(doc, cutted, cut);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }
                t.Commit();
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class JoinMultipleElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string warnings = "";
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Selection selection = uiDoc.Selection;
            IList<Reference> cuttingElements = selection.PickObjects(ObjectType.Element, "Select the Elements which are being cut");
            IList<Reference> cuttedElements = selection.PickObjects(ObjectType.Element, "Select the Cutting Elements");
            using (Transaction t = new Transaction(doc, "Join Multiple Elements"))
            {
                t.Start();
                foreach (Reference cuttingRef in cuttingElements)
                {
                    foreach (Reference cuttedRef in cuttedElements)
                    {
                        Element cut = doc.GetElement(cuttingRef.ElementId);
                        Element cutted = doc.GetElement(cuttedRef.ElementId);
                        try
                        {
                            JoinGeometryUtils.JoinGeometry(doc, cutted, cut);
                        }
                        catch (Exception e)
                        {
                            warnings += e.Message + "\n";
                            continue;
                        }
                    }
                }
                t.Commit();
            }
            if (warnings != "")
            {
                Utils.Show(warnings);
            }
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IsolatedJoinedElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Selection selection = uiDoc.Selection;
            IList<Reference> sourceElements = selection.PickObjects(ObjectType.Element, "Select Elements");
            IList<ElementId> elementsToIsolate = new List<ElementId>();
            foreach(Reference sourceElement in sourceElements)
            {
                elementsToIsolate.Add(sourceElement.ElementId);
                ICollection<ElementId> joinedElements = JoinGeometryUtils.GetJoinedElements(doc, doc.GetElement(sourceElement));
                foreach(ElementId elementId in joinedElements)
                {
                    if (!elementsToIsolate.Contains(elementId))
                    {
                        elementsToIsolate.Add(elementId);
                    }
                }
            }
            using(Transaction transaction = new Transaction(doc, "Isolate Joined Elements"))
            {
                transaction.Start();
                doc.ActiveView.IsolateElementsTemporary(elementsToIsolate);
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SwitchJoinOrder : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            Selection selection = uiDoc.Selection;
            IList<Reference> cuttingElements = selection.PickObjects(ObjectType.Element, "Select the First Elements");
            IList<Reference> cuttedElements = selection.PickObjects(ObjectType.Element, "Select the Second Elements");
            using (Transaction t = new Transaction(doc, "Switch Join Order Multiple Elements"))
            {
                t.Start();
                foreach (Reference cuttingRef in cuttingElements)
                {
                    foreach (Reference cuttedRef in cuttedElements)
                    {
                        Element cut = doc.GetElement(cuttingRef.ElementId);
                        Element cutted = doc.GetElement(cuttedRef.ElementId);
                        if(JoinGeometryUtils.AreElementsJoined(doc, cut, cutted))
                        {
                            JoinGeometryUtils.SwitchJoinOrder(doc, cut, cutted);
                        }
                    }
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }

    public class JoiningUtils
    {
        public class WallBeamFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;


                BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

                if (builtInCategory == BuiltInCategory.OST_StructuralFraming || builtInCategory == BuiltInCategory.OST_Walls) return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

            public static int GetCategoryIdAsInteger(Element element)
            {
                return element?.Category?.Id?.IntegerValue ?? -1;
            }
        }

    }
}
