using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeyondRevit.Gaia
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GaiaCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            GaiaViewModel viewmodel = new GaiaViewModel(commandData.Application.Application);
            GaiaMainWindow window = new GaiaMainWindow(viewmodel);
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}
