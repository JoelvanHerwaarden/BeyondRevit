using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using adskWindows = Autodesk.Windows;

namespace BeyondRevit.ProjectTabs
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CloseAllTabsToTheLeft : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            UIDocument uiDoc = uiApp.ActiveUIDocument;
            
            foreach(UIView view in uiDoc.GetOpenUIViews())
            {
                if(view.ViewId != uiDoc.ActiveView.Id)
                {
                    view.Close();
                }
                else
                {
                    break;
                }
            }
            return Result.Succeeded;
        }
    }

}
