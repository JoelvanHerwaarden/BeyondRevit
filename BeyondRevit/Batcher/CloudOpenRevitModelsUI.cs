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
    public class CloudOpenRevitModelsUI : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            List<BatcherModel> models = BatcherUtils.PromptOpenBatcherModels();
            if (models == null)
            {
                return Result.Cancelled;
            }
            foreach (BatcherModel model in models)
            {
                model.OpenAsRevitModelInUI(commandData.Application);

            }
            Utils.Show("Models Opened!");
            return Result.Succeeded;
        }
    }
}
