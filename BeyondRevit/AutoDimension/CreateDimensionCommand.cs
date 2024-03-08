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
    public class CreateDimensionCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            Utils.SketchplaneByView(activeView);
            Reference elementToDimension = selection.PickObject(ObjectType.Element);
            XYZ startPoint = selection.PickPoint();
            XYZ endPoint = selection.PickPoint();
            using (Transaction transaction = new Transaction(doc, "Create Dimension"))
            {
                transaction.Start();
                View3D view3D = View3D.CreateIsometric(doc, AutoDimUtils.View3DFamilyType(doc).Id);
                view3D.IsolateElementsTemporary(new List<ElementId>() { elementToDimension.ElementId });
                ReferenceIntersector intersector = new ReferenceIntersector(view3D);
                XYZ direction = startPoint.Add(endPoint);
                IList<ReferenceWithContext> results = intersector.Find(startPoint, direction);
                Reference startReference = AutoDimUtils.FindNearestReference(intersector, startPoint, activeView);
                Reference endReference = AutoDimUtils.FindNearestReference(intersector, endPoint, activeView);
                Line line = Line.CreateBound(startPoint, endPoint);
                ReferenceArray refs = new ReferenceArray();
                refs.Append(startReference);
                refs.Append(endReference);
                doc.Create.NewDimension(activeView, line, refs);
                doc.Delete(view3D.Id);
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateMultiDimensionCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            Utils.SketchplaneByView(activeView);

            LinePatternElement elementType = new FilteredElementCollector(doc).OfClass(typeof(LinePatternElement)).WhereElementIsNotElementType().First() as LinePatternElement;
            
            //Reference elementToDimension = selection.PickObject(ObjectType.Element);
            //XYZ startPoint = selection.PickPoint();
            //XYZ endPoint = selection.PickPoint();
            //using (Transaction transaction = new Transaction(doc, "Create Dimension"))
            //{
            //    transaction.Start();
            //    View3D view3D = View3D.CreateIsometric(doc, AutoDimUtils.View3DFamilyType(doc).Id);
            //    view3D.IsolateElementsTemporary(new List<ElementId>() { elementToDimension.ElementId });
            //    ReferenceIntersector intersector = new ReferenceIntersector(view3D);
            //    XYZ direction = startPoint.Add(endPoint);
            //    IList<ReferenceWithContext> results = intersector.Find(startPoint, direction);
            //    Reference startReference = AutoDimUtils.FindNearestReference(intersector, startPoint, activeView);
            //    Reference endReference = AutoDimUtils.FindNearestReference(intersector, endPoint, activeView);
            //    Line line = Line.CreateBound(startPoint, endPoint);
            //    ReferenceArray refs = new ReferenceArray();
            //    refs.Append(startReference);
            //    refs.Append(endReference);
            //    doc.Create.NewDimension(activeView, line, refs);
            //    doc.Delete(view3D.Id);
            //    transaction.Commit();
            //}
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DrawRaysCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            using (Transaction transaction = new Transaction(doc, "Create Dimension"))
            {
                transaction.Start();

                Utils.SketchplaneByView(activeView);
                XYZ startPoint = selection.PickPoint();
                View3D view3D = View3D.CreateIsometric(doc, AutoDimUtils.View3DFamilyType(doc).Id);
                AutoDimUtils.DrawRays(doc, startPoint, activeView);
                doc.Delete(view3D.Id);
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    public class AutoDimUtils
    {
        public static ViewFamilyType View3DFamilyType(Document document)
        {
            ViewFamilyType result = null;
            IList<Element> elements = new FilteredElementCollector(document).OfClass(typeof(ViewFamilyType)).ToElements();
            foreach (ViewFamilyType vft in elements)
            {
                if (vft.ViewFamily == ViewFamily.ThreeDimensional)
                {
                    result = vft;
                    break;
                }
            }
            return result;
        }

        public static Reference FindNearestReference(ReferenceIntersector intersector, XYZ point, View view)
        {
            XYZ up = view.UpDirection.Multiply(10000);
            XYZ down = -view.UpDirection.Multiply(10000);
            XYZ right = view.RightDirection.Multiply(10000);
            XYZ left = -view.RightDirection.Multiply(10000);
            XYZ front = view.ViewDirection.Multiply(10000);
            XYZ back = -view.ViewDirection.Multiply(10000);
            ReferenceWithContext upRef = intersector.FindNearest(point, up);
            ReferenceWithContext downRef = intersector.FindNearest(point, down);
            ReferenceWithContext rightRef = intersector.FindNearest(point, right);
            ReferenceWithContext leftRef = intersector.FindNearest(point, left);
            ReferenceWithContext frontRef = intersector.FindNearest(point, front);
            ReferenceWithContext backRef = intersector.FindNearest(point, back);
            Dictionary<double, ReferenceWithContext> references = new Dictionary<double, ReferenceWithContext>();
            if (upRef != null)
            {
                references.Add(upRef.Proximity, upRef);
            }
            if (downRef != null)
            {
                references.Add(downRef.Proximity, downRef);
            }
            if (rightRef != null)
            {
                references.Add(rightRef.Proximity, rightRef);
            }
            if (leftRef != null)
            {
                references.Add(leftRef.Proximity, leftRef);
            }
            if (frontRef != null)
            {
                references.Add(frontRef.Proximity, frontRef);
            }
            if (backRef != null)
            {
                references.Add(backRef.Proximity, backRef);
            }
            List<double> proximities = references.Keys.ToList<double>();

            proximities.Sort();
            ReferenceWithContext reference = references[proximities[0]];
            return reference.GetReference();
        }

        public static void DrawRays(Document doc, XYZ point, View view)
        {
            XYZ up = view.UpDirection.Multiply(Utils.ToInternalUnits(doc, 1000));
            XYZ down = -view.UpDirection.Multiply(Utils.ToInternalUnits(doc, 1000));
            XYZ right = view.RightDirection.Multiply(Utils.ToInternalUnits(doc, 1000));
            XYZ left = -view.RightDirection.Multiply(Utils.ToInternalUnits(doc, 1000));
            XYZ front = view.ViewDirection.Multiply(10000);
            XYZ back = -view.ViewDirection.Multiply(10000);
            SketchPlane plane = view.SketchPlane;
            Line rightRay = Line.CreateBound(point, point.Add(right));
            Line leftRay = Line.CreateBound(point, point.Add(left));
            Line upRay = Line.CreateBound(point, point.Add(up));
            Line downRay = Line.CreateBound(point, point.Add(down));
            doc.Create.NewModelCurve(rightRay, plane);
            doc.Create.NewModelCurve(leftRay, plane);
            doc.Create.NewModelCurve(upRay, plane);
            doc.Create.NewModelCurve(downRay, plane);
        }

    }
}