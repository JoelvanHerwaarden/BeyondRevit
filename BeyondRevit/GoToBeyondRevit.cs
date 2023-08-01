using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using adskWindows = Autodesk.Windows;

namespace BeyondRevit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GoToBeyondRevit : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            adskWindows.RibbonTab beyondRevit = FindBeyondRevitTab();
            beyondRevit.IsActive = true;
            return Result.Succeeded;
        }


        private static adskWindows.RibbonTab FindBeyondRevitTab()
        {
            adskWindows.RibbonTab result = null;
            adskWindows.RibbonTabCollection tabs = adskWindows.ComponentManager.Ribbon.Tabs;
            foreach (adskWindows.RibbonTab tab in tabs)
            {
                if (tab.Name == "Beyond Revit")
                {
                    result = tab;
                    break;
                }
            }
            return result;
        }
    }
}
