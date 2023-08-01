using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using BeyondRevit.UI;
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
            taskDialog.CommonButtons = TaskDialogCommonButtons.Yes|TaskDialogCommonButtons.No|TaskDialogCommonButtons.Retry;
            return taskDialog.Show();
        }

        public static List<dynamic> SelectivePurge(List<Element> elements, ExternalCommandData commandData)
        {
            Dictionary<string, dynamic> elementDictionary = new Dictionary<string, dynamic>();
            foreach(dynamic element in elements)
            {
                elementDictionary.Add(element.Name+ " - "+element.Id, element);
            }
            elementDictionary = Utils.SortDictionary(elementDictionary);
            GenericDropdownWindow selectionWindow = new GenericDropdownWindow("Select Elements", "Select only the Unused Elements you want to purge", elementDictionary, Utils.RevitWindow(commandData), true);
            selectionWindow.ShowDialog();
            if (selectionWindow.Cancelled)
            {
                return null;
            }
            else
            {
                return selectionWindow.SelectedItems;
            }
        }
        public static List<dynamic> SelectivePurge(List<dynamic> categories, ExternalCommandData commandData)
        {
            Dictionary<string, dynamic> elementDictionary = new Dictionary<string, dynamic>();
            foreach (dynamic element in categories)
            {
                elementDictionary.Add(element.Name + " - " + element.Id, element);
            }
            elementDictionary = Utils.SortDictionary(elementDictionary);
            GenericDropdownWindow selectionWindow = new GenericDropdownWindow("Select Elements", "Select only the Unused Elements you want to purge", elementDictionary, Utils.RevitWindow(commandData), true);
            selectionWindow.ShowDialog();
            if (selectionWindow.Cancelled)
            {
                return null;
            }
            else
            {
                return selectionWindow.SelectedItems;
            }
        }


    }
}
