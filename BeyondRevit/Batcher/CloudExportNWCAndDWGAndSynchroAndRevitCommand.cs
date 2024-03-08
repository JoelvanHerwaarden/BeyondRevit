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
    public class CloudExportNWCAndDWGAndSynchroAndRevitCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<BatcherModel> models = BatcherUtils.PromptOpenBatcherModels();
            Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();
            if(dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                string directory = dialog.SelectedPath;
                foreach (BatcherModel model in models)
                {
                    Document document = model.OpenAsRevitModel(commandData.Application.Application);
                    List<View> views = model.GetViews(document);
                    BatcherUtils.ExportDWG(document, model.Views, directory, model.DWGExportSettings);
                    BatcherUtils.ExportNWC(document, model.Views, directory);
                    BatcherUtils.ExportSynchro(document, model.Views, directory);
                    BatcherUtils.ExportRevit(document, views, directory);
                    document.Close(false);
                }
            }
            return Result.Succeeded;
        }
    }
}
