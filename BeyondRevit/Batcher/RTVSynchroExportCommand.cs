using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forms = System.Windows.Forms;
using System.Xml.Linq;
using System.Windows.Controls;

namespace BeyondRevit.Batcher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RTVSynchroExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string[] batchFiles = SynchroUtils.OpenRevitBatchFiles();
            if (batchFiles == null)
            {
                return Result.Cancelled;
            }
            Forms.FolderBrowserDialog folderBrowser = new Forms.FolderBrowserDialog()
            {
                Description = "Select Where to Store the SPF Files"
            };
            if (folderBrowser.ShowDialog() == Forms.DialogResult.OK)
            {
                string folderPath = folderBrowser.SelectedPath;
                foreach (string batchFile in batchFiles)
                {
                    Dictionary<string, string> info = SynchroUtils.ReadBatchFile(batchFile);
                    ModelPath path = ModelPathUtils.ConvertCloudGUIDsToCloudPath(ModelPathUtils.CloudRegionEMEA, Guid.Parse(info["ProjectGuid"]), Guid.Parse(info["ModelGuid"]));
                    OpenOptions options = new OpenOptions() { DetachFromCentralOption = DetachFromCentralOption.DoNotDetach };
                    Document batchDocument = commandData.Application.Application.OpenDocumentFile(path, options);

                    try
                    {
                        View view = new FilteredElementCollector(batchDocument).OfClass(typeof(View)).WhereElementIsNotElementType().Where(v => v.Name == info["View"]).First() as View;

                        using (Transaction transaction = new Transaction(batchDocument, "Create 4D Parameter"))
                        {
                            transaction.Start();
                            SynchroUtils.Create4D_CodeParameter(batchDocument);
                            foreach (Element element in new FilteredElementCollector(batchDocument, view.Id).WhereElementIsNotElementType().ToElements())
                            {
                                Parameter parameterWV = element.LookupParameter("TriAX_Locatie_03");
                                Parameter parameterObject = element.LookupParameter("TriAX_Objectnummer");
                                Parameter parameterType = element.LookupParameter("TriAX_Objecttype");
                                if (parameterWV != null && parameterObject != null && parameterType != null)
                                {
                                    string value = string.Format("{0}_{1}_{2}",
                                    element.LookupParameter("TriAX_Locatie_03").AsString(),
                                    element.LookupParameter("TriAX_Objectnummer").AsString(),
                                    element.LookupParameter("TriAX_Objecttype").AsString());
                                    element.LookupParameter("4D_Code").Set(value);
                                }

                            }
                            transaction.Commit();
                        }

                        if (view != null)
                        {
                            string fileName = string.Format("{0}.spx", batchDocument.Title.Replace("BMO", "EXP"));
                            string spfFilePath = System.IO.Path.Combine(folderPath, fileName);
                            SYN.DataGenerator generator = new SYN.DataGenerator(commandData.Application.Application, batchDocument, view);
                            Synchro.Material material = new Synchro.Material();
                            material.setDiffuse(new Synchro.ColorRGB(0.0, 0.0, 1.0));
                            material.setTransmission(new Synchro.ColorRGB(0.75, 0.75, 0.75));
                            generator.SetWorkspaceMaterial(material);
                            generator.SetBuildResourceTreeOnImport(true);
                            generator.SaveSPFile(spfFilePath);
                            Utils.ShowInfoBalloon(fileName + " exported Succesfully!");
                        }
                    }
                    catch (Exception)
                    {
                        Utils.Show(string.Format("View {0} not Found in Model {1}", info["View"], batchDocument.Title));
                    }
                    batchDocument.Close(false);
                }
            }
            Utils.Show("Exporting Done!");
            return Result.Succeeded;
        }


    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RTVSynchroExportTest : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            string[] batchFiles = SynchroUtils.OpenRevitBatchFiles();
            if (batchFiles == null)
            {
                return Result.Cancelled;
            }
            Forms.FolderBrowserDialog folderBrowser = new Forms.FolderBrowserDialog()
            {
                Description = "Select Where to Store the SPF Files"
            };
            if (folderBrowser.ShowDialog() == Forms.DialogResult.OK)
            {
                string folderPath = folderBrowser.SelectedPath;
                foreach (string batchFile in batchFiles)
                {
                    Dictionary<string, string> info = SynchroUtils.ReadBatchFile(batchFile);
                    Utils.Show(info["View"]);
                }
            }
            Utils.Show("Exporting Done!");
            return Result.Succeeded;
        }


    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Test_Create4DParameter : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            using (Transaction transaction = new Transaction(doc, "Create 4D Parameter"))
            {
                transaction.Start();
                Create4DParameter(doc);
                foreach (Element element in new FilteredElementCollector(doc, doc.ActiveView.Id).WhereElementIsNotElementType().ToElements())
                {
                    Parameter parameterWV = element.LookupParameter("TriAX_Locatie_03");
                    Parameter parameterObject = element.LookupParameter("TriAX_Objectnummer");
                    Parameter parameterType = element.LookupParameter("TriAX_Objecttype");
                    if(parameterWV != null && parameterObject !=null && parameterType != null)
                    {
                        string value = string.Format("{0}_{1}_{2}",
                        element.LookupParameter("TriAX_Locatie_03").AsString(),
                        element.LookupParameter("TriAX_Objectnummer").AsString(),
                        element.LookupParameter("TriAX_Objecttype").AsString());
                        element.LookupParameter("4D_Code").Set(value);
                    }
                    
                }
                transaction.Commit();
            }
            return Result.Succeeded;
        }

        public static void Create4DParameter(Document document)
        {
            
            List<string> invalidCategories = new List<string>()
            {
                "Point Clouds",
                "Pipe Segments",
                "Electrical Spare/Space Circuits",
                "Routing Preferences",
                "Coordination Model",
                "Raster Images",
                "Analysis Display Style",
                "Analysis Results",
                "Imports in Families",
                "Masking Region",
                "Filled region",
                //"Structural Rebar Couplers",
                //"Structural Connections",
                //"Structural Fabric Areas",
                //"Structural Fabric Reinforcement",
                "Rebar Shape",
                //"Structural Path Reinforcement",
                //"Structural Area Reinforcement",
                //"Structural Rebar",
                "MEP Fabrication Containment",
                "MEP Fabrication Pipework",
                "MEP Fabrication Hangers",
                "MEP Fabrication Ductwork",
                "Pipe Placeholders",
                "Duct Placeholders",
                "Cable Tray Runs",
                "Conduit Runs",
                "Conduits",
                "Cable Trays",
                "Conduit Fittings",
                "Cable Tray Fittings",
                "Duct Linings",
                "Duct Insulations",
                "Pipe Insulations",
                "HVAC Zones",
                "Switch System",
                "Sprinklers",
                "Lighting Devices",
                "Fire Alarm Devices",
                "Data Devices",
                "Communication Devices",
                "Security Devices",
                "Nurse Call Devices",
                "Telephone Devices",
                "Pipe Accessories",
                "Flex Pipes",
                "Pipe Fittings",
                "Pipes",
                "Piping Systems",
                "Wires",
                "Electrical Circuits",
                "Flex Ducts",
                "Duct Accessories",
                "Duct Systems",
                "Air Terminals",
                "Duct Fittings",
                "Ducts",
                "Structural Tendons",
                "Expansion Joints",
                "Vibration Management",
                //"Bridge Framing",
                //"Bearings",
                //"Bridge Decks",
                //"Bridge Cables",
                //"Piers",
                //"Abutments",
                "Spaces",
                "Mass",
                "Areas",
                "Project Information",
                "Sheets",
                //"Detail Items",
                "Entourage",
                "Planting",
                //"Structural Stiffeners",
                //"RVT Links",
                "Specialty Equipment",
                //"Topography",
                //"Structural Trusses",
                //"Structural Columns",
                //"Structural Beam Systems",
                //"Structural Framing",
                //"Structural Foundations",
                //"Site",
                "Roads",
                "Parking",
                "Plumbing Fixtures",
                "Mechanical Equipment",
                "Lighting Fixtures",
                "Furniture Systems",
                "Electrical Fixtures",
                "Signage",
                "Audio Visual Devices",
                "Vertical Circulation",
                "Fire Protection",
                "Medical Equipment",
                "Food Service Equipment",
                "Electrical Equipment",
                "Temporary Structures",
                "Hardscape",
                "Zone Equipment",
                "Water Loops",
                "Air Systems",
                //"Casework",
                "Shaft Openings",
                "Mechanical Equipment Sets",
                "Materials",
                "Curtain Systems",
                //"Parts",
                "Ramps",
                "Curtain Grids",
                "Curtain Wall Mullions",
                "Curtain Panels",
                "Rooms",
                //"Generic Models",
                //"Railings",
                //"Stairs",
                //"Columns",
                //"Furniture",
                "Lines",
                //"Ceilings",
                //"Roofs",
                //"Floors",
                //"Doors",
                //"Windows",
                //"Walls",
                "KNM_A10_V_XR2_KWM_XX.dwg",
                "KNM_SNC_V_XR2_KWM_ZZ.dwg",
                "KNM_TOT_N_XR2_VTI_WKS projectering.dwg",
                "KNM_TOT_B_XR2_DTM_XX_TOTAAL.dwg",
                "KNM_TOT_B_XR2_RLM_XX_RIOLERING.dwg",
                "KNM_TOT_B_XR2_KLM_XX_ACTUEEL.dwg",
                "KNM_TOT_N_BMO_RLM_XX_HWA.dwg",
                "KNM_KNP_N_XR2_GKE_XX_definitieve keringen.dwg",
                "KNM_KNP_T_XR2_GKE_XX_tijdelijke keringen.dwg",
                "TRiAX_TOT_Geluidsschermen_B_KWM_gld_3dsolids.dwg",
            };
            CategorySet modelCats = new CategorySet();
            Categories categories = document.Settings.Categories;
            foreach (Category category in categories)
            {
                if (category.CategoryType == CategoryType.Model && !invalidCategories.Contains(category.Name) && !category.Name.Contains(".dwg"))
                {
                    modelCats.Insert(category);
                }
            }
            Utils.RawCreateProjectParameter(document.Application,document, "4D_Code", true, modelCats, BuiltInParameterGroup.PG_DATA, true);
        }
    }

    public class SynchroUtils
    {

        public static void Create4D_CodeParameter(Document document)
        {

            List<string> invalidCategories = new List<string>()
            {
                "Point Clouds",
                "Pipe Segments",
                "Electrical Spare/Space Circuits",
                "Routing Preferences",
                "Coordination Model",
                "Raster Images",
                "Analysis Display Style",
                "Analysis Results",
                "Imports in Families",
                "Masking Region",
                "Filled region",
                //"Structural Rebar Couplers",
                //"Structural Connections",
                //"Structural Fabric Areas",
                //"Structural Fabric Reinforcement",
                "Rebar Shape",
                //"Structural Path Reinforcement",
                //"Structural Area Reinforcement",
                //"Structural Rebar",
                "MEP Fabrication Containment",
                "MEP Fabrication Pipework",
                "MEP Fabrication Hangers",
                "MEP Fabrication Ductwork",
                "Pipe Placeholders",
                "Duct Placeholders",
                "Cable Tray Runs",
                "Conduit Runs",
                "Conduits",
                "Cable Trays",
                "Conduit Fittings",
                "Cable Tray Fittings",
                "Duct Linings",
                "Duct Insulations",
                "Pipe Insulations",
                "HVAC Zones",
                "Switch System",
                "Sprinklers",
                "Lighting Devices",
                "Fire Alarm Devices",
                "Data Devices",
                "Communication Devices",
                "Security Devices",
                "Nurse Call Devices",
                "Telephone Devices",
                "Pipe Accessories",
                "Flex Pipes",
                "Pipe Fittings",
                "Pipes",
                "Piping Systems",
                "Wires",
                "Electrical Circuits",
                "Flex Ducts",
                "Duct Accessories",
                "Duct Systems",
                "Air Terminals",
                "Duct Fittings",
                "Ducts",
                "Structural Tendons",
                "Expansion Joints",
                "Vibration Management",
                //"Bridge Framing",
                //"Bearings",
                //"Bridge Decks",
                //"Bridge Cables",
                //"Piers",
                //"Abutments",
                "Spaces",
                "Mass",
                "Areas",
                "Project Information",
                "Sheets",
                //"Detail Items",
                "Entourage",
                "Planting",
                //"Structural Stiffeners",
                //"RVT Links",
                "Specialty Equipment",
                //"Topography",
                //"Structural Trusses",
                //"Structural Columns",
                //"Structural Beam Systems",
                //"Structural Framing",
                //"Structural Foundations",
                //"Site",
                "Roads",
                "Parking",
                "Plumbing Fixtures",
                "Mechanical Equipment",
                "Lighting Fixtures",
                "Furniture Systems",
                "Electrical Fixtures",
                "Signage",
                "Audio Visual Devices",
                "Vertical Circulation",
                "Fire Protection",
                "Medical Equipment",
                "Food Service Equipment",
                "Electrical Equipment",
                "Temporary Structures",
                "Hardscape",
                "Zone Equipment",
                "Water Loops",
                "Air Systems",
                //"Casework",
                "Shaft Openings",
                "Mechanical Equipment Sets",
                "Materials",
                "Curtain Systems",
                //"Parts",
                "Ramps",
                "Curtain Grids",
                "Curtain Wall Mullions",
                "Curtain Panels",
                "Rooms",
                //"Generic Models",
                //"Railings",
                //"Stairs",
                //"Columns",
                //"Furniture",
                "Lines",
                //"Ceilings",
                //"Roofs",
                //"Floors",
                //"Doors",
                //"Windows",
                //"Walls",
                "KNM_A10_V_XR2_KWM_XX.dwg",
                "KNM_SNC_V_XR2_KWM_ZZ.dwg",
                "KNM_TOT_N_XR2_VTI_WKS projectering.dwg",
                "KNM_TOT_B_XR2_DTM_XX_TOTAAL.dwg",
                "KNM_TOT_B_XR2_RLM_XX_RIOLERING.dwg",
                "KNM_TOT_B_XR2_KLM_XX_ACTUEEL.dwg",
                "KNM_TOT_N_BMO_RLM_XX_HWA.dwg",
                "KNM_KNP_N_XR2_GKE_XX_definitieve keringen.dwg",
                "KNM_KNP_T_XR2_GKE_XX_tijdelijke keringen.dwg",
                "TRiAX_TOT_Geluidsschermen_B_KWM_gld_3dsolids.dwg",
            };
            CategorySet modelCats = new CategorySet();
            Categories categories = document.Settings.Categories;
            foreach (Category category in categories)
            {
                if (category.CategoryType == CategoryType.Model && !invalidCategories.Contains(category.Name) && !category.Name.Contains(".dwg"))
                {
                    modelCats.Insert(category);
                }
            }
            Utils.RawCreateProjectParameter(document.Application, document, "4D_Code", true, modelCats, BuiltInParameterGroup.PG_DATA, true);
        }
        public static string[] OpenRevitBatchFiles()
        {
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog()
            {
                Filter = "Revit Batch Files (*.rbxml)|*.rbxml",
                Multiselect = true
            };
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                return dialog.FileNames;
            }
            else
            {
                return null;
            }
        }
        public static Dictionary<string, string> ReadBatchFile(string filepath)
        {
            Dictionary<string, string> result = new Dictionary<string, string>()
            {
                { "ProjectGuid", "" },
                { "ModelGuid", "" },
                { "View", "" },
            };
            XDocument batch = XDocument.Load(filepath);
            try
            {

                IEnumerator<XElement> project = batch.Descendants("Batch").Descendants("RevitModel").Descendants("WorksharingProjectGUID").GetEnumerator();

                while (project.MoveNext())
                {
                    result["ProjectGuid"] = project.Current.Value;
                }
            }
            catch
            {
                Utils.Show("Could not Find Project Guid");
            }
            try
            {
                IEnumerator<XElement> model = batch.Descendants("Batch").Descendants("RevitModel").Descendants("WorksharingCentralGUID").GetEnumerator();
                while (model.MoveNext())
                {
                    result["ModelGuid"] = model.Current.Value;
                }

            }
            catch
            {
                Utils.Show("Could not Find Model Guid");
            }

            try
            {
                IEnumerator<XElement> view = batch.Descendants("Batch").Descendants("Views").Descendants("View").GetEnumerator();
                while (view.MoveNext())
                {
                    if(view.Current.Descendants("ViewType").ToList().First().Value == "3D Views")
                    {
                        result["View"] = view.Current.Descendants("ViewName").ToList().First().Value;
                    }
                }
            }
            catch
            {
                Utils.Show("Could not Find View");
            }
            return result;
        }
    }
}
