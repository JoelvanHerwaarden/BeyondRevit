using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using BeyondRevit.ViewModels;
using Autodesk.Revit.UI.Selection;

namespace BeyondRevit.WorkplaneCommands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneBy3Points : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            //Utils.SketchplaneByView(doc.ActiveView);
            XYZ firstPoint = selection.PickPoint(
                ObjectSnapTypes.Midpoints |
                ObjectSnapTypes.Perpendicular |
                ObjectSnapTypes.Quadrants |
                ObjectSnapTypes.Centers |
                ObjectSnapTypes.Endpoints |
                ObjectSnapTypes.Intersections |
                ObjectSnapTypes.Nearest |
                ObjectSnapTypes.Perpendicular |
                ObjectSnapTypes.Points |
                ObjectSnapTypes.Quadrants |
                ObjectSnapTypes.Tangents
                , "Pick First Point");
            XYZ secondPoint = selection.PickPoint("Pick Second Point");
            XYZ thirdPoint = selection.PickPoint("Pick Third Point");
            View activeView = doc.ActiveView;
            
            using (Transaction transaction = new Transaction(doc, "Set Workplane by 3 Points"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, Plane.CreateByThreePoints(firstPoint, secondPoint, thirdPoint));
                activeView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneOnTopOfSectionBox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            if (activeView.GetType() != typeof(View3D))
            {
                Utils.Show("You can only use this tool in a 3D View");
                return Result.Succeeded;
            }
            Dictionary<string, Plane> sectionBoxPlanes = WorkplaneSectionboxUtils.GetSectionBoxPlanes(activeView as View3D);
            if (sectionBoxPlanes == null)
            {
                Utils.Show("The current view does not have a Sectionbox");
                return Result.Succeeded;
            }
            using (Transaction transaction = new Transaction(doc, "Set Workplane to Top of Sectionbox"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, sectionBoxPlanes["Top"]);
                activeView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneOnBottomOfSectionBox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            if (activeView.GetType() != typeof(View3D))
            {
                Utils.Show("You can only use this tool in a 3D View");
                return Result.Succeeded;
            }
            Dictionary<string, Plane> sectionBoxPlanes = WorkplaneSectionboxUtils.GetSectionBoxPlanes(activeView as View3D);
            if (sectionBoxPlanes == null)
            {
                Utils.Show("The current view does not have a Sectionbox");
                return Result.Succeeded;
            }
            using (Transaction transaction = new Transaction(doc, "Set Workplane to Bottom of Sectionbox"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, sectionBoxPlanes["Bottom"]);
                activeView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneOnFrontOfSectionBox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            if (activeView.GetType() != typeof(View3D))
            {
                Utils.Show("You can only use this tool in a 3D View");
                return Result.Succeeded;
            }
            Dictionary<string, Plane> sectionBoxPlanes = WorkplaneSectionboxUtils.GetSectionBoxPlanes(activeView as View3D);
            if (sectionBoxPlanes == null)
            {
                Utils.Show("The current view does not have a Sectionbox");
                return Result.Succeeded;
            }
            using (Transaction transaction = new Transaction(doc, "Set Workplane to Front of Sectionbox"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, sectionBoxPlanes["Front"]);
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneOnBackOfSectionBox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            if (activeView.GetType() != typeof(View3D))
            {
                Utils.Show("You can only use this tool in a 3D View");
                return Result.Succeeded;
            }
            Dictionary<string, Plane> sectionBoxPlanes = WorkplaneSectionboxUtils.GetSectionBoxPlanes(activeView as View3D);
            if (sectionBoxPlanes == null)
            {
                Utils.Show("The current view does not have a Sectionbox");
                return Result.Succeeded;
            }
            using (Transaction transaction = new Transaction(doc, "Set Workplane to Back of Sectionbox"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, sectionBoxPlanes["Back"]);
                activeView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneOnLeftOfSectionBox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            if (activeView.GetType() != typeof(View3D))
            {
                Utils.Show("You can only use this tool in a 3D View");
                return Result.Succeeded;
            }
            Dictionary<string, Plane> sectionBoxPlanes = WorkplaneSectionboxUtils.GetSectionBoxPlanes(activeView as View3D);
            if (sectionBoxPlanes == null)
            {
                Utils.Show("The current view does not have a Sectionbox");
                return Result.Succeeded;
            }
            using (Transaction transaction = new Transaction(doc, "Set Workplane to Left of Sectionbox"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, sectionBoxPlanes["Left"]);
                activeView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class WorkplaneOnRightOfSectionBox : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            View activeView = doc.ActiveView;
            if (activeView.GetType() != typeof(View3D))
            {
                Utils.Show("You can only use this tool in a 3D View");
                return Result.Succeeded;
            }
            Dictionary<string, Plane> sectionBoxPlanes = WorkplaneSectionboxUtils.GetSectionBoxPlanes(activeView as View3D);
            if (sectionBoxPlanes == null)
            {
                Utils.Show("The current view does not have a Sectionbox");
                return Result.Succeeded;
            }
            using (Transaction transaction = new Transaction(doc, "Set Workplane to Left of Sectionbox"))
            {
                transaction.Start();
                activeView.SketchPlane = SketchPlane.Create(doc, sectionBoxPlanes["Right"]);
                activeView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SketchplaneByCurrentView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            using (Transaction transaction = new Transaction(doc, "Set Sketchplane to Active View"))
            {
                transaction.Start();
                Utils.SketchplaneByView(doc.ActiveView);
                doc.ActiveView.ShowActiveWorkPlane();
                transaction.Commit();
            }
            return Result.Succeeded;
        }

    }
    internal class WorkplaneSectionboxUtils
    {
        /// <summary>
        /// Creates a solid based on a Sectionbox Element in a 3D View
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        internal static Solid CreateSolidFromBoundingBox(View3D view)
        {
            BoundingBoxXYZ bbox = view.GetSectionBox();

            // Corners in BBox coords

            XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
            XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
            XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);

            // Edges in BBox coords

            Line edge0 = Line.CreateBound(pt0, pt1);
            Line edge1 = Line.CreateBound(pt1, pt2);
            Line edge2 = Line.CreateBound(pt2, pt3);
            Line edge3 = Line.CreateBound(pt3, pt0);

            // Create loop, still in BBox coords

            List<Curve> edges = new List<Curve>();
            edges.Add(edge0);
            edges.Add(edge1);
            edges.Add(edge2);
            edges.Add(edge3);

            double height = bbox.Max.Z - bbox.Min.Z;

            CurveLoop baseLoop = CurveLoop.Create(edges);

            List<CurveLoop> loopList = new List<CurveLoop>();
            loopList.Add(baseLoop);

            Solid preTransformBox = GeometryCreationUtilities
              .CreateExtrusionGeometry(loopList, XYZ.BasisZ,
                height);

            Solid transformBox = SolidUtils.CreateTransformed(
              preTransformBox, bbox.Transform);

            return transformBox;
        }

        /// <summary>
        /// Returns the planes for each side of a Sectionbox within a 3D View. Return null if there is no SectionBox
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        internal static Dictionary<string, Plane> GetSectionBoxPlanes(View3D view)
        {
            BoundingBoxXYZ boundingBox = view.GetSectionBox();
            if (boundingBox == null)
            {
                return null;
            }
            Solid boundingBoxSolid = CreateSolidFromBoundingBox(view);

            Dictionary<string, Plane> planes = new Dictionary<string, Plane>()
            {
                {"Top", CreatePlaneByFace((PlanarFace)boundingBoxSolid.Faces.get_Item(0)) },
                {"Bottom", CreatePlaneByFace((PlanarFace)boundingBoxSolid.Faces.get_Item(1)) },
                {"Front", CreatePlaneByFace((PlanarFace)boundingBoxSolid.Faces.get_Item(2)) },
                {"Right", CreatePlaneByFace((PlanarFace)boundingBoxSolid.Faces.get_Item(3)) },
                {"Back", CreatePlaneByFace((PlanarFace)boundingBoxSolid.Faces.get_Item(4)) },
                {"Left", CreatePlaneByFace((PlanarFace)boundingBoxSolid.Faces.get_Item(5)) },
            };
            return planes;
        }

        /// <summary>
        /// Creates a Plane by a gives Planar Face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        internal static Plane CreatePlaneByFace(PlanarFace face)
        {
            Plane plane = Plane.CreateByOriginAndBasis(
                face.Evaluate(new UV(0.5, 0.5)),
                face.XVector,
                face.YVector);
            return plane;
        }

    }
}
