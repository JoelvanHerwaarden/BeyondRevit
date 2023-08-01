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
using Autodesk.Internal.InfoCenter;
using adskWindows = Autodesk.Windows;

namespace BeyondRevit.Commands
{
    public class TrainingWheelsProtocol
    {
        [Transaction(TransactionMode.Manual)]
        [Regeneration(RegenerationOption.Manual)]
        public class RemoveTrainingWheels : IExternalCommand
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                UIApplication uiApp = commandData.Application;
                AddInCommandBinding modelInPlaceCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ModelInPlace));
                modelInPlaceCommandBinding.BeforeExecuted -= ModelInPlaceBlockFunction_Event;


                AddInCommandBinding importCADCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ImportCAD));
                importCADCommandBinding.BeforeExecuted -= ImportCADBlockFunction_Event;

                Utils.ShowInfoBalloon("Training Wheels Protocol De-Activated");
                
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

        public static void SubscribeTrainingWheelsProtocol(UIControlledApplication uiApp)
        {
            AddInCommandBinding modelInPlaceCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ModelInPlace));
            modelInPlaceCommandBinding.BeforeExecuted += ModelInPlaceBlockFunction_Event;

            AddInCommandBinding importCADCommandBinding = uiApp.CreateAddInCommandBinding(RevitCommandId.LookupPostableCommandId(PostableCommand.ImportCAD));
            importCADCommandBinding.BeforeExecuted += ImportCADBlockFunction_Event;

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
            Document document = uiApp.ActiveUIDocument.Document;
            if (!document.IsFamilyDocument)
            {
                PromptScreen("If you want to display a 2D DWG as XRef in your view, use Link CAD function.\nIf you have a 3D DWG, import the DWG into a Generic Models Family Template");
                e.Cancel = true;
            }
        }
    }

    
}