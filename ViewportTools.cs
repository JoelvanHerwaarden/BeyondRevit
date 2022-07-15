using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.ViewportCommands
{
    public class ViewportToolsUtils
    {
        internal sealed class ViewportFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(Viewport)) return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

        }

        public static Dictionary<string, double> GetVerticalBounds(IList<XYZ> points)
        {
            var verticalBounds = new Dictionary<string, double>();
            List<double> values = new List<double>();
            foreach(XYZ p in points)
            {
                values.Add(p.Y);
            }
            values.Sort();
            verticalBounds.Add("Min", values.First());
            verticalBounds.Add("Max", values.Last());
            return verticalBounds;
        }
        public static Dictionary<string, double> GetHorizontalBounds(IList<XYZ> points)
        {
            var verticalBounds = new Dictionary<string, double>();
            List<double> values = new List<double>();
            foreach (XYZ p in points)
            {
                values.Add(p.X);
            }
            values.Sort();
            verticalBounds.Add("Min", values.First());
            verticalBounds.Add("Max", values.Last());
            return verticalBounds;
        }
        public static IList<Viewport> SelectViewports(Document document, Selection selection)
        {
            IList<Reference> viewportRefs = selection.PickObjects(ObjectType.Element, new ViewportFilter(), "Select Viewports To Align");
            IList<Viewport> viewportList = new List<Viewport>();
            foreach (Reference viewportRef in viewportRefs)
            {
                viewportList.Add(document.GetElement(viewportRef) as Viewport);
            }
            return viewportList;
        }
        public static IList<XYZ> GetViewportPoints(IList<Viewport> viewports)
        {
            IList<XYZ> points = new List<XYZ>();
            foreach(Viewport vp in viewports)
            {
                points.Add(vp.GetBoxCenter());
            }
            return points;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenViewportView : IExternalCommand
    {
        internal sealed class ViewportFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(Viewport)) return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            ViewportFilter filter = new ViewportFilter();
            //IList<Reference> references = selection.PickObjects(ObjectType.Element, filter, "Select Viewport");
            IList<Reference> references = Utils.GetCurrentSelection(uidoc, filter, "Select Viewport");
            foreach (Reference reference in references)
            {
                Viewport viewport = (Viewport)document.GetElement(reference);
                View view = (View)document.GetElement(viewport.ViewId);
                uidoc.ActiveView = view;
            }
            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HideCropRegion : IExternalCommand
    {
        internal sealed class ViewportFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(Viewport)) return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            ViewportFilter filter = new ViewportFilter();
            IList<Reference> references = selection.PickObjects(ObjectType.Element, filter, "Select Viewports");
            using (Transaction t = new Transaction(document, "Hide Crop Region"))
            {
                t.Start();
                foreach (Reference reference in references)
                {
                    Viewport viewport = (Viewport)document.GetElement(reference.ElementId);
                    View view = (View)document.GetElement(viewport.ViewId);
                    view.CropBoxVisible = false;
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ShowCropRegion : IExternalCommand
    {
        internal sealed class SelectionFilterSheet : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(Viewport)) return true;

                return false;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }

        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            SelectionFilterSheet filter = new SelectionFilterSheet();
            IList<Reference> references = selection.PickObjects(ObjectType.Element, filter, "Select Viewports");
            using (Transaction t = new Transaction(document, "Show Crop Region"))
            {
                t.Start();
                foreach (Reference reference in references)
                {
                    Viewport viewport = (Viewport)document.GetElement(reference.ElementId);
                    View view = (View)document.GetElement(viewport.ViewId);
                    view.CropBoxVisible = true;
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DistributeVertical : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<Viewport> sortedViewports = viewportList.OrderBy(x => x.GetBoxCenter().Y).ToList();
            IList<XYZ> points = ViewportToolsUtils.GetViewportPoints(viewportList);
            Dictionary<string, double> bounds = ViewportToolsUtils.GetVerticalBounds(points);
            double difference = bounds["Max"] - bounds["Min"];
            double step = difference / (viewportList.Count - 1);
            using (Transaction transaction = new Transaction(document, "Distribute Viewports Vertical"))
            {
                transaction.Start();
                double start = bounds["Min"];
                foreach (Viewport vp in sortedViewports)
                {
                    XYZ current = vp.GetBoxCenter();
                    XYZ newCenter = new XYZ(current.X, start, current.Z);
                    vp.SetBoxCenter(newCenter);
                    start += step;
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DistributeHorizontal : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<Viewport> sortedViewports = viewportList.OrderBy(x => x.GetBoxCenter().X).ToList();
            IList<XYZ> points = ViewportToolsUtils.GetViewportPoints(viewportList);
            Dictionary<string, double> bounds = ViewportToolsUtils.GetHorizontalBounds(points);
            double difference = bounds["Max"] - bounds["Min"];
            double step = difference / (viewportList.Count - 1);
            using (Transaction transaction = new Transaction(document, "Distribute Viewports Horizontal"))
            {
                transaction.Start();
                double start = bounds["Min"];
                foreach (Viewport vp in sortedViewports)
                {
                    XYZ current = vp.GetBoxCenter();
                    XYZ newCenter = new XYZ(start, current.Y, current.Z);
                    vp.SetBoxCenter(newCenter);
                    start += step;
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignHorizontalLeft : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<Viewport> sortedViewports = viewportList.OrderBy(x => x.GetBoxCenter().X).ToList();
            IList<XYZ> points = ViewportToolsUtils.GetViewportPoints(viewportList);
            Dictionary<string, double> bounds = ViewportToolsUtils.GetHorizontalBounds(points);
            using (Transaction transaction = new Transaction(document, "Align Viewports Horizontal Center"))
            {
                transaction.Start();
                foreach (Viewport vp in viewportList)
                {
                    XYZ current = vp.GetBoxCenter();
                    XYZ newCenter = new XYZ(bounds["Min"], current.Y, current.Z);
                    vp.SetBoxCenter(newCenter);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignHorizontalRight : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<Viewport> sortedViewports = viewportList.OrderBy(x => x.GetBoxCenter().X).ToList();
            IList<XYZ> points = ViewportToolsUtils.GetViewportPoints(viewportList);
            Dictionary<string, double> bounds = ViewportToolsUtils.GetHorizontalBounds(points);
            using (Transaction transaction = new Transaction(document, "Align Viewports Horizontal Center"))
            {
                transaction.Start();
                foreach (Viewport vp in viewportList)
                {
                    XYZ current = vp.GetBoxCenter();
                    XYZ newCenter = new XYZ(bounds["Max"], current.Y, current.Z);
                    vp.SetBoxCenter(newCenter);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignVerticalTop : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<Viewport> sortedViewports = viewportList.OrderBy(x => x.GetBoxCenter().X).ToList();
            IList<XYZ> points = ViewportToolsUtils.GetViewportPoints(viewportList);
            Dictionary<string, double> bounds = ViewportToolsUtils.GetHorizontalBounds(points);
            using (Transaction transaction = new Transaction(document, "Align Viewports Horizontal Center"))
            {
                transaction.Start();
                foreach (Viewport vp in viewportList)
                {
                    XYZ current = vp.GetBoxCenter();
                    XYZ newCenter = new XYZ(current.X, bounds["Min"], current.Z) ;
                    vp.SetBoxCenter(newCenter);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignVerticalBottom : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<Viewport> sortedViewports = viewportList.OrderBy(x => x.GetBoxCenter().Y).ToList();
            IList<XYZ> points = ViewportToolsUtils.GetViewportPoints(viewportList);
            Dictionary<string, double> bounds = ViewportToolsUtils.GetHorizontalBounds(points);
            using (Transaction transaction = new Transaction(document, "Align Viewports Horizontal Center"))
            {
                transaction.Start();
                foreach (Viewport vp in viewportList)
                {
                    XYZ current = vp.GetBoxCenter();
                    XYZ newCenter = new XYZ(current.X, bounds["Min"], current.Z);
                    vp.SetBoxCenter(newCenter);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ViewTitlesToUpper : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            using (Transaction transaction = new Transaction(document, "Titles to upper"))
            {
                transaction.Start();
                foreach (Viewport vp in viewportList)
                {
                    View view = (View)document.GetElement(vp.ViewId);
                    if(view.Title != String.Empty)
                    {
                        Parameter parameter = Utils.GetParameterByName(view, "Title on Sheet");
                        parameter.Set(parameter.AsString().ToUpper());
                    }
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
}
