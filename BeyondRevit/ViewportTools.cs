using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using BeyondRevit.UI;
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

        public static Dictionary<string, Curve> GetViewportCurves(Viewport viewport)
        {
            Document doc = viewport.Document;
            View view = doc.GetElement(viewport.ViewId) as View;
            CurveLoop curveloop = view.GetCropRegionShapeManager().GetCropShape().First();
            List<Curve> curves = new List<Curve>();
            CurveLoopIterator iter = curveloop.GetCurveLoopIterator();
            while (iter.MoveNext())
            {
                Curve c = iter.Current as Curve;
                curves.Add(c);
            }

            Dictionary<string, Curve> result = new Dictionary<string, Curve>()
            {
                {"Left", curves[0] },
                {"Top",  curves[1] },
                {"Right",  curves[2] },
                {"Bottom",  curves[3] },
            };
            return result;
        }

        public static Dictionary<string, dynamic> GetSheetDictionary(Document document)
        {
            IList<Element> sheets = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).WhereElementIsNotElementType().ToElements();
            Dictionary<string, dynamic> sheetDictionary = new Dictionary<string, dynamic>();
            foreach (ViewSheet sheet in sheets)
            {
                string key = string.Format("{0} - {1} ({2})", sheet.SheetNumber, sheet.Name, sheet.Id.ToString());
                sheetDictionary.Add(key, sheet);
            }
            return sheetDictionary;

        }

        internal class BRViewport
        {
            public string DetailNumber { get; }
            public ElementId ViewportType { get; }
            public ElementId View { get; }
            public XYZ Position { get; }
            public Document Document { get; }
            public BRViewport(Viewport viewport, bool createCopy = false)
            {
                List<ViewType> CopyableViewTypes = new List<ViewType>()
                {
                        ViewType.Schedule,
                        ViewType.ColumnSchedule,
                        ViewType.PanelSchedule,
                        ViewType.DraftingView,
                        ViewType.Legend
                };
                View = viewport.ViewId;
                Position = viewport.GetBoxCenter();
                Document = viewport.Document;
                DetailNumber = viewport.LookupParameter("Detail Number").AsString();


                if (createCopy)
                {
                    View v = (View)viewport.Document.GetElement(this.View);
                    this.View = v.Duplicate(ViewDuplicateOption.WithDetailing);
                }

                View view = (View)Document.GetElement(this.View);
                ViewportType = viewport.GetTypeId();
                if (!CopyableViewTypes.Contains(view.ViewType) && createCopy == false)
                {
                    Document.Delete(viewport.Id);
                }

            }
        }

        public static Dictionary<string, double> GetVerticalBounds(IList<XYZ> points)
        {
            var verticalBounds = new Dictionary<string, double>();
            List<double> values = new List<double>();
            foreach (XYZ p in points)
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
            foreach (Viewport vp in viewports)
            {
                points.Add(vp.GetBoxCenter());
            }
            return points;
        }

        public static IList<FamilySymbol> GetBreakLineSymbols(Document document, out FamilySymbol defaultSymbol)
        {
            List<string> searchWords = new List<string>()
            {
                "afbreeklijn",
                "afbreek",
                "breakline",
                "break"
            };
            IList<Element> breakLinesFamilies = new FilteredElementCollector(document)
                .OfClass(typeof(Family))
                .ToElements()
                .Where(f => ((Family)f).FamilyPlacementType == FamilyPlacementType.CurveBasedDetail)
                .ToList();

            IList<FamilySymbol> symbols = new List<FamilySymbol>(); 
            foreach(Family f in breakLinesFamilies)
            {
                foreach(ElementId id in f.GetFamilySymbolIds())
                {
                    symbols.Add(document.GetElement(id) as FamilySymbol);
                }
            }

            defaultSymbol = null;
            foreach (string word in searchWords)
            {
                foreach (FamilySymbol element in symbols)
                {
                    if (element.Name.ToLower().Contains(word.ToLower()))
                    {
                        defaultSymbol = element;
                        break;
                    }
                }
                if (defaultSymbol != null) { break; }
            }
            return symbols;
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
            Dictionary<string, double> bounds = ViewportToolsUtils.GetVerticalBounds(points);
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
                    if (view.Title != String.Empty)
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
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MoveViewportsToAnotherSheet : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            BeyondRevit.UI.GenericDropdownWindow selectSheet = new UI.GenericDropdownWindow("Select Sheet", "Select Sheet", ViewportToolsUtils.GetSheetDictionary(document), Utils.RevitWindow(commandData), false);
            selectSheet.ShowDialog();
            if (selectSheet.Cancelled)
            {
                return Result.Cancelled;
            }
            using (Transaction transaction = new Transaction(document, "Move Viewport to Other Sheet"))
            {
                transaction.Start();
                ViewSheet targetSheet = selectSheet.SelectedItems.FirstOrDefault();
                foreach (Viewport vp in viewportList)
                {
                    ViewportToolsUtils.BRViewport brViewport = new ViewportToolsUtils.BRViewport(vp);
                    Viewport newViewport = Viewport.Create(document, targetSheet.Id, brViewport.View, brViewport.Position);
                    newViewport.ChangeTypeId(brViewport.ViewportType);
                    newViewport.LookupParameter("Detail Number").Set(brViewport.DetailNumber);
                }

                transaction.Commit();
            }
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyViewportsToAnotherSheet : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            BeyondRevit.UI.GenericDropdownWindow selectSheet = new UI.GenericDropdownWindow("Select Sheet", "Select Sheet", ViewportToolsUtils.GetSheetDictionary(document), Utils.RevitWindow(commandData), false);
            selectSheet.ShowDialog();
            if (selectSheet.Cancelled)
            {
                return Result.Cancelled;
            }
            using (Transaction transaction = new Transaction(document, "Move Viewport to Other Sheet"))
            {
                transaction.Start();
                ViewSheet targetSheet = selectSheet.SelectedItems.FirstOrDefault();
                foreach (Viewport vp in viewportList)
                {
                    ViewportToolsUtils.BRViewport brViewport = new ViewportToolsUtils.BRViewport(vp, true);
                    Viewport newViewport = Viewport.Create(document, targetSheet.Id, brViewport.View, brViewport.Position);
                    newViewport.ChangeTypeId(brViewport.ViewportType);
                    //try
                    //{

                    //    newViewport.LookupParameter("Detail Number").Set(brViewport.DetailNumber);
                    //}
                    //catch { }
                }

                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExtendCropRegionCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);

            CropViewWindow window = new CropViewWindow(Utils.RevitWindow(commandData));
            window.ShowDialog();

            using (Transaction transaction = new Transaction(document, "Modify Crop Region"))
            {
                transaction.Start();

                transaction.Commit();
            }
            return Result.Succeeded;
        }
        private static void StretchViewport(Viewport viewport, double top, double bottom, double left, double right)
        {
            Document doc = viewport.Document;
            View view = (View)doc.GetElement(viewport.ViewId);
            XYZ horizontalDirection = view.RightDirection;
            XYZ verticalDirection = view.UpDirection;
            List<Curve> horizontalCurves = new List<Curve>();
            List<Curve> verticalCurves = new List<Curve>();

            foreach(CurveLoop curveLoop in view.GetCropRegionShapeManager().GetCropShape())
            {
                foreach(Curve curve in curveLoop)
                {
                    XYZ dir = curve.Evaluate(0.5, true);
                    if (Math.Abs(horizontalDirection.DotProduct(dir)) > 0.9)
                    {
                        horizontalCurves.Add(curve);
                    }
                    else
                    {
                        verticalCurves.Add(curve);
                    }
                }
            }

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateBreakLines : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDocument = commandData.Application.ActiveUIDocument;
            Document document = uiDocument.Document;
            Selection selection = uiDocument.Selection;
            IList<Viewport> viewportList = ViewportToolsUtils.SelectViewports(document, selection);
            IList<FamilySymbol> breakLineFamilies = ViewportToolsUtils.GetBreakLineSymbols(document, out FamilySymbol defaultElement);

            BreaklinesWindow BLWindow = new BreaklinesWindow(Utils.RevitWindow(commandData), defaultElement, breakLineFamilies);
            BLWindow.ShowDialog();

            if (BLWindow.Cancelled)
            {
                return Result.Cancelled;
            }
            
            using(Transaction t = new Transaction(document, "Create Breaklines"))
            {
                t.Start();
                foreach (Viewport viewport in viewportList)
                {
                    View view = (View)document.GetElement(viewport.ViewId);
                    Dictionary<string, Curve> curves = ViewportToolsUtils.GetViewportCurves(viewport);
                    if (BLWindow.TopBreakLine)
                    {
                        Curve curve = curves["Top"];
                        Line line = Line.CreateBound(curve.GetEndPoint(1), curve.GetEndPoint(0));
                        document.Create.NewFamilyInstance(line, BLWindow.BreakLineFamily, view);
                    }
                    if (BLWindow.RightBreakLine)
                    {
                        Curve curve = curves["Right"];
                        Line line = Line.CreateBound(curve.GetEndPoint(1), curve.GetEndPoint(0));
                        document.Create.NewFamilyInstance(line, BLWindow.BreakLineFamily, view);
                    }
                    if (BLWindow.BottomBreakLine)
                    {
                        Curve curve = curves["Bottom"];
                        Line line = Line.CreateBound(curve.GetEndPoint(1), curve.GetEndPoint(0));
                        document.Create.NewFamilyInstance(line, BLWindow.BreakLineFamily, view);
                    }
                    if (BLWindow.LeftBreakLine)
                    {
                        Curve curve = curves["Left"];
                        Line line = Line.CreateBound(curve.GetEndPoint(1), curve.GetEndPoint(0));
                        document.Create.NewFamilyInstance(line, BLWindow.BreakLineFamily, view);
                    }
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
}
