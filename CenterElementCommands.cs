using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using adskWindows = Autodesk.Windows;

namespace BeyondRevit.CenterElementCommands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CenterBetweenPoints : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            try
            {
                using (Transaction transaction = new Transaction(doc, "Center Element Between Points"))
                {
                    transaction.Start();
                    //Select Start and End Point. Calculate Mid Point
                    Reference referenceToCenter = selection.PickObject(ObjectType.Element, "Select Object to Center");
                    Utils.SketchplaneByView(doc.ActiveView);
                    XYZ start = selection.PickPoint("Pick the First Point");
                    XYZ end = selection.PickPoint("Pick the First Point");
                    XYZ mid = Line.CreateBound(start, end).Evaluate(0.5, true);

                    //Get Element Location
                    Element element = doc.GetElement(referenceToCenter);
                    XYZ elementLocation = CenterElementUtils.GetElementLocation(element);

                    //Project Points
                    XYZ newLocation = CenterElementUtils.Get2DPointInView(doc.ActiveView, mid);
                    XYZ oldLocation = CenterElementUtils.Get2DPointInView(doc.ActiveView, elementLocation);

                    ElementTransformUtils.MoveElement(doc, element.Id, newLocation.Subtract(oldLocation));
                    transaction.Commit();
                }
            }
            catch (OperationCanceledException)
            {

                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CenterBetweenPointsPerpendicular : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            try
            {
                using (Transaction transaction = new Transaction(doc, "Center Element Between Points"))
                {
                    transaction.Start();
                    //Select Start and End Point. Calculate Mid Point
                    Reference referenceToCenter = selection.PickObject(ObjectType.Element, "Select Object to Center");
                    Utils.SketchplaneByView(doc.ActiveView);
                    XYZ start = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick the First Point"));
                    XYZ end = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick the Second Point"));
                    Line line = Line.CreateBound(start, end);
                    XYZ mid = line.Evaluate(0.5, true);

                    //Get Element Location
                    Element element = doc.GetElement(referenceToCenter);
                    XYZ elementLocation = CenterElementUtils.Get2DPointInView(doc.ActiveView, CenterElementUtils.GetElementLocation(element));

                    //Project Points
                    XYZ projectionPlaneOrigin = CenterElementUtils.Get2DPointInView(doc.ActiveView, mid);
                    Plane projectionPlane = Plane.CreateByNormalAndOrigin(line.Direction, projectionPlaneOrigin);
                    XYZ newLocation = CenterElementUtils.Get2DPointInPlane(projectionPlane, elementLocation);

                    ElementTransformUtils.MoveElement(doc, element.Id, newLocation.Subtract(elementLocation));
                    transaction.Commit();
                }
            }
            catch (OperationCanceledException)
            {

                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RedistributeElements : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            try
            {
                using (Transaction transaction = new Transaction(doc, "Center Element Between Points"))
                {
                    transaction.Start();
                    
                    //Select Start and End Point. Calculate Mid Point
                    IList<Reference> references = selection.PickObjects(ObjectType.Element, "Select Elements to Redistribute");
                    Utils.SketchplaneByView(doc.ActiveView);
                    List<double> range = CenterElementUtils.CreateRange(0, 1, references.Count);
                    XYZ start = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick the First Point"));
                    XYZ end = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick the Second Point"));
                    Line line = Line.CreateBound(start, end);
                    for (int i = 0; i < references.Count; i++)
                    {
                        Element element = doc.GetElement(references[i]);
                        XYZ elementLocation = CenterElementUtils.Get2DPointInView(doc.ActiveView, CenterElementUtils.GetElementLocation(element));
                        XYZ newLocation = line.Evaluate(range[i], true);
                        ElementTransformUtils.MoveElement(doc, element.Id, newLocation.Subtract(elementLocation));
                    }
                    transaction.Commit();
                }
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }
            
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RedistributeElementsWithoutEnds : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            
            try
            {
                using (Transaction transaction = new Transaction(doc, "Center Element Between Points"))
                {
                    transaction.Start();

                    //Select Start and End Point. Calculate Mid Point
                    IList<Reference> references = selection.PickObjects(ObjectType.Element, "Select Elements to Redistribute");
                    Utils.SketchplaneByView(doc.ActiveView);
                    List<double> range = CenterElementUtils.CreateRange(0, 1, references.Count + 2);
                    range.Remove(range[references.Count + 1]);
                    range.Remove(range[0]);

                    XYZ start = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick the First Point"));
                    XYZ end = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick the Second Point"));
                    Line line = Line.CreateBound(start, end);
                    for (int i = 0; i < references.Count; i++)
                    {
                        Element element = doc.GetElement(references[i]);
                        XYZ elementLocation = CenterElementUtils.Get2DPointInView(doc.ActiveView, CenterElementUtils.GetElementLocation(element));
                        XYZ newLocation = line.Evaluate(range[i], true);
                        ElementTransformUtils.MoveElement(doc, element.Id, newLocation.Subtract(elementLocation));
                    }
                    transaction.Commit();
                }
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignWithOffset : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;


            try
            {
                using (Transaction transaction = new Transaction(doc, "Align Elements With Offset"))
                {
                    transaction.Start();

                    //Select Elements
                    IList<Reference> refs = Utils.GetCurrentSelection(uidoc, null, "Select Elements");
                    IList<ElementId> elementIds = new List<ElementId>();
                    foreach (Reference r in refs)
                    {
                        elementIds.Add(r.ElementId);
                    }

                    //Select Ref Point
                    Utils.SketchplaneByView(doc.ActiveView);
                    XYZ refPoint = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick Point to Align"));

                    //Compute Plane By Face
                    Reference refFace = selection.PickObject(ObjectType.Face, "Select Face");
                    GeometryObject geoObject = doc.GetElement(refFace).GetGeometryObjectFromReference(refFace);
                    PlanarFace face = geoObject as PlanarFace;

                    XYZ normal = face.ComputeNormal(new UV(0.5, 0.5));
                    XYZ origin = face.Evaluate(new UV(0.5, 0.5));
                    Plane facePlane = Plane.CreateByNormalAndOrigin(normal, origin);

                    //Get New Location
                    facePlane.Project(refPoint, out UV newLocationUV, out double distance);
                    XYZ newLocation = CenterElementUtils.UVToXYZInPlane(facePlane, newLocationUV);
                    if (CenterElementUtils.GetOffsetValue(out double offset))
                    {
                        double alignmentDistance = newLocation.DistanceTo(refPoint) - Utils.ToInternalUnits(doc, offset);
                        ElementTransformUtils.MoveElements(doc, elementIds, (newLocation.Subtract(refPoint).Normalize().Multiply(alignmentDistance)));
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.RollBack();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MoveWithOffset : Autodesk.Revit.UI.IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            try
            {
                
                using (Transaction transaction = new Transaction(doc, "Move Elements With Offset"))
                {
                    transaction.Start();
                    //Select Elements
                    IList<Reference> refs = Utils.GetCurrentSelection(uidoc, null, "Select Elements");
                    IList<ElementId> elementIds = new List<ElementId>();
                    foreach (Reference r in refs)
                    {
                        elementIds.Add(r.ElementId);
                    }

                    //Select Ref Point
                    Utils.SketchplaneByView(doc.ActiveView);
                    XYZ startPoint = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick Startpoint"));
                    XYZ endPoint = CenterElementUtils.Get2DPointInView(doc.ActiveView, selection.PickPoint("Pick Startpoint"));

                    if (CenterElementUtils.GetOffsetValue(out double offset))
                    {
                        double alignmentDistance = endPoint.DistanceTo(startPoint) - Utils.ToInternalUnits(doc, offset);
                        ElementTransformUtils.MoveElements(doc, elementIds, (endPoint.Subtract(startPoint).Normalize().Multiply(alignmentDistance)));
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.RollBack();
                    }
                }
            }
            catch (OperationCanceledException) { }
            
            return Result.Succeeded;
        }
    }

    public class CenterElementUtils
    {
        public static bool GetOffsetValue(out double offset)
        {
            adskWindows.RibbonTextBox textbox = FindUIElement("Modify", "AlignPlusDistanceTextbox") as adskWindows.RibbonTextBox;
            if (!double.TryParse(textbox.TextValue, out offset))
            {
                if (textbox.TextValue == null) 
                {
                    offset = 0;
                    textbox.TextValue = "0";
                    return true;
                }
                else
                {
                    Utils.Show("Offset Value must be a number");
                    return false;
                }
            }

            return true;
        }

        private static adskWindows.RibbonTab FindBeyondRevitTab()
        {
            adskWindows.RibbonTab result = null;
            adskWindows.RibbonTabCollection tabs = adskWindows.ComponentManager.Ribbon.Tabs;
            foreach (adskWindows.RibbonTab tab in tabs)
            {
                if (tab.Name == "Beyond Revit")
                {
                    result = tab;
                    break;
                }
            }
            return result;
        }
        private static adskWindows.RibbonItem FindUIElement(string panelName, string uiElementName)
        {
            string id = string.Format("CustomCtrl_%CustomCtrl_%Beyond Revit%{0}%{1}", panelName, uiElementName);
            adskWindows.RibbonItem result = null;
            adskWindows.RibbonTab beyondRevitRibbon = FindBeyondRevitTab();
            result = beyondRevitRibbon.FindItem(id, true);
            return result;
        }


        public static XYZ Get2DPointInView(View view, XYZ point)
        {
            //Project Points
            Plane viewPlane = Utils.PlaneByView(view);
            XYZ result = Get2DPointInPlane(viewPlane, point);
            return result;
        }

        public static XYZ Get2DPointInPlane(Plane plane, XYZ point)
        {
            //Project Points
            plane.Project(point, out UV pointUV, out double distanceUV);
            XYZ result = UVToXYZInPlane(plane, pointUV);
            return result;
        }

        

        public static XYZ UVToXYZInPlane(Plane plane, UV uv)
        {
            XYZ point = plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
            return point;
        }
        public static XYZ GetElementLocation(Element element)
        {
            XYZ elementLocation = null;

            LocationPoint location = element.Location as LocationPoint;
            if(elementLocation != null)
            {
                elementLocation = location.Point;
            }
            else
            {
                Document doc = element.Document;
                BoundingBoxXYZ bbox = element.get_BoundingBox(doc.ActiveView);
                elementLocation = (bbox.Max + bbox.Min) / 2;
            }
            return elementLocation;
        }



        public static List<double> CreateRange(double start, double end, int numberOfSteps)
        {
            //0, 1, 5 -> [0,0.25,0.5,0.75,1]
            double distance = end - start;
            double step = distance/ (numberOfSteps-1);
            List<double> result = new List<double>();
            for(int i = 0; i<=numberOfSteps; i ++)
            {
                result.Add(start);
                start += step;
            }
            return result;
        }
    }
}
