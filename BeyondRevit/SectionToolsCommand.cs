using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.UI.Selection;
using BeyondRevit.UI;
using BeyondRevit.UI.SectioningTools;

namespace BeyondRevit.Commands
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class SectionToolsCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Document document = commandData.Application.ActiveUIDocument.Document;
            SectioningToolsExternalEventHandler handler = new SectioningToolsExternalEventHandler();
            ExternalEvent externalEvent = ExternalEvent.Create(handler);
            SectioningToolsViewModel viewmodel = new SectioningToolsViewModel(document, handler, externalEvent);
            SectioningToolsWindow sectioningToolsWindow = new SectioningToolsWindow(viewmodel);
            sectioningToolsWindow.Owner = Utils.RevitWindow(commandData);
            sectioningToolsWindow.Show();
            return Result.Succeeded;
        }
    }
}