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
    public class EditFamilyPrompt
    {
        public static void EnableFamilyEditorPrompt(ControlledApplication application)
        {
            application.DocumentOpening += PromptFamilyEditing;
        }

        private static void PromptFamilyEditing(object sender, Autodesk.Revit.DB.Events.DocumentOpeningEventArgs e)
        {
            if(e.DocumentType == DocumentType.Family)
            {
                TaskDialog taskDialog = new TaskDialog("Opening Family");
                taskDialog.MainInstruction = "Are you sure you want to edit this family?";
                taskDialog.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                taskDialog.DefaultButton = TaskDialogResult.No;
                if(taskDialog.Show() == TaskDialogResult.No)
                {
                    e.Cancel();
                }
            }
        }
    }
}
