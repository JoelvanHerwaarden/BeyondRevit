using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;

namespace BeyondRevit.Commands
{
    public class MIPOverride
    {
        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class RemoveMIPOverride : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIApplication uiApp = commandData.Application;
                AddInCommandBinding modelInPlaceCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ModelInPlace));
                modelInPlaceCommandBinding.BeforeExecuted -= ModelInPlaceBlockFunction_Event;


                AddInCommandBinding importCADCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ImportCAD));
                importCADCommandBinding.BeforeExecuted -= ImportCADBlockFunction_Event;
                Utils.Show("Training wheels protocol de-activated");
                return Result.Succeeded;
            }
        }

        public static bool PromptScreen(string explanation, bool canBeUsed = false)
        {
            bool result = false;
            TaskDialog dialog = new TaskDialog("Training wheels protocol");
            dialog.MainContent = explanation;
            dialog.MainIcon = TaskDialogIcon.TaskDialogIconShield;
            dialog.MainInstruction = "This function is blocked";
            if (canBeUsed)
            {
                dialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            }
            else
            {
                dialog.CommonButtons = TaskDialogCommonButtons.Yes;
            }
            if (dialog.Show() == TaskDialogResult.Yes)
            {
                result = true;
            }
            return result;
        }

        public static void TrainingWheelsProtocol(UIControlledApplication uiApp)
        {
            try
            {
                AddInCommandBinding modelInPlaceCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ModelInPlace));
                modelInPlaceCommandBinding.BeforeExecuted += ModelInPlaceBlockFunction_Event;

                AddInCommandBinding importCADCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ImportCAD));
                importCADCommandBinding.BeforeExecuted += ImportCADBlockFunction_Event;

                Utils.Show("Training wheels protocol activated");
            }
            catch(Exception e)
            {
                Utils.Show("Could not activate training wheels protocol:\n"+e.Message);
            }
           
        }
        public static void ModelInPlaceBlockFunction_Event(object sender, Autodesk.Revit.UI.Events.BeforeExecutedEventArgs e)
        {
            UIApplication uiApp = (UIApplication)sender;
            bool result = PromptScreen("To model Geometry in Revit use System Components or Families.\nAre you sure you want to model this way?", true) ;
            if (result)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }
        public static void ImportCADBlockFunction_Event(object sender, Autodesk.Revit.UI.Events.BeforeExecutedEventArgs e)
        {
            UIApplication uiApp = (UIApplication)sender;
            PromptScreen("To get a DWG file into your model, use the Link CAD function.");
            e.Cancel = true;
        }
    }

    
}