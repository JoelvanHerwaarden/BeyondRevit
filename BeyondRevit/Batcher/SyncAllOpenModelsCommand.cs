using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Forms = System.Windows.Forms;
using System.Xml.Linq;

namespace BeyondRevit.Batcher
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SyncAllOpenModelsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            BeyondRevit.Commands.BeyondRevitSynchronizerUtils.SaveAllDocuments(commandData.Application);
            BeyondRevit.Commands.BeyondRevitSynchronizerUtils.SyncAllDocuments(commandData.Application);
            BeyondRevit.Commands.BeyondRevitSynchronizerUtils.ReloadAllDocuments(commandData.Application);
            Utils.Show("Models Synced!");
            return Result.Succeeded;
        }
    }
}
