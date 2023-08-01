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
    public class AutoDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            IList<Reference> elementsToDimension = selection.PickObjects(ObjectType.Element, new AutodimensionFilter(), "Select elements to Dimension");
            ICollection<ElementId> detailLinesInView = new FilteredElementCollector(document, document.ActiveView.Id).OfCategoryId(document.Settings.Categories.get_Item("Lines").Id).WhereElementIsNotElementType().ToElementIds();
            ReferenceArray references = new ReferenceArray();
            if (elementsToDimension.Count > 1)
            {
                IList<ElementId> idsToIsolate = new List<ElementId>();
                foreach (Reference referenceElement in elementsToDimension)
                {
                    Element element = document.GetElement(referenceElement);
                    idsToIsolate.Add(element.Id);
                    Reference reference = DimensionUtils.ParseToStableReference(document, new Reference(element));
                    references.Append(reference);
                }
                foreach(ElementId id in detailLinesInView)
                {
                    idsToIsolate.Add(id);
                }
                using(Transaction trans = new Transaction(document, "Isolate Elements to Dimension"))
                {
                    trans.Start();
                    document.ActiveView.IsolateElementsTemporary(idsToIsolate);
                    trans.Commit();
                }
            }
            else
            {
                Utils.Show("Select at least 2 elements");
            }
            ElementId dimensionDetailLine = selection.PickObject(ObjectType.Element, new DetailLineFilter(), "Select Detail Line where the Dimension should go").ElementId;


            using (Transaction trans = new Transaction(document, "Auto Dimension"))
            {
                trans.Start();
                DetailLine detailLine = (DetailLine)document.GetElement(dimensionDetailLine);
                Line dimensionLine = Line.CreateBound(detailLine.GeometryCurve.GetEndPoint(0), detailLine.GeometryCurve.GetEndPoint(1));
                document.Create.NewDimension(document.ActiveView, dimensionLine, references);
                document.ActiveView.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                trans.Commit();
            }

            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DuplicateDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            StartDuplication:

            using (Transaction trans = new Transaction(document, "Duplicate Dimension"))
            {
                trans.Start();
                try
                {
                    Reference dimensionReference = uidoc.Selection.PickObject(ObjectType.Element, new LinearDimensionFilter(), "Select Dimension to Duplicate");
                    DimensionUtils.SketchplaneByView(document.ActiveView);
                    XYZ point = uidoc.Selection.PickPoint(ObjectSnapTypes.None, "Select the Direction where the new Dimension should be placed");

                    Dimension sourceDimension = (Dimension)document.GetElement(dimensionReference);
                    DimensionUtils.CopyDimension(sourceDimension, point);
                }
                catch(Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }

                

                trans.Commit();
            }
            goto StartDuplication;

        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BringDimensionToFront : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            StartSelection:
            using (Transaction trans = new Transaction(document, "Bring Dimension to the Front"))
            {
                trans.Start();
                try
                {
                    Reference dimensionReference = uidoc.Selection.PickObject(ObjectType.Element, new LinearDimensionFilter(), "Select Dimension to Duplicate");
                    DimensionUtils.SketchplaneByView(document.ActiveView);

                    Dimension sourceDimension = (Dimension)document.GetElement(dimensionReference);
                    DimensionUtils.CopyDimension(sourceDimension);
                    document.Delete(sourceDimension.Id);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                trans.Commit();
            }
            goto StartSelection;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateTotalDimension : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            SelectDimensionSegment:
            using (Transaction trans = new Transaction(document, "Create Total Dimension Dimension"))
            {
                trans.Start();
                try
                {
                    Reference dimensionReference = uidoc.Selection.PickObject(ObjectType.Element, new MultiSegmentDimensionFilter(), "Select Dimension to be totaled");
                    DimensionUtils.SketchplaneByView(document.ActiveView);
                    XYZ point = uidoc.Selection.PickPoint(ObjectSnapTypes.None, "Select the Direction where the total Dimension should be placed");

                    Dimension sourceDimension = (Dimension)document.GetElement(dimensionReference);
                    int size = sourceDimension.References.Size;
                    ReferenceArray references = new ReferenceArray();
                    references.Append(DimensionUtils.ParseToStableReference(document, sourceDimension.References.get_Item(0)));
                    references.Append(DimensionUtils.ParseToStableReference(document, sourceDimension.References.get_Item(sourceDimension.NumberOfSegments)));
                    Line newDimensionCurve = DimensionUtils.CreateOffsetCurve(sourceDimension, point);
                    Dimension newDimension = document.Create.NewDimension(sourceDimension.View, newDimensionCurve, references);
                    newDimension.DimensionType = sourceDimension.DimensionType;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                trans.Commit();
            }
            goto SelectDimensionSegment;


        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RemoveDimensionSegment : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;

            Reference othersegment = selection.PickObject(ObjectType.Element, new MultiSegmentDimensionFilter(), "Select The Dimension line where you want to delete a dimension segment.");
            Dimension dimension = (Dimension)document.GetElement(othersegment);
            SelectDimensionSegment:
            if(dimension.NumberOfSegments >1) 
            {
                using (Transaction trans = new Transaction(document, "Remove Dimension Segment"))
                {
                    trans.Start();
                    DimensionUtils.SketchplaneByView(document.ActiveView);
                    try
                    {
                        XYZ point = selection.PickPoint(ObjectSnapTypes.None, "Select Which Dimension Segment should be deleted. Press Escape to end this command");
                        DimensionUtils.SketchplaneByView(document.ActiveView);

                        dynamic deletedSegmentIndex = DimensionUtils.GetReferenceIndexByPoint(dimension, point);
                        dimension = DeleteDimensionSegment(dimension, deletedSegmentIndex);

                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        return Result.Succeeded;
                    }
                    trans.Commit();
                }

                goto SelectDimensionSegment;
            }


            return Result.Succeeded;
        }

        public static Dimension DeleteDimensionSegment(Dimension dimension, int indexOfSegment)
        {
            ReferenceArray references = new ReferenceArray();
            for (int i = 0; i < dimension.References.Size; i++)
            {
                if (i != indexOfSegment)
                {
                    references.Append(DimensionUtils.ParseToStableReference(dimension.Document,dimension.References.get_Item(i)));
                }
            }
            Line dimensionLine = dimension.Curve as Line;
            Document doc = dimension.Document;
            DimensionType dimensionType = doc.GetElement(dimension.GetTypeId()) as DimensionType;
            Dimension newDim = doc.Create.NewDimension(doc.ActiveView, dimensionLine, references, dimensionType);
            doc.Delete(dimension.Id);
            return newDim;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SplitDimensionLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;

            SelectSplitDimension:

            using (Transaction trans = new Transaction(document, "Split Dimension"))
            {
                trans.Start();
                try
                {
                    Reference othersegment = selection.PickObject(ObjectType.Element, new MultiSegmentDimensionFilter(), "Select The Dimension line which you want to split.");
                    Dimension dimension = (Dimension)document.GetElement(othersegment);

                    DimensionUtils.SketchplaneByView(document.ActiveView);
                    XYZ point = selection.PickPoint(ObjectSnapTypes.None, "Select Where you want to split.");

                    dynamic splitSegmentIndex = DimensionUtils.GetReferenceIndexByPoint(dimension, point);
                    List<Dimension> newDimensions = SplitDimension(dimension, splitSegmentIndex);


                    Dimension firstDim = newDimensions[0];
                    Dimension secondDim = newDimensions[1];
                    int index = 0;
                    int switchIndex = 0;
                    if (firstDim.NumberOfSegments > 0)
                    {

                        switchIndex = firstDim.NumberOfSegments - 1;
                    }
                    for (int i = 0; i <= dimension.NumberOfSegments - 1; i++)
                    {
                        DimensionSegment sourceSegment = dimension.Segments.get_Item(i);
                        if (i < switchIndex)
                        {
                            dynamic target = firstDim;
                            if (firstDim.NumberOfSegments > 1)
                            {
                                target = firstDim.Segments.get_Item(index);
                            }
                            DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceSegment, target, true);
                        }
                        else if (i == switchIndex)
                        {
                            dynamic target = firstDim;
                            if (firstDim.NumberOfSegments > 1)
                            {
                                target = firstDim.Segments.get_Item(index);
                            }
                            DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceSegment, target, true);
                            index = -1;
                        }
                        else
                        {
                            dynamic target = secondDim;
                            if (secondDim.NumberOfSegments > 1)
                            {
                                target = secondDim.Segments.get_Item(index);
                            }
                            DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceSegment, target, true);
                        }
                        index++;
                    }
                    document.Delete(dimension.Id);

                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }

                trans.Commit();
            }
            goto SelectSplitDimension;


        }

        public static List<Dimension> SplitDimension(Dimension dimension, int indexOfSegment)
        {
            ReferenceArray firstReferences = new ReferenceArray();
            ReferenceArray secondReferences = new ReferenceArray();

            for (int i = 0; i < dimension.References.Size; i++)
            {
                if (i < indexOfSegment)
                {
                    Reference reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    firstReferences.Append(reference);
                }
                else if(i== indexOfSegment)
                {
                    Reference reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    firstReferences.Append(reference);
                    reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    secondReferences.Append(reference);
                }
                else
                {
                    Reference reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    secondReferences.Append(reference);
                }
            }

            Line dimensionLine = dimension.Curve as Line;
            Document doc = dimension.Document;
            DimensionType dimensionType = doc.GetElement(dimension.GetTypeId()) as DimensionType;
            Dimension firstNewDim = doc.Create.NewDimension(doc.ActiveView, dimensionLine, firstReferences);
            Dimension secondNewDim = doc.Create.NewDimension(doc.ActiveView, dimensionLine, secondReferences);
            return new List<Dimension>() { firstNewDim, secondNewDim };
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MergeDimensions : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;

            SelectMergeDimension:
            using (Transaction trans = new Transaction(document, "Merge Dimension"))
            {
                try
                {
                    Reference firstDimensionReference = selection.PickObject(ObjectType.Element, new DimensionFilter(), "Select The First Dimension");
                    Dimension firstDimension = (Dimension)document.GetElement(firstDimensionReference);

                    ParallelDimensionFilter parallelDimensionFilter = new ParallelDimensionFilter();
                    parallelDimensionFilter.SourceDimension = firstDimension;
                    Reference secondDimensionReference = selection.PickObject(ObjectType.Element, new DimensionFilter(), "Select The Second Dimension");
                    Dimension secondDimension = (Dimension)document.GetElement(secondDimensionReference);

                    trans.Start();
                    ReferenceArray references = new ReferenceArray();
                    List<string> referenceRepresentations = new List<string>();
                    foreach (Reference reference in firstDimension.References)
                    {
                        string represent = reference.ConvertToStableRepresentation(document);
                        if (!referenceRepresentations.Contains(represent))
                        {
                            references.Append(reference);
                            referenceRepresentations.Add(represent);

                        }
                    }
                    foreach (Reference reference in secondDimension.References)
                    {
                        string represent = reference.ConvertToStableRepresentation(document);
                        if (!referenceRepresentations.Contains(represent))
                        {
                            references.Append(reference);
                            referenceRepresentations.Add(represent);
                        }
                    }
                    Dimension mergeDimension = document.Create.NewDimension(document.ActiveView, (Line)firstDimension.Curve, references);
                    document.Delete(firstDimension.Id);
                    document.Delete(secondDimension.Id);

                    trans.Commit();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }

            }
            goto SelectMergeDimension;


        }

        public static List<Dimension> SplitDimension(Dimension dimension, int indexOfSegment)
        {
            ReferenceArray firstReferences = new ReferenceArray();
            ReferenceArray secondReferences = new ReferenceArray();

            for (int i = 0; i < dimension.References.Size; i++)
            {
                if (i < indexOfSegment)
                {
                    Reference reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    firstReferences.Append(reference);
                }
                else if (i == indexOfSegment)
                {
                    Reference reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    firstReferences.Append(reference);
                    reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    secondReferences.Append(reference);
                }
                else
                {
                    Reference reference = DimensionUtils.ParseToStableReference(dimension.Document, dimension.References.get_Item(i));
                    secondReferences.Append(reference);
                }
            }

            Line dimensionLine = dimension.Curve as Line;
            Document doc = dimension.Document;
            DimensionType dimensionType = doc.GetElement(dimension.GetTypeId()) as DimensionType;
            Dimension firstNewDim = doc.Create.NewDimension(doc.ActiveView, dimensionLine, firstReferences);
            Dimension secondNewDim = doc.Create.NewDimension(doc.ActiveView, dimensionLine, secondReferences);
            return new List<Dimension>() { firstNewDim, secondNewDim };
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyDimensionsOverrides : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;

            using (Transaction trans = new Transaction(document, "Copy Dimension overrides"))
            {
                trans.Start();
                Reference sourceReference = selection.PickObject(ObjectType.Element, new SingleDimensionSegmentFilter(), "Select Source Dimensions");
                IList<Reference> targetReferences = selection.PickObjects(ObjectType.Element, new SingleDimensionSegmentFilter(), "Select Target Dimensions");
                Dimension sourceDimension = (Dimension)document.GetElement(sourceReference);
                foreach(Reference targetRef in targetReferences)
                {
                    Dimension targetDimension = (Dimension)document.GetElement(targetRef);
                    DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceDimension, targetDimension);
                }
                trans.Commit();
            }


            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MoveDimensionEnds : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;

            using (Transaction trans = new Transaction(document, "Move Dimension Ends"))
            {
                trans.Start();
                IList<Reference> references = selection.PickObjects(ObjectType.Element, new MultiSegmentDimensionFilter(), "Select Source Dimensions");
                foreach(Reference reference in references)
                {
                    Dimension dim = document.GetElement(reference) as Dimension;
                    EvaluateDimensionEnds(dim);
                }
                trans.Commit();
            }


            return Result.Succeeded;
        }

        private static void EvaluateDimensionEnds(Dimension dim)
        {
            View view = dim.View;
            int minValue = view.Scale * 6;
            Curve dimensionCurve = dim.Curve;
            dimensionCurve.MakeBound(0, 1);
            XYZ startRot = dimensionCurve.GetEndPoint(0);
            XYZ endRot = dimensionCurve.GetEndPoint(1);
            XYZ OffsetVector = (endRot - startRot).Normalize();

            DimensionSegment firstSegment = dim.Segments.get_Item(0);
            DimensionSegment lastSegment = dim.Segments.get_Item(dim.NumberOfSegments - 1);
            if (double.Parse(firstSegment.ValueString) <= minValue)
            {
                XYZ point = firstSegment.LeaderEndPosition;
                double translationValue = firstSegment.ValueString.Length * (view.Scale * 2);
                XYZ translation = OffsetVector * Utils.ToInternalUnits(dim.Document, -translationValue);
                XYZ newPoint = point.Add(translation);
                firstSegment.LeaderEndPosition = newPoint;
            }

            if (double.Parse(lastSegment.ValueString) <= minValue)
            {
                XYZ point = lastSegment.LeaderEndPosition;
                double translationValue = lastSegment.ValueString.Length * (view.Scale * 2);
                XYZ translation = OffsetVector * Utils.ToInternalUnits(dim.Document, translationValue);
                XYZ newPoint = point.Add(translation);
                lastSegment.LeaderEndPosition = newPoint;
            }

        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyDimensionOverrides : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            List<dynamic> dimensions = new List<dynamic>();
            MultiSegmentDimensionFilter multiFilter = new MultiSegmentDimensionFilter();
            SingleDimensionSegmentFilter singleFilter = new SingleDimensionSegmentFilter();
            IList<ElementId> dimensionIds = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(Dimension)).ToElementIds().ToList();
            foreach(ElementId id in dimensionIds)
            {
                Element element = doc.GetElement(id);
                if (multiFilter.AllowElement(element))
                {
                    Dimension dim = (Dimension)element;
                    foreach (DimensionSegment s in dim.Segments)
                    {
                        dimensions.Add(s);
                    }
                }
                else if (singleFilter.AllowElement(element))
                {
                    dimensions.Add((Dimension)doc.GetElement(id));
                }
            }

            Selection selection = uidoc.Selection;
            DimensionUtils.SketchplaneByView(doc.ActiveView);
            dynamic sourceDimension = GetDimensionSegmentByPoint(selection.PickPoint("Select a point close to the Source Dimension segment"), dimensions);
            SelectTargetDimension:
            using (Transaction trans = new Transaction(doc, "Copy Dimension Overrides"))
            {
                trans.Start();
                try
                {
                    dynamic targetDimension = GetDimensionSegmentByPoint(selection.PickPoint("Select a point close to the Target Dimension segment"), dimensions);
                    DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceDimension, targetDimension);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                trans.Commit();
            }
            goto SelectTargetDimension;



        }

        public static dynamic GetDimensionSegmentByPoint(XYZ pt, List<dynamic> dims)
        {
            dynamic result = null;
            double closestDistance = double.NaN;
            foreach (dynamic dim in dims)
            {
                double distance = pt.DistanceTo(dim.LeaderEndPosition);
                if (double.IsNaN(closestDistance) || closestDistance>distance)
                {
                    closestDistance = distance;
                    result = dim;
                }
            }
            return result;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignFirstDimensionbyDistance : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            SelectDimensionSegment:
            using (Transaction trans = new Transaction(document, "Align First Dimension by Distance"))
            {
                trans.Start();
                try
                {
                    Reference dimensionReference = uidoc.Selection.PickObject(ObjectType.Element, new LinearDimensionFilter(), "Select Dimension to be aligned. Press Escape to Cancel");
                    DimensionUtils.SketchplaneByView(document.ActiveView);
                    XYZ point = uidoc.Selection.PickPoint(ObjectSnapTypes.Endpoints, "Select to point to align to");
                    Dimension sourceDimension = (Dimension)document.GetElement(dimensionReference);
                    Curve dimensionCurve = sourceDimension.Curve;
                    XYZ evaluationPoint = dimensionCurve.Project(point).XYZPoint;
                    double distance = Utils.FromInternalUnits(document, evaluationPoint.DistanceTo(point));
                    double ruledistance = document.ActiveView.Scale * 25;
                    double offsetDistance = ruledistance - distance;
                    Line newCurve = (Line)DimensionUtils.CreateOffsetCurve(sourceDimension, point, offsetDistance, false);
                    Dimension newDimension = document.Create.NewDimension(document.ActiveView, newCurve, sourceDimension.References);
                    if (sourceDimension.NumberOfSegments > 1)
                    {
                        for(int i =0; i<sourceDimension.NumberOfSegments; i++)
                        {
                            DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceDimension.Segments.get_Item(i), newDimension.Segments.get_Item(i));
                        }
                    }
                    else
                    {
                        DimensionUtils.CopyDimensionOverridesBetweenSegments(sourceDimension, newDimension);
                    }
                    document.Delete(sourceDimension.Id);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                trans.Commit();
            }
            goto SelectDimensionSegment;


        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IsolateDimensionHosts : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            using (Transaction trans = new Transaction(document, "Isolate Dimension Hosts"))
            {
                trans.Start();
                try
                {
                    IList<Reference> dimensions = uidoc.Selection.PickObjects(ObjectType.Element, new DimensionFilter(), "Select Dimensions to Isolate. Press Escape to Cancel");
                    List<ElementId> elementsToIsolate = new List<ElementId>();
                    foreach(Reference r in dimensions)
                    {
                        Dimension dim = (Dimension)document.GetElement(r);
                        elementsToIsolate.AddRange(DimensionUtils.GetHostElementIds(dim));

                    }
                    document.ActiveView.IsolateElementsTemporary(elementsToIsolate);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                trans.Commit();
            }

            return Result.Succeeded;

        }
    }

    internal sealed class DimensionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            Type type = elem.GetType();

            if (type == typeof(Dimension))
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }

    internal sealed class ParallelDimensionFilter : ISelectionFilter
    {
        public Dimension SourceDimension { get; set; }
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            Type type = elem.GetType();

            if (type == typeof(Dimension) && elem.Id != this.SourceDimension.Id)
            {
                Dimension dimension = (Dimension)elem;
                if (IsParallelDimension(dimension))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }

        public bool IsParallelDimension(Dimension dimension)
        {
            XYZ sourceDirection = this.SourceDimension.Curve.Evaluate(0.5, true);
            XYZ targetDirection = dimension.Curve.Evaluate(0.5, true);
            double angle = sourceDirection.AngleTo(targetDirection);
            Utils.Show(angle.ToString());
            if(angle == 0 | angle == 180)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    internal sealed class SingleDimensionSegmentFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            Type type = elem.GetType();

            if (type == typeof(Dimension))
            {
                Dimension dim = (Dimension)elem;
                if (dim.NumberOfSegments <1)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class MultiSegmentDimensionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            Type type = elem.GetType();

            if (type == typeof(Dimension))
            {
                Dimension dim = (Dimension)elem;
                if (dim.NumberOfSegments > 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class DetailLineFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            Type type = elem.GetType();

            if (type == typeof(DetailLine))
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class AutodimensionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;
            List<Type> allowedTypes = new List<Type>()
            {
                typeof(DetailCurve),
                typeof(DetailArc),
                typeof(DetailLine),
                typeof(ReferencePlane),
                typeof(ReferencePoint),
                typeof(Grid),
                typeof(ModelArc),
                typeof(ModelCurve),
                typeof(ModelLine)
            };

            Type type = elem.GetType();

            if (allowedTypes.Contains(type))
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class LinearDimensionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            if (elem is null) return false;

            Type type = elem.GetType();

            if (type == typeof(Dimension))
            {
                Dimension dim = (Dimension)elem;
                if (dim.DimensionShape == DimensionShape.Linear)
                {
                    return true;
                }
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            throw new NotImplementedException();
        }
    }
    public class DimensionUtils
    {
        public static void SketchplaneByView(View view)
        {
            Document document = view.Document;
            
            if(view.ViewType != ViewType.DraftingView)
            {
                if (!document.IsModifiable)
                {
                    using (Transaction transaction = new Transaction(document, "Set Sketchplane"))
                    {
                        transaction.Start();
                        using (SubTransaction t = new SubTransaction(view.Document))
                        {
                            t.Start();
                            Plane plane = Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
                            SketchPlane sp = SketchPlane.Create(view.Document, plane);
                            view.SketchPlane = sp;
                            t.Commit();
                        }
                        transaction.Commit();
                    }
                }
                else
                {
                    using (SubTransaction t = new SubTransaction(view.Document))
                    {
                        t.Start();
                        Plane plane = Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
                        SketchPlane sp = SketchPlane.Create(view.Document, plane);
                        view.SketchPlane = sp;
                        t.Commit();
                    }
                }
            }
        }

        public static XYZ Pick3DPoint(UIDocument uidoc, string PickPointPrompt, bool ShowWorkplane = false)
        {
            XYZ selectedPoint = null;
            Document doc = uidoc.Document;
            Reference face = uidoc.Selection.PickObject(ObjectType.Face, "Select Face to Work on");
            Element e = doc.GetElement(face.ElementId);

            if (null != e)
            {
                PlanarFace PFace
                  = e.GetGeometryObjectFromReference(face)
                    as PlanarFace;

                if (PFace != null)
                {
                    Plane plane = Plane.CreateByNormalAndOrigin(PFace.FaceNormal, PFace.Origin);

                    Transaction t = new Transaction(doc);

                    t.Start("Temporarily set work plane"
                      + " to pick point in 3D");

                    SketchPlane sp = SketchPlane.Create(doc, plane);

                    uidoc.ActiveView.SketchPlane = sp;
                    if (ShowWorkplane)
                    {
                        uidoc.ActiveView.ShowActiveWorkPlane();
                    }

                    try
                    {
                        selectedPoint = uidoc.Selection.PickPoint(ObjectSnapTypes.Endpoints|
                            ObjectSnapTypes.Centers |
                            ObjectSnapTypes.Midpoints ,
                            PickPointPrompt);
                    }
                    catch (OperationCanceledException)
                    {
                    }

                    t.RollBack();
                }
            }
            return selectedPoint;
            
        }
        public static int GetReferenceIndexByPoint(Dimension dimension, XYZ point)
        {
            List<XYZ> refPoints = GetDimensionPoints(dimension);
            dynamic index = null;
            dynamic distance = null;
            for (int i = 0; i < refPoints.Count(); i++)
            {
                double length = point.DistanceTo(refPoints[i]);
                if (distance == null || distance > length)
                {
                    distance = length;
                    index = i;
                }
            }
            return index;
        }

        /// <summary>
        /// Retrieve the start and end points of
        /// each dimension segment, based on the 
        /// dimension origin determined above.
        /// </summary>
        public static List<XYZ> GetDimensionPoints(Dimension dim)
        {
            XYZ pStart = GetDimensionStartPoint(dim);
            Line dimLine = dim.Curve as Line;
            List<XYZ> pts = new List<XYZ>();

            dimLine.MakeBound(0, 1);
            XYZ pt1 = dimLine.GetEndPoint(0);
            XYZ pt2 = dimLine.GetEndPoint(1);
            XYZ direction = pt2.Subtract(pt1).Normalize();

            if (0 == dim.Segments.Size)
            {
                XYZ v = 0.5 * (double)dim.Value * direction;
                pts.Add(pStart - v);
                pts.Add(pStart + v);
            }
            else
            {
                XYZ p = pStart;
                foreach (DimensionSegment seg in dim.Segments)
                {
                    XYZ v = (double)seg.Value * direction;
                    if (0 == pts.Count)
                    {
                        pts.Add(p = (pStart - 0.5 * v));
                    }
                    pts.Add(p = p.Add(v));
                }
            }
            return pts;
        }

        /// <summary>
        /// Return dimension origin, i.e., the midpoint
        /// of the dimension or of its first segment.
        /// </summary>
        public static XYZ GetDimensionStartPoint(Dimension dim)
        {
            XYZ p = null;

            try
            {
                p = dim.Origin;
            }
            catch (Autodesk.Revit.Exceptions.ApplicationException)
            {
                foreach (DimensionSegment seg in dim.Segments)
                {
                    p = seg.Origin;
                    break;
                }
            }
            return p;
        }

        public static void CopyDimensionOverridesBetweenSegments(dynamic source, dynamic target, bool includeLeaders = false)
        {
            target.Above = source.Above;
            target.Below = source.Below;
            target.Prefix = source.Prefix;
            target.Suffix = source.Suffix;
            target.ValueOverride = source.ValueOverride;

            if (includeLeaders)
            {
                target.LeaderEndPosition = source.LeaderEndPosition;
                target.TextPosition = source.TextPosition;
            }
        }

        public static Reference ParseToStableReference(Document doc, Reference reference)
        {
            Element element = doc.GetElement(reference);
            if (element.GetType() == typeof(Grid) || element.GetType() == typeof(ReferencePlane))
            {
                reference = new Reference(element);
            }
            return reference;
        }

        public static Line CreateOffsetCurve(Dimension dimension, XYZ offsetReference, double OffsetDistance = 0, bool closest = true)
        {
            View view = dimension.View;
            Line dimensionLine = (Line)dimension.Curve;
            double offset = 0;
            if (OffsetDistance == 0)
            {
                offset = Utils.ToInternalUnits(dimension.Document, dimension.View.Scale * Utils.FromInternalUnits(dimension.Document, dimension.DimensionType.get_Parameter(BuiltInParameter.DIM_STYLE_DIM_LINE_SNAP_DIST).AsDouble()));
            }
            else
            {
                offset = Utils.ToInternalUnits(dimension.Document, OffsetDistance);
            }
            XYZ offsetDirection = GenerateOffsetVector(dimensionLine, view, offsetReference, closest);
            Transform translation = Transform.CreateTranslation(offsetDirection * offset);
            Line line = (Line)dimensionLine.CreateTransformed(translation);
            return line;
        }

        public static XYZ GenerateOffsetVector(Line line, View view, XYZ reference, bool towardsReference = true)
        {
            XYZ Result = null;
            XYZ rotationAxis = view.ViewDirection;
            Transform transform = Transform.CreateRotation(rotationAxis, ((Math.PI / 180) * 90));
            Line newDirectionLine = (Line)line.CreateTransformed(transform);
            newDirectionLine.MakeBound(0, 1);

            XYZ startRot = newDirectionLine.GetEndPoint(0);
            XYZ endRot = newDirectionLine.GetEndPoint(1);
            XYZ OffsetVector = (endRot - startRot).Normalize();

            Transform translation = Transform.CreateTranslation(OffsetVector * 100);
            Transform otherTranslation = Transform.CreateTranslation(OffsetVector * -100);
            Line positive = (Line)line.CreateTransformed(translation);
            Line negative = (Line)line.CreateTransformed(otherTranslation);
            double distancePositive = reference.DistanceTo(positive.Origin);
            double distanceNegative = reference.DistanceTo(negative.Origin);
            if (towardsReference)
            {
                if (distancePositive > distanceNegative)
                {
                    Result = OffsetVector * -1;
                }
                else
                {
                    Result = OffsetVector;
                }
            }
            else
            {

                if (distancePositive > distanceNegative)
                {
                    Result = OffsetVector;
                }
                else
                {
                    Result = OffsetVector*1;
                }
            }



            return Result;

        }

        public static List<ElementId> GetHostElementIds(Dimension dimension)
        {
            List<ElementId> elements = new List<ElementId>();
            elements.Add(dimension.Id);
            foreach(Reference r in dimension.References)
            {
                Element host = dimension.Document.GetElement(r);
                elements.Add(host.Id);
            }
            return elements;
        }

        public static Dimension CopyDimension(Dimension sourceDimension, XYZ offsetPoint = null)
        {
            Document document = sourceDimension.Document;
            ReferenceArray references = new ReferenceArray();
            foreach (Reference sourceReference in sourceDimension.References)
            {
                references.Append(DimensionUtils.ParseToStableReference(document, sourceReference));
            }

            Line newDimensionCurve = sourceDimension.Curve as Line;
            if (offsetPoint != null)
            {
                newDimensionCurve = DimensionUtils.CreateOffsetCurve(sourceDimension, offsetPoint);
            } 
            Dimension newDimension = document.Create.NewDimension(sourceDimension.View, newDimensionCurve, references);
            newDimension.DimensionType = sourceDimension.DimensionType;

            for (int i = 0; i < sourceDimension.NumberOfSegments; i++)
            {
                DimensionSegment source = sourceDimension.Segments.get_Item(i);
                DimensionSegment target = newDimension.Segments.get_Item(i);
                DimensionUtils.CopyDimensionOverridesBetweenSegments(source, target);
            }
            return newDimension;
        }
    }
    
}

