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

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ApplyMaterialSettingsAsOverrides : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Selection selection = commandData.Application.ActiveUIDocument.Selection;

            SelectElements:
            try
            {
                IList<Reference> references = selection.PickObjects(ObjectType.Element, "Select elements");

                IList<Element> elementsList = new FilteredElementCollector(doc).OfClass(typeof(Material)).ToElements();
                Dictionary<string, dynamic> materials = new Dictionary<string, dynamic>();
                foreach(Element element in elementsList)
                {
                    try
                    {
                        materials.Add(element.Name, element);
                    }
                    catch
                    {

                    }
                }
                GenericDropdownWindow window = new GenericDropdownWindow("Select Materials", "Select Materials", materials, Utils.RevitWindow(commandData), false);
                window.Show();
                if (!window.Cancelled)
                {
                    using(Transaction transaction = new Transaction(doc, "Override Elements with Material settings"))
                    {
                        transaction.Start(); 
                        Material material = window.SelectedItems[0];
                        OverrideGraphicSettings overrides = new OverrideGraphicSettings();

                        FillPatternElement surfaceForegroundPattern = (FillPatternElement)doc.GetElement(material.SurfaceForegroundPatternId);
                        if(surfaceForegroundPattern != null && surfaceForegroundPattern.GetFillPattern().Target == FillPatternTarget.Drafting)
                        {
                            overrides.SetSurfaceForegroundPatternId(material.SurfaceForegroundPatternId);
                            overrides.SetSurfaceForegroundPatternColor(material.SurfaceForegroundPatternColor);
                        }

                        FillPatternElement surfaceBackgroundPattern = (FillPatternElement)doc.GetElement(material.SurfaceBackgroundPatternId);
                        if (surfaceBackgroundPattern != null && surfaceBackgroundPattern.GetFillPattern().Target == FillPatternTarget.Drafting)
                        {
                            overrides.SetSurfaceBackgroundPatternId(material.SurfaceBackgroundPatternId);
                            overrides.SetSurfaceBackgroundPatternColor(material.SurfaceBackgroundPatternColor);
                        }

                        FillPatternElement cutForegroundPattern = (FillPatternElement)doc.GetElement(material.CutForegroundPatternId);
                        if (cutForegroundPattern != null && cutForegroundPattern.GetFillPattern().Target == FillPatternTarget.Drafting)
                        {
                            overrides.SetCutForegroundPatternId(material.CutForegroundPatternId);
                            overrides.SetCutForegroundPatternColor(material.CutForegroundPatternColor);
                        }

                        FillPatternElement cutBackgroundPattern = (FillPatternElement)doc.GetElement(material.CutBackgroundPatternId);
                        if (cutBackgroundPattern != null && cutBackgroundPattern.GetFillPattern().Target == FillPatternTarget.Drafting)
                        {
                            overrides.SetCutBackgroundPatternId(material.CutBackgroundPatternId);
                            overrides.SetCutBackgroundPatternColor(material.CutBackgroundPatternColor);
                        }


                        foreach (Reference reference in references)
                        {
                            Element element = doc.GetElement(reference);
                            doc.ActiveView.SetElementOverrides(element.Id, overrides);
                        }
                        transaction.Commit();
                    }
                    
                }
                else
                {
                    return Result.Succeeded;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Succeeded;
            }
            goto SelectElements;

        }
            
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ApplyMaterialSettingsAsOverridesToDirectShapes : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            ICollection<ElementId> directShapes = new FilteredElementCollector(doc).OfClass(typeof(DirectShape)).WhereElementIsNotElementType().ToElementIds();
            Dictionary<string, OverrideGraphicSettings> overridesDictionary = CreateDirectShapeOverridesDictionary(doc, directShapes);

            using(Transaction transaction = new Transaction(doc, "Apply Material Overrides to Directshapes"))
            {
                transaction.Start();
                List<Element> views = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements().ToList();
                foreach (View view in views)
                {
                    try
                    {
                        ICollection<ElementId> directShapesInView = new FilteredElementCollector(doc, view.Id).OfClass(typeof(DirectShape)).WhereElementIsNotElementType().ToElementIds();
                        foreach (ElementId id in directShapesInView)
                        {
                            OverrideGraphicSettings overrides = overridesDictionary[id.ToString()];
                            view.SetElementOverrides(id, overrides);
                        }
                    }
                    catch { }
                    
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }

        public static Dictionary<string, OverrideGraphicSettings> CreateOverridesDictionary(Document doc, ICollection<ElementId> materialIds)
        {
            Dictionary<string, OverrideGraphicSettings> result = new Dictionary<string, OverrideGraphicSettings>();
            foreach(ElementId id in materialIds)
            {
                if (!result.Keys.Contains(id.ToString()))
                {
                    Material material = (Material)doc.GetElement(id);
                    OverrideGraphicSettings overrides = new OverrideGraphicSettings();
                    overrides.SetSurfaceForegroundPatternId(material.SurfaceForegroundPatternId);
                    overrides.SetSurfaceForegroundPatternColor(material.SurfaceForegroundPatternColor);
                    overrides.SetSurfaceBackgroundPatternId(material.SurfaceBackgroundPatternId);
                    overrides.SetSurfaceBackgroundPatternColor(material.SurfaceBackgroundPatternColor);

                    overrides.SetCutForegroundPatternId(material.CutForegroundPatternId);
                    overrides.SetCutForegroundPatternColor(material.CutForegroundPatternColor);
                    overrides.SetCutBackgroundPatternId(material.CutBackgroundPatternId);
                    overrides.SetCutBackgroundPatternColor(material.CutBackgroundPatternColor);
                    result.Add(id.ToString(), overrides);
                }
            }
            return result;

        }

        public static Dictionary<string, OverrideGraphicSettings> CreateDirectShapeOverridesDictionary(Document doc, ICollection<ElementId> directshapes)
        {
            Dictionary<string, OverrideGraphicSettings> result = new Dictionary<string, OverrideGraphicSettings>();
            foreach (ElementId id in directshapes)
            {
                if (!result.Keys.Contains(id.ToString()))
                {
                    DirectShape directShape = (DirectShape)doc.GetElement(id);
                    Material material = (Material)doc.GetElement(directShape.GetMaterialIds(false).FirstOrDefault());
                    OverrideGraphicSettings overrides = new OverrideGraphicSettings();
                    overrides.SetSurfaceForegroundPatternId(material.SurfaceForegroundPatternId);
                    overrides.SetSurfaceForegroundPatternColor(material.SurfaceForegroundPatternColor);
                    overrides.SetSurfaceBackgroundPatternId(material.SurfaceBackgroundPatternId);
                    overrides.SetSurfaceBackgroundPatternColor(material.SurfaceBackgroundPatternColor);

                    overrides.SetCutForegroundPatternId(material.CutForegroundPatternId);
                    overrides.SetCutForegroundPatternColor(material.CutForegroundPatternColor);
                    overrides.SetCutBackgroundPatternId(material.CutBackgroundPatternId);
                    overrides.SetCutBackgroundPatternColor(material.CutBackgroundPatternColor);
                    result.Add(id.ToString(), overrides);
                }
            }
            return result;

        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateFilledRegionTypesByMaterials : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            IList<Element> elementsList = new FilteredElementCollector(doc).OfClass(typeof(Material)).ToElements();
            Dictionary<string, dynamic> materials = new Dictionary<string, dynamic>();
            foreach (Element element in elementsList)
            {
                try
                {
                    materials.Add(element.Name, element);
                }
                catch
                {

                }
            }
            GenericDropdownWindow window = new GenericDropdownWindow("Select Materials", "Select Materials", materials, Utils.RevitWindow(commandData), false);
            window.ShowDialog();
            if (!window.Cancelled)
            {
                using (Transaction transaction = new Transaction(doc, "Create Filled Regions by Material"))
                {
                    transaction.Start();

                    Material material = window.SelectedItems[0];

                    string projectionName = material.Name + "_Projection";
                    string sectionName = material.Name + "_Section";
                    FilledRegionType filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).Where(e => e.Name == projectionName).FirstOrDefault() as FilledRegionType;
                    if (filledRegion == null)
                    {
                        filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).FirstOrDefault() as FilledRegionType;
                        FilledRegionType newFilledRegion = (FilledRegionType)filledRegion.Duplicate(projectionName);
                        newFilledRegion.ForegroundPatternId = material.SurfaceForegroundPatternId;
                        newFilledRegion.ForegroundPatternColor = material.SurfaceForegroundPatternColor;
                        newFilledRegion.BackgroundPatternId = material.SurfaceBackgroundPatternId;
                        newFilledRegion.BackgroundPatternColor = material.SurfaceBackgroundPatternColor;
                    }

                    filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).Where(e => e.Name == sectionName).FirstOrDefault() as FilledRegionType;
                    if (filledRegion == null)
                    {
                        filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).FirstOrDefault() as FilledRegionType;
                        FilledRegionType newFilledRegion = (FilledRegionType)filledRegion.Duplicate(sectionName);
                        newFilledRegion.ForegroundPatternId = material.CutForegroundPatternId;
                        newFilledRegion.ForegroundPatternColor = material.CutForegroundPatternColor;
                        newFilledRegion.BackgroundPatternId = material.CutBackgroundPatternId;
                        newFilledRegion.BackgroundPatternColor = material.CutBackgroundPatternColor;
                    }

                    transaction.Commit();
                }

            }
            else
            {
            }

            return Result.Succeeded;


        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateFilledRegionTypesByMaterialsShaded : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            IList<Element> elementsList = new FilteredElementCollector(doc).OfClass(typeof(Material)).ToElements();
            Dictionary<string, dynamic> materials = new Dictionary<string, dynamic>();
            foreach (Element element in elementsList)
            {
                try
                {
                    materials.Add(element.Name, element);
                }
                catch
                {

                }
            }
            GenericDropdownWindow window = new GenericDropdownWindow("Select Materials", "Select Materials", materials, Utils.RevitWindow(commandData), false);
            window.ShowDialog();
            if (!window.Cancelled)
            {
                using (Transaction transaction = new Transaction(doc, "Create Filled Regions by Material Shaded"))
                {
                    transaction.Start();

                    Material material = window.SelectedItems[0];

                    string projectionName = material.Name + "_Projection_Shaded";
                    string sectionName = material.Name + "_Section_Shaded";
                    FillPatternElement backgroundPattern = new FilteredElementCollector(doc).OfClass(typeof(FillPatternElement)).Where(e => e.Name == "<Solid fill>").FirstOrDefault() as FillPatternElement;

                    FilledRegionType filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).Where(e => e.Name == projectionName).FirstOrDefault() as FilledRegionType;
                    if (filledRegion == null)
                    {
                        filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).FirstOrDefault() as FilledRegionType;
                        FilledRegionType newFilledRegion = (FilledRegionType)filledRegion.Duplicate(projectionName);
                        newFilledRegion.ForegroundPatternId = material.SurfaceForegroundPatternId;
                        newFilledRegion.ForegroundPatternColor = material.SurfaceForegroundPatternColor;
                        newFilledRegion.BackgroundPatternId = backgroundPattern.Id;
                        newFilledRegion.BackgroundPatternColor = material.Color;
                    }

                    filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).Where(e => e.Name == sectionName).FirstOrDefault() as FilledRegionType;
                    if (filledRegion == null)
                    {
                        filledRegion = new FilteredElementCollector(doc).OfClass(typeof(FilledRegionType)).FirstOrDefault() as FilledRegionType;
                        FilledRegionType newFilledRegion = (FilledRegionType)filledRegion.Duplicate(sectionName);
                        newFilledRegion.ForegroundPatternId = material.CutForegroundPatternId;
                        newFilledRegion.ForegroundPatternColor = material.CutForegroundPatternColor;
                        newFilledRegion.BackgroundPatternId = backgroundPattern.Id;
                        newFilledRegion.BackgroundPatternColor = material.Color;
                    }

                    transaction.Commit();
                }

            }
            else
            {
            }

            return Result.Succeeded;


        }

    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HideSubCategoryInAllViews : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Categories categories = doc.Settings.Categories;
            Dictionary<string, dynamic> categoriesDict = new Dictionary<string, dynamic>();
            foreach (Category cat in categories)
            {
                try
                {
                    categoriesDict.Add(cat.Name, cat);
                }
                catch
                {

                }
            }

            GenericDropdownWindow categoryWindow = new GenericDropdownWindow("Select Main Category", "Select Main Category", categoriesDict, Utils.RevitWindow(commandData), false);
            categoryWindow.ShowDialog();
            if (!categoryWindow.Cancelled)
            {
                Category selectedCategory = categoryWindow.SelectedItems[0];
                CategoryNameMap subCategories = selectedCategory.SubCategories;
                Dictionary<string, dynamic> subCategoryDict = new Dictionary<string, dynamic>();
                foreach (Category subCat in subCategories)
                {
                    try
                    {
                        subCategoryDict.Add(subCat.Name, subCat);
                    }
                    catch
                    {

                    }
                }
                GenericDropdownWindow subCategoryWindow = new GenericDropdownWindow("Select Subcategory", "Select Subcategory to Hide", subCategoryDict, Utils.RevitWindow(commandData), false);
                subCategoryWindow.ShowDialog();
                if (!subCategoryWindow.Cancelled)
                {
                    using (Transaction transaction = new Transaction(doc, "Hide Subcategory in All Views"))
                    {
                        transaction.Start();

                        transaction.Commit();
                    }
                }
            }
            else
            {
                return Result.Succeeded;
            }
            return Result.Succeeded;

        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GoToProjectLocationCycloMedia : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            try
            {
                using (Transaction transaction = new Transaction(doc, "Open Project Location in CycloMedia"))
                {
                    transaction.Start();

                    Utils.SetProjectToMeters(doc);

                    string baseUrl = @"https://www.google.nl/maps/@52.1311596,4.6390818,14z";
                    BasePoint projectBasepoint = new FilteredElementCollector(doc).OfClass(typeof(BasePoint)).ToElements()[1] as BasePoint;
                    double X = Utils.FeetToM(projectBasepoint.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble());
                    double Y = Utils.FeetToM(projectBasepoint.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble());
                    double lat = projectBasepoint.get_Parameter(BuiltInParameter.BASEPOINT_LATITUDE_PARAM).AsDouble();
                    double lng = projectBasepoint.get_Parameter(BuiltInParameter.BASEPOINT_LONGITUDE_PARAM).AsDouble();
                    TaskDialog.Show("Beyond Revit", lat.ToString() + "   " + lng.ToString());
                    double num2 = X - 50.0;
                    double num3 = X + 50.0;
                    double num4 = Y - 50.0;
                    double num5 = Y + 50.0;
                    string str1 = "https://streetsmart.cyclomedia.com/streetsmart?mq=";
                    string str2 = "&msrs=EPSG:28992";
                    string url = str1 + num2.ToString() + ";" + num4.ToString() + ";" + num3.ToString() + ";" + num5.ToString() + str2;
                    
                    TaskDialog.Show("Beyond Revit", "Url Copied");
                    Process.Start(url);

                    Utils.SetProjectToMillimeters(doc);
                    transaction.Commit();
                }

            }
            catch (Exception e)
            {
                TaskDialog.Show("Beyond Revit", e.Message);
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LoadAerialImage : IExternalCommand
    {
        public ExternalCommandData CommandData { get; set; }
        double Dimension { get; set; }
        BasePoint ProjectBasePoint { get; set; }
        XYZ Point { get; set; }
        Double Pixels { get; set; }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Utils.SetProjectToMeters(commandData.Application.ActiveUIDocument.Document);
            ProjectBasePoint = new FilteredElementCollector(commandData.Application.ActiveUIDocument.Document).OfClass(typeof(BasePoint)).ToElements()[1] as BasePoint;
            AerialDimensionPrompt dimensionPrompt = new AerialDimensionPrompt(commandData.Application.ActiveUIDocument);
            dimensionPrompt.Owner = Utils.RevitWindow(commandData);
            dimensionPrompt.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Result result = Result.Succeeded;

            if (dimensionPrompt.ShowDialog() == true)
            {
                using (Transaction transaction = new Transaction(doc, "LoadAerialImage"))
                {
                    transaction.Start();
                    try
                    {
                        Dimension = dimensionPrompt.Dimension;
                        if (Dimension == 0)
                        {
                            Exception e = new Exception();
                            throw e;
                        }
                    }
                    catch
                    {
                        result = Result.Cancelled;
                        return result;
                    }
                    Pixels = dimensionPrompt.Pixels;
                    Point = dimensionPrompt.Point;
                    string filepath = DownloadAerial2018(doc, Dimension, Pixels, Point);

                    if (dimensionPrompt.CheckBox3D.IsChecked == true)
                    {
                        //Create a material
                        Material material = GetAerialMaterial(doc, filepath, Dimension);

                        //Create a solid
                        CreateSurfaceElement(doc, material, Dimension, Point);
                    }
                    if (dimensionPrompt.CheckBoxRaster.IsChecked == true)
                    {
                        PlaceImage(filepath, doc, dimensionPrompt.Pixels, dimensionPrompt.Point, dimensionPrompt.Dimension);
                    }
                    transaction.Commit();
                }

            }
            else
            {
                result = Result.Cancelled;
            }
            Utils.SetProjectToMeters(commandData.Application.ActiveUIDocument.Document);
            return result;
        }

        /// <summary>
        /// Downloads a Aerial Image, given the Dimensions and a Point of Origin in Meters
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="dim">The Length/Width of the Photo</param>
        /// <param name="pixels">The number of Pixels</param>
        /// <param name="point">Revit Point in Meters</param>
        /// <returns></returns>
        private static string DownloadAerial2018(Document doc, double dim, double pixels, XYZ point)
        {
            //Get temp path
            string filepath = Path.Combine(System.IO.Path.GetTempPath(), "AerialImage.png");

            //Download image
            string numberOfPixels = Math.Round(pixels).ToString();
            double x = point.X;
            double y = point.Y;

            string minX = (x - (dim / 2)).ToString();
            string minY = (y - (dim / 2)).ToString();
            string maxX = (x + (dim / 2)).ToString();
            string maxY = (y + (dim / 2)).ToString();

            //string requestUrl = string.Format(@"https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?&request=GetMap&VERSION=1.3.0&STYLES=default&layers=2018_ortho25&bbox={0},{1},{2},{3}&width={4}&height={4}&format=image/png&crs=EPSG:28992", minX, minY, maxX, maxY, numberOfPixels);
            string requestUrl = string.Format(@"https://geodata.nationaalgeoregister.nl/luchtfoto/rgb/wms?&request=GetMap&VERSION=1.3.0&STYLES=&layers=Actueel_ortho25&bbox={0},{1},{2},{3}&width={4}&height={4}&format=image/jpeg&crs=EPSG:28992", minX, minY, maxX, maxY, numberOfPixels);
            using (var client = new WebClient())
            {
                client.DownloadFile(requestUrl, filepath);
            }
            return filepath;
        }

        private static Material GetAerialMaterial(Document doc, string filepath, double dim)
        {
            string name = "Aerial";
            Material aerialMat = null;
            AppearanceAssetElement assetAppearance = null;
            using (SubTransaction t = new SubTransaction(doc))
            {
                t.Start();
                IList<Element> materials = new FilteredElementCollector(doc).OfClass(typeof(Material)).ToElements();
                Material originalMaterial = null;
                AppearanceAssetElement originalAppearanceAsset = null;
                foreach (Element element in materials)
                {
                    Material material = (Material)element;
                    if (material.Name == name)
                    {
                        AppearanceAssetElement appearanceAsset = (AppearanceAssetElement)doc.GetElement(material.AppearanceAssetId);
                        if (appearanceAsset.Name == name)
                        {
                            doc.Delete(material.AppearanceAssetId);
                        }
                        doc.Delete(material.Id);
                    }
                    else
                    {
                        AppearanceAssetElement appearance = (AppearanceAssetElement)doc.GetElement(material.AppearanceAssetId);
                        if (appearance != null)
                        {
                            if (appearance.GetRenderingAsset().Name.Contains("Generic"))
                            {
                                originalMaterial = material;
                                originalAppearanceAsset = (AppearanceAssetElement)doc.GetElement(material.AppearanceAssetId);
                            }
                        }
                    }
                }
                aerialMat = originalMaterial.Duplicate(name);
                assetAppearance = originalAppearanceAsset.Duplicate(name);

                aerialMat.AppearanceAssetId = assetAppearance.Id;
                doc.Regenerate();
                t.Commit();
            }

            using (SubTransaction t = new SubTransaction(doc))
            {
                t.Start();
                using (AppearanceAssetEditScope scope = new AppearanceAssetEditScope(doc))
                {
                    Asset editableAsset = scope.Start(assetAppearance.Id);
                    AssetProperty assetProperty = editableAsset.FindByName("generic_diffuse");

                    assetProperty.RemoveConnectedAsset();

                    Asset connectedAsset = assetProperty.GetSingleConnectedAsset() as Asset;
                    if (connectedAsset == null)
                    {
                        assetProperty.AddConnectedAsset("UnifiedBitmap");
                        connectedAsset = assetProperty.GetSingleConnectedAsset();
                    }

                    //Set the Image
                    AssetPropertyString path = connectedAsset.FindByName(UnifiedBitmap.UnifiedbitmapBitmap) as AssetPropertyString;
                    path.Value = filepath;

                    //Unlock Scale and offset
                    AssetPropertyBoolean scaleLock = (AssetPropertyBoolean)connectedAsset.FindByName(UnifiedBitmap.TextureScaleLock);
                    AssetPropertyBoolean offsetLock = (AssetPropertyBoolean)connectedAsset.FindByName(UnifiedBitmap.TextureOffsetLock);
                    offsetLock.Value = false;
                    scaleLock.Value = false;

                    //Set the Scale
                    AssetPropertyDistance scaleX = (AssetPropertyDistance)connectedAsset.FindByName(UnifiedBitmap.TextureRealWorldScaleX);
                    scaleX.Value = (dim * 1000) / 25.4;
                    AssetPropertyDistance scaleY = (AssetPropertyDistance)connectedAsset.FindByName(UnifiedBitmap.TextureRealWorldScaleY);
                    scaleY.Value = (dim * 1000) / 25.4;

                    //Set the Offset
                    AssetPropertyDistance offsetX = (AssetPropertyDistance)connectedAsset.FindByName(UnifiedBitmap.TextureRealWorldOffsetX);
                    offsetX.Value = (-dim / 2 * 1000) / 25.4;
                    AssetPropertyDistance offsetY = (AssetPropertyDistance)connectedAsset.FindByName(UnifiedBitmap.TextureRealWorldOffsetY);
                    offsetY.Value = (-dim / 2 * 1000) / 25.4;

                    //Lock Scale and offset
                    offsetLock.Value = true;
                    scaleLock.Value = true;

                    AssetPropertyBoolean repeat = (AssetPropertyBoolean)connectedAsset.FindByName(UnifiedBitmap.TextureURepeat);
                    repeat.Value = false;
                    repeat = (AssetPropertyBoolean)connectedAsset.FindByName(UnifiedBitmap.TextureVRepeat);
                    repeat.Value = false;

                    scope.Commit(true);
                }
                t.Commit();
                return aerialMat;
            }

        }
        private void CreateSurfaceElement(Document doc, Material material, double dim, XYZ point)
        {
            IList<GeometryObject> solids = new List<GeometryObject>();
            using (SubTransaction transaction = new SubTransaction(doc))
            {
                transaction.Start();
                IList<CurveLoop> profile = new List<CurveLoop>();
                IList<Element> bps = new FilteredElementCollector(doc).OfClass(typeof(BasePoint)).ToElements();

                point = Utils.ToRCS(point, this.ProjectBasePoint);
                //Convert Dimension from Meters to Feet;
                dim /= 2;
                double minX = Utils.MToFeet(point.X - dim);
                double minY = Utils.MToFeet(point.Y - dim);
                double maxX = Utils.MToFeet(point.X + dim);
                double maxY = Utils.MToFeet(point.Y + dim);

                //Create a Curve Container
                CurveLoop curveLoop = CurveLoop.Create(new List<Curve>()
                {
                    Line.CreateBound(
                    new XYZ(minX,minY, 0),
                    new XYZ(maxX, minY, 0)),
                    Line.CreateBound(
                    new XYZ(maxX, minY, 0),
                    new XYZ(maxX, maxY, 0)),
                    Line.CreateBound(
                    new XYZ(maxX, maxY, 0),
                    new XYZ(minX, maxY, 0)),
                    Line.CreateBound(
                    new XYZ(minX, maxY, 0),
                    new XYZ(minX, minY, 0))
                });


                profile.Add(curveLoop);
                ElementId graphicsStyle = new FilteredElementCollector(doc).OfClass(typeof(GraphicsStyle)).ToElementIds().ToList<ElementId>()[0];
                SolidOptions options = new SolidOptions(material.Id, graphicsStyle);
                Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(profile, new XYZ(0, 0, -1), 0.5, options);
                solids.Add(solid);

                DirectShape directShape = DirectShape.CreateElement(doc, Category.GetCategory(doc, BuiltInCategory.OST_GenericModel).Id);
                directShape.AppendShape(solids);
                transaction.Commit();
            }
        }

        private void PlaceImage(string filepath, Document doc, double resolution, XYZ point, double size)
        {
            using (SubTransaction t = new SubTransaction(doc))
            {
                t.Start();
                ImageTypeOptions options = new ImageTypeOptions(filepath, false, ImageTypeSource.Link);
                options.Resolution = resolution;
                ImageType type = ImageType.Create(doc, options);

                point = Utils.XYZMToFeet(Utils.ToRCS(point, this.ProjectBasePoint));
                try
                {
                    ImageInstance image = ImageInstance.Create(doc, doc.ActiveView, type.Id, new ImagePlacementOptions(point, BoxPlacement.Center));
                    image.LockProportions = true;
                    image.Height = size;
                    t.Commit();
                }
                catch (Exception e)
                {
                    Utils.Show("Could not place Raster image:\n\n" + e.Message);
                    t.RollBack();
                }

            }
        }
    }

    



    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ManageAddinPanels : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            adskWindows.RibbonTabCollection tabs = adskWindows.ComponentManager.Ribbon.Tabs;
            List<adskWindows.RibbonTab> Tabs = new List<adskWindows.RibbonTab>();
            foreach (adskWindows.RibbonTab tab in tabs)
            {
                if (tab.Name != null)
                {
                    Tabs.Add(tab);
                }
            }
            ManageTabsWindow ManageTabs = new ManageTabsWindow(Tabs);
            ManageTabs.Show();
            return Result.Succeeded;
        }
    }

    
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignElevations : IExternalCommand
    {
        internal sealed class SelectionFilterElevationTags : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                BuiltInCategory builtInCategory = (BuiltInCategory)GetCategoryIdAsInteger(elem);

                if (builtInCategory == BuiltInCategory.OST_SpotElevations) return true;

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
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            SelectionFilterElevationTags filter = new SelectionFilterElevationTags();
            IList<Reference> selectedElements = selection.PickObjects(ObjectType.Element, filter, "Select Elevation to Align");

            List<SpotDimension> leftTags = new List<SpotDimension>();
            List<SpotDimension> rightTags = new List<SpotDimension>();
            foreach (Reference reference in selectedElements)
            {
                ElementId id = reference.ElementId;
                SpotDimension elevationtag = (SpotDimension)document.GetElement(id);
                bool isLeft = IsLeftTag(elevationtag);
                if (isLeft)
                {
                    leftTags.Add(elevationtag);
                }
                else
                {
                    rightTags.Add(elevationtag);
                }
            }
            AlignTags(document, selection, leftTags, rightTags);

            return Result.Succeeded;
        }

        private static void AlignTags(Document document, Selection selection, List<SpotDimension> leftTags, List<SpotDimension> rightTags)
        {
            using (Transaction transaction = new Transaction(document, "Align Elevation Tags"))
            {
                transaction.Start();
                OverrideGraphicSettings highlight = new OverrideGraphicSettings();
                highlight.SetProjectionLineColor(new Color(255, 153, 51));
                Utils.SketchplaneByView(document.ActiveView);
                transaction.Commit();

                XYZ guide = null;
                if (leftTags.Count > 0)
                {
                    transaction.Start();
                    foreach (SpotDimension tag in leftTags)
                    {
                        document.ActiveView.SetElementOverrides(tag.Id, highlight);
                    }
                    transaction.Commit();
                    transaction.Start();
                    guide = selection.PickPoint("Select Position for Left Elevation Tags");
                    foreach (SpotDimension tag in leftTags)
                    {
                        tag.get_Parameter(BuiltInParameter.SPOT_ELEV_BEND_LEADER).Set(0);
                        tag.LeaderEndPosition = new XYZ(guide.X, guide.Y, tag.LeaderEndPosition.Z);
                        document.ActiveView.SetElementOverrides(tag.Id, new OverrideGraphicSettings());
                    }
                    transaction.Commit();

                }
                if (rightTags.Count > 0)
                {
                    transaction.Start();
                    foreach (SpotDimension tag in rightTags)
                    {
                        document.ActiveView.SetElementOverrides(tag.Id, highlight);
                    }
                    transaction.Commit();
                    transaction.Start();
                    guide = selection.PickPoint("Select Position for Right Elevation Tags");
                    foreach (SpotDimension tag in rightTags)
                    {
                        tag.get_Parameter(BuiltInParameter.SPOT_ELEV_BEND_LEADER).Set(0);
                        tag.LeaderEndPosition = new XYZ(guide.X, guide.Y, tag.LeaderEndPosition.Z);
                        document.ActiveView.SetElementOverrides(tag.Id, new OverrideGraphicSettings());
                    }
                    transaction.Commit();
                }
            }

        }


        private static bool IsLeftTag(SpotDimension elevationTag)
        {
            View view = elevationTag.View;
            XYZ viewDirection = view.ViewDirection;
            double angle = viewDirection.AngleTo(new XYZ(0, 1, 0));
            if (viewDirection.Y < 0)
            {
                angle -= Math.PI;
            }
            XYZ leaderPosition = elevationTag.LeaderEndPosition;
            XYZ origin = elevationTag.Origin;
            Transform rotation = Transform.CreateRotationAtPoint(new XYZ(0, 0, 1), angle, view.Origin);
            leaderPosition = rotation.OfPoint(leaderPosition);
            origin = rotation.OfPoint(origin);
            if (leaderPosition.X < origin.X)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectAllSubcomponents : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            List<ElementId> SelectionList = new List<ElementId>();
            List<Reference> sourceElements = selection.PickObjects(ObjectType.Element, "Select the Source Element").ToList();
            foreach (Reference reference in sourceElements)
            {
                ElementId id = reference.ElementId;
                SelectionList.Add(id);
                Element element = document.GetElement(id);
                if (element.GetType() == typeof(FamilyInstance))
                {
                    FamilyInstance fam = (FamilyInstance)element;
                    if (HasSubComponents(fam))
                    {
                        List<ElementId> subComponents = GetSubComponents(document, fam);
                        SelectionList.AddRange(subComponents);
                    }
                }

            }
            selection.SetElementIds(SelectionList);
            return Result.Succeeded;
        }

        private List<ElementId> GetSubComponents(Document doc, FamilyInstance element)
        {
            List<ElementId> result = new List<ElementId>();
            foreach (ElementId subId in element.GetSubComponentIds())
            {
                FamilyInstance subComponent = (FamilyInstance)doc.GetElement(subId);
                result.Add(subId);
                if (HasSubComponents(subComponent))
                {
                    result.AddRange(GetSubComponents(doc, subComponent));
                }
            }
            return result;
        }

        private bool HasSubComponents(FamilyInstance element)
        {
            if (element.GetSubComponentIds().Count != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }



    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OrganizeViewsOnSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Window RevitWindow = Utils.RevitWindow(commandData);
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View currentView = doc.ActiveView;
            if (currentView.GetType() == typeof(ViewSheet))
            {
                OrganizeViewsWindow organizeViews = new OrganizeViewsWindow();
                ViewSheet sheet = (ViewSheet)currentView;
                organizeViews.DataContext = new OrganizeViewsViewModel(uidoc, sheet);
                organizeViews.Owner = RevitWindow;
                organizeViews.ShowDialog();
                if (!organizeViews.Cancelled)
                {
                    OrganizeViewsViewModel context = (OrganizeViewsViewModel)organizeViews.DataContext;
                    OrganizeViews(doc, context.Views);
                }
            }
            else
            {
                Utils.Show("This Command can only run in a Sheet View");
            }
            return Result.Succeeded;
        }
        private static void OrganizeViews(Document doc, List<OrganizeViewsModel> viewsModels)
        {
            using (Transaction t = new Transaction(doc, "Organize Views"))
            {
                List<string> detailNumbers = new List<string>();
                t.Start();
                foreach (OrganizeViewsModel model in viewsModels)
                {
                    View view = model.View;
                    Viewport viewport = model.ViewPort;
                    view.Name = Guid.NewGuid().ToString();
                    viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).Set(Guid.NewGuid().ToString());
                    view.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(Guid.NewGuid().ToString());
                }
                doc.Regenerate();

                foreach (OrganizeViewsModel model in viewsModels)
                {
                    View view = model.View;
                    Viewport viewport = model.ViewPort;
                    view.Name = model.ViewName;
                    string viewNumber = model.ViewNumber;
                    if (model.ViewNumber == string.Empty)
                    {
                        viewNumber = GenerateViewNumber(detailNumbers);
                        detailNumbers.Add(viewNumber);
                    }
                    viewport.get_Parameter(BuiltInParameter.VIEWPORT_DETAIL_NUMBER).Set(viewNumber);
                    view.get_Parameter(BuiltInParameter.VIEW_DESCRIPTION).Set(model.ViewTitle);
                }
                t.Commit();
            }

        }
        public static string GenerateViewNumber(List<string> list)
        {
            string viewNumber = "X";
            bool exists = list.Contains(viewNumber);
            while (exists)
            {
                viewNumber=viewNumber+"X";
                exists = list.Contains(viewNumber);
            }
            return viewNumber;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BirdToolsHack : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Utils.Show(BirdTagAlignmentTool.entt.ToString());
            //BirdTagAlignmentTool.entt = 1;
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignTagsAndNotes : IExternalCommand
    {
        internal sealed class SelectionFilterElevationTags : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(IndependentTag) || type == typeof(TextNote)) return true;

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
            SelectionFilterElevationTags filter = new SelectionFilterElevationTags();
            IList<Reference> selectedElements = selection.PickObjects(ObjectType.Element, filter, "Select Tags to Align");
            using (Transaction transaction = new Transaction(document, "Align Tags"))
            {
                transaction.Start();
                SketchplaneByView(document.ActiveView);
                XYZ guide = selection.PickPoint("Select a Point");
                foreach (Reference r in selectedElements)
                {
                    Element element = document.GetElement(r.ElementId);
                    if (element.GetType() == typeof(IndependentTag))
                    {
                        IndependentTag tag = (IndependentTag)element;
                        try
                        {
                            tag.TagHeadPosition = new XYZ(guide.X, guide.Y, tag.TagHeadPosition.Z);
                        }
                        catch (Exception e)
                        {
                            Utils.Show(e.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            TextNote tag = (TextNote)element;
                            List<XYZ> elbows = new List<XYZ>();
                            List<XYZ> ends = new List<XYZ>();
                            foreach (Leader leader in tag.GetLeaders())
                            {
                                elbows.Add(leader.Elbow);
                                ends.Add(leader.End);
                            }
                            XYZ currentPosition = tag.Coord;
                            XYZ translation = new XYZ(guide.X, guide.Y, currentPosition.Z) - currentPosition;
                            ElementTransformUtils.MoveElement(document, tag.Id, translation);

                            for (int i = 0; i < tag.LeaderCount; i++)
                            {
                                Leader leader = tag.GetLeaders()[i];
                                leader.End = ends[i];
                                leader.Elbow = elbows[i];
                            }
                        }
                        catch (Exception e)
                        {
                            Utils.Show(e.Message);
                        }

                    }
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }

        private static void SketchplaneByView(View view)
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

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SnapTextNoteLeaderEnd : IExternalCommand
    {
        internal sealed class SelectionFilterElevationTags : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(IndependentTag) || type == typeof(TextNote)) return true;

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
            SelectionFilterElevationTags filter = new SelectionFilterElevationTags();
            Reference selectedElement = selection.PickObject(ObjectType.Element, filter, "Select Tag/TextNotes for which to move leader");
            using (Transaction transaction = new Transaction(document, "Snap Leader End"))
            {
                transaction.Start();
                SketchplaneByView(document.ActiveView);
                XYZ guide = selection.PickPoint("Select a Point");
                Element element = document.GetElement(selectedElement.ElementId);
                if (element.GetType() == typeof(IndependentTag))
                {
                    IndependentTag tag = (IndependentTag)element;
                    try
                    {
#if Revit2021 || Revit2022
                        tag.LeaderEnd = guide;
#else
                        foreach (Reference reference in tag.GetTaggedReferences())
                        {
                            tag.SetLeaderElbow(reference, guide);
                        }
#endif
                    }
                    catch (Exception e)
                    {
                        Utils.Show(e.Message);
                    }
                }
                else
                {
                    try
                    {
                        TextNote tag = (TextNote)element;
                        List<XYZ> elbows = new List<XYZ>();
                        List<XYZ> ends = new List<XYZ>();
                        foreach (Leader leader in tag.GetLeaders())
                        {
                            leader.End = guide;
                        }
                    }
                    catch (Exception e)
                    {
                        Utils.Show(e.Message);
                    }

                }
                
                transaction.Commit();
            }
            return Result.Succeeded;
        }

        private static void SketchplaneByView(View view)
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

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IsolateCurrentWorkset : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            WorksetTable table = document.GetWorksetTable();
            WorksetId activeId = table.GetActiveWorksetId();

            using (Transaction transaction = new Transaction(document, "Create Connections"))
            {
                transaction.Start();
                ElementWorksetFilter worksetFilter = new ElementWorksetFilter(activeId);
                View activeView = document.ActiveView;
                FilteredElementCollector collector = new FilteredElementCollector(document, activeView.Id).WherePasses(worksetFilter);
                List<ElementId> elementsInWorkset = collector.ToElementIds().ToList();
                activeView.IsolateElementsTemporary(elementsInWorkset);
            }
            return Result.Succeeded;
        }

    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ManageParts : IExternalCommand
    {
        internal sealed class SelectionFilterElevationTags : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(Part)) return true;

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
            SelectionFilterElevationTags filter = new SelectionFilterElevationTags();
            Reference reference = selection.PickObject(ObjectType.Element, filter, "Select Part");
            Part part = document.GetElement(reference.ElementId) as Part;
            PartMaker maker = part.PartMaker;
            List<LinkElementId> sourceIds = maker.GetSourceElementIds().ToList();
            string msg = "";
            using (Transaction t = new Transaction(document, "Parts"))
            {
                foreach (LinkElementId linkElementId in sourceIds)
                {
                    ElementId hostId = linkElementId.HostElementId;
                    string hostName = "null";
                    if (hostId != ElementId.InvalidElementId)
                    {
                        hostName = document.GetElement(hostId).Name;
                    }
                    ElementId linkId = linkElementId.LinkedElementId;
                    string linkName = "null";
                    if (linkId != ElementId.InvalidElementId)
                    {
                        linkName = document.GetElement(linkId).Name;
                    }
                    msg += string.Format("Host = {0}, Link = {1}\n", hostName, linkName);
                }
                msg = "Count = " + sourceIds.Count + "\n\n" + msg;
                Utils.Show(msg);
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenTempPath : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string path = Path.GetTempPath();
            Process.Start(path);
            return Result.Succeeded;
        }
    }

    

    

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GetEntities : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;


            IList<Element> collector = new FilteredElementCollector(document).OfClass(typeof(DataStorage)).WhereElementIsNotElementType().ToElements();
            int count = collector.Count;
            string msg = "";
            foreach (DataStorage dataStorage in collector)
            {
                IList<Guid> schemaGuids = dataStorage.GetEntitySchemaGuids();
                foreach (Guid guid in schemaGuids)
                {
                    Schema schema = Schema.Lookup(guid);
                    msg += schema.SchemaName + "\n";
                }

            }

            Utils.Show(string.Format("I found {0} Datastorages in this Model:\n\n{1}", count.ToString(), msg));
            return Result.Succeeded;
        }

    }

    
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectEverythingInView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            Selection selection = uidoc.Selection;
            IList<ElementId> elementIdsInView = new FilteredElementCollector(document, document.ActiveView.Id).WhereElementIsNotElementType().ToElementIds().ToList<ElementId>();
            selection.SetElementIds(elementIdsInView);
            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }

    }
    
    

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StackTags : IExternalCommand
    {
        internal sealed class TagFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if(type == typeof(IndependentTag))
                {
                    return true;
                }

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
            TagFilter tagFilter = new TagFilter();

            IList<Reference> tagsRefs = selection.PickObjects(ObjectType.Element, tagFilter, "Select the Tags to stack");
            IndependentTag TargetTag = document.GetElement(selection.PickObject(ObjectType.Element, tagFilter, "Select the Tag with the Final Position")) as IndependentTag;

            using (Transaction transaction = new Transaction(document, "Stack Tags"))
            {
                transaction.Start();
                foreach(Reference refTag in tagsRefs)
                {
                    IndependentTag tag = document.GetElement(refTag) as IndependentTag;
                    XYZ oldPos = tag.TagHeadPosition;
                    XYZ newPos = TargetTag.TagHeadPosition;
                    XYZ trans = oldPos - newPos;
                    tag.TagHeadPosition = newPos;

#if Revit2022 || Revit2021
                    tag.LeaderElbow -= trans;
#else
                    foreach(Reference reference in tag.GetTaggedReferences())
                    {
                        XYZ pos = tag.GetLeaderElbow(reference);
                        tag.SetLeaderElbow(reference, pos-=trans);
                    }
#endif
                }
                transaction.Commit();
            }

            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class StackTagHeads : IExternalCommand
    {
        internal sealed class TagFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(IndependentTag))
                {
                    return true;
                }

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
            TagFilter tagFilter = new TagFilter();

            IList<Reference> tagsRefs = selection.PickObjects(ObjectType.Element, tagFilter, "Select the Tags to stack");
            IndependentTag TargetTag = document.GetElement(selection.PickObject(ObjectType.Element, tagFilter, "Select the Tag with the Final Position")) as IndependentTag;

            using (Transaction transaction = new Transaction(document, "Stack Tags"))
            {
                transaction.Start();
                foreach (Reference refTag in tagsRefs)
                {
                    IndependentTag tag = document.GetElement(refTag) as IndependentTag;
                    XYZ oldPos = tag.TagHeadPosition;
                    XYZ newPos = TargetTag.TagHeadPosition;
                    XYZ trans = oldPos - newPos;
                    tag.TagHeadPosition = newPos;
                }
                transaction.Commit();
            }

            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }
    }

    
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MatchDimensionOverrides : IExternalCommand
    {
        internal sealed class DimensionFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(Dimension))
                {
                    Dimension dim = elem as Dimension;
                    if (dim.NumberOfSegments == 0)
                    {
                        return true;
                    }
                }

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
            DimensionFilter tagFilter = new DimensionFilter();

            IList<Reference> dimRefs = selection.PickObjects(ObjectType.Element, tagFilter, "Select the Dimensions to Override");
            Dimension sourceDim = document.GetElement(selection.PickObject(ObjectType.Element, tagFilter, "Select the Dimension Source")) as Dimension;

            using (Transaction transaction = new Transaction(document, "Override Dimension"))
            {
                transaction.Start();
                foreach (Reference refTag in dimRefs)
                {
                    Dimension dim = document.GetElement(refTag) as Dimension;
                    dim.Above = sourceDim.Above;
                    dim.Below = sourceDim.Below;
                    dim.Prefix = sourceDim.Prefix;
                    dim.Suffix = sourceDim.Suffix;
                    dim.ValueOverride = sourceDim.ValueOverride;
                }
                transaction.Commit();
            }
            
            
            

            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SetCropView : IExternalCommand
    {
        internal sealed class TagFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(IndependentTag))
                {
                    return true;
                }

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

            PickedBox box = selection.PickBox(PickBoxStyle.Directional, "Make a rectangle where you want you crop region");


            using (Transaction transaction = new Transaction(document, "Stack Tags"))
            {
                transaction.Start();
                View view = document.ActiveView;
                ViewCropRegionShapeManager manager = view.GetCropRegionShapeManager();
                manager.SetCropShape(CreateLoop(box));

                transaction.Commit();
            }

            uidoc.RefreshActiveView();
            return Result.Succeeded;
        }

        private static CurveLoop CreateLoop(PickedBox box)
        {
            XYZ p1 = box.Min;
            XYZ p2 = new XYZ(box.Min.X, box.Min.Y, box.Max.Z);
            XYZ p3 = box.Max;
            XYZ p4 = new XYZ(box.Max.X, box.Max.Y, box.Min.Z);
            CurveLoop result = new CurveLoop();
            result.Append(Line.CreateBound(p1, p2));
            result.Append(Line.CreateBound(p2, p3));
            result.Append(Line.CreateBound(p3, p4));
            result.Append(Line.CreateBound(p4, p1));
            return result;
        }
    }

    

    
    
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class MakeHalftone : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            uidoc.RefreshActiveView();
            View activeView = document.ActiveView;

            IList<Reference> references = Utils.GetCurrentSelection(uidoc, null, "Select Elements to Make Halftone");
            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
            overrideGraphicSettings.SetHalftone(true);
            string errors = "Errors Found:\n\n";
            int errorCount = 0;
            using (Transaction transaction = new Transaction(document, "Make Elements Halftone"))
            {
                transaction.Start();
                foreach(Reference reference in references)
                {
                    try
                    {
                        activeView.SetElementOverrides(reference.ElementId, overrideGraphicSettings);
                    }
                    catch(Exception e)
                    {
                        errors += e.Message + "\n";
                        errorCount++;
                    }
                }
                if (errorCount > 0)
                {
                    Utils.Show(errors);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RemoveOverrides : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            uidoc.RefreshActiveView();
            View activeView = document.ActiveView;

            IList<Reference> references = Utils.GetCurrentSelection(uidoc, null, "Select Elements to Remove Overrides");
            OverrideGraphicSettings overrideGraphicSettings = new OverrideGraphicSettings();
            string errors = "Errors Found:\n\n";
            int errorCount = 0;
            using (Transaction transaction = new Transaction(document, "Remove Element Overrides"))
            {
                transaction.Start();
                foreach (Reference reference in references)
                {
                    try
                    {
                        activeView.SetElementOverrides(reference.ElementId, overrideGraphicSettings);
                    }
                    catch (Exception e)
                    {
                        errors += e.Message + "\n";
                        errorCount++;
                    }
                }
                if (errorCount > 0)
                {
                    Utils.Show(errors);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyOverrides : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            uidoc.RefreshActiveView();
            View activeView = document.ActiveView;

            Reference source = Utils.GetCurrentSelectionSingleElement(uidoc, null, "Select Element to use as Source for Overrides");
            uidoc.Selection.SetElementIds(new Collection<ElementId>());
            IList<Reference> references = Utils.GetCurrentSelection(uidoc, null, "Select Elements to Apply Overrides to");

            OverrideGraphicSettings overrideGraphicSettings = activeView.GetElementOverrides(source.ElementId);
            string errors = "Errors Found:\n\n";
            int errorCount = 0;
            using (Transaction transaction = new Transaction(document, "Apply Element Overrides"))
            {
                transaction.Start();
                foreach (Reference reference in references)
                {
                    try
                    {
                        activeView.SetElementOverrides(reference.ElementId, overrideGraphicSettings);
                    }
                    catch (Exception e)
                    {
                        errors += e.Message + "\n";
                        errorCount++;
                    }
                }
                if (errorCount > 0)
                {
                    Utils.Show(errors);
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class IsolateOverrides : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            uidoc.RefreshActiveView();
            View activeView = document.ActiveView;
            IList<ElementId> ids = new FilteredElementCollector(document, activeView.Id).WhereElementIsNotElementType().ToElementIds().ToList();
            string errors = "Errors Found:\n\n";
            int errorCount = 0;
            List<ElementId> idsWithOverrides = new List<ElementId>();
            using (Transaction transaction = new Transaction(document, "Remove Element Overrides"))
            {
                transaction.Start();
                foreach(ElementId id in ids)
                {
                    try
                    {
                        OverrideGraphicSettings overrides = activeView.GetElementOverrides(id);
                        Dictionary<string, dynamic> overrideDictionary = ParseToDictionary(overrides);
                        Dictionary<string, dynamic> defaultDictionary = ParseToDictionary(new OverrideGraphicSettings());
                        foreach(string key in overrideDictionary.Keys)
                        {
                            dynamic value = overrideDictionary[key];
                            dynamic defaultValue = defaultDictionary[key];
                            if (value.ToString() != defaultValue.ToString())
                            {
                                idsWithOverrides.Add(id);
                                break;
                            }
                        }
                    }
                    catch(Exception e)
                    {
                        errors += e.Message + "\n";
                        errorCount++;
                    }
                    
                }
                if (errorCount > 0)
                {
                    Utils.Show(errors);
                }
                activeView.IsolateElementsTemporary(idsWithOverrides);
                transaction.Commit();
            }
            return Result.Succeeded;
        }

        public static Dictionary<string, dynamic> ParseToDictionary(OverrideGraphicSettings settings)
        {
            Dictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>
            {
                {"CutBackgroundPatternColor", ColorToString(settings.CutBackgroundPatternColor) },
                {"CutForegroundPatternColor", ColorToString(settings.CutForegroundPatternColor) },
                {"SurfaceBackgroundPatternColor", ColorToString(settings.SurfaceBackgroundPatternColor) },
                {"SurfaceForegroundPatternColor", ColorToString(settings.SurfaceForegroundPatternColor) },
                {"CutBackgroundPatternId", settings.CutBackgroundPatternId.ToString() },
                {"CutForegroundPatternId", settings.CutForegroundPatternId.ToString() },
                {"SurfaceBackgroundPatternId", settings.SurfaceBackgroundPatternId.ToString() },
                {"SurfaceForegroundPatternId", settings.SurfaceForegroundPatternId.ToString() },
                {"IsCutBackgroundPatternVisible", settings.IsCutBackgroundPatternVisible.ToString() },
                {"IsCutForegroundPatternVisible", settings.IsCutForegroundPatternVisible.ToString() },
                {"IsSurfaceBackgroundPatternVisible", settings.IsSurfaceBackgroundPatternVisible.ToString() },
                {"IsSurfaceForegroundPatternVisible", settings.IsSurfaceForegroundPatternVisible.ToString() },
                {"CutLineColor", ColorToString(settings.CutLineColor) },
                {"ProjectionLineColor", ColorToString(settings.ProjectionLineColor) },
                {"CutLinePatternId", settings.CutLinePatternId.ToString() },
                {"ProjectionLinePatternId", settings.ProjectionLinePatternId.ToString() },
                {"CutLineWeight", settings.CutLineWeight.ToString() },
                {"ProjectionLineWeight", settings.ProjectionLineWeight.ToString() },
                {"Halftone", settings.Halftone.ToString() },
                {"DetailLevel", settings.DetailLevel.ToString() },
                {"Transparency", settings.Transparency.ToString() }
            };
            return dictionary;
        }
        public static string ColorToString(Color c)
        {
            try
            {
                string result = string.Format("{0},{1},{2}", c.Red.ToString(), c.Green.ToString(), c.Blue.ToString());
                return result;
            }
            catch
            {
                return "Failed To Compose Color";
            }
        }
    }

    

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateWorksetFilters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            FilteredWorksetCollector worksets = new FilteredWorksetCollector(document).OfKind(WorksetKind.UserWorkset);
            IList<ElementId> filters = new FilteredElementCollector(document).OfClass(typeof(ParameterFilterElement)).ToElementIds().ToList();
            BuiltInParameter worksetParam = BuiltInParameter.ELEM_PARTITION_PARAM;
            List<string> filterNames = new List<string>();
            foreach (ElementId filterId in filters)
            {
                ParameterFilterElement filter = (ParameterFilterElement)document.GetElement(filterId);
                filterNames.Add(filter.Name);
            }

            List<ElementId> categoryIds = new List<ElementId>();
            foreach(Category category in document.Settings.Categories)
            {
                if(category.CategoryType == CategoryType.Model)
                {
                    categoryIds.Add(category.Id);
                }
            }
            string outputMessage = "";
            using (Transaction t = new Transaction(document, "Create Workset Filters"))
            {
                t.Start();
                foreach(Workset workset in worksets)
                {
                    string worksetName = workset.Name;
                    if (!filterNames.Contains(worksetName))
                    {
                        FilterRule rule = ParameterFilterRuleFactory.CreateBeginsWithRule(new ElementId(worksetParam), worksetName,true);
                        ElementFilter elementFilter = new ElementParameterFilter(rule);
                        ParameterFilterElement newFilter = ParameterFilterElement.Create(document, worksetName, categoryIds, elementFilter);
                        outputMessage += workset.Name + " - Created Filter\n";
                    }
                    else
                    {
                        outputMessage += workset.Name + " - Filter Already Exists!\n";
                    }
                }
                Utils.Show(outputMessage);
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateSheetFilters : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;

            IList<Element> sheets = new FilteredElementCollector(document).OfClass(typeof(ViewSheet)).ToElements().ToList();
            IList<ElementId> filters = new FilteredElementCollector(document).OfClass(typeof(ParameterFilterElement)).ToElementIds().ToList();
            BuiltInParameter sheetNumberParameter = BuiltInParameter.VIEWPORT_SHEET_NUMBER;

            List<string> filterNames = new List<string>();
            foreach (ElementId filterId in filters)
            {
                ParameterFilterElement filter = (ParameterFilterElement)document.GetElement(filterId);
                filterNames.Add(filter.Name);
            }

            List<ElementId> categoryIds = new List<ElementId>()
            {
                document.Settings.Categories.get_Item(BuiltInCategory.OST_Callouts).Id,
                document.Settings.Categories.get_Item(BuiltInCategory.OST_Sections).Id
            };

            string outputMessage = "";
            using (Transaction t = new Transaction(document, "Create Sheets Filters"))
            {
                t.Start();
                foreach (ViewSheet sheet in sheets)
                {
                    string filterName = string.Format("{0} - {1}", sheet.SheetNumber, sheet.Name);
                    if (!filterNames.Contains(filterName))
                    {
                        FilterRule rule = ParameterFilterRuleFactory.CreateNotEqualsRule(new ElementId(sheetNumberParameter), sheet.SheetNumber, true);
                        ElementFilter elementFilter = new ElementParameterFilter(rule);
                        ParameterFilterElement newFilter = ParameterFilterElement.Create(document, filterName, categoryIds, elementFilter);
                        outputMessage += filterName + " - Created Filter\n";
                    }
                    else
                    {
                        outputMessage += filterName + " - Filter Already Exists!\n";
                    }
                }
                Utils.Show(outputMessage);
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FlipHand : IExternalCommand
    {
        internal sealed class CanFlipHandFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(FamilyInstance))
                {
                    FamilyInstance familyInstance = (FamilyInstance)elem;
                    if (familyInstance.CanFlipHand)
                    {
                        return true;
                    }
                }

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
            CanFlipHandFilter filter = new CanFlipHandFilter();
            IList<Reference> references = Utils.GetCurrentSelection(uidoc, filter, "Select FamilyInstance to flip");
            using (Transaction transaction = new Transaction(uidoc.Document, "Flip FamilyInstance"))
            {
                transaction.Start();
                foreach (Reference reference in references)
                {
                    FamilyInstance instance = (FamilyInstance)uidoc.Document.GetElement(reference.ElementId);
                    instance.flipHand();
                }
                transaction.Commit();
                return Result.Succeeded;
            }
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class FlipFace : IExternalCommand
    {
        internal sealed class CanFlipHandFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(FamilyInstance))
                {
                    FamilyInstance familyInstance = (FamilyInstance)elem;
                    if (familyInstance.CanFlipFacing)
                    {
                        return true;
                    }
                }

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
            CanFlipHandFilter filter = new CanFlipHandFilter();
            IList<Reference> references = Utils.GetCurrentSelection(uidoc, filter, "Select FamilyInstance to flip");
            using (Transaction transaction = new Transaction(uidoc.Document, "Flip FamilyInstance"))
            {
                transaction.Start();
                foreach (Reference reference in references)
                {
                    FamilyInstance instance = (FamilyInstance)uidoc.Document.GetElement(reference.ElementId);
                    instance.flipFacing();
                }
                transaction.Commit();
                return Result.Succeeded;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RotateInstance : IExternalCommand
    {
        internal sealed class CanFlipHandFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem is null) return false;

                Type type = elem.GetType();

                if (type == typeof(FamilyInstance))
                {
                    FamilyInstance familyInstance = (FamilyInstance)elem;
                    if (familyInstance.CanRotate)
                    {
                        return true;
                    }
                }

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
            CanFlipHandFilter filter = new CanFlipHandFilter();
            IList<Reference> references = Utils.GetCurrentSelection(uidoc, filter, "Select FamilyInstance to flip");
            using (Transaction transaction = new Transaction(uidoc.Document, "Flip FamilyInstance"))
            {
                transaction.Start();
                foreach (Reference reference in references)
                {
                    FamilyInstance instance = (FamilyInstance)uidoc.Document.GetElement(reference.ElementId);
                    instance.rotate();
                }
                transaction.Commit();
                return Result.Succeeded;
            }
        }
    }

   
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class TurnOffRebarCategoryInAllViews : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            View currentView = doc.ActiveView;
            if (currentView.ViewType != ViewType.ThreeD)
            {
                Utils.Show("This Command Can only be used in a 3D View");
            }
            IList<Element> views = new FilteredElementCollector(doc).OfClass(typeof(View)).ToElements();
            using (Transaction transaction = new Transaction(doc, "Turn off Rebars in View"))
            {
                string errors = "";
                transaction.Start();
                foreach(View view in views)
                {
                    try
                    {
                        view.SetCategoryHidden(new ElementId(BuiltInCategory.OST_Rebar), true);
                    }
                    catch(Exception e)
                    {
                        errors += e.Message + "\n";
                    }
                }
                Utils.Show(errors);
                transaction.Commit();
            }
            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportFamilies : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            IList<Element> families = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToElements();
            Dictionary<string, dynamic> famDict = new Dictionary<string, dynamic>();
            foreach(Family family in families)
            {
                famDict.Add(family.Name+" - " +family.Id.ToString(), family);
            }
            GenericDropdownWindow window = new GenericDropdownWindow("Select Families to Export", "Select Families to Export", famDict, Utils.RevitWindow(commandData), true);
            window.ShowDialog();
            if (!window.Cancelled)
            {
                List<dynamic> selectedFamilies = window.SelectedItems;
                Forms.FolderBrowserDialog folderSelection = new Forms.FolderBrowserDialog();
                if(folderSelection.ShowDialog() == Forms.DialogResult.OK)
                {
                    string msg = "";
                    string folderPath = folderSelection.SelectedPath;
                    foreach (Family fam in selectedFamilies)
                    {
                        Document familyDocument = doc.EditFamily(fam);
                        string fileName = Path.Combine(folderPath, fam.Name + ".rfa");
                        try
                        {

                            familyDocument.SaveAs(fileName, new SaveAsOptions() { OverwriteExistingFile = true, Compact = true });
                        }
                        catch(Exception e)
                        {
                            msg += e.Message + "\n";
                        }

                        familyDocument.Close();

                    }
                    Cleanup(folderPath);
                    if (msg != "")
                    {
                        Utils.Show(msg);
                    }
                }
                
            }
            return Result.Succeeded;
        }

        public void Cleanup(string FolderPath)
        {
            string[] files = Directory.GetFiles(FolderPath);
            foreach(string file in files)
            {
                if (Path.GetFileName(file).Contains(".00"))
                {
                    File.Delete(file);
                }
            }
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyViewsBewtweenModels : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.DB.Document doc = commandData.Application.ActiveUIDocument.Document;
            Document targetDocument = null;
            foreach(Document document in commandData.Application.Application.Documents)
            {
                if(document.GetHashCode() != doc.GetHashCode())
                {
                    targetDocument = document;
                    break;
                }
            }
            Dictionary<string, dynamic> views = GetViewsInModel(doc);
            GenericDropdownWindow window = new GenericDropdownWindow("Select Views", "Select Views", views, Utils.RevitWindow(commandData), false);
            window.ShowDialog();
            if (!window.Cancelled)
            {
                using (Transaction transaction = new Transaction(targetDocument, "Copy Views between Documents"))
                {
                    transaction.Start();
                    ICollection<ElementId> viewIds = new Collection<ElementId>();
                    foreach(View view in window.SelectedItems)
                    {
                        viewIds.Add(view.Id);
                    }
                    ElementTransformUtils.CopyElements(doc, viewIds, targetDocument, Transform.CreateTranslation(new XYZ(0, 0, 0)), new CopyPasteOptions());
                    transaction.Commit();
                }
            }
            return Result.Succeeded;
        }
        public static Dictionary<string, dynamic> GetViewsInModel(Autodesk.Revit.DB.Document doc)
        {
            IList<Element> collector = new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().ToElements();
            Dictionary<string, dynamic> filteredViews = new Dictionary<string, dynamic>();
            foreach (View view in collector)
            {
                if (!view.IsTemplate)
                {
                    try
                    {

                        filteredViews.Add(view.Name, view);
                    }
                    catch { }
                }
            }
            return filteredViews;
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyComments : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            startSelecting:

            using (Transaction trans = new Transaction(doc, "Copy Comments"))
            {
                trans.Start();
                try
                {
                    Element element = doc.GetElement(selection.PickObject(ObjectType.Element, "Source"));
                    Parameter source = element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);

                    Element targetelement = doc.GetElement(selection.PickObject(ObjectType.Element, "Target"));
                    Parameter target = targetelement.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                    target.Set(source.AsString());
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }
                trans.Commit();
            }
            goto startSelecting;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SendViewportToTheBack : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            using (Transaction trans = new Transaction(doc, "Copy Comments"))
            {
                trans.Start();
                try
                {
                    Element element = doc.GetElement(selection.PickObject(ObjectType.Element, "Viewport"));
                    ViewSheet sheet = doc.ActiveView as ViewSheet;
                    foreach(ElementId id in sheet.GetAllViewports())
                    {
                        if (id.ToString() != element.Id.ToString())
                        {
                            Viewport vp = doc.GetElement(id) as Viewport;
                            ElementId viewId = vp.ViewId;
                            XYZ point = vp.GetBoxCenter();
                            doc.Delete(id);
                            Viewport.Create(doc, sheet.Id, viewId, point);
                        }
                    }
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


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ApplyCurrentWorksetsToCurrentViewTemplate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;

            using (Transaction trans = new Transaction(doc, "Apply Workset Overrides"))
            {
                trans.Start();
                View currentView = doc.ActiveView;
                ElementId viewTemplateId = currentView.ViewTemplateId;
                if (viewTemplateId != ElementId.InvalidElementId)
                {
                    View viewTemplate = doc.GetElement(viewTemplateId) as View;
                    ICollection<WorksetId> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksetIds();
                    foreach(WorksetId worksetId in worksets)
                    {
                        WorksetVisibility visibility = currentView.GetWorksetVisibility(worksetId);
                        viewTemplate.SetWorksetVisibility(worksetId, visibility);
                    }
                }

                trans.Commit();
            }
            return Result.Succeeded;
        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyWorksetOverridesBetweenViewports : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            Reference sourceReference = selection.PickObject(ObjectType.Element, "Select Viewport");
            IList<Reference> references = selection.PickObjects(ObjectType.Element, "Select Viewports");
            using (Transaction trans = new Transaction(doc, "Copy workset Overrides"))
            {
                trans.Start();
                Viewport sourceViewport = (Viewport)doc.GetElement(sourceReference);
                View sourceView = (View)doc.GetElement(sourceViewport.ViewId);

                foreach (Reference targetReference in references)
                {
                    Viewport targetViewport = (Viewport)doc.GetElement(targetReference);
                    View targetView = (View)doc.GetElement(targetViewport.ViewId);
                    ICollection<WorksetId> worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksetIds();
                    foreach (WorksetId worksetId in worksets)
                    {
                        WorksetVisibility visibility = sourceView.GetWorksetVisibility(worksetId);
                        targetView.SetWorksetVisibility(worksetId, visibility);
                    }
                }

                trans.Commit();
            }
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CopyTagText : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            Reference reference = selection.PickObject(ObjectType.Element, new TagFilter(), "Select Tag to copy text");
            IndependentTag tagElement = (IndependentTag)doc.GetElement(reference);
            string tagText = tagElement.TagText;
            Clipboard.SetText(tagText);
            return Result.Succeeded;
        }

        internal class TagFilter : ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                if (elem.GetType() == typeof(IndependentTag))
                {
                    return true;
                }
                else { return false; }
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                throw new NotImplementedException();
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SelectRebarWithSameNumber : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            Reference reference = selection.PickObject(ObjectType.Element, "Select Rebar Element");
            Element element = doc.GetElement(reference);
            Parameter p = element.LookupParameter("2D_Rebar_Number");
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_DetailComponents);
            List<ElementId> ids = new List<ElementId>();
            foreach (Element e in collector.ToElements())
            {
                Parameter parameter = e.LookupParameter("2D_Rebar_Number");
                if (parameter != null)
                {
                    if (parameter.AsString() == p.AsString())
                    {
                        ids.Add(e.Id);
                    }
                }

            }
            selection.SetElementIds(ids);

            return Result.Succeeded;
        }

    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AlignStructuralFramings : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            List<BuiltInCategory> category = new List<BuiltInCategory>() { BuiltInCategory.OST_StructuralFraming };

            Reference reference = selection.PickObject(ObjectType.Element, new Utils.CategorySelectionFilter(doc, category), "Select Guide Structural Framing");
            IList<Reference> references = selection.PickObjects(ObjectType.Element, new Utils.CategorySelectionFilter(doc, category), "Select Structural Framings to Align");
            
            using(Transaction transaction = new Transaction(doc, "Align Structural Framings"))
            {
                transaction.Start();
                FamilyInstance guideBeam = (FamilyInstance)doc.GetElement(reference);
                XYZ guideDirection = GetFamilyDirection(guideBeam);
                foreach (Reference beamRef in references)
                {
                    FamilyInstance targetBeam = (FamilyInstance)doc.GetElement(beamRef);
                    XYZ direction = GetFamilyDirection(targetBeam);
                    double dotProduct = direction.DotProduct(guideDirection);
                    if (dotProduct < 0)
                    {
                        LocationCurve curve = (LocationCurve)targetBeam.Location;
                        curve.Curve = curve.Curve.CreateReversed();
                    }
                }
                transaction.Commit();

            }

            XYZ v1 = new XYZ(1, 0, 0);
            XYZ v2 = new XYZ(-1, 0, 0);
            double dot = v1.DotProduct(v2);
            if (dot < 0)
            {

            }

            return Result.Succeeded;
        }

        private static XYZ GetFamilyDirection(FamilyInstance family)
        {
            LocationCurve location = (LocationCurve)family.Location;
            Line directionLine = Line.CreateBound(location.Curve.GetEndPoint(0), location.Curve.GetEndPoint(1));
            XYZ guideDirection = directionLine.Direction.Normalize();

            return guideDirection;
        }

    }



    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class _PreviewFamily : IExternalCommand
    {
        List<ElementId> _added_element_ids = new List<ElementId>();
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;
            Autodesk.Revit.DB.Document doc = uidoc.Document;
            Selection selection = uidoc.Selection;
            FilteredElementCollector collector
      = new FilteredElementCollector(doc);

            collector.OfCategory(BuiltInCategory.OST_Site);
            collector.OfClass(typeof(FamilySymbol));

            FamilySymbol symbol = collector.FirstElement() as FamilySymbol;

            _added_element_ids.Clear();

            app.DocumentChanged
              += new EventHandler<DocumentChangedEventArgs>(
                OnDocumentChanged);
            try
            {

                uidoc.PromptForFamilyInstancePlacement(symbol);
            }
            catch (OperationCanceledException)
            {
                
            }
            app.DocumentChanged
              -= new EventHandler<DocumentChangedEventArgs>(
                OnDocumentChanged);

            int n = _added_element_ids.Count;

            TaskDialog.Show(
              "Place Family Instance",
              string.Format(
                "{0} element{1} added.", n,
                ((1 == n) ? "" : "s")));

            return Result.Succeeded;
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _added_element_ids.AddRange(e.GetAddedElementIds());
        }

    }

   

}

