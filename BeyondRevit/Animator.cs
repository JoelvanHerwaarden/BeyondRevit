using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using BeyondRevit.ViewModels;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using Forms = System.Windows.Forms;
using adskWindows = Autodesk.Windows;
using Autodesk.Revit.UI.Selection;
using System.Collections.ObjectModel;
using BeyondRevit.Models;
using Autodesk.Revit.DB.ExtensibleStorage;
using System.Drawing.Printing;
using static System.Drawing.Printing.PrinterSettings;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.ApplicationServices;


namespace BeyondRevit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateSliderAnimation : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Element SectionBox = document.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Select Section Box"));
            Element Path = document.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Select Model Curve"));
            ModelCurve curve = (ModelCurve)Path;
            XYZ direction = curve.GeometryCurve.Evaluate(0.5, true);
            Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                UIView view = uidoc.GetOpenUIViews().FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id);
                int numberOfIterations = 1000;
                ImageExportOptions options = new ImageExportOptions();
                options.SetViewsAndSheets(new List<ElementId>() { view.ViewId });

                for (int i = 0; i < numberOfIterations; i++)
                {
                    string filepath = System.IO.Path.Combine(dialog.SelectedPath, string.Format("image{0}.jpg", i.ToString()));
                    options.FilePath = filepath;
                    using (Transaction transaction = new Transaction(document, "Move Section Box"))
                    {
                        transaction.Start();
                        ElementTransformUtils.MoveElement(document, SectionBox.Id, direction.Normalize().Multiply(Utils.ToInternalUnits(document, 100)));
                        transaction.Commit();
                    }
                    view.ZoomToFit();
                    document.ExportImage(options);

                }
            }

            return Result.Succeeded;
        }
        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class Create3DSections : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document document = uidoc.Document;
                Element SectionBox = document.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Select Section Box"));
                Element Camera = AnimatorUtils.GetViewCamera(document.ActiveView);
                Element Path = document.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Select Model Curve"));
                ModelCurve curve = (ModelCurve)Path;

                XYZ direction = Line.CreateBound(curve.GeometryCurve.Evaluate(0, true), curve.GeometryCurve.Evaluate(1, true)).Direction.Normalize();
                View3D view = (View3D)uidoc.ActiveView;
                UIView uiView = uidoc.GetOpenUIViews().FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id);
                int numberOfIterations = 150;
                double moveDistance = 2500;
                List<View> sectionViews = new List<View>();
                UIView startUIView = AnimatorUtils.GetUIView(view);
                IList<XYZ> corners = startUIView.GetZoomCorners();
                List<Double> distances = new List<Double>();

                List<BoundingBoxXYZ> bboxes = new List<BoundingBoxXYZ>();
                using (Transaction transaction = new Transaction(document, "Create Views"))
                {
                    transaction.Start();


                    for (int i = 0; i < numberOfIterations; i++)
                    {
                        view.CropBoxVisible = true;
                        view.CropBoxActive = true;

                        double currentMoveDistance = i * moveDistance;
                        string name = view.Name + "_" + (currentMoveDistance).ToString();

                        ElementTransformUtils.MoveElement(document, SectionBox.Id, direction.Multiply(Utils.ToInternalUnits(document, moveDistance)));
                        ElementTransformUtils.MoveElement(document, Camera.Id, direction.Multiply(Utils.ToInternalUnits(document, moveDistance)));
                        bboxes.Add(view.CropBox);
                        uidoc.Selection.SetElementIds(new ElementId[] { SectionBox.Id });
                        startUIView.ZoomToFit();
                        document.Regenerate();
                        uidoc.RefreshActiveView();
                        View newView = (View)document.GetElement(view.Duplicate(ViewDuplicateOption.Duplicate));
                        newView.CropBoxVisible = true;
                        newView.CropBoxActive = true;
                        newView.CropBox = view.CropBox;

                        sectionViews.Add(newView);
                        newView.Name = name;


                    }

                    transaction.Commit();
                }
                return Result.Succeeded;
            }
        }

        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class ZoomToSectionBoxes : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                IList<Reference> viewRefs = Utils.GetCurrentSelection(uidoc);
                List<View> views = new List<View>();
                foreach (Reference r in viewRefs)
                {
                    View view = (View)uidoc.Document.GetElement(r);
                    views.Add(view);
                }
                List<UIView> uiViews = AnimatorUtils.OpenViews(views);

                AnimatorUtils.ZoomToSectionbox(uidoc, uiViews);
                return Result.Succeeded;
            }
        }

        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class OpenViews : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                IList<Reference> viewRefs = Utils.GetCurrentSelection(uidoc);
                List<View> views = new List<View>();
                foreach (Reference r in viewRefs)
                {
                    View view = (View)uidoc.Document.GetElement(r);
                    views.Add(view);
                }
                AnimatorUtils.OpenAndZoom(views);
                return Result.Succeeded;
            }
        }
        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class ZoomToSectionbox : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;
                View view = uidoc.ActiveView;
                UIView uiview = AnimatorUtils.GetUIView(view);
                uidoc.Selection.SetElementIds(new List<ElementId>() { AnimatorUtils.GetSectionBoxId(view) });
                uiview.ZoomToFit();
                uiview.Close();
                return Result.Succeeded;
            }
        }
        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class ZoomToSectionBoxCurrentView : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                List<UIView> uiViews = new List<UIView>() { (UIView)uidoc.GetOpenUIViews().FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id) };

                AnimatorUtils.ZoomToSectionbox(uidoc, uiViews);
                return Result.Succeeded;
            }
        }

        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class SetEyePositionCurrentView : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                View3D view = (View3D)uidoc.ActiveView;
                BoundingBoxXYZ bbox = view.GetSectionBox();
                XYZ mid = Line.CreateBound(bbox.Max, bbox.Min).Evaluate(0.5, true);
                using (Transaction transaction = new Transaction(uidoc.Document, ""))
                {
                    transaction.Start();
                    ViewOrientation3D orientation = view.GetOrientation();
                    ViewOrientation3D newOrientation = new ViewOrientation3D(mid, orientation.UpDirection, orientation.ForwardDirection);
                    view.SetOrientation(newOrientation);
                    transaction.Commit();
                }
                return Result.Succeeded;
            }
        }
        public class AnimatorUtils
        {
            public static UIView GetUIView(View view)
            {
                UIDocument uidoc = new UIDocument(view.Document);
                UIView viewUi = uidoc.GetOpenUIViews().FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id);
                return viewUi;
            }
            public static List<UIView> OpenViews(List<View> views)
            {
                List<UIView> uiViews = new List<UIView>();

                Document document = views.First().Document;
                UIDocument uidoc = new UIDocument(document);
                foreach (View view in views)
                {
                    uidoc.ActiveView = view;
                    UIView viewUi = uidoc.GetOpenUIViews().FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id);
                    uidoc.ActiveView = view;
                    uidoc.RefreshActiveView();
                    uiViews.Add(viewUi);
                }
                //uidoc.RefreshActiveView();
                return uiViews;
            }
            public static void OpenAndZoom(List<View> views)
            {
                List<UIView> uiViews = new List<UIView>();

                Document document = views.First().Document;
                UIDocument uidoc = new UIDocument(document);
                foreach (View view in views)
                {
                    uidoc.ActiveView = view;
                    uidoc.RefreshActiveView();

                    UIView viewUi = uidoc.GetOpenUIViews().FirstOrDefault(item => item.ViewId == uidoc.ActiveView.Id);
                    uiViews.Add(viewUi);
                    ElementId sectionBoxId = GetSectionBoxId(view);
                    uidoc.Selection.SetElementIds(new List<ElementId>());
                    uidoc.Selection.SetElementIds(new List<ElementId>() { sectionBoxId });
                    viewUi.ZoomToFit();
                }
            }
            public static void ZoomToSectionbox(UIDocument uidoc, List<UIView> views)
            {
                foreach(UIView UIView in views)
                {
                    //View view = uidoc.ActiveView;
                    //uidoc.ActiveView = view;
                    //uidoc.RequestViewChange(view);
                    //uidoc.RefreshActiveView();
                    //ElementId sectionBoxId = GetSectionBoxId(view);
                    //uidoc.Selection.SetElementIds(new List<ElementId>());
                    //uidoc.Selection.SetElementIds(new List<ElementId>() { sectionBoxId });
                    //UIView.ZoomToFit();
                }

            }
            public static ElementId GetSectionBoxId(View view)
            {
                ElementId sectionboxId = null;
                Document document = view.Document;
                ElementClassFilter filter = new ElementClassFilter(typeof(View3D));
                foreach (Element e in new FilteredElementCollector(document, view.Id).WhereElementIsNotElementType().ToElements())
                {
                    try
                    {
                        if (e.Category != null && e.Category.Name == "Section Boxes")
                        {
                            foreach (ElementId id in e.GetDependentElements(filter))
                            {
                                Element dElement = document.GetElement(id);
                                if (dElement.Id == view.Id)
                                {
                                    sectionboxId = new ElementId(dElement.Id.IntegerValue - 1);
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                };
                return sectionboxId;
            }
            public static Element GetViewCamera(View view)
            {
                string name = view.Name;
                Element camera = new FilteredElementCollector(view.Document, view.Id).OfCategory(BuiltInCategory.OST_Cameras).ToElements().Where(c => c.Name == name).FirstOrDefault();
                return camera;
            }
        }
    }

}
