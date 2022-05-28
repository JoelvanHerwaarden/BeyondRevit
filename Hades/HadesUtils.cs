using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeyondRevit.Hades
{
    public class HadesUtils
    {
        public static TaskDialogResult Show(string mainInstruction, string extendedInstruction = null)
        {
            TaskDialog taskDialog = new TaskDialog("Hades");
            taskDialog.MainInstruction = mainInstruction;

            if(extendedInstruction != null)
            {
                taskDialog.MainContent = extendedInstruction;
            }
            taskDialog.CommonButtons = TaskDialogCommonButtons.Yes|TaskDialogCommonButtons.No;
            return taskDialog.Show();
        }
    }
}
