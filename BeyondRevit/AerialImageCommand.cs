using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeyondRevit.UI;
using System.Net;
using Autodesk.Revit.DB.Visual;
using System.IO;

namespace BeyondRevit
{
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
}
