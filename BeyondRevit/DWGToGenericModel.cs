using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI.Selection;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;
using BeyondRevit.UI;

namespace BeyondRevit.Commands
{

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DWGToGenericModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Access the Autodesk Revit application object
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;

            // Access the active UI document and the current Revit document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Create a selection object to interact with the user's selections in the active view
            Selection selection = uidoc.Selection;

            // Use a utility function to get a reference to a single selected DWG element in the document
            // It displays a "Select DWG" prompt to the user and filters for ImportInstance elements (DWG files)
            IList<Reference> selectedDWGRefs = Utils.GetCurrentSelection(uidoc, new Utils.TypeSelectionFilter(doc, new List<Type>() { typeof(ImportInstance) }), "Select DWG");
            foreach(Reference selectedDWGRef in selectedDWGRefs)
            {
                // Convert the selected reference to an ImportInstance object
                ImportInstance selectedDWG = doc.GetElement(selectedDWGRef) as ImportInstance;
                FamilyInstance familyInstance = DWGUtils.DWGLinkToGenericModelFamily(selectedDWG);
                if(familyInstance == null)
                {
                    Utils.Show("Failed");
                }
            }
            return Result.Succeeded;

        }
    }
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DWGToGenericModelPlanOnly : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            // Access the Autodesk Revit application object
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;

            // Access the active UI document and the current Revit document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Create a selection object to interact with the user's selections in the active view
            Selection selection = uidoc.Selection;

            // Use a utility function to get a reference to a single selected DWG element in the document
            // It displays a "Select DWG" prompt to the user and filters for ImportInstance elements (DWG files)
            IList<Reference> selectedDWGRefs = Utils.GetCurrentSelection(uidoc, new Utils.TypeSelectionFilter(doc, new List<Type>() { typeof(ImportInstance) }), "Select DWG");
            foreach (Reference selectedDWGRef in selectedDWGRefs)
            {

                // Convert the selected reference to an ImportInstance object
                ImportInstance selectedDWG = doc.GetElement(selectedDWGRef) as ImportInstance;
                FamilyElementVisibility familyElementVisibility = new FamilyElementVisibility(FamilyElementVisibilityType.Model);
                familyElementVisibility.IsShownInFrontBack = false;
                familyElementVisibility.IsShownInLeftRight = false;
                FamilyInstance familyInstance = DWGUtils.DWGLinkToGenericModelFamily(selectedDWG, familyElementVisibility, "PlanView Only");
                if (familyInstance == null)
                {
                    Utils.Show("Failed");
                }
            }
            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DWGToGenericModelSectionsOnly : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            // Access the Autodesk Revit application object
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;

            // Access the active UI document and the current Revit document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Create a selection object to interact with the user's selections in the active view
            Selection selection = uidoc.Selection;

            // Use a utility function to get a reference to a single selected DWG element in the document
            // It displays a "Select DWG" prompt to the user and filters for ImportInstance elements (DWG files)
            IList<Reference> selectedDWGRefs = Utils.GetCurrentSelection(uidoc, new Utils.TypeSelectionFilter(doc, new List<Type>() { typeof(ImportInstance) }), "Select DWG");
            foreach (Reference selectedDWGRef in selectedDWGRefs)
            {
                // Convert the selected reference to an ImportInstance object
                ImportInstance selectedDWG = doc.GetElement(selectedDWGRef) as ImportInstance;
                FamilyElementVisibility familyElementVisibility = new FamilyElementVisibility(FamilyElementVisibilityType.Model);
                familyElementVisibility.IsShownInTopBottom = false;
                FamilyInstance familyInstance = DWGUtils.DWGLinkToGenericModelFamily(selectedDWG, familyElementVisibility, "SectionView Only");
                if (familyInstance == null)
                {
                    Utils.Show("Failed");
                }
            }
            return Result.Succeeded;

        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LinkMultipleDWG : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            // Access the Autodesk Revit application object
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;

            // Access the active UI document and the current Revit document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Filter = "DWG files (*.dwg)|*.dwg";

            if(dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                // Start a transaction for importing the DWG file into the family document
                using (Transaction transaction = new Transaction(doc, "Import DWG"))
                {
                    transaction.Start();

                    // Set up import options for the DWG file
                    DWGImportOptions importOptions = new DWGImportOptions();
                    importOptions.Placement = ImportPlacement.Shared;
                    importOptions.VisibleLayersOnly = true;
                    importOptions.OrientToView = false;
                    importOptions.Unit = ImportUnit.Default;
                    foreach (string path in dialog.FileNames)
                    {
                        // Import the DWG file into the family document
                        ElementId importedContent;
                        doc.Link(path, importOptions, doc.ActiveView, out importedContent);

                    }
                    transaction.Commit();
                }
            }

            return Result.Succeeded;

        }
    }


    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RelinkDWG : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {// Access the Autodesk Revit application object
            Autodesk.Revit.ApplicationServices.Application app = commandData.Application.Application;

            // Access the active UI document and the current Revit document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Create a selection object to interact with the user's selections in the active view
            Selection selection = uidoc.Selection;

            // Use a utility function to get a reference to a single selected DWG element in the document
            // It displays a "Select DWG" prompt to the user and filters for ImportInstance elements (DWG files)
            IList<Reference> selectedDWGRefs = Utils.GetCurrentSelection(uidoc, new Utils.TypeSelectionFilter(doc, new List<Type>() { typeof(ImportInstance) }), "Select DWG");
            foreach (Reference selectedDWGRef in selectedDWGRefs)
            {

                // Convert the selected reference to an ImportInstance object
                ImportInstance selectedDWG = doc.GetElement(selectedDWGRef) as ImportInstance;
                ElementId succes = DWGUtils.ReLinkDWG(selectedDWG);
                if (succes == null)
                {
                    Utils.Show("Failed");
                }
            }
            return Result.Succeeded;

        }
    }

    public class DWGUtils
    {


        public static ElementId ReLinkDWG(ImportInstance selectedDWG)
        {
            // Initialize a variable to store the FamilyName
            string FamilyName = String.Empty;

            // Import the DWG file into the family document
            ElementId importedContent = null;
            Document doc = selectedDWG.Document;
            UIDocument uidoc = new UIDocument(doc);
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;

            // Get the file path of the selected DWG file
            string filepath = DWGUtils.GetLinkedCADFilePath(selectedDWG);
            Clipboard.SetText(filepath);


            // If the file path is not found, display an error message and return a failure result
            if (filepath == null)
            {
                Utils.Show("Could not find File");
                return null;
            }
            using (Transaction t = new Transaction(doc, "Relink DWG"))
            {
                t.Start();
                doc.Delete(selectedDWG.Id);

                // Set up import options for the DWG file
                DWGImportOptions importOptions = new DWGImportOptions();
                importOptions.Placement = ImportPlacement.Shared;
                importOptions.VisibleLayersOnly = true;
                importOptions.OrientToView = false;
                importOptions.Unit = ImportUnit.Default;
                doc.Link(filepath, importOptions, doc.ActiveView, out importedContent);
                Utils.Show(importedContent.ToString());
                t.Commit();
            }
            return importedContent;
        }
        public static FamilyInstance DWGLinkToGenericModelFamily(ImportInstance selectedDWG, FamilyElementVisibility familyElementVisibility = null, string prefix = "")
        {
            // Initialize a variable to store the FamilyName
            string FamilyName = String.Empty;

            Document doc = selectedDWG.Document;
            UIDocument uidoc = new UIDocument(doc);
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;

            // Calculate the midpoint (insertion point) of the selected DWG in the Revit document
            XYZ insertationPoint = DWGUtils.GetMidPoint(doc, selectedDWG);

            // Get the file path of the selected DWG file
            string filepath = DWGUtils.GetLinkedCADFilePath(selectedDWG);

            // If the file path is not found, display an error message and return a failure result
            if (filepath == null)
            {
                Utils.Show("Could not find File");
                return null;
            }

            TaskDialogResult deleteDWG = TaskDialog.Show("Linked CAD to Family", "Do you want to the delete the Linked CAD File afterwards?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No);

            // Extract the FamilyName from the file path
            FamilyName = Path.GetFileNameWithoutExtension(filepath);
            DWGUtils.CheckIfAlreadyExists(doc, FamilyName);

            // Get the path to the family template in Revit
            string familyTemplatePath = app.FamilyTemplatePath;

            // Get the path to a generic model family template using a utility function
            string genericModelPath = DWGUtils.GetGenericModelFamilyTemplate(familyTemplatePath);


            bool Switched = false;
            FamilyInstance familyInstance = null;
            // Create a new family document using the generic model family template
            Document famDoc = app.NewFamilyDocument(genericModelPath);
            Switched = DWGUtils.SwitchDocument(famDoc);
            if (Switched)
            {

                Dictionary<string, string> fileInfo = GetFileInfo(doc, filepath);
                // Start a transaction for importing the DWG file into the family document
                using (Transaction transaction = new Transaction(famDoc, "Import DWG"))
                {
                    transaction.Start();
                    // Set up import options for the DWG file
                    DWGImportOptions importOptions = new DWGImportOptions();
                    importOptions.Placement = ImportPlacement.Centered;
                    importOptions.VisibleLayersOnly = true;
                    importOptions.OrientToView = false;
                    importOptions.Unit = ImportUnit.Default;

                    // Import the DWG file into the family document
                    ElementId importedContent;
                    famDoc.Import(filepath, importOptions, famDoc.ActiveView, out importedContent);
                    FamilyManager manager = famDoc.FamilyManager;

                    string typeName = fileInfo["FileName"];
                    if(prefix != "")
                    {
                        typeName = prefix + " - " + typeName;
                    }
                    manager.NewType(typeName) ;
#if Revit2021
                    FamilyParameter userParameter = manager.AddParameter("FamilyCreator", BuiltInParameterGroup.PG_IDENTITY_DATA, ParameterType.Text, false);
                    FamilyParameter dateParameter = manager.AddParameter("FileDate", BuiltInParameterGroup.PG_IDENTITY_DATA, ParameterType.Text, false);
                    FamilyParameter fileNameParameter = manager.AddParameter("FileName", BuiltInParameterGroup.PG_IDENTITY_DATA, ParameterType.Text, false);
                    FamilyParameter filePathParameter = manager.AddParameter("FilePath", BuiltInParameterGroup.PG_IDENTITY_DATA, ParameterType.Text, false);
#else
                    FamilyParameter userParameter = manager.AddParameter("FamilyCreator", GroupTypeId.IdentityData, SpecTypeId.String.Text, true);
                    FamilyParameter dateParameter = manager.AddParameter("FileDate", GroupTypeId.IdentityData, SpecTypeId.String.Text, true);
                    FamilyParameter fileNameParameter = manager.AddParameter("FileName", GroupTypeId.IdentityData, SpecTypeId.String.Text, true);
                    FamilyParameter filePathParameter = manager.AddParameter("FilePath", GroupTypeId.IdentityData, SpecTypeId.String.Text, true);

#endif
                    manager.SetFormula(userParameter, "\"" + fileInfo["User"] + "\"");
                    manager.SetFormula(dateParameter, "\"" + fileInfo["Date"] + "\"");
                    manager.SetFormula(fileNameParameter, "\"" + fileInfo["FileName"] + "\"");
                    manager.SetFormula(filePathParameter, "\"" + fileInfo["FilePath"] + "\"");

                    //// Get the imported element and commit the transaction
                    ImportInstance element = (ImportInstance)famDoc.GetElement(importedContent);
                    if (familyElementVisibility != null)
                    {
                        element.SetVisibility(familyElementVisibility);
                    }
                    transaction.Commit();

                }

                // Generate a file path for the new family document
                string familyFilePath = Path.Combine(Path.GetTempPath(), FamilyName + ".rfa");

                // Load the family from the saved family document into the current document
                Family family = famDoc.LoadFamily(doc);

                // Start a transaction to place a family instance in the current document
                using (Transaction transaction = new Transaction(doc, "Place Family"))
                {
                    transaction.Start();

                    family.Name = FamilyName;
                    // Activate the family symbol for placement

                    FamilySymbol symbol = (FamilySymbol)doc.GetElement(family.GetFamilySymbolIds().First());
                    symbol.Activate();
                    symbol.Name = symbol.FamilyName;
                    // Create a new family instance at the insertion point
                    familyInstance = doc.Create.NewFamilyInstance(insertationPoint, symbol, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    if (deleteDWG == TaskDialogResult.Yes)
                    {
                        doc.Delete(selectedDWG.Id);
                    }

                    // Commit the transaction and return a success result
                    transaction.Commit();


                }

                uidoc.ShowElements(familyInstance.Id);



                //famDoc.Close(false);


            }

            // Close the family document with saving changes
            famDoc.Close(false);
            return familyInstance;
        }
        public static void CheckIfAlreadyExists(Document doc, string name)
        {
            TaskDialogResult result = TaskDialogResult.No;
            var matchingElements = new FilteredElementCollector(doc).OfClass(typeof(Family)).ToElements().Where(f => f.Name == name);
            if (matchingElements.Any())
            {
                if(TaskDialog.Show("Linked CAD to Family", "There is already a Family for the Linked CAD File\nDo you which to replace it?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
                {
                    using (Transaction transaction = new Transaction(doc, "Delete existing Family"))
                    {
                        transaction.Start();
                        foreach( Element element in matchingElements)
                        {
                            doc.Delete(element.Id);
                        }

                        // Commit the transaction and return a success result
                        transaction.Commit();
                    }

                }
            }
        }
        public static XYZ GetMidPoint(Document doc, ImportInstance importInstance)
        {
            BoundingBoxXYZ bbox = importInstance.get_BoundingBox(doc.ActiveView);
            Line line = Line.CreateBound(new XYZ(bbox.Min.X, bbox.Min.Y, 0), new XYZ(bbox.Max.X, bbox.Max.Y, 0));
            return line.Evaluate(0.5, true);
        }

        public static Dictionary<string, string> GetFileInfo(Document doc, string filepath)
        {
            Autodesk.Revit.ApplicationServices.Application app = doc.Application;
            string user = app.Username;
            string fileName = Path.GetFileName(filepath);
            string date = File.GetLastWriteTime(filepath).ToString("G");
            string path = filepath;
            return new Dictionary<string, string>()
            {
                {"User", user },
                {"FileName", fileName},
                {"Date", date},
                {"FilePath", path}
            };

        }

        public static bool SwitchDocument(Document doc)
        {
            try
            {
                //Find 3DView in Doc
                var e = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(Level)).ToElementIds();
                foreach (ElementId id in e)
                {
                    new UIDocument(doc).ShowElements(id);
                }
                UIDocument familyUIDoc = new UIDocument(doc);

                var views = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(View)).ToElements();
                View floorplan = null;
                View threeDView = null;
                foreach (View view in views)
                {
                    if (view.ViewType == ViewType.FloorPlan)
                    {
                        floorplan = view;
                    }
                    else if (view.ViewType == ViewType.ThreeD)
                    {
                        threeDView = view;
                    }
                }
                familyUIDoc.ActiveView = floorplan;

                //familyUIDoc.RequestViewChange((View)threeDView);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void SetFloorPlanView(Document doc)
        {
            UIDocument familyUIDoc = new UIDocument(doc);
            var views = new FilteredElementCollector(doc).WhereElementIsNotElementType().OfClass(typeof(View)).ToElements();
            View floorplan = null;
            foreach (View view in views)
            {
                if (view.ViewType.ToString().ToLower().Contains("plan") && view.IsTemplate == false)
                {
                    floorplan = view;
                }
            }
            familyUIDoc.ActiveView = floorplan;
        }
        public static string GetGenericModelFamilyTemplate(string familyTemplatePath)
        {
            string result = null;
            string[] familyTemplates = Directory.GetFiles(familyTemplatePath, "*.rft");
            Dictionary<string, dynamic> map = new Dictionary<string, dynamic>();
            foreach(string path in familyTemplates)
            {
                string fileName = Path.GetFileName(path);
                map.Add(fileName, path);
                if(fileName.EndsWith("Generic Model.rft"))
                {
                    result = path;
                }
            }
            if (result == null)
            {
                Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
                dialog.InitialDirectory = Path.GetDirectoryName(familyTemplates[0]);
                dialog.Filter = "Family Template files (*.rft)|*.rft";
                dialog.Multiselect = false;
                dialog.ShowDialog();
                result = dialog.FileName;
            }
            return result;
        }

        public static string GetLinkedCADFilePath(ImportInstance linkedDWG)
        {
            Document doc = linkedDWG.Document;
            if (!linkedDWG.IsLinked)
            {
                return null;
            }
            CADLinkType cadLinkType = doc.GetElement(linkedDWG.GetTypeId()) as CADLinkType;

            try
            {
                ExternalFileReference exFileRef = cadLinkType.GetExternalFileReference();
                ModelPath modelPath = exFileRef.GetPath();
                string filepath = ModelPathUtils.ConvertModelPathToUserVisiblePath(modelPath);
                if (!filepath.EndsWith(".dwg"))
                {
                    return filepath + ".dwg";
                }
                else
                {
                    return filepath;
                }

            }
            catch
            {
                GetResourceData:
                ExternalResourceReference externalRef = cadLinkType.GetExternalResourceReferences().Values.First();
                string path = externalRef.InSessionPath;
                if (path == String.Empty)
                {
                    return null;
                }
                if (!path.EndsWith(".dwg"))
                {
                    using (Transaction t = new Transaction(doc, "Reload Link"))
                    {
                        t.Start();
                        cadLinkType.Reload();
                        t.Commit();
                    }
                    goto GetResourceData;
                }
                else
                {
                    //Check if Autodesk Docs File
                    if (path.StartsWith("Autodesk Docs://"))
                    {
                        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        path = path.Replace("Autodesk Docs://", "");
                        path = path.Replace("/", "\\");
                        path = @"C:\Users\907335\DC\ACCDocs\" + path;
                        path = path.Replace("\\", "/");
                    }
                    Clipboard.SetText(path);
                    return path;
                }
            }

            
        }
    }
}
