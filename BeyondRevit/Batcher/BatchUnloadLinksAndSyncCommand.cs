using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Forms = System.Windows.Forms;

namespace BeyondRevit.Batcher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExportRevitModelCommand : IExternalCommand
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
                Description = "Select Where to Store the RVT Files"
            };
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string folderPath = folderBrowser.SelectedPath;
                foreach (string batchFile in batchFiles)
                {
                    Dictionary<string, string> info = SynchroUtils.ReadBatchFile(batchFile);
                    ModelPath path = ModelPathUtils.ConvertCloudGUIDsToCloudPath(ModelPathUtils.CloudRegionEMEA, Guid.Parse(info["ProjectGuid"]), Guid.Parse(info["ModelGuid"]));
                    OpenOptions options = new OpenOptions()
                    {
                        DetachFromCentralOption = DetachFromCentralOption.DoNotDetach,
                        Audit = true,
                        AllowOpeningLocalByWrongUser = true,
                    };
                    Document batchDocument = commandData.Application.Application.OpenDocumentFile(path, options);
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

                    string filePath = System.IO.Path.Combine(folderPath, batchDocument.Title + ".rvt");

                    DeleteLinks(batchDocument);
                    DeleteAllButOneViews(batchDocument, info["View"]);

                    try
                    {
                        batchDocument.SaveAs(ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath), saveOptions);
                    }
                    catch (Exception e)
                    {
                        Utils.Show(e.Message);
                    }
                    batchDocument.Close(false);
                }
            }
            
            
            Utils.Show("Exporting Revit Files Done!");
            return Result.Succeeded;
        }

        public Result Execute2(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            Forms.FolderBrowserDialog folderBrowser = new Forms.FolderBrowserDialog()
            {
                Description = "Select Where to Store the RVT Files"
            };
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
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

                string folderPath = folderBrowser.SelectedPath;
                string filePath = System.IO.Path.Combine(folderPath, doc.Title+".rvt"); 
                DeleteLinks(doc);
                DeleteAllButCurrentViews(doc);

                try
                {
                    doc.SaveAs(ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath), saveOptions);
                }
                catch (Exception e)
                {
                    Utils.Show(e.Message);
                }
            }

            Utils.Show("Unloading Done!");
            return Result.Succeeded;
        }
        public static void DeleteLinks(Document batchDocument)
        {
            string msg = "";
            using (Transaction transaction = new Transaction(batchDocument, "Unload All"))
            {
                transaction.Start();
                ICollection<ElementId> revitLinks = new FilteredElementCollector(batchDocument).OfClass(typeof(RevitLinkType)).ToElementIds();
                ICollection<ElementId> cadLinks = new FilteredElementCollector(batchDocument).OfClass(typeof(CADLinkType)).ToElementIds();
                ICollection<ElementId> imageLinks = new FilteredElementCollector(batchDocument).OfClass(typeof(ImageType)).ToElementIds();
                foreach (ElementId id in revitLinks)
                {
                    try
                    {
                        batchDocument.Delete(id);
                    }
                    catch (Exception ex) { msg += ex.Message + "\n"; }
                }
                foreach (ElementId id in cadLinks)
                {
                    try
                    {
                        batchDocument.Delete(id);
                    }
                    catch (Exception ex) { msg += ex.Message + "\n"; }
                }
                foreach (ElementId id in imageLinks)
                {
                    try
                    {
                        batchDocument.Delete(id);
                    }
                    catch (Exception ex) { msg += ex.Message + "\n"; }
                }
                transaction.Commit();
            }
        }
        public static void DeleteAllButOneViews(Document batchDocument, string ViewName)
        {
            using (Transaction transaction = new Transaction(batchDocument, "Unload All"))
            {
                transaction.Start();
                ICollection<Element> views = new FilteredElementCollector(batchDocument).OfClass(typeof(View)).Where(v => v.Name != ViewName).ToList();
                foreach (Element element in views)
                {
                    try
                    {
                        batchDocument.Delete(element.Id);
                    }
                    catch { }
                }
                transaction.Commit();
            }
        }
        public static void DeleteAllButCurrentViews(Document batchDocument)
        {
            View current = batchDocument.ActiveView;
            string msg = "";
            using (Transaction transaction = new Transaction(batchDocument, "Unload All"))
            {
                transaction.Start();
                ICollection<Element> views = new FilteredElementCollector(batchDocument).OfClass(typeof(View)).Where(v => v.Id != current.Id).ToList();
                foreach (Element element in views)
                {
                    try
                    {
                        batchDocument.Delete(element.Id);
                    }
                    catch (Exception ex) { msg += ex.Message + "\n"; }    
                }
                //Utils.Show(msg);
                transaction.Commit();
            }
        }

        public static void DeleteAllElementsWhichAreNotVisible(Document batchDocument, string ViewName)
        {
            Element current = new FilteredElementCollector(batchDocument).OfClass(typeof(View)).Where(v => v.Name == ViewName).ToList().First();
            string msg = "";
            using (Transaction transaction = new Transaction(batchDocument, "Unload All"))
            {
                transaction.Start();
                ICollection<ElementId> visibleElements = new FilteredElementCollector(batchDocument, ((View)current).Id).OfClass(typeof(Element)).ToElementIds();
                ICollection<ElementId> allElements = new FilteredElementCollector(batchDocument).OfClass(typeof(Element)).ToElementIds();
                foreach (ElementId id in allElements)
                {
                    try
                    {
                        if(!visibleElements.Contains(id))
                        {
                            batchDocument.Delete(id);
                        }
                        
                    }
                    catch (Exception ex) { msg += ex.Message + "\n"; }
                }
                transaction.Commit();
            }
        }
    }
}
