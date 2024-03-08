using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;
using Forms = System.Windows.Forms;

namespace BeyondRevit.Batcher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LocalExportRevitCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;
            List<View> views = BatcherUtils.PromptViews(document, commandData);
            Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();
            
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                BatcherUtils.RegisterDismisPopups(commandData.Application);
                string directory = dialog.SelectedPath;
                if(TaskDialog.Show("Save Backup", "Do you want to create a backup first?", TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No) == TaskDialogResult.Yes)
                {
                    document = BatcherUtils.SaveAsDocument(document, directory, out string backupPath);
                }
                using (Transaction t = new Transaction(document, "Delete Links, Views and Elements"))
                {
                    t.Start();
                    BatcherUtils.RevitDeleteLinks(document);
                    BatcherUtils.RevitDeleteAllButGivenViews(document, views);
                    //BatcherUtils.RevitGetElementsNotInViews(views);
                    t.Commit();
                }
                BatcherUtils.ExportRevit(document, views, directory);
                BatcherUtils.UnRegisterDismisPopups(commandData.Application);

            }

            return Result.Succeeded;
        }
    }
    
}
