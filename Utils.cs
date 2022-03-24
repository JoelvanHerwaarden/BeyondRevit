using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BeyondRevit
{
    public class Utils
    {

        public static double FeetToMm(double number)
        {
            return number * 304.8;
        }
        public static double MmToFeet(double number)
        {
            return number / 304.8;
        }
        public static double FeetToM(double number)
        {
            return number * 0.3048;
        }
        public static double MToFeet(double number)
        {
            return number / 0.3048;
        }
        public static double MToInch(double number)
        {
            return number / 0.0254;
        }

        public static XYZ XYZFeetToMm(XYZ point)
        {
            double x = FeetToMm(point.X);
            double y = FeetToMm(point.Y);
            double z = FeetToMm(point.Z);
            return new XYZ(x, y, z);
        }
        public static XYZ XYZFeetToM(XYZ point)
        {
            double x = FeetToM(point.X);
            double y = FeetToM(point.Y);
            double z = FeetToM(point.Z);
            return new XYZ(x, y, z);
        }
        public static XYZ XYZMToFeet(XYZ point)
        {
            double x = MToFeet(point.X);
            double y = MToFeet(point.Y);
            double z = MToFeet(point.Z);
            return new XYZ(x, y, z);
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }
        public static XYZ ToWCS(XYZ point, BasePoint basePoint)
        {
            double ew = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
            double ns = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
            XYZ result = new XYZ(point.X + Utils.FeetToM(ew), point.Y + Utils.FeetToM(ns), 0);

            return result;
        }

        /// <summary>
        /// Translates a Revit Point to the Revit Coordinate System In Meters
        /// </summary>
        /// <param name="point"></param>
        /// <param name="basePoint"></param>
        /// <returns></returns>
        public static XYZ ToRCS(XYZ point, BasePoint basePoint)
        {
            double ew = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
            double ns = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
            XYZ result = new XYZ(point.X - Utils.FeetToM(ew), point.Y - Utils.FeetToM(ns), 0);
            return result;
        }

        public static void SetProjectToMeters(Document doc)
        {
            Units units = doc.GetUnits();
            units.GetFormatOptions(UnitType.UT_Length).DisplayUnits = DisplayUnitType.DUT_METERS;
            doc.SetUnits(units);
        }
        public static void SetProjectToMillimeters(Document doc)
        {
            Units units = doc.GetUnits();
            units.GetFormatOptions(UnitType.UT_Length).DisplayUnits = DisplayUnitType.DUT_MILLIMETERS;
            doc.SetUnits(units);
        }

        public static Window RevitWindow(ExternalCommandData commandData)
        {
            IntPtr RevitWindowHandle = commandData.Application.MainWindowHandle;
            HwndSource hwndSource = HwndSource.FromHwnd(RevitWindowHandle);
            Window RevitWindow = hwndSource.RootVisual as Window;
            return RevitWindow;
        }

        public static void Show(string message)
        {
            TaskDialog taskDialog = new TaskDialog("Beyond Revit")
            {
                ExpandedContent = message,
                MainContent = message,
                MainIcon = TaskDialogIcon.TaskDialogIconInformation
            };
            TaskDialog.Show("Beyond Revit", message);
        }

        public static double ToInternalUnits(Document doc, double d)
        {
            DisplayUnitType currentType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            return UnitUtils.ConvertToInternalUnits(d, currentType);
        }

        public static double FromInternalUnits(Document doc, double d)
        {
            DisplayUnitType currentType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            return UnitUtils.ConvertFromInternalUnits(d, currentType);
        }

        public static void Uiapp_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            UIApplication uiapp = (UIApplication)sender;
            Document doc = uiapp.ActiveUIDocument.Document;
            SynchronizeWithCentralOptions syncOptions = new SynchronizeWithCentralOptions()
            {
                SaveLocalBefore = true,
                Comment = "Periodic Save",
                SaveLocalAfter = false
            };
            TransactWithCentralOptions transOptions = new TransactWithCentralOptions();
            doc.SynchronizeWithCentral(transOptions, syncOptions);
        }

        public static System.Windows.Media.ImageSource PngImageSource(RibbonPanel panel, string embeddedPath)
        {
            Stream stream = panel.GetType().Assembly.GetManifestResourceStream(embeddedPath);
            var decoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);

            return decoder.Frames[0];
        }

        public static IList<Reference> GetCurrentSelection(UIDocument uidoc, ISelectionFilter filter = null, string message = null)
        {
            IList<Reference> result = null;
            ICollection<ElementId> selection = uidoc.Selection.GetElementIds();
            if(selection.Count != 0)
            {
                result = new List<Reference>();
                if (filter == null)
                {
                    foreach(ElementId id in selection)
                    {
                        Reference reference = new Reference(uidoc.Document.GetElement(id));
                        result.Add(reference);
                    }
                    return result;
                }
                else
                {
                    bool nothing = true;
                    foreach(ElementId id in selection)
                    {
                        Element element = uidoc.Document.GetElement(id);
                        if (filter.AllowElement(element))
                        {
                            nothing = false;
                            result.Add(new Reference(element));
                        }
                    }
                    if (nothing)
                    {
                        uidoc.Selection.SetElementIds(new Collection<ElementId>());
                        result = Utils.GetCurrentSelection(uidoc, filter, message);
                    }
                    return result;
                }
            }
            else
            {
                if(message==null && filter == null)
                {
                    result = uidoc.Selection.PickObjects(ObjectType.Element);
                }
                else if (message == null)
                {
                    result = uidoc.Selection.PickObjects(ObjectType.Element, filter);
                }
                else if(filter==null)
                {
                    result = uidoc.Selection.PickObjects(ObjectType.Element, message);
                }
                else
                {
                    result = uidoc.Selection.PickObjects(ObjectType.Element, filter, message);
                }
                if (result == null)
                {
                    return null;
                }
                return result;
            }
        }

        public static Reference GetCurrentSelectionSingleElement(UIDocument uidoc, ISelectionFilter filter = null, string message = null)
        {
            Reference result = null;
            ICollection<ElementId> selection = uidoc.Selection.GetElementIds();
            if (selection.Count == 1)
            {
                ElementId id = selection.First<ElementId>();
                if (filter == null)
                {
                    result = new Reference(uidoc.Document.GetElement(id));
                    return result;
                }
                else
                {
                    bool nothing = true;
                    
                    Element element = uidoc.Document.GetElement(id);
                    if (filter.AllowElement(element))
                    {
                        nothing = false;
                        result = new Reference(element);
                    }
                    if (nothing)
                    {
                        uidoc.Selection.SetElementIds(new Collection<ElementId>());
                        result = Utils.GetCurrentSelectionSingleElement(uidoc, filter, message);
                    }
                    return result;
                }
            }
            else
            {
                if (message == null && filter == null)
                {
                    result = uidoc.Selection.PickObject(ObjectType.Element);
                }
                else if (message == null)
                {
                    result = uidoc.Selection.PickObject(ObjectType.Element, filter);
                }
                else if (filter == null)
                {
                    result = uidoc.Selection.PickObject(ObjectType.Element, message);
                }
                else
                {
                    result = uidoc.Selection.PickObject(ObjectType.Element, filter, message);
                }
                if (result == null)
                {
                    return null;
                }
                return result;
            }
        }

        public static Parameter GetParameterByName(Element element, string name)
        {
            Parameter result = null;
            foreach (Parameter parameter in element.Parameters)
            {
                if (parameter.Definition.Name == name)
                {
                    result = parameter;
                    break;
                }
            }
            return result;
        }
    }
}
