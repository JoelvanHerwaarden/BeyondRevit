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
    public class SaveModelAsBadgerModelCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;
            Dictionary<string, dynamic>  items = new Dictionary<string, dynamic>();
            List<View> views = BatcherUtils.PromptViews(doc, commandData);
            if (views == null)
            {
                return Result.Cancelled;
            }
            Forms.SaveFileDialog dialog = new Forms.SaveFileDialog();
            dialog.Filter = "Batcher files (*.json)|*.json";
            dialog.FileName = doc.Title;
            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                string filepath = dialog.FileName;
                BatcherModel model = new BatcherModel(doc, views);
                model.SaveAsFile(filepath);
            }
            return Result.Succeeded;
        }
    }
}
