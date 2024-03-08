using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using BeyondRevit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Forms = System.Windows.Forms;

namespace BeyondRevit.Batcher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class LocalExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;
            Dictionary<string, dynamic> exportTypes = new Dictionary<string, dynamic>()
            {
                {"Autocad (DWG)", ExportType.Autocad },
                { "Navisworks (NWC)", ExportType.Navisworks },
                { "Synchro (SPX)",ExportType.Synchro },
                { "Revit (RVT)", ExportType.Revit }
            };
            BatcherModel model = BatcherUtils.PromptOpenBatcherModels(false).FirstOrDefault();
            if (model.IsModelOpened(commandData.Application))
            {
                GenericDropdownWindow window = new GenericDropdownWindow("Exports", "Select Export Types", exportTypes, Utils.RevitWindow(commandData), true);
                window.ShowDialog();
                if (window.Cancelled)
                {
                    return Result.Cancelled;
                }
                List<dynamic> selectedTypes = window.SelectedItems;

                Forms.FolderBrowserDialog dialog = new Forms.FolderBrowserDialog();

                if (dialog.ShowDialog() == Forms.DialogResult.OK)
                {
                    BatcherUtils.RegisterDismisPopups(commandData.Application);
                    string directory = dialog.SelectedPath;
                    if (selectedTypes.Contains(ExportType.Autocad))
                    {
                        BatcherUtils.ExportDWG(document, model.Views, directory, model.DWGExportSettings);
                    }
                    if (selectedTypes.Contains(ExportType.Navisworks))
                    {
                        BatcherUtils.ExportNWC(document, model.Views, directory);
                    }
                    if (selectedTypes.Contains(ExportType.Synchro))
                    {
                        BatcherUtils.ExportSynchro(document, model.Views, directory);
                    }
                    if (selectedTypes.Contains(ExportType.Revit))
                    {
                        BatcherUtils.ExportRevit(document, model.Views.Select(v => v.GetView(document)).ToList(), directory);
                    }
                }
            }
            return Result.Succeeded;

        }
        private enum ExportType
        {
            Autocad = 0,
            Navisworks = 1,
            Synchro = 2,
            Revit = 3,
        }
    }
}
