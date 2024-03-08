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
using System.IO;
using System.Collections.ObjectModel;
using BeyondRevit.UI;
using System.Threading;
using System.Windows;
using System.Security.Cryptography;
using Autodesk.Revit.UI.Events;
using Autodesk.Revit.DB.Electrical;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace BeyondRevit.Batcher
{
    public class BatcherUtils
    {
        public static void ExportSynchro(Document document, List<BatcherView> viewsToExport, string DirectoryPath)
        {
            foreach (BatcherView batcherView in viewsToExport)
            {
                View view = batcherView.GetView(document);
                if (view != null && view.ViewType == ViewType.ThreeD)
                {
                    string fileName = batcherView.ExportFileName;
                    string filePath = System.IO.Path.Combine(DirectoryPath, fileName + ".spx");
                    SYN.DataGenerator generator = new SYN.DataGenerator(document.Application, document, view);
                    Synchro.Material material = new Synchro.Material();
                    material.setDiffuse(new Synchro.ColorRGB(0.0, 0.0, 1.0));
                    material.setTransmission(new Synchro.ColorRGB(0.75, 0.75, 0.75));
                    generator.SetWorkspaceMaterial(material);
                    generator.SetBuildResourceTreeOnImport(true);
                    generator.SaveSPFile(filePath);
                    Utils.ShowInfoBalloon(fileName + " exported Succesfully!");
                }
            }
        }
        public static void ExportNWC(Document document, List<BatcherView> viewsToExport, string DirectoryPath)
        {
            foreach (BatcherView batcherView in viewsToExport)
            {
                View view = batcherView.GetView(document);
                if (view != null && view.ViewType == ViewType.ThreeD)
                {
                    string fileName = batcherView.ExportFileName;
                    string filePath = System.IO.Path.Combine(DirectoryPath, fileName + ".spx");
                    NavisworksExportOptions exportOptions = new NavisworksExportOptions()
                    {
                        ExportLinks = false,
                        ExportScope = NavisworksExportScope.View,
                        ViewId = view.Id,
                        Coordinates = NavisworksCoordinates.Shared,
                        ConvertElementProperties = true
                    };
                    document.Export(DirectoryPath, batcherView.ExportFileName, exportOptions);
                }
            }
        }
        public static void ExportDWG(Document document, List<BatcherView> viewsToExport, string DirectoryPath, string DWGExportSettings)
        {
            try
            {
                DWGExportOptions exportOptions = DWGExportOptions.GetPredefinedOptions(document, DWGExportSettings);
                foreach (BatcherView batcherView in viewsToExport)
                {
                    View view = batcherView.GetView(document);
                    if (view != null)
                    {
                        ICollection<ElementId> views = new Collection<ElementId>() { view.Id };
                        string fileName = batcherView.ExportFileName;
                        string filePath = System.IO.Path.Combine(DirectoryPath, fileName + ".spx");
                        document.Export(DirectoryPath, batcherView.ExportFileName, views, exportOptions);
                        Utils.ShowInfoBalloon(fileName + " exported Succesfully!");
                    }
                }
            }
            catch
            {
                Utils.Show(string.Format("DWG Export Settings {0} do not exist in the Project", DWGExportSettings));
            }

        }
        public static void ExportRevit(Document document, List<View> viewsToExport, string DirectoryPath, bool CloseAfterwards = true)
        {
            BatcherUtils.RegisterDismisPopups(new UIApplication(document.Application));
            if (TaskDialog.Show("Save Backup", "Do you want to create a backup first?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
            {
                document = BatcherUtils.SaveAsDocument(document, DirectoryPath, out string backupPath);
            }
            using (Transaction t = new Transaction(document, "Delete Links, Views and Elements"))
            {
                t.Start();
                BatcherUtils.RevitDeleteLinks(document);
                BatcherUtils.RevitDeleteAllButGivenViews(document, viewsToExport);
                BatcherUtils.RevitGetElementsNotInViewsv2(viewsToExport);
                t.Commit();
            }
            try
            {
                BatcherUtils.SaveAsDocument(document, DirectoryPath, out string outputFilePath);
            }
            catch (Exception e)
            {
                Utils.Show(e.Message);
            }
            if (CloseAfterwards)
            {
                try
                {
                    document.Close(false);
                }
                catch { };
            }

            BatcherUtils.UnRegisterDismisPopups(new UIApplication(document.Application));

        }
        public static void ExportRevitSeperateViews(Document document, List<View> viewsToExport, string DirectoryPath)
        {
            TransactWithCentralOptions transOptions = new TransactWithCentralOptions();
            SynchronizeWithCentralOptions options = new SynchronizeWithCentralOptions()
            {
                SaveLocalAfter = true
            };
            if (document.IsWorkshared && !document.IsLinked)
            {
                document.SynchronizeWithCentral(transOptions, options);
            }
            SaveAsOptions saveOptions = new SaveAsOptions()
            {
                OverwriteExistingFile = true,
                MaximumBackups = 1,
            };

            WorksharingSaveAsOptions wsOptions = new WorksharingSaveAsOptions();
            wsOptions.OpenWorksetsDefault = SimpleWorksetConfiguration.AllWorksets;
            wsOptions.ClearTransmitted = false;
            wsOptions.SaveAsCentral = true;
            saveOptions.SetWorksharingOptions(wsOptions);

            foreach(View view in viewsToExport)
            {
                using(TransactionGroup tg = new TransactionGroup(document))
                {
                    tg.Start();
                    string filePath = System.IO.Path.Combine(DirectoryPath, string.Format("{0} - {1}.rvt", document.Title, view.Name));
                    using (Transaction t = new Transaction(document, "Delete Links, Views and Elements"))
                    {
                        t.Start();
                        RevitDeleteLinks(document);
                        RevitDeleteAllButGivenViews(document, viewsToExport);
                        //RevitGetElementsNotInViews(viewsToExport);
                        t.Commit();
                    }

                    try
                    {
                        document.SaveAs(ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath), saveOptions);
                    }
                    catch (Exception e)
                    {
                        Utils.Show(e.Message);
                    }
                    tg.RollBack();
                }
                
            }

            
            document.Close(false);
        }
        public static void RevitDeleteLinks(Document document)
        {
            string msg = "";
            ICollection<ElementId> revitLinks = new FilteredElementCollector(document).OfClass(typeof(RevitLinkType)).ToElementIds();
            ICollection<ElementId> cadLinks = new FilteredElementCollector(document).OfClass(typeof(CADLinkType)).ToElementIds();
            ICollection<ElementId> imageLinks = new FilteredElementCollector(document).OfClass(typeof(ImageType)).ToElementIds();
            ICollection<ElementId> coordinationModels = new FilteredElementCollector(document).OfCategory(BuiltInCategory.OST_Coordination_Model).ToElementIds();
            foreach (ElementId id in revitLinks)
            {
                try
                {
                    document.Delete(id);
                }
                catch (Exception ex) { msg += ex.Message + "\n"; }
            }
            foreach (ElementId id in cadLinks)
            {
                try
                {
                    document.Delete(id);
                }
                catch (Exception ex) { msg += ex.Message + "\n"; }
            }
            foreach (ElementId id in imageLinks)
            {
                try
                {
                    document.Delete(id);
                }
                catch (Exception ex) { msg += ex.Message + "\n"; }
            }
            foreach (ElementId id in coordinationModels)
            {
                try
                {
                    document.Delete(id);
                }
                catch (Exception ex) { msg += ex.Message + "\n"; }
            }
        }
        public static void RevitDeleteAllButGivenViews(Document document, List<View> views)
        {

            List<ElementId> viewIds = views.Select(v => v.Id).ToList();
            ICollection<Element> viewsToDelete = new FilteredElementCollector(document).OfClass(typeof(View)).Where(v => !viewIds.Contains(v.Id)).ToList();
            foreach (Element element in viewsToDelete)
            {
                try
                {
                    document.Delete(element.Id);
                }
                catch { }
            }
        }
        public static Document SaveAsDocument(Document document, string directory, out string outputFilepath)
        {

            SaveAsOptions saveOptions = new SaveAsOptions()
            {
                OverwriteExistingFile = true,
                MaximumBackups = 1,
            };

            WorksharingSaveAsOptions wsOptions = new WorksharingSaveAsOptions();
            wsOptions.OpenWorksetsDefault = SimpleWorksetConfiguration.AllWorksets;
            wsOptions.ClearTransmitted = false;
            wsOptions.SaveAsCentral = true;
            saveOptions.SetWorksharingOptions(wsOptions);

            outputFilepath = Path.Combine(directory, document.Title + ".rvt");
            Clipboard.SetText(outputFilepath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputFilepath));
            document.SaveAs(ModelPathUtils.ConvertUserVisiblePathToModelPath(outputFilepath), saveOptions);

            return document;
        }
        public static List<BatcherModel> PromptOpenBatcherModels(bool selectMultiple = true)
        {
            List<BatcherModel> batcherModels = new List<BatcherModel>();    
            Forms.OpenFileDialog dialog = new Forms.OpenFileDialog();
            dialog.Filter = "Batcher files (*.json)|*.json";
            dialog.Multiselect = selectMultiple;
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                foreach(string filepath in dialog.FileNames)
                {
                    string json = File.ReadAllText(filepath);
                    BatcherModel batcherModel = Newtonsoft.Json.JsonConvert.DeserializeObject<BatcherModel>(json);
                    batcherModels.Add(batcherModel);
                }
                return batcherModels;
            }
            else
            {
                return null;
            }
        }
        public static void RevitGetElementsNotInViews(List<View> views)
        {
            Document doc = views.First().Document;
            List<WorksetId> worksetsNotVisibleInViews = new List<WorksetId>();
            List<ElementId> elementsNotVisible = new List<ElementId>();
            List<string> worksetNamesToIgnore = new List<string>()
            {
                "Shared Views, Levels and Grids"
            };

            ICollection<Workset> worksets = new FilteredWorksetCollector(doc).ToWorksets();
            foreach (Workset workset in worksets)
            {
                if (workset.Kind == WorksetKind.UserWorkset && !worksetNamesToIgnore.Contains(workset.Name))
                {
                    foreach (View v in views)
                    {
                        View view = v;
                        if (view.ViewTemplateId != ElementId.InvalidElementId)
                        {
                            view = (View)view.Document.GetElement(view.ViewTemplateId);
                        }
                        if (view.GetWorksetVisibility(workset.Id) == WorksetVisibility.Hidden && !worksetsNotVisibleInViews.Contains(workset.Id))
                        {
                            worksetsNotVisibleInViews.Add(workset.Id);
                        }
                    }
                }
            }

            foreach (WorksetId wsId in worksetsNotVisibleInViews)
            {
                ElementWorksetFilter worksetFilter = new ElementWorksetFilter(wsId);
                elementsNotVisible.AddRange(new FilteredElementCollector(doc).WhereElementIsNotElementType().WherePasses(worksetFilter).ToElementIds());

            }

            string msg = "";
            foreach (ElementId id in elementsNotVisible)
            {
                try
                {
                    doc.Delete(id);
                }
                catch (Exception e)
                {
                    msg += e.Message + "\n";
                }
            }
        }
        public static void RevitGetElementsNotInViewsv2(List<View> views)
        {
            Document doc = views.First().Document;
            List<WorksetId> worksetsNotVisibleInViews = new List<WorksetId>();
            List<ElementId> elementsNotVisible = new List<ElementId>();
            List<Element> allElements = new List<Element>();

            List<string> CategoriesToExclude = new List<string>()
            {
                "Materials",
                "Legend Components",
                "Material Assets",
                "Project Information",
                "Primary Contours",
                "<Sketch>",
                "Internal Origin",
                "Survey Point",
                "Project Base Point",
            };
            foreach (Element element in new FilteredElementCollector(doc).WhereElementIsNotElementType().ToElements())
            {
                try
                {
                    if (element.Category != null)
                    {
                        if (element.Category.CategoryType == CategoryType.Model && !CategoriesToExclude.Contains(element.Category.Name))
                        {
                            allElements.Add(element);
                        }
                    };
                }
                catch { }
            }
            foreach (View v in views)
            {
                foreach (Element element in new FilteredElementCollector(doc, v.Id).WhereElementIsNotElementType().ToElements())
                {
                    try
                    {
                        if (element.Category != null)
                        {
                            if (element.Category.CategoryType == CategoryType.Model)
                            {
                                if (allElements.Contains(element))
                                {
                                    allElements.Remove(element);
                                }
                            }
                        };
                    }
                    catch { }
                }
            }


            string msg = "";
            foreach (Element element in allElements)
            {
                msg += string.Format("{0} - {1} - {2}\n", element.GetType().ToString(), element.Category.Name, element.Name);

                //try
                //{
                //    doc.Delete(element.Id);
                //}
                //catch (Exception e)
                //{
                //    msg += e.Message + "\n";
                //}
            }
            System.IO.File.WriteAllText("C:\\Users\\907335\\Desktop\\debug.txt", msg);
        }
        public static List<View> PromptViews(Document doc, ExternalCommandData commandData)
        {
            List<View> result = new List<View>();
            Dictionary<string, dynamic> items = new Dictionary<string, dynamic>();
            foreach (View v in new FilteredElementCollector(doc).OfClass(typeof(View)).WhereElementIsNotElementType().Where(v => ((View)v).IsTemplate == false).ToList())
            {
                items.Add(string.Format("{0} - {1}", v.Name, v.Id.ToString()), v);
            }
            GenericDropdownWindow window = new GenericDropdownWindow("Select Views", "Select which Views to Save in a Batcher File", items, Utils.RevitWindow(commandData), true);
            window.ShowDialog();
            if (window.Cancelled)
            {
                return null;
            }
            List<View> views = new List<View>();
            foreach (View v in window.SelectedItems)
            {
                views.Add(v);
            }
            return views;
        }
        public static void RegisterDismisPopups(UIApplication uiapp)
        {
            try { uiapp.DialogBoxShowing += UiAppOnDialogBoxShowing; }
            catch (Exception e) { }
        }
        public static void UnRegisterDismisPopups(UIApplication uiapp)
        {
            try { uiapp.DialogBoxShowing -= UiAppOnDialogBoxShowing; }
            catch (Exception e) { }
        }
        private static void UiAppOnDialogBoxShowing(object sender, DialogBoxShowingEventArgs args)
        {
            switch (args)
            {
                // (Konrad) Dismiss Unresolved References pop-up.
                case TaskDialogShowingEventArgs args2:
                    if (args2.DialogId == "TaskDialog_Unresolved_References")
                        args2.OverrideResult(1002);
                    break;
                default:
                    return;
            }
        }
    }
}
